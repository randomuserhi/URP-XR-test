using InteractionTK.HandTracking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VirtualRealityTK;
using static OVRPlugin;

public class ITKHandNonPhysics : MonoBehaviour
{
    public PhysicMaterial material;

    public ITKHand.Handedness type;

    public ITKHandModel model;

    public ITKSkeleton skeleton { private set; get; }

    private bool safeEnable = true;
    private int safeEnableFrame = 5; // Enable after 5 frames

    private bool frozen;
    private float massWeight = 1f;

    private bool _active = true;
    public bool active
    {
        set
        {
            _active = value;
            if (_active) Enable();
            else Disable();
        }
        get => _active;
    }

    public int movingAverageFrameRange = 0;
    private int movingAverageCount = 0;
    private int movingAverageIndex = 0;
    private ITKHand.Pose[] movingAverage;

    private void Start()
    {
        movingAverage = new ITKHand.Pose[movingAverageFrameRange];
        // Due to bug on hololens, capsule colliders don't behave properly, so box colliders are used instead
        ITKHand.SkeletonDescription description = VRTK.device == VRTK.Device.Hololens2 ? ITKHand.handSkeletonBox : ITKHand.handSkeleton;
        skeleton = new ITKSkeleton(type, transform, description, material);
        for (int i = 0; i < skeleton.nodes.Length; ++i) // Remove colliders to stop physics interactions
            skeleton.nodes[i].DeleteColliders();
        Disable(true);
    }

    public void Enable()
    {
        if (safeEnable) return; // Wait till safe enable finishes

        if (_active) return;
        _active = true;

        model?.Enable();

        // If we were frozen lerp velocity to prevent jitter
        if (frozen)
        {
            massWeight = -0.01f;
            frozen = false;
        }

        for (int i = 0; i < skeleton.nodes.Length; ++i)
            skeleton.nodes[i].Enable();
    }

    public void Enable(ITKHand.Pose pose, bool forceEnable = false)
    {
        if (safeEnable) return; // Wait till safe enable finishes

        // Only enable if hand is not inside an object or forceEnable is set to true
        if (forceEnable || !Physics.CheckSphere(pose.positions[ITKHand.Root], 0.1f, ~LayerMask.GetMask("ITKHandIgnore")))
        {
            if (!_active)
                // Check if a teleport is actually needed
                if (Vector3.Distance(skeleton.root.rb.position, pose.positions[ITKHand.Root]) > 1f)
                    Teleport(pose.positions[ITKHand.Root]);
            Enable();
        }
    }

    public void Disable(bool forceDisable = false)
    {
        if (!_active) return;
        _active = false;

        model?.Disable();

        for (int i = 0; i < skeleton.nodes.Length; ++i)
            skeleton.nodes[i].Disable();
    }

    public void Teleport(Vector3 position)
    {
        for (int i = 0; i < skeleton.nodes.Length; ++i)
        {
            ITKSkeleton.Node n = skeleton.nodes[i];
            n.rb.velocity = Vector3.zero;
            n.rb.angularVelocity = Vector3.zero;
            n.Teleport(position);
        }
    }

    public void Track(ITKHand.Pose pose, bool frozen = false)
    {
        ITKSkeleton.Node root = skeleton.root;

        this.frozen = frozen;

        if (movingAverage != null && movingAverage.Length > 0) // Calculate moving average
        {
            if (movingAverage[movingAverageIndex].positions == null) movingAverage[movingAverageIndex].positions = new Vector3[ITKHand.NumJoints];
            if (movingAverage[movingAverageIndex].rotations == null) movingAverage[movingAverageIndex].rotations = new Quaternion[ITKHand.NumJoints];
            movingAverage[movingAverageIndex].Copy(pose);
            movingAverageIndex = (movingAverageIndex + 1) % movingAverage.Length;
            if (movingAverageCount < movingAverage.Length) ++movingAverageCount;

            if (movingAverageCount > 0)
            {
                for (int i = 0; i < ITKHand.NumJoints; ++i)
                {
                    Vector3 averagePos = Vector3.zero;
                    Vector4 cumulative = new Vector4(0, 0, 0, 0);
                    for (int j = 0; j < movingAverageCount; ++j)
                    {
                        averagePos += movingAverage[j].positions[i];
                        VRTKUtils.AverageQuaternion_Internal(ref cumulative, movingAverage[j].rotations[i], movingAverage[0].rotations[i]);
                    }
                    float addDet = 1f / movingAverageCount;
                    float x = cumulative.x * addDet;
                    float y = cumulative.y * addDet;
                    float z = cumulative.z * addDet;
                    float w = cumulative.w * addDet;
                    //note: if speed is an issue, you can skip the normalization step
                    pose.rotations[i] = VRTKUtils.NormalizeQuaternion(new Quaternion(x, y, z, w));
                    pose.positions[i] = averagePos / movingAverageCount;
                }
            }
        }

        // Track joints
        root.Track(pose, Quaternion.identity);

        // Make sure the root doesn't overshoot
        Vector3 anchor = root.rb.position + root.rb.rotation * root.j.anchor;
        Vector3 dir = pose.positions[ITKHand.Root] - anchor;
        if (root.rb.velocity.magnitude > 0.1f && Vector3.Dot(root.rb.velocity, dir) < 0)
        {
            for (int i = 0; i < skeleton.nodes.Length; ++i)
            {
                skeleton.nodes[i].rb.velocity *= 0.5f;
                skeleton.nodes[i].rb.angularVelocity *= 0.5f;
            }
        }

        // Update nodes
        for (int i = 0; i < skeleton.nodes.Length; ++i)
        {
            skeleton.nodes[i].FixedUpdate(skeleton.settings, massWeight);
        }

        // teleport and set joints velocity to zero if unstable (far from target)
        if (Vector3.Distance(root.rb.position + root.rb.rotation * root.j.anchor, root.j.connectedAnchor) > skeleton.settings.maxError)
        {
            Teleport(pose.positions[ITKHand.Root]);
        }

        // safely enable when we are tracked properly - TODO:: check if hand is not inside of anything before enabling
        if (safeEnable && safeEnableFrame <= 0 && !Physics.CheckSphere(root.rb.position, 0.1f, ~LayerMask.GetMask("ITKHandIgnore")) && Vector3.Distance(root.rb.position, pose.positions[ITKHand.Root]) < 0.1f)
        {
            safeEnable = false;
            for (int i = 0; i < skeleton.nodes.Length; ++i)
            {
                skeleton.nodes[i].Reset();
            }
            Enable();
        }
        else if (safeEnableFrame > 0) --safeEnableFrame;

        // Lerp velocity weight to 1
        massWeight = Mathf.Lerp(massWeight, 1, 0.5f);

        model?.Track(pose, skeleton);
    }
}
