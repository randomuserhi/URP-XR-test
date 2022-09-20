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
        public UnityEvent<ITKInteractable> OnHoverExit;
        public UnityEvent<ITKInteractable, ITKHandInteractController> OnInteract;
        public UnityEvent<ITKInteractable, ITKHandInteractController> OnNoInteract;
        public UnityEvent<ITKInteractable, ITKHandInteractController> OnInteractExit;

        // If true it means the interaction was already grabbing, and needs to wait till its no longer grabbing
        public Dictionary<ITKHandInteractController, bool> hoveringControllers = new Dictionary<ITKHandInteractController, bool>();
        public Dictionary<ITKHandInteractController, Type> interactingControllers = new Dictionary<ITKHandInteractController, Type>();

        public void Add(ITKHandInteractController controller)
        {
            if (!hoveringControllers.ContainsKey(controller))
                hoveringControllers.Add(controller, (!intention || controller.gesture.intention > 0.5) && isInteracting(controller, out _));
        }

        public void Remove(ITKHandInteractController controller)
        {
            OnInteractExit?.Invoke(this, controller);
            hoveringControllers.Remove(controller);
            interactingControllers.Remove(controller);
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

        private bool onHoverExit = false;
        private bool onInteractExit = false;
        private void FixedUpdate()
        {
            if (hoveringControllers.Count > 0)
            {
                onHoverExit = true;
                OnHover?.Invoke(this);
            }
            else if (onHoverExit)
            {
                onHoverExit = false;
                OnHoverExit?.Invoke(this);
            }

            ITKHandInteractController[] controllers = hoveringControllers.Keys.ToArray();
            for (int i = 0; i < controllers.Length; ++i)
            {
                ITKHandInteractController controller = controllers[i];
                Type interactionType;
                bool interact = isInteracting(controller, out interactionType);
                bool intent = (!intention || controller.gesture.intention > 0.5);

                if (hoveringControllers[controller])
                {
                    hoveringControllers[controller] = interact;
                }
                else
                {
                    if (interact)
                    {
                        if (controller.locked || intent)
                        {
                            controller.Lock(this);
                            if (!interactingControllers.ContainsKey(controller))
                                interactingControllers.Add(controller, interactionType);

                            onInteractExit = true;
                            OnInteract?.Invoke(this, controller);
                        }
                        else if (!intent) // No intent to grab, set interact state to true to ensure grab doesn't happen as hand moves back into intent frame
                        {
                            hoveringControllers[controller] = true;
                        }
                    }
                    else if (!interact)
                    {
                        interactingControllers.Remove(controller);
                        controller.Unlock(this);

                        if (onInteractExit)
                        {
                            onInteractExit = false;
                            OnInteractExit?.Invoke(this, controller);
                        }
                        OnNoInteract?.Invoke(this, controller);
                    }
                }
            }
        }
    }
}
