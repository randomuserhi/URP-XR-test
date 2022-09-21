using InteractionTK.HandTracking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InteractionTK.HandTracking
{
    public class ITKPinchGestureController : MonoBehaviour
    {
        public ITKHand.Handedness type;

        public ITKGestures gesture;
        public ITKHandInteractController controller;

        public GameObject pinchCursor;
        public GameObject line;
        public GameObject pinchArrow;
        private SkinnedMeshRenderer pinchArrowRenderer;
        private LineRenderer lineRenderer;

        public Ray ray;

        public RaycastHit hit;
        [HideInInspector]
        public Vector3 localHit;
        private float _pinch;
        private float _intention;

        public float pinch { get => _pinch; }
        public float intention { get => _intention; }

        public float lingerTimer = 0.1f;

        [HideInInspector]
        public ITKPinchInteractable interactable;
        private float linger = 0;

        public void SwapInteractable(ITKPinchInteractable newInteractable) // may be undefined behaviour when swapping interaction whilst locked
        {
            interactable.hoveringControllers.Remove(this);
            interactable = newInteractable;
        }

        private bool _locked = false;
        public bool locked { get => _locked; }
        public void Lock(ITKPinchInteractable caller)
        {
            if (interactable != caller)
            {
                interactable.hoveringControllers.Remove(this);
                interactable = caller;
            }
            if (!interactable.hoveringControllers.ContainsKey(this))
                interactable.hoveringControllers.Add(this, false);
            _locked = true;
        }

        public void Unlock(ITKPinchInteractable caller)
        {
            if (caller == interactable)
                _locked = false;
        }

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
            if (lineRenderer) lineRenderer.enabled = true;
        }
        public void Disable()
        {
            if (!_active) return;
            _active = false;

            if (pinchArrow) pinchArrow.SetActive(false);
            if (pinchCursor) pinchCursor.SetActive(false);
            if (lineRenderer) lineRenderer.enabled = false;

            _pinch = 0;
            _intention = 0;

            if (interactable)
            {
                interactable.Remove(this);
                interactable = null;
            }
        }

        private void Start()
        {
            if (!gesture)
            {
                Debug.LogError("Please assign gestures for this interact controller.");
                return;
            }

            if (pinchArrow) pinchArrowRenderer = pinchArrow.GetComponent<SkinnedMeshRenderer>();
            if (line) lineRenderer = line.GetComponent<LineRenderer>();
        }

        private void OnDisable()
        {
            Disable();
        }

        private void OnEnable()
        {
            Enable();
        }

        public void FixedUpdate()
        {
            // If hand controller is interacting with something or gesture is inactive then disable as long as its not locked
            if (!_locked &&
                controller && controller.enabled && 
                (controller.locked || (controller.interactable != null && (!controller.interactable.intention || gesture.intention > 0.5f))))
                Disable();
            else Enable();
            if (!_active) return;

            const float threshhold = 0.45f;
            _pinch = Mathf.Clamp01((gesture.pinch - threshhold) / (1f - threshhold));

            Vector3 position = (gesture.pose.positions[ITKHand.ThumbTip] + gesture.pose.positions[ITKHand.IndexTip]) * 0.5f;
            Quaternion rotation = gesture.pose.rotations[ITKHand.Wrist] * (type == ITKHand.Handedness.Left ? Quaternion.Euler(36, 313, 310) : Quaternion.Euler(36, 47, 310));

            ray.direction = (rotation * Vector3.forward).normalized;
            ray.origin = gesture.pose.positions[ITKHand.Wrist] + gesture.pose.rotations[ITKHand.Wrist] * new Vector3(type == ITKHand.Handedness.Left ? 0.015f : -0.015f, -0.07f, 0.1f);

            if (pinchArrow) pinchArrow.transform.position = ray.origin;

            float newIntent = 0;
            RaycastHit newHit = new RaycastHit();
            ITKPinchInteractable newInteractable = null;
            float distance = float.PositiveInfinity;
            if (_pinch > 0 && Physics.Raycast(ray, out newHit, float.PositiveInfinity, ~LayerMask.GetMask("ITKHand")))
            {
                newIntent = gesture.CalculateIntent(newHit.point);
                newInteractable = newHit.collider.GetComponent<ITKPinchInteractable>();
                distance = newHit.distance;

                Vector3 dir = newHit.point - position;
                if (pinchArrow) pinchArrow.transform.rotation = Quaternion.LookRotation(dir);
            }
            else
            {
                if (pinchArrow) pinchArrow.transform.rotation = rotation;
            }

            if (_active && gesture.pinch > threshhold)
            {
                if (pinchArrow) pinchArrow.SetActive(true);
                if (lineRenderer) lineRenderer.enabled = true;
            }
            else
            {
                if (pinchArrow) pinchArrow.SetActive(false);
                if (lineRenderer) lineRenderer.enabled = false;
            }

            if (!_locked)
            {
                if (interactable != null)
                {
                    if (interactable == newInteractable)
                    {
                        linger = lingerTimer;

                        AssignValues(newHit, newIntent);
                    }
                    else if (linger >= 0)
                        linger -= Time.fixedDeltaTime;
                    else
                    {
                        interactable.Remove(this);
                        interactable = null;
                    }
                }
                else if (newInteractable != null && distance < newInteractable.distance && !newInteractable.hoveringControllers.ContainsKey(this))
                {
                    AssignValues(newHit, newIntent);

                    interactable = newInteractable;
                    interactable.Add(this);
                }
                else
                {
                    AssignValues(newHit, newIntent);
                }
            }

            if (pinchArrowRenderer)
            {
                pinchArrowRenderer.SetBlendShapeWeight(0, 100f * _pinch);
                pinchArrowRenderer.SetBlendShapeWeight(1, 100f * _pinch);
            }

            if (lineRenderer)
            {
                lineRenderer.positionCount = 2;
                float dist = 5;
                if (interactable)
                {
                    Vector3 pos = interactable.transform.TransformPoint(localHit);
                    dist = Vector3.Distance(pos, pinchArrow.transform.position);
                    lineRenderer.SetPosition(1, pos);
                }
                else if (newHit.transform)
                    dist = newHit.distance;
                dist = Mathf.Clamp(dist, 0, 5f);
                Vector3 dir = (pinchArrow.transform.rotation * Vector3.forward).normalized;
                lineRenderer.SetPosition(0, pinchArrow.transform.position + dir * Mathf.Clamp(dist * 0.5f, 0.1f, 0.3f));
                if (!interactable) lineRenderer.SetPosition(1, pinchArrow.transform.position + dir * dist);

                AnimationCurve curve = new AnimationCurve();
                curve.AddKey(0, Mathf.Clamp((1f - pinch), 0.1f, 1f) * 0.003f);
                lineRenderer.widthCurve = curve;
                lineRenderer.material.SetFloat("_Transparency", Mathf.Clamp01(0.01f + 0.5f * pinch));
            }
        }

        private void AssignValues(RaycastHit newHit, float newIntent)
        {
            hit = newHit;
            if (hit.transform) localHit = hit.transform.InverseTransformPoint(hit.point);
            else localHit = Vector3.zero;
            _intention = newIntent;
        }
    }
}
