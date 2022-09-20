using InteractionTK.HandTracking;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class ITKPinchInteractable : MonoBehaviour
{
    public float distance = float.PositiveInfinity;
    public bool intention = true;

    public UnityEvent<ITKPinchInteractable> OnHover;
    public UnityEvent<ITKPinchInteractable> OnHoverExit;
    public UnityEvent<ITKPinchInteractable, ITKPinchGestureController> OnInteract;
    public UnityEvent<ITKPinchInteractable, ITKPinchGestureController> OnNoInteract;
    public UnityEvent<ITKPinchInteractable, ITKPinchGestureController> OnInteractExit;

    // If true it means the interaction was already grabbing, and needs to wait till its no longer grabbing
    public Dictionary<ITKPinchGestureController, bool> hoveringControllers = new Dictionary<ITKPinchGestureController, bool>();
    public HashSet<ITKPinchGestureController> interactingControllers = new HashSet<ITKPinchGestureController>();

    public bool isInteracting(ITKPinchGestureController controller)
    {
        return controller.pinch > 0.8f;
    }

    public void Add(ITKPinchGestureController controller)
    {
        if (!hoveringControllers.ContainsKey(controller))
            hoveringControllers.Add(controller, (!intention || controller.intention > 0.5) && isInteracting(controller));
    }

    public void Remove(ITKPinchGestureController controller)
    {
        OnInteractExit?.Invoke(this, controller);
        hoveringControllers.Remove(controller);
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

        ITKPinchGestureController[] controllers = hoveringControllers.Keys.ToArray();
        for (int i = 0; i < controllers.Length; ++i)
        {
            ITKPinchGestureController controller = controllers[i];
            bool interact = isInteracting(controller);
            bool intent = (!intention || controller.intention > 0.5);

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
                        if (!interactingControllers.Contains(controller)) interactingControllers.Add(controller);

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
