using InteractionTK.HandTracking;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace InteractionTK
{
    public class ITKPinchInteractable : MonoBehaviour
    {
        public static HashSet<ITKPinchInteractable> interactables = new HashSet<ITKPinchInteractable>();

        public float distance = float.PositiveInfinity;
        public Collider[] colliders;

        public bool intention = true;

        public UnityEvent<ITKPinchInteractable, ITKPinchController> OnHover;
        public UnityEvent<ITKPinchInteractable, ITKPinchController> OnHoverExit;
        public UnityEvent<ITKPinchInteractable, ITKPinchController> OnInteract;
        public UnityEvent<ITKPinchInteractable, ITKPinchController> OnNoInteract;
        public UnityEvent<ITKPinchInteractable, ITKPinchController> OnInteractExit;

        public class Data
        {
            // If true it means the interaction was already grabbing, and needs to wait till its no longer grabbing
            public bool initialState;
            public bool onHoverExit = false;
            public bool onInteractExit = false;

            public Data(bool state)
            {
                initialState = state;
            }
        }
        public Dictionary<ITKPinchController, Data> hoveringControllers = new Dictionary<ITKPinchController, Data>();
        public HashSet<ITKPinchController> interactingControllers = new HashSet<ITKPinchController>();

        public bool isInteracting(ITKPinchController controller)
        {
            return controller.pinch > 0.8f;
        }

        public void Add(ITKPinchController controller)
        {
            if (!hoveringControllers.ContainsKey(controller))
                hoveringControllers.Add(controller, new Data((!intention || controller.intention > 0.5) && isInteracting(controller)));
        }

        public void Remove(ITKPinchController controller)
        {
            OnInteractExit?.Invoke(this, controller);
            hoveringControllers.Remove(controller);
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
            ITKPinchController[] controllers = hoveringControllers.Keys.ToArray();
            for (int i = 0; i < controllers.Length; ++i)
            {
                ITKPinchController controller = controllers[i];
                bool interact = isInteracting(controller);
                bool intent = (!intention || controller.intention > 0.5);

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
                            if (!interactingControllers.Contains(controller)) interactingControllers.Add(controller);

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
