using InteractionTK.HandTracking;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using VirtualRealityTK;
using static DebugUIBuilder;

namespace InteractionTK.Menus
{
    public class MTKButton : MonoBehaviour
    {
        public Vector2 bounds = new Vector2(0.03f, 0.03f);
        public ITKInteractable interactable;
        public ITKPinchInteractable pinchInteractable;

        public TextMeshProUGUI tmp;

        public UnityEvent<float> OnHover;
        public UnityEvent OnClick;

        // Start is called before the first frame update
        void Start()
        {
            if (interactable == null) interactable = GetComponent<ITKInteractable>();
            if (interactable)
            {
                interactable.OnHover.AddListener(OnInteractHover);
                interactable.OnHoverExit.AddListener(OnHoverExit);
            }

            if (pinchInteractable == null) pinchInteractable = GetComponent<ITKPinchInteractable>();
            if (pinchInteractable)
            {
                pinchInteractable.OnHover.AddListener(OnPinchHover);
                pinchInteractable.OnHoverExit.AddListener(OnHoverExit);
                pinchInteractable.OnInteract.AddListener(OnPinch);
                pinchInteractable.OnInteractExit.AddListener(OnPinchExit);
            }
        }

        private class Data
        {
            public bool[] initialState = new bool[ITKHand.NumTips];
            public bool pressed = false;
            public ITKHand.Joint? pressedJoint;
            public Data()
            {
                for (int i = 0; i < initialState.Length; ++i) initialState[i] = false;
            }
        }
        private Dictionary<ITKHandController, Data> hoveringHandControllers = new Dictionary<ITKHandController, Data>();
        private Dictionary<ITKPinchController, Data> pinchControllers = new Dictionary<ITKPinchController, Data>();

        private void OnPinch(ITKPinchInteractable interactable, ITKPinchController controller)
        {
            if (!pinchControllers.ContainsKey(controller))
                pinchControllers.Add(controller, new Data());

            Data data = pinchControllers[controller];
            if (!data.pressed)
            {
                OnClick?.Invoke();
                data.pressed = true;
            }
        }

        private void OnPinchExit(ITKPinchInteractable interactable, ITKPinchController controller)
        {
            if (!pinchControllers.ContainsKey(controller))
                pinchControllers.Add(controller, new Data());

            Data data = pinchControllers[controller];
            if (data.pressed)
            {
                data.pressed = false;
            }
        }

        private void OnPinchHover(ITKPinchInteractable interactable, ITKPinchController controller)
        {

        }

        private void OnHoverExit(ITKPinchInteractable interactable, ITKPinchController controller)
        {
            pinchControllers.Remove(controller);
        }

        private void OnInteractHover(ITKInteractable interactable, ITKHandController controller)
        {
            if (!hoveringHandControllers.ContainsKey(controller))
                hoveringHandControllers.Add(controller, new Data());
            else
            {
                Data data = hoveringHandControllers[controller];

                for (int i = 0; i < ITKHand.NumTips; ++i)
                {
                    ITKHand.Joint joint = ITKHand.fingerTips[i];

                    // Check if tip is in the bounds of the button
                    Vector3 localPos = transform.InverseTransformPoint(controller.gesture.pose.positions[joint]);
                    Vector3 scaledBounds = Vector3.Scale(bounds, transform.localScale);
                    bool bounded =
                        localPos.x > -scaledBounds.x / 2f && localPos.x < scaledBounds.x / 2f &&
                        localPos.y > -scaledBounds.y / 2f && localPos.y < scaledBounds.y / 2f;

                    if (bounded)
                    {
                        if (!data.initialState[i])
                            data.initialState[i] = Vector3.Dot(transform.position - controller.gesture.pose.positions[joint], transform.rotation * Vector3.forward) > 0;
                        else
                        {
                            float distance = VRTKUtils.SignedProjectedDistance(controller.gesture.pose.positions[joint], transform.position, transform.rotation * Vector3.forward);
                            
                            OnHover?.Invoke(distance);
                            
                            if (distance < -0.01f && !data.pressed)
                            {
                                data.pressed = true;
                                data.pressedJoint = joint;
                                OnClick?.Invoke();
                            }
                            else if (distance > 0.01 && (data.pressedJoint == null || data.pressedJoint.Value == joint))
                            {
                                data.pressed = false;
                                data.pressedJoint = null;
                            }
                        }
                    }
                    else
                    {
                        data.initialState[i] = false;
                        if (data.pressedJoint == null || data.pressedJoint.Value == joint) data.pressed = false;
                    }
                }
            }
        }

        private void OnHoverExit(ITKInteractable interactable, ITKHandController controller)
        {
            hoveringHandControllers.Remove(controller);
        }
    }
}
