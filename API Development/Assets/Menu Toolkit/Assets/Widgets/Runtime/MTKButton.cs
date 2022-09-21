using InteractionTK.HandTracking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using VirtualRealityTK;

namespace InteractionTK.Menus
{
    public class MTKButton : MonoBehaviour
    {
        public Vector2 bounds = new Vector2(0.03f, 0.03f);
        public ITKInteractable interactable;
        public ITKPinchInteractable pinchInteractable;

        public UnityEvent<float> OnHover;
        public UnityEvent OnPress;

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
        private Dictionary<ITKHandController, Data> hoveringControllers = new Dictionary<ITKHandController, Data>();

        private void OnInteractHover(ITKInteractable interactable, ITKHandController controller)
        {
            if (!hoveringControllers.ContainsKey(controller))
                hoveringControllers.Add(controller, new Data());
            else
            {
                Data data = hoveringControllers[controller];

                for (int i = 0; i < ITKHand.NumTips; ++i)
                {
                    ITKHand.Joint joint = ITKHand.fingerTips[i];

                    // Check if tip is in the bounds of the button
                    Vector3 localPos = transform.InverseTransformPoint(controller.gesture.pose.positions[joint]);
                    bool bounded =
                        localPos.x > -bounds.x / 2f && localPos.x < bounds.x / 2f &&
                        localPos.y > -bounds.y / 2f && localPos.y < bounds.y / 2f;

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
                                OnPress?.Invoke();

                                Debug.Log("Press");
                            }
                            else if (distance > 0.01 && (data.pressedJoint == null || data.pressedJoint.Value == joint))
                            {
                                data.pressed = false;
                                data.pressedJoint = null;
                                
                                //OnPressUp?.Invoke();
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
            hoveringControllers.Remove(controller);
        }
    }
}
