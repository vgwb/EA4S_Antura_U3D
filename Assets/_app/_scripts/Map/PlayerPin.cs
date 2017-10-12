﻿using System.Collections;
using Antura.Audio;
using Antura.Core;
using DG.Tweening;
using System.Linq;
using Antura.Teacher;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Antura.Map
{
    /// <summary>
    /// The pin representing the player on the map.
    /// The player pin will move from one Pin to the next
    /// </summary>
    public class PlayerPin : MonoBehaviour
    {
        [Header("References")]
        public StageMapsManager stageMapsManager;
        public StageMap currentStageMap;
        //public FingerStage swipeScript;

        [Header("UIButtons")]
        public GameObject moveRightButton;
        public GameObject moveLeftButton;

        // Animation
        private Tween moveTween, rotateTween;
        private bool isAnimating = false;

        public bool IsAnimating { get { return isAnimating; } }

        public System.Action onMoveStart, onMoveEnd;

        #region Initialisation

        void Start()
        {
            StartFloatingAnimation();

            if (!AppManager.I.Player.IsFirstContact()) {
                CheckMovementButtonsEnabling();
            }
        }

        void OnDestroy()
        {
            moveTween.Kill();
            rotateTween.Kill();
        }

        #endregion

        #region Animation

        void StartFloatingAnimation()
        {
            transform.DOBlendableMoveBy(new Vector3(0, 1, 0), 1).SetLoops(-1, LoopType.Yoyo);
        }

        #endregion

        void LateUpdate()
        {
            // Map movement controls
            // @note: using late update so this interaction happens after FingerStage (so that touch swipe takes precedence)
            if (Input.GetMouseButtonUp(0) && !EventSystem.current.IsPointerOverGameObject()) { 
          //      && !swipeScript.isSwiping) {  // TODO: check the new map camera controller instead
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                RaycastHit hit;
                int layerMask = 1 << 15;
                if (Physics.Raycast(ray, out hit, 500, layerMask))
                {
                    if (hit.collider.CompareTag("Pin"))
                    {
                        var pin = hit.collider.transform.gameObject.GetComponent<Pin>();
                        if (pin.isLocked) return;

                        AudioManager.I.PlaySound(Sfx.UIButtonClick);
                        
                        MoveToPin(pin.pinIndex);
                    }
                }
            }
        }

        #region Movement

        private int CurrentPinIndex
        {
            get { return currentStageMap.CurrentPinIndex; }
        }

        private int CurrentTargetPosIndex
        {
            get { return StageMapsManager.GetPosIndexFromJourneyPosition(currentStageMap, StageMapsManager.CurrentJourneyPosition); }
        }

        public void MoveToNextDot()
        {
            MoveToPin(CurrentTargetPosIndex + 1);
        }

        public void MoveToPreviousDot()
        {
            MoveToPin(CurrentTargetPosIndex - 1);
        }

        public void MoveToJourneyPosition(JourneyPosition journeyPosition)
        {
            MoveToPin(StageMapsManager.GetPosIndexFromJourneyPosition(currentStageMap, journeyPosition)); 
        }

        private void MoveToPin(int pinIndex)
        {
            //if (pinIndex == CurrentTargetPosIndex) return;
            if (CanMoveTo(pinIndex))
            {
                stageMapsManager.mapCamera.SetAutoFollowTransformCurrentMap(transform);
                int lastIndex = CurrentPinIndex;
                AnimateToPin(pinIndex);
                LookAtPin(pinIndex < lastIndex, true, currentStageMap.CurrentPlayerPosJourneyPosition);
            }
        }

        private bool CanMoveTo(int pinIndex)
        {
            return pinIndex >= 0 &&
                   (pinIndex < currentStageMap.mapLocations.Count) &&
                   (pinIndex <= currentStageMap.MaxUnlockedPinIndex);
        }

        public void ForceToJourneyPosition(JourneyPosition journeyPosition, bool justVisuals = false)
        {
            int posIndex = StageMapsManager.GetPosIndexFromJourneyPosition(currentStageMap, journeyPosition);
            ForceToPin(posIndex, justVisuals);
            LookAtNextPin(false);
        }

        public void ResetPlayerPositionAfterStageChange(bool comingFromHigherStage)
        {
            if (comingFromHigherStage) {
                ForceToPin(currentStageMap.MaxUnlockedPinIndex);
                LookAtPreviousPin(false);
            } else {
                ForceToPin(0);
                LookAtNextPin(false);
            }
        }

        private Coroutine animateToPinCO;
        void AnimateToPin(int newIndex)
        {
            StopAnimation();
            animateToPinCO = StartCoroutine(AnimateToPinCO(newIndex));
        }

        public void StopAnimation(bool stopWhereItIs = true)
        {
            if (animateToPinCO != null && isAnimating)
            {
                StopCoroutine(animateToPinCO);
                animateToPinCO = null;
                if (stopWhereItIs)
                {
                    UpdatePlayerJourneyPosition(currentStageMap.CurrentPlayerPosJourneyPosition);
                }
            }
        }

        IEnumerator AnimateToPinCO(int targetIndex)
        {
            isAnimating = true;
            if (onMoveStart != null) onMoveStart();
            CheckMovementButtonsEnabling();
            int tmpCurrentIndex = currentStageMap.CurrentPinIndex;
            UpdatePlayerJourneyPosition(currentStageMap.mapLocations[targetIndex].JourneyPos);
            while (tmpCurrentIndex != targetIndex)
            {
                float stepDuration = Mathf.Max(0.1f, 0.5f / Mathf.Abs(targetIndex - tmpCurrentIndex));
                bool isAdvancing = targetIndex > tmpCurrentIndex;
                tmpCurrentIndex += isAdvancing ? 1 : -1;
                LookAtPin(!isAdvancing, true, currentStageMap.mapLocations[tmpCurrentIndex].JourneyPos);
                var nextPos = currentStageMap.mapLocations[tmpCurrentIndex].Position;
                yield return MoveToCO(nextPos, stepDuration);
                currentStageMap.ForceCurrentPinIndex(tmpCurrentIndex);
            }

            CheckMovementButtonsEnabling();
            isAnimating = false;
            if (onMoveEnd != null) onMoveEnd();
        }

        void ForceToPin(int newIndex, bool justVisuals = false)
        {
            //Debug.Log("Forcing to " + newIndex);
            currentStageMap.ForceCurrentPinIndex(newIndex);
            ForceToCO(currentStageMap.CurrentPlayerPosVector);

            if (!justVisuals) UpdatePlayerJourneyPosition(currentStageMap.CurrentPlayerPosJourneyPosition);
            CheckMovementButtonsEnabling();
        }

        private void UpdatePlayerJourneyPosition(JourneyPosition journeyPos)
        {
            AppManager.I.Player.SetCurrentJourneyPosition(journeyPos, false);
            stageMapsManager.UpdateDotHighlights();
            //Debug.LogWarning("Setting journey pos current: " + AppManager.I.Player.CurrentJourneyPosition);
        }

        #endregion

        #region LookAt

        void LookAtNextPin(bool animated)
        {
            LookAtPin(false, animated, AppManager.I.Player.CurrentJourneyPosition);
        }

        void LookAtPreviousPin(bool animated)
        {
            LookAtPin(true, animated, AppManager.I.Player.CurrentJourneyPosition);
        }

        void LookAtPin(bool lookAtPrevious, bool animated, JourneyPosition fromJourneyPosition)
        {
            rotateTween.Kill();

            // Target rotation 
            int fromPinIndex = CurrentPinIndex;
            int toPinIndex = lookAtPrevious ? CurrentPinIndex -1 : CurrentPinIndex + 1;

            var fromPin = currentStageMap.PinForIndex(fromPinIndex);
            var toPin = currentStageMap.PinForIndex(toPinIndex);
            var lookingFromTr = fromPin != null? fromPin.transform : toPin.transform;
            var lookingToTr = toPin != null ? toPin.transform : fromPin.transform;
            Quaternion toRotation = Quaternion.LookRotation(lookingToTr.transform.position - lookingFromTr.transform.position, Vector3.up);
            //Debug.Log("Look from " + fromPin + " To " + toPin);
            //Debug.Log("Current " + transform.rotation + " To " + toRotation);

            if (animated) {
                transform.rotation = transform.rotation;
                rotateTween = transform.DORotate(toRotation.eulerAngles, 0.15f).SetEase(Ease.InOutQuad);
            } else {
                transform.rotation = toRotation;
            }
        }

        #endregion

        #region Actual Movement

        // If animate is TRUE, animates the movement, otherwise applies the movement immediately
        private IEnumerator MoveToCO(Vector3 position, float stepDuration)
        {
            //Debug.Log("Moving to " + position);
            if (moveTween != null) {
                moveTween.Kill();
            }
            moveTween = transform.DOMove(position, stepDuration).SetEase(Ease.Linear);
            yield return moveTween.WaitForCompletion();
        }

        private void ForceToCO(Vector3 position)
        {
            if (moveTween != null)
            {
                moveTween.Kill();
            }
            transform.position = position;
        }

    #endregion

    #region UI

    public void CheckMovementButtonsEnabling()
        {
            //Debug.Log("Enabling buttons for " + CurrentPinIndex);
            if (CurrentPinIndex == 0) {
                if (currentStageMap.MaxUnlockedPinIndex == 0) {
                    moveRightButton.SetActive(false);
                    moveLeftButton.SetActive(false);
                } else {
                    moveRightButton.SetActive(false);
                    moveLeftButton.SetActive(true);
                }
            } else if (CurrentPinIndex == currentStageMap.MaxUnlockedPinIndex) {
                moveRightButton.SetActive(true);
                moveLeftButton.SetActive(false);
            } else {
                moveRightButton.SetActive(true);
                moveLeftButton.SetActive(true);
            }
        }

        #endregion
    }
}