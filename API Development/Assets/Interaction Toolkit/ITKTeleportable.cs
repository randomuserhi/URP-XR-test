using InteractionTK.HandTracking;
using VirtualRealityTK;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InteractionTK
{
    public class ITKTeleportable : MonoBehaviour
    {
        public ITKPinchInteractable pinchInteractable;

        private void Start()
        {
            if (pinchInteractable == null) pinchInteractable = GetComponent<ITKPinchInteractable>();
            if (pinchInteractable)
            {
                pinchInteractable.OnInteract.AddListener(OnInteract);
                pinchInteractable.OnInteractExit.AddListener(OnInteractExit);
            }
        }

        private bool teleport = false;
        public void OnInteract(ITKPinchInteractable interactable, ITKPinchController controller)
        {
            if (!enabled) return;

            if (controller.master.teleport && !teleport)
            {
                teleport = true;
                Vector3 location = new Vector3(controller.hit.point.x, 0, controller.hit.point.z);
                Vector3 dir = location - controller.master.transform.position;
                controller.master.transform.position = location - dir.normalized * 0.7f;
            }
        }

        public void OnInteractExit(ITKPinchInteractable interactable, ITKPinchController controller)
        {
            if (!enabled) return;

            teleport = false;
        }
    }
}
