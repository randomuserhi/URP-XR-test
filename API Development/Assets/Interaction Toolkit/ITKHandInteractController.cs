using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InteractionTK.HandTracking
{
    public class ITKHandInteractController : MonoBehaviour
    {
        public ITKGestures gesture;
        public ITKHandPhysics physicsHand;

        public float lingerTimer = 0.1f;

        private ITKInteractable interactable;
        private float linger = 0;

        public void SwapInteractable(ITKInteractable newInteractable) // may be undefined behaviour when swapping interaction whilst locked
        {
            interactable.nearbyControllers.Remove(this);
            interactable = newInteractable;
        }

        private bool active = true;
        public bool isLocked { get => !active; }
        public void Lock(ITKInteractable caller)
        {
            if (interactable != caller)
            {
                interactable.nearbyControllers.Remove(this);
                interactable = caller;
            }
            interactable.nearbyControllers.Add(this);
            active = false;
        }

        public void Unlock(ITKInteractable caller)
        {
            if (caller == interactable)
                active = true;
        }

        private void FixedUpdate()
        {
            if (!gesture)
            {
                Debug.LogError("Please assign gestures for this interact controller.");
                return;
            }

            if (!active) return;

            if (!gesture.active)
            {
                if (interactable)
                {
                    interactable.nearbyControllers.Remove(this);
                    interactable = null;
                }
                return;
            }

            float closest = float.PositiveInfinity;
            ITKInteractable newInteractable = null;
            for (int i = 0; i < ITKInteractable.interactables.Count; ++i)
            {
                ITKInteractable current = ITKInteractable.interactables[i];
                float dist = gesture.Distance(current.colliders);
                if (dist < closest && !current.isInteracting(this, out _))
                {
                    closest = dist;
                    newInteractable = current;
                }
            }

            if (interactable != null)
            {
                float distance = gesture.Distance(interactable.colliders);
                if (distance < interactable.distance && // Still within range of object
                   interactable == newInteractable || // Other object is itself
                    (distance < closest && // Another object is not closer
                    (newInteractable == null || closest > newInteractable.distance))) // The other object is within interact range
                {
                    linger = lingerTimer;
                }
                else if (linger >= 0)
                    linger -= Time.fixedDeltaTime;
                else
                {
                    interactable.nearbyControllers.Remove(this);
                    interactable = null;
                }
            }
            else if (newInteractable != null && closest < newInteractable.distance && !newInteractable.nearbyControllers.Contains(this))
            {
                interactable = newInteractable;
                interactable.nearbyControllers.Add(this);
            }
        }
    }
}
