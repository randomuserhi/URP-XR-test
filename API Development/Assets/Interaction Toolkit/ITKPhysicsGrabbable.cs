using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// TODO:: 2 hand grab, grasping
//        set position of grab

namespace InteractionTK.HandTracking
{
    public class ITKPhysicsGrabbable : MonoBehaviour
    {
        public float safeRadius = 0.06f;

        private int layer = -1;
        private Rigidbody rb;
        private ConfigurableJoint joint;

        private void Start()
        {
            rb = GetComponent<Rigidbody>();
        }

        public void OnHover(ITKInteractable interactable)
        {
            if (!enabled) return;

            GetComponent<MeshRenderer>().material.color = Color.red;
        }

        public void OnHoverExit(ITKInteractable interactable)
        {
            if (!enabled) return;

            GetComponent<MeshRenderer>().material.color = Color.white;
        }

        public void OnInteract(ITKInteractable interactable)
        {
            if (!enabled) return;

            if (layer < 0) layer = gameObject.layer;
            gameObject.layer = LayerMask.NameToLayer("ITKHandIgnore");

            ITKHandInteractController[] controllers = interactable.interactingControllers.Keys.ToArray();
            for (int i = 0; i < controllers.Length; ++i)
            {
                ITKHandInteractController controller = controllers[i];

                if (controller.physicsHand != null && rb != null)
                {
                    if (joint == null)
                    {
                        joint = gameObject.AddComponent<ConfigurableJoint>();
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
                            maximumForce = 20f
                        };
                        joint.xDrive = drive;
                        joint.yDrive = drive;
                        joint.zDrive = drive;

                        joint.anchor = transform.InverseTransformPoint(controller.gesture.ClosestPointFromJoint(interactable.colliders, ITKHand.ThumbTip));

                        joint.autoConfigureConnectedAnchor = false;

                        joint.targetRotation = Quaternion.identity;
                        joint.targetPosition = Vector3.zero;
                    }

                    if (joint)
                    {
                        Vector3 thumbTip = controller.physicsHand.skeleton.joints[ITKHand.ThumbDistal].rb.position + controller.physicsHand.skeleton.joints[ITKHand.ThumbDistal].rb.rotation * new Vector3(0, 0, 0.03f);
                        Vector3 position = Quaternion.Inverse(controller.physicsHand.skeleton.root.rb.rotation) * (thumbTip - controller.physicsHand.skeleton.root.rb.position);
                        joint.connectedAnchor = position;
                    }
                }
                else if (rb == null || rb.isKinematic == true)
                {
                    transform.position = controller.physicsHand.skeleton.root.rb.position;
                    transform.rotation = controller.physicsHand.skeleton.root.rb.rotation;
                }
            }
        }

        public void OnInteractExit(ITKInteractable interactable)
        {
            if (!enabled) return;

            if (interactable.interactingControllers.Count == 0)
            {
                if (joint)
                    Destroy(joint);
                if (rb) // wake up rb by adding tiny velocity => sometimes rb is asleep causing it to freeze in air
                    rb.velocity += new Vector3(0, 0.0001f, 0);

                if (!Physics.CheckSphere(transform.position, safeRadius, LayerMask.GetMask("ITKHand")))
                {
                    layer = -1;
                    gameObject.layer = 0;
                }
            }
        }
    }
}
