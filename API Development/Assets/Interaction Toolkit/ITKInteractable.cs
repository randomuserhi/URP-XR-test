using Oculus.Interaction;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace InteractionTK.HandTracking
{
    public class ITKInteractable : MonoBehaviour
    {
        public static List<ITKInteractable> interactables = new List<ITKInteractable>();

        public float distance = 0.05f;
        public Collider[] colliders;

        public bool intention = true;
        public bool pinch = true;
        public bool grasp = true;

        public enum Type
        {
            None,
            Grasp,
            Pinch
        }

        public UnityEvent<ITKInteractable> OnHover;
        public UnityEvent<ITKInteractable> OnHoverExit;
        public UnityEvent<ITKInteractable, ITKHandInteractController> OnInteract;
        public UnityEvent<ITKInteractable, ITKHandInteractController> OnInteractExit;

        public HashSet<ITKHandInteractController> nearbyControllers = new HashSet<ITKHandInteractController>();
        public Dictionary<ITKHandInteractController, Type> interactingControllers = new Dictionary<ITKHandInteractController, Type>();

        public bool isInteracting(ITKHandInteractController controller, out Type interactionType)
        {
            bool interact = false;
            interactionType = Type.None;
            if (controller.gesture.grasp > 0.6f)
            {
                if (grasp)
                {
                    interactionType = Type.Grasp;
                    interact = true;
                }
            }
            else if (controller.gesture.pinch > 0.8f)
            {
                if (pinch)
                {
                    interactionType = Type.Pinch;
                    interact = true;
                }
            }

            return interact;
        }

        private void Start()
        {
            interactables.Add(this);
        }

        private void OnDestroy()
        {
            interactables.Remove(this);
        }

        private void FixedUpdate()
        {
            if (nearbyControllers.Count > 0)
            {
                OnHover?.Invoke(this);
            }
            else
            {
                OnHoverExit?.Invoke(this);
            }

            foreach (ITKHandInteractController controller in nearbyControllers)
            {
                Type interactionType;
                bool interact = isInteracting(controller, out interactionType);

                if (interact && (controller.isLocked || !intention || controller.gesture.intention > 0.2))
                {
                    controller.Lock(this);
                    if (!interactingControllers.ContainsKey(controller))
                        interactingControllers.Add(controller, interactionType);
                    
                    OnInteract?.Invoke(this, controller);
                }
                else if (!interact)
                {
                    interactingControllers.Remove(controller);
                    controller.Unlock(this);

                    OnInteractExit?.Invoke(this, controller);
                }
            }
        }
    }
}
