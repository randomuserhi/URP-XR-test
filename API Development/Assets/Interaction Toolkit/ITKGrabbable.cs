using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Schema;
using UnityEngine;

// TODO:: 2 hand grab, grasping
//        set position of grab

namespace InteractionTK.HandTracking
{
    public class ITKGrabbable : MonoBehaviour
    {
        public float safeRadius = 0.06f;

        private int layer = -1;
        private Rigidbody rb;

        private class Grab
        {
            public Vector3 anchor;
            public ConfigurableJoint joint;
            private ITKGrabbable self;
            public Grab(ITKGrabbable self, ITKInteractable interactable, ITKHandInteractController controller)
            {
                this.self = self;
                anchor = self.transform.InverseTransformPoint(controller.gesture.ClosestPointFromJoint(interactable.colliders, ITKHand.ThumbTip));

                if (self.rb)
                {
                    joint = self.gameObject.AddComponent<ConfigurableJoint>();
                    joint.connectedBody = controller.physicsHand.skeleton.root.rb;
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

                    joint.anchor = anchor;

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
        private Dictionary<ITKHandInteractController, Grab> hands = new Dictionary<ITKHandInteractController, Grab>();

        private void Start()
        {
            rb = GetComponent<Rigidbody>();
        }

        public void OnHover(ITKInteractable interactable)
        {
            if (!enabled) return;

            GetComponent<MeshRenderer>().material.color = Color.red;
        }

        public void OnNoHover(ITKInteractable interactable)
        {
            if (!enabled) return;

            GetComponent<MeshRenderer>().material.color = Color.white;
        }

        public void OnInteract(ITKInteractable interactable, ITKHandInteractController controller)
        {
            if (!enabled) return;

            if (layer < 0) layer = gameObject.layer;
            gameObject.layer = LayerMask.NameToLayer("ITKHandIgnore");

            if (!hands.ContainsKey(controller))
            {
                Grab grab = new Grab(this, interactable, controller);
                hands.Add(controller, grab);
            }
            else
            {
                if (controller.physicsHand != null)
                {
                    if (rb == null || rb.isKinematic)
                    {
                        if (interactable.interactingControllers[controller] == ITKInteractable.Type.Pinch)
                        {

                        }
                        else if (interactable.interactingControllers[controller] == ITKInteractable.Type.Grasp)
                        {

                        }
                    }
                    else
                    {
                        if (interactable.interactingControllers[controller] == ITKInteractable.Type.Pinch)
                        {
                            Vector3 thumbTip = controller.physicsHand.skeleton.joints[ITKHand.ThumbDistal].rb.position + controller.physicsHand.skeleton.joints[ITKHand.ThumbDistal].rb.rotation * new Vector3(0, 0, 0.03f);
                            Vector3 position = Quaternion.Inverse(controller.physicsHand.skeleton.root.rb.rotation) * (thumbTip - controller.physicsHand.skeleton.root.rb.position);
                            hands[controller].joint.connectedAnchor = position;
                            hands[controller].joint.anchor = hands[controller].anchor;
                        }
                        else if (interactable.interactingControllers[controller] == ITKInteractable.Type.Grasp)
                        {
                            hands[controller].joint.connectedAnchor = new Vector3(0, -0.03f, 0.02f);
                            hands[controller].joint.anchor = Vector3.zero;
                        }
                    }
                }
                else
                {
                    if (rb == null || rb.isKinematic)
                    {
                        if (interactable.interactingControllers[controller] == ITKInteractable.Type.Pinch)
                        {

                        }
                        else if (interactable.interactingControllers[controller] == ITKInteractable.Type.Grasp)
                        {

                        }
                    }
                    else
                    {
                        if (interactable.interactingControllers[controller] == ITKInteractable.Type.Pinch)
                        {
                            Vector3 thumbTip = controller.gesture.pose.positions[ITKHand.ThumbTip];
                            Vector3 position = Quaternion.Inverse(controller.gesture.pose.rotations[ITKHand.Root]) * (thumbTip - controller.gesture.pose.positions[ITKHand.Root]);
                            hands[controller].joint.connectedAnchor = position;
                            hands[controller].joint.anchor = hands[controller].anchor;
                        }
                        else if (interactable.interactingControllers[controller] == ITKInteractable.Type.Grasp)
                        {
                            hands[controller].joint.connectedAnchor = new Vector3(0, -0.03f, 0.02f);
                            hands[controller].joint.anchor = Vector3.zero;
                        }
                    }
                }
            }
        }

        public void OnNoInteraction(ITKInteractable interactable, ITKHandInteractController controller)
        {
            if (!enabled) return;

            if (hands.ContainsKey(controller))
            {
                hands[controller].Destroy();
                hands.Remove(controller);
            }

            if (!Physics.CheckSphere(transform.position, safeRadius, LayerMask.GetMask("ITKHand")))
            {
                if (layer >= 0) gameObject.layer = layer;
                layer = -1;
            }
        }
    }
}
