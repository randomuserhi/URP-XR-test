using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        public UnityEvent<ITKInteractable> OnNoHover;
        public UnityEvent<ITKInteractable, ITKHandInteractController> OnInteract;
        public UnityEvent<ITKInteractable, ITKHandInteractController> OnNoInteract;

        // If true it means the interaction was already grabbing, and needs to wait till its no longer grabbing
        public Dictionary<ITKHandInteractController, bool> nearbyControllers = new Dictionary<ITKHandInteractController, bool>();
        public Dictionary<ITKHandInteractController, Type> interactingControllers = new Dictionary<ITKHandInteractController, Type>();

        public void Add(ITKHandInteractController controller)
        {
            if (!nearbyControllers.ContainsKey(controller))
                nearbyControllers.Add(controller, (!intention || controller.gesture.intention > 0.5) && isInteracting(controller, out _));
        }

        public void Remove(ITKHandInteractController controller)
        {
            nearbyControllers.Remove(controller);
        }

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
                OnNoHover?.Invoke(this);
            }

            ITKHandInteractController[] controllers = nearbyControllers.Keys.ToArray();
            for (int i = 0; i < controllers.Length; ++i)
            {
                ITKHandInteractController controller = controllers[i];
                Type interactionType;
                bool interact = isInteracting(controller, out interactionType);
                bool intent = (!intention || controller.gesture.intention > 0.5);

                if (nearbyControllers[controller])
                {
                    nearbyControllers[controller] = interact;
                }
                else
                {
                    if (interact)
                    {
                        if (controller.isLocked || intent)
                        {
                            controller.Lock(this);
                            if (!interactingControllers.ContainsKey(controller))
                                interactingControllers.Add(controller, interactionType);

                            OnInteract?.Invoke(this, controller);
                        }
                        else if (!intent) // No intent to grab, set interact state to true to ensure grab doesn't happen as hand moves back into intent frame
                        {
                            nearbyControllers[controller] = true;
                        }
                    }
                    else if (!interact)
                    {
                        interactingControllers.Remove(controller);
                        controller.Unlock(this);

                        OnNoInteract?.Invoke(this, controller);
                    }
                }
            }
        }
    }
}
