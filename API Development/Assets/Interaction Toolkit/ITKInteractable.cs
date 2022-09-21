using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

using InteractionTK.HandTracking;

namespace InteractionTK
{
    public class ITKInteractable : MonoBehaviour
    {
        public static HashSet<ITKInteractable> interactables = new HashSet<ITKInteractable>();

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

        public UnityEvent<ITKInteractable, ITKHandController> OnHover;
        public UnityEvent<ITKInteractable, ITKHandController> OnHoverExit;
        public UnityEvent<ITKInteractable, ITKHandController> OnInteract;
        public UnityEvent<ITKInteractable, ITKHandController> OnNoInteract;
        public UnityEvent<ITKInteractable, ITKHandController> OnInteractExit;

        public class Data
        {
            // If true it means the interaction was already grabbing, and needs to wait till its no longer grabbing
            public bool initialState;
            public bool onHoverExit = false;
            public bool onInteractExit = false;

            public Data (bool state)
            {
                initialState = state;
            }
        }
        public Dictionary<ITKHandController, Data> hoveringControllers = new Dictionary<ITKHandController, Data>();
        public Dictionary<ITKHandController, Type> interactingControllers = new Dictionary<ITKHandController, Type>();

        public void Add(ITKHandController controller)
        {
            if (!hoveringControllers.ContainsKey(controller))
                hoveringControllers.Add(controller, new Data((!intention || controller.gesture.intention > 0.5) && isInteracting(controller, out _)));
        }

        public void Remove(ITKHandController controller)
        {
            OnInteractExit?.Invoke(this, controller);
            hoveringControllers.Remove(controller);
            interactingControllers.Remove(controller);
        }

        public bool isInteracting(ITKHandController controller, out Type interactionType)
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
            ITKHandController[] controllers = hoveringControllers.Keys.ToArray();
            for (int i = 0; i < controllers.Length; ++i)
            {
                ITKHandController controller = controllers[i];
                Type interactionType;
                bool interact = isInteracting(controller, out interactionType);
                bool intent = (!intention || controller.gesture.intention > 0.5);

                Data data = hoveringControllers[controller];

                if (intent)
                {
                    data.onHoverExit = true;
                    OnHover?.Invoke(this, controller);
                }
                else if (data.onHoverExit)
                {
                    data.onHoverExit = false;
                    OnHoverExit?.Invoke(this, controller);
                }

                if (data.initialState)
                    data.initialState = interact;
                else
                {
                    if (interact)
                    {
                        if (controller.locked || intent)
                        {
                            controller.Lock(this);
                            if (!interactingControllers.ContainsKey(controller))
                                interactingControllers.Add(controller, interactionType);

                            data.onInteractExit = true;
                            OnInteract?.Invoke(this, controller);
                        }
                        else if (!intent) // No intent to grab, set interact state to true to ensure grab doesn't happen as hand moves back into intent frame
                        {
                            data.initialState = true;
                        }
                    }
                    else if (!interact)
                    {
                        interactingControllers.Remove(controller);
                        controller.Unlock(this);

                        if (data.onInteractExit)
                        {
                            data.onInteractExit = false;
                            OnInteractExit?.Invoke(this, controller);
                        }
                        OnNoInteract?.Invoke(this, controller);
                    }
                }
            }
        }
    }
}
