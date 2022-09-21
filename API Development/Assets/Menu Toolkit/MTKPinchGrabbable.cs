using InteractionTK.HandTracking;
using InteractionTK;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InteractionTK.Menus
{
    public class MTKPinchGrabbable : MonoBehaviour
    {
        private class Grab
        {
            public ConfigurableJoint joint;
            private MTKPinchGrabbable self;
            public Grab(MTKPinchGrabbable self, ITKPinchController controller)
            {
                this.self = self;

                if (self.rb)
                {
                    joint = self.gameObject.AddComponent<ConfigurableJoint>();
                    JointDrive drive = new JointDrive()
                    {
                        positionSpring = 1e+20f,
                        positionDamper = 5e+18f,
                        maximumForce = 50f
                    };
                    joint.xDrive = drive;
                    joint.yDrive = drive;
                    joint.zDrive = drive;

                    joint.anchor = controller.localHit;
                    joint.autoConfigureConnectedAnchor = false;

                    joint.targetPosition = Vector3.zero;
                }
            }

            public void Destroy()
            {
                UnityEngine.Object.Destroy(joint);

                if (self.rb) // wake up rb by adding tiny velocity => sometimes rb is asleep causing it to freeze in air
                    self.rb.velocity += new Vector3(0, 0.0001f, 0);
            }
        }

        public ITKPinchInteractable pinchInteractable;
        public float minDist = 0.15f;
        public GameObject center;

        private Rigidbody rb;
        private bool physicsObject;

        private Dictionary<ITKPinchController, Grab> interactingHands = new Dictionary<ITKPinchController, Grab>();
        private HashSet<ITKPinchController> hands = new HashSet<ITKPinchController>();

        private void Start()
        {
            if (pinchInteractable == null) pinchInteractable = GetComponent<ITKPinchInteractable>();
            if (pinchInteractable)
            {
                pinchInteractable.OnInteract.AddListener(OnInteract);
                pinchInteractable.OnInteractExit.AddListener(OnInteractExit);
            }

            rb = GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody>();
                rb.isKinematic = true;
                rb.mass = 0.5f; // Make mass reasonable
            }

            physicsObject = !rb.isKinematic;
        }

        public void OnInteract(ITKPinchInteractable interactable, ITKPinchController controller)
        {
            if (!enabled) return;

            if (!hands.Contains(controller))
                hands.Add(controller);

            if (!interactingHands.ContainsKey(controller))
            {
                Grab grab = new Grab(this, controller);
                interactingHands.Add(controller, grab);
            }

            if (interactingHands.ContainsKey(controller))
            {
                if (!physicsObject) rb.isKinematic = false;
                rb.useGravity = false;

                if (rb) // wake up rb by adding tiny velocity => sometimes rb is asleep causing it to freeze in air
                    rb.velocity += new Vector3(0, 0.0001f, 0);

                // Make sure the object doesn't overshoot
                Vector3 anchor = controller.ray.origin + controller.ray.direction * Mathf.Clamp(controller.hit.distance, minDist, float.PositiveInfinity);
                Vector3 dir = anchor - rb.position;
                if (rb.velocity.magnitude > 0.1f && Vector3.Dot(rb.velocity, dir) < 0)
                {
                    rb.velocity *= 0.5f;
                    rb.angularVelocity *= 0.5f;
                }
                interactingHands[controller].joint.connectedAnchor = anchor;
                transform.rotation = Quaternion.LookRotation(center.transform.position - Camera.main.transform.position);
            }
        }

        public void OnInteractExit(ITKPinchInteractable interactable, ITKPinchController controller)
        {
            if (!enabled) return;

            if (interactingHands.ContainsKey(controller))
            {
                if (!physicsObject) rb.isKinematic = true;
                rb.useGravity = true;

                interactingHands[controller].Destroy();
                interactingHands.Remove(controller);
            }
        }
    }
}