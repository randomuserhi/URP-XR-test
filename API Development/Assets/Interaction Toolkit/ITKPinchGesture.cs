using InteractionTK.HandTracking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InteractionTK.HandTracking
{
    public class ITKPinchGesture : MonoBehaviour
    {
        public ITKHand.Handedness type;

        public ITKGestures gesture;

        public GameObject pinchCursor;
        public GameObject pinchArrow;
        private SkinnedMeshRenderer pinchArrowRenderer;

        public Ray ray;
        public float pinch;
        public float intention;

        public bool active
        {
            get => _active;
            set
            {
                _active = value;
                if (_active) Enable();
                else Disable();
            }
        }

        private bool _active = true;
        public void Enable()
        {
            if (_active) return;
            _active = true;

            if (pinchArrow) pinchArrow.SetActive(true);
            if (pinchCursor) pinchCursor.SetActive(true);
        }
        public void Disable()
        {
            if (!_active) return;
            _active = false;

            if (pinchArrow) pinchArrow.SetActive(false);
            if (pinchCursor) pinchCursor.SetActive(false);
        }

        private void Start()
        {
            pinchArrowRenderer = pinchArrow.GetComponent<SkinnedMeshRenderer>();
        }

        private static Quaternion offsetRot = Quaternion.Euler(50, 0, 0);
        private static Vector3 offsetPos = new Vector3(0.009f, -0.07f, 0.1f);
        public void Track(ITKHand.Pose pose)
        {
            if (!gesture)
            {
                Debug.LogError("Please assign gestures for this interact controller.");
                return;
            }

            const float threshhold = 0.45f;
            pinch = Mathf.Clamp01((gesture.pinch - threshhold) / (1f - threshhold));

            if (_active && gesture.pinch > threshhold)
                pinchArrow.SetActive(true);
            else
                pinchArrow.SetActive(false);

            Vector3 position = (pose.positions[ITKHand.ThumbTip] + pose.positions[ITKHand.IndexTip]) * 0.5f;
            Quaternion rotation = pose.rotations[ITKHand.Wrist] * offsetRot;
            ray.direction = rotation * Vector3.forward;

            if (pinchArrow) pinchArrow.transform.position = position;

            RaycastHit hit;
            ray.origin = pose.positions[ITKHand.Wrist] + pose.rotations[ITKHand.Wrist] * offsetPos;
            if (Physics.Raycast(ray, out hit, float.PositiveInfinity, ~LayerMask.GetMask("ITKHand")))
            {
                Vector3 dir = hit.point - position;
                if (pinchArrow) pinchArrow.transform.rotation = Quaternion.LookRotation(dir);
            }
            else
            {
                if (pinchArrow) pinchArrow.transform.rotation = rotation;
            }

            if (pinchArrowRenderer)
            {
                pinchArrowRenderer.SetBlendShapeWeight(0, 100f * pinch);
                pinchArrowRenderer.SetBlendShapeWeight(1, 100f * pinch);
            }
        }
    }
}
