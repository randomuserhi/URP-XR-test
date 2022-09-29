using InteractionTK.HandTracking;
using Oculus.Platform.Models;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InteractionTK
{
    public class ITKPinchGrabbable : MonoBehaviour
    {
        private class Grab
        {
            public ConfigurableJoint joint;
            private ITKPinchGrabbable self;
            public Grab(ITKPinchGrabbable self, ITKPinchController controller)
            {
                this.self = self;

                if (self.rb)
                {
                    joint = self.gameObject.AddComponent<ConfigurableJoint>();
                    joint.rotationDriveMode = RotationDriveMode.Slerp;
                    joint.slerpDrive = new JointDrive()
                    {
                        positionSpring = 1e+20f,
                        positionDamper = 1e+18f,
                        maximumForce = 5f
                    };
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

                    joint.targetRotation = Quaternion.identity;
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

        private Rigidbody rb;
        public bool physicsObject = true;
        public bool useGravity = true;

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
            }
        }

        public void OnInteractExit(ITKPinchInteractable interactable, ITKPinchController controller)
        {
            if (!enabled) return;

            if (interactingHands.ContainsKey(controller))
            {
                if (!physicsObject) rb.isKinematic = true;
                rb.useGravity = useGravity;

                interactingHands[controller].Destroy();
                interactingHands.Remove(controller);
            }
        }
    }
}