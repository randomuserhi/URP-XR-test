using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InteractionTK.HandTracking;

namespace InteractionTK.Menus
{
    public class MTKButton : MonoBehaviour
    {
        private Rigidbody rb;
        private Collider col;
        public Vector3 axis = Vector3.forward;
        public float depth = 0.02f;
        public GameObject button;

        private void Start()
        {
            if (!button) CreateButton();
            
            rb = button.GetComponent<Rigidbody>();
            if (!rb) rb = button.AddComponent<Rigidbody>();

            col = GetComponentInChildren<Collider>();

            rb.constraints = RigidbodyConstraints.FreezeRotation;
            rb.useGravity = false;
        }

        private void CreateButton()
        {
            GameObject o = GameObject.CreatePrimitive(PrimitiveType.Cube);
            o.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
            o.transform.parent = transform;
            button = o;
        }

        //TODO:: add layers for hand and menus such that menus dont interact with world
        //       this should be a toggleable feature

        private bool ghosted = false;
        private void FixedUpdate()
        {
            //TODO:: make it be a force
            rb.velocity = (transform.position + axis * depth - rb.transform.position) / Time.fixedDeltaTime;
            
            rb.transform.position = Vector3.Project(rb.transform.position, transform.rotation * axis);
            Vector3 dir = rb.transform.position - transform.position;
            if (Vector3.Dot(dir, transform.rotation * axis) < 0 ||
                dir.magnitude > depth)
            {
                rb.transform.position = transform.position;
                rb.isKinematic = true;
                if (col) col.enabled = false; // Make a flag to enable or disable this feature of disabling the collider
            }

            if (rb.isKinematic && !Physics.CheckSphere(rb.transform.position, 0.025f))
            {
                rb.isKinematic = false;
                if (col) col.enabled = true;
            }
        }
    }
}