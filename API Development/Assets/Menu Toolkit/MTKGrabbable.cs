using InteractionTK.HandTracking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InteractionTK.Menus
{
    public class MTKGrabbable : MonoBehaviour
    {
        private class Grab
        {
            public Vector3 thumbLocalScaledAnchor;
            public Vector3 thumbLocalAnchor;
            public Vector3 thumbAnchor;
            public Vector3 palmLocalScaledAnchor;
            public Vector3 palmLocalAnchor;
            public Vector3 palmAnchor;
            public ConfigurableJoint joint;
            private MTKGrabbable self;
            public Grab(MTKGrabbable self, ITKInteractable interactable, ITKHandController controller)
            {
                this.self = self;

                thumbAnchor = controller.gesture.ClosestPointFromJoint(interactable.colliders, ITKHand.ThumbTip, out _);
                thumbLocalAnchor = Quaternion.Inverse(controller.gesture.pose.rotations[ITKHand.Root]) * (self.transform.position - thumbAnchor - controller.gesture.pose.positions[ITKHand.Root]);
                thumbLocalScaledAnchor = self.transform.InverseTransformPoint(thumbAnchor);

                palmAnchor = controller.gesture.ClosestPointFromJoint(interactable.colliders, ITKHand.Palm, out _);
                palmLocalAnchor = Quaternion.Inverse(controller.gesture.pose.rotations[ITKHand.Root]) * (self.transform.position - palmAnchor - controller.gesture.pose.positions[ITKHand.Root]);
                palmLocalScaledAnchor = self.transform.InverseTransformPoint(palmAnchor);

                if (self.rb)
                {
                    joint = self.gameObject.AddComponent<ConfigurableJoint>();
                    if (controller.physicsHand) joint.connectedBody = controller.physicsHand.skeleton.root.rb;
                    else if (controller.nonPhysicsHand) joint.connectedBody = controller.nonPhysicsHand.skeleton.root.rb;
                    JointDrive drive = new JointDrive()
                    {
                        positionSpring = 1e+20f,
                        positionDamper = 5e+18f,
                        maximumForce = 50f
                    };
                    joint.xDrive = drive;
                    joint.yDrive = drive;
                    joint.zDrive = drive;

                    joint.anchor = thumbLocalScaledAnchor;

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

        public ITKInteractable interactable;
        public float safeDist = 0.01f;
        public GameObject center;

        private Rigidbody rb;
        private bool physicsObject;

        private Dictionary<ITKHandController, Grab> interactingHands = new Dictionary<ITKHandController, Grab>();
        private HashSet<ITKHandController> hands = new HashSet<ITKHandController>();

        private void Start()
        {
            if (interactable == null) interactable = GetComponent<ITKInteractable>();
            if (interactable)
            {
                interactable.OnInteract.AddListener(OnInteract);
                interactable.OnInteractExit.AddListener(OnInteractExit);
                interactable.OnNoInteract.AddListener(OnNoInteract);
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

        public void OnInteract(ITKInteractable interactable, ITKHandController controller)
        {
            if (!enabled) return;

            if (!hands.Contains(controller))
            {
                hands.Add(controller);
                if (controller.physicsHand) controller.physicsHand.IgnoreCollision(interactable.colliders);
            }

            if (!interactingHands.ContainsKey(controller))
            {
                Grab grab = new Grab(this, interactable, controller);
                interactingHands.Add(controller, grab);
            }

            if (interactingHands.ContainsKey(controller))
            {
                if (!physicsObject) rb.isKinematic = false;

                Grab grab = interactingHands[controller];
                transform.rotation = Quaternion.LookRotation(center.transform.position - Camera.main.transform.position);

                if (controller.physicsHand)
                {
                    if (interactable.interactingControllers[controller] == ITKInteractable.Type.Pinch)
                    {
                        Vector3 thumbTip = controller.physicsHand.skeleton.joints[ITKHand.ThumbDistal].rb.position + controller.physicsHand.skeleton.joints[ITKHand.ThumbDistal].rb.rotation * new Vector3(0, 0, 0.03f);
                        Vector3 position = Quaternion.Inverse(controller.physicsHand.skeleton.root.rb.rotation) * (thumbTip - controller.physicsHand.skeleton.root.rb.position);
                        grab.joint.connectedAnchor = position;
                        grab.joint.anchor = grab.thumbLocalScaledAnchor;
                    }
                    else if (interactable.interactingControllers[controller] == ITKInteractable.Type.Grasp)
                    {
                        grab.joint.connectedAnchor = new Vector3(0, -0.03f, 0.02f);
                        grab.joint.anchor = grab.palmLocalScaledAnchor;
                    }
                }
                else if (controller.nonPhysicsHand)
                {
                    if (interactable.interactingControllers[controller] == ITKInteractable.Type.Pinch)
                    {
                        Vector3 thumbTip = controller.nonPhysicsHand.skeleton.joints[ITKHand.ThumbDistal].rb.position + controller.nonPhysicsHand.skeleton.joints[ITKHand.ThumbDistal].rb.rotation * new Vector3(0, 0, 0.03f);
                        Vector3 position = Quaternion.Inverse(controller.nonPhysicsHand.skeleton.root.rb.rotation) * (thumbTip - controller.nonPhysicsHand.skeleton.root.rb.position);
                        grab.joint.connectedAnchor = position;
                        grab.joint.anchor = grab.thumbLocalScaledAnchor;
                    }
                    else if (interactable.interactingControllers[controller] == ITKInteractable.Type.Grasp)
                    {
                        grab.joint.connectedAnchor = new Vector3(0, -0.03f, 0.02f);
                        grab.joint.anchor = grab.palmLocalScaledAnchor;
                    }
                }
                else
                {
                    if (interactable.interactingControllers[controller] == ITKInteractable.Type.Pinch)
                    {
                        Vector3 thumbTip = controller.gesture.pose.positions[ITKHand.ThumbTip];
                        Vector3 position = Quaternion.Inverse(controller.gesture.pose.rotations[ITKHand.Root]) * (thumbTip - controller.gesture.pose.positions[ITKHand.Root]);
                        grab.joint.connectedAnchor = position;
                        grab.joint.anchor = grab.thumbLocalScaledAnchor;
                    }
                    else if (interactable.interactingControllers[controller] == ITKInteractable.Type.Grasp)
                    {
                        grab.joint.connectedAnchor = new Vector3(0, -0.03f, 0.02f);
                        grab.joint.anchor = grab.palmLocalScaledAnchor;
                    }
                }
            }
        }
        public void OnNoInteract(ITKInteractable interactable, ITKHandController controller)
        {
            if (!enabled) return;

            if (hands.Contains(controller) && controller.gesture.Distance(interactable.colliders) > safeDist)
            {
                hands.Remove(controller);
                if (controller.physicsHand) controller.physicsHand.IgnoreCollision(interactable.colliders, false);
            }
        }

        public void OnInteractExit(ITKInteractable interactable, ITKHandController controller)
        {
            if (!enabled) return;

            if (interactingHands.ContainsKey(controller))
            {
                if (!physicsObject) rb.isKinematic = true;

                interactingHands[controller].Destroy();
                interactingHands.Remove(controller);
            }
        }
    }
}