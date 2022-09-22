using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using InteractionTK.HandTracking;

namespace InteractionTK
{
    public class ITKHandController : MonoBehaviour
    {
        public ITKGestures gesture;
        public ITKHandPhysics physicsHand;
        public ITKHandNonPhysics nonPhysicsHand;

        public float lingerTimer = 0.1f;

        [HideInInspector]
        public ITKInteractable interactable;
        private float linger = 0;

        public void SwapInteractable(ITKInteractable newInteractable) // may be undefined behaviour when swapping interaction whilst locked
        {
            interactable.Remove(this);
            interactable = newInteractable;
        }

        private bool _locked = false;
        public bool locked { get => _locked; }
        public void Lock(ITKInteractable caller)
        {
            if (interactable != caller)
            {
                interactable.Remove(this);
                interactable = caller;
            }
            if (!interactable.hoveringControllers.ContainsKey(this))
                interactable.hoveringControllers.Add(this, new ITKInteractable.Data(false));
            _locked = true;
        }

        public void Unlock(ITKInteractable caller)
        {
            if (caller == interactable)
                _locked = false;
        }

        private bool _active = true;
        public void Enable()
        {
            if (_active) return;
            _active = true;
        }
        public void Disable()
        {
            if (!_active) return;
            _active = false;

            _locked = false;
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
        }

        private void OnDisable()
        {
            Disable();
        }

        private void OnEnable()
        {
            Enable();
        }

        private void FixedUpdate()
        {
            if (_locked)
            {
                if (!ITKInteractable.interactables.Contains(interactable) || !interactable.enabled || !interactable.gameObject.activeInHierarchy)
                {
                    Unlock(interactable);
                }
                return;
            }
            if (!gesture.active) Disable();
            else Enable();
            if (!_active) return;

            float closest = float.PositiveInfinity;
            ITKInteractable newInteractable = null;
            foreach (ITKInteractable interactable in ITKInteractable.interactables)
            {
                float dist = gesture.Distance(interactable.colliders);
                if (dist < closest)
                {
                    closest = dist;
                    newInteractable = interactable;
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
                    interactable.Remove(this);
                    interactable = null;
                }
            }
            else if (newInteractable != null && closest < newInteractable.distance && !newInteractable.hoveringControllers.ContainsKey(this))
            {
                interactable = newInteractable;
                interactable.Add(this);
            }
        }
    }
}
