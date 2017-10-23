﻿using Antura.Core;
using Antura.UI;
using Boo.Lang;
using DG.DeExtensions;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Antura.Map
{
    /// <summary>
    /// A pin on the map. 
    /// Defines a PlaySession. Either assessment or minigame.
    /// </summary>
    public class Pin : MonoBehaviour, IMapLocation
    {
        [HideInInspector]
        public JourneyPosition journeyPosition;

        [Header("References")]
        public Dot mainDot;  
        [HideInInspector]
        public Rope rope;
        [HideInInspector]
        public List<Dot> dots = new List<Dot>();

        public GameObject pinV1;
        public GameObject pinV2;
        public GameObject pinAssessment;

        public GameObject roadSignGO;
        public TextRender roadSignTextUI;

        [HideInInspector]
        public GameObject currentPinMesh;

        public PlaySessionStateFeedback playSessionFeedback;

        [HideInInspector]
        public bool isLocked;

        private Transform shadowTr;

        [Header("Animation")]
        private Vector3 startPinPosition;
        private Vector3 startRopeScale;

        public Vector3 Position
        {
            get { return transform.position; }
        }

        public JourneyPosition JourneyPos
        {
            get { return journeyPosition; }
        }

        // Sequential index for the stage map this pin is in
        public int pinIndex;

        public void Initialise(int _pinIndex, JourneyPosition _journeyPosition)
        {
            pinIndex = _pinIndex;
            journeyPosition = _journeyPosition;

            name = "Pin_" + _journeyPosition;

            // Road sign with LB
            roadSignGO.SetActive(journeyPosition.PlaySession == 1);
            roadSignTextUI.SetText(journeyPosition.Stage + "-" + journeyPosition.LearningBlock);

            // Choosing the correct PIN based on the journey position
            pinV1.gameObject.SetActive(false);
            pinV2.gameObject.SetActive(false);
            pinAssessment.gameObject.SetActive(false);
            if (journeyPosition.IsAssessment())
            {
                currentPinMesh = pinAssessment;
            }
            else
            {
                if (journeyPosition.LearningBlock % 2 == 0)
                {
                    currentPinMesh = pinV1;
                }
                else {
                    currentPinMesh = pinV2;
                }
            }
            currentPinMesh.gameObject.SetActive(true);

            shadowTr = transform.Find("shadow");
        }

        #region Appear / Disappear

        private bool appeared = false;

        public bool Appeared { get { return appeared; } }

        public void Disappear()
        {
            appeared = false;
            startPinPosition = currentPinMesh.transform.position;
            if (rope != null) startRopeScale = rope.meshRenderer.transform.localScale;

            currentPinMesh.transform.position = startPinPosition + Vector3.up * 60;
            mainDot.transform.SetLocalScale(0);
            if (rope != null)
            {
                rope.meshRenderer.transform.SetLocalScale(0);
                foreach (var dot in dots)
                    dot.Disappear();
            }

            shadowTr.SetLocalScale(0);
            currentPinMesh.gameObject.SetActive(false);
            playSessionFeedback.gameObject.SetActive(false);
        }

        public void Appear(float duration)
        {
            if (appeared) return;
            appeared = true;
            currentPinMesh.gameObject.SetActive(true);
            currentPinMesh.transform.DOMove(startPinPosition, duration*0.5f);
            mainDot.transform.DOScale(Vector3.one * 6, duration * 0.5f).SetEase(Ease.OutElastic).SetDelay(duration * 0.5f).OnComplete(
                () =>
                playSessionFeedback.gameObject.SetActive(true)  // make the feedback appear at the end
                );
            shadowTr.DOScale(Vector3.one * 12.5f, duration * 0.5f).SetEase(Ease.OutElastic).SetDelay(duration * 0.5f);

            if (rope != null) rope.meshRenderer.transform.DOScale(startRopeScale, duration * 0.5f).SetDelay(duration * 0.5f);
        }

        public void FlushAppear()
        {
            if (appeared) return;
            appeared = true;
            currentPinMesh.gameObject.SetActive(true);
            currentPinMesh.transform.position = startPinPosition;
            mainDot.transform.localScale = Vector3.one * 6;
            shadowTr.transform.localScale = Vector3.one * 12.5f;
            playSessionFeedback.gameObject.SetActive(true);  

            if (rope != null) rope.meshRenderer.transform.localScale = startRopeScale;
        }

        #endregion

        #region Locking

        public void SetUnlocked()
        {
            isLocked = false;
            mainDot.gameObject.SetActive(true);

            playSessionFeedback.ShowUnhighlightedInfo();
        }

        public void SetLocked()
        {
            isLocked = true;
            mainDot.gameObject.SetActive(false);

            playSessionFeedback.HideAllInfo();
        }
        
        #endregion

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                currentPinMesh.SetActive(false);
                if (Dot.highlightOnPlayerCollision) Highlight(true);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                currentPinMesh.SetActive(true);
                if (Dot.highlightOnPlayerCollision ) Highlight(false);
            }
        }

        public void Highlight(bool choice)
        {
            if (!isLocked)
            {
                mainDot.Highlight(choice);
                playSessionFeedback.Highlight(choice);
            }
        }

        #region Play Session State

        public void SetPlaySessionState(PlaySessionState playSessionState)
        {
            playSessionFeedback.Initialise(journeyPosition, playSessionState);
        }

        #endregion

    }
}