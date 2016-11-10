﻿using UnityEngine;
using System.Collections;

namespace EA4S.ThrowBalls
{
    public class ArrowBodyController : MonoBehaviour
    {
        public static ArrowBodyController instance;

        void Awake()
        {
            instance = this;
        }

        public void OnUpdateDistance(Vector3 distanceVector)
        {
            UpdateRotation(Mathf.Rad2Deg * GetAngleOfVector(distanceVector));
            UpdateScale(distanceVector.magnitude);
            UpdatePosition(distanceVector);
        }

        private void UpdatePosition(Vector3 distance)
        {
            float bodyLength = transform.localScale.x;

            float deltaX = Mathf.Cos(Mathf.Deg2Rad * (180 - transform.localRotation.eulerAngles.y)) * 0.5f * bodyLength;
            float deltaZ = Mathf.Sin(Mathf.Deg2Rad * (180 - transform.localRotation.eulerAngles.y)) * 0.5f * bodyLength;

            Vector3 origin = SlingshotController.instance.GetSlingshotCenterPosition();
            origin.y = GroundController.instance.transform.position.y;

            Vector3 position = origin;
            position.x += deltaX;
            position.z += deltaZ;
            transform.position = position;
        }

        private float GetAngleOfVector(Vector3 vector)
        {
            return Mathf.Atan2(vector.z, -vector.x);
        }

        private void UpdateRotation(float rotationInDegrees)
        {
            transform.rotation = Quaternion.Euler(90, rotationInDegrees, 0);
        }

        private void UpdateScale(float scale)
        {
            Vector3 currentScale = transform.localScale;
            currentScale.x = scale;
            transform.localScale = currentScale;
        }
    }
}