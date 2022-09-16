using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InteractionTK.HandTracking
{
    public class ITKInteractable : MonoBehaviour
    {
        public static List<ITKInteractable> interactables = new List<ITKInteractable>();

        public float distance = 0.05f;
        public Collider[] colliders;

        public bool pinch = true;
        public bool grasp = true;

        //[HideInInspector]
        public HashSet<ITKHandInteractController> connectedControllers = new HashSet<ITKHandInteractController>();

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
            if (connectedControllers.Count > 0)
            {
                GetComponent<MeshRenderer>().material.color = Color.red;
            }
            else
            {
                GetComponent<MeshRenderer>().material.color = Color.white;
            }
        }
    }
}
