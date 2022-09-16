using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InteractionTK.HandTracking
{
    public class ITKHandInteractController : MonoBehaviour
    {
        public ITKGestures gestures;
        public ITKHandPhysics physicsHand;

        public float lingerTimer = 0.2f;

        private ITKInteractable interactable;
        private float linger = 0;

        private void FixedUpdate()
        {
            if (!gestures)
            {
                Debug.LogError("Please assign gestures for this interact controller.");
                return;
            }

            if (interactable == null)
            {
                float closest = float.PositiveInfinity;
                ITKInteractable newInteractable = null;
                for (int i = 0; i < ITKInteractable.interactables.Count; ++i)
                {
                    ITKInteractable current = ITKInteractable.interactables[i];
                    float dist = gestures.Distance(current.colliders);
                    if (dist < closest)
                    {
                        closest = dist;
                        newInteractable = current;
                    }
                }

                if (newInteractable != null && closest < newInteractable.distance && !newInteractable.connectedControllers.Contains(this))
                {
                    interactable = newInteractable;
                    newInteractable.connectedControllers.Add(this);
                }
            }
            else
            {
                float dist = gestures.Distance(interactable.colliders);
                if (dist < interactable.distance)
                    linger = lingerTimer;
                else if (linger > 0)
                    linger -= Time.fixedDeltaTime;
                else
                {
                    interactable.connectedControllers.Remove(this);
                    interactable = null;
                }
            }
        }
    }
}
