using InteractionTK.HandTracking;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ITKPinchGrabbable : MonoBehaviour
{
    private class Grab
    {
        public ConfigurableJoint joint;
        private ITKPinchGrabbable self;
        public Grab(ITKPinchGrabbable self, ITKPinchInteractable interactable, ITKPinchGestureController controller)
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
    private bool physicsObject;

    private Dictionary<ITKPinchGestureController, Grab> interactingHands = new Dictionary<ITKPinchGestureController, Grab>();
    private HashSet<ITKPinchGestureController> hands = new HashSet<ITKPinchGestureController>();

    private void Start()
    {
        if (pinchInteractable == null) pinchInteractable = GetComponent<ITKPinchInteractable>();
        if (pinchInteractable)
        {
            pinchInteractable.OnHover.AddListener(OnHover);
            pinchInteractable.OnHoverExit.AddListener(OnHoverExit);
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

    public void OnHover(ITKPinchInteractable interactable)
    {
        if (!enabled) return;

        GetComponent<MeshRenderer>().material.color = Color.red;
    }

    public void OnHoverExit(ITKPinchInteractable interactable)
    {
        if (!enabled) return;

        GetComponent<MeshRenderer>().material.color = Color.white;
    }

    public void OnInteract(ITKPinchInteractable interactable, ITKPinchGestureController controller)
    {
        if (!enabled) return;

        if (!hands.Contains(controller))
            hands.Add(controller);

        if (!interactingHands.ContainsKey(controller))
        {
            Grab grab = new Grab(this, interactable, controller);
            interactingHands.Add(controller, grab);
        }

        if (interactingHands.ContainsKey(controller))
        {
            if (!physicsObject) rb.isKinematic = false;
            rb.useGravity = false;

            if (rb) // wake up rb by adding tiny velocity => sometimes rb is asleep causing it to freeze in air
                rb.velocity += new Vector3(0, 0.0001f, 0);

            interactingHands[controller].joint.connectedAnchor = controller.ray.origin + controller.ray.direction * Mathf.Clamp(controller.hit.distance, minDist, float.PositiveInfinity);
        }
    }

    public void OnInteractExit(ITKPinchInteractable interactable, ITKPinchGestureController controller)
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
