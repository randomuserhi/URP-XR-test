using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using UnityEngine;

using Microsoft;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;

namespace InteractionTK.HandTracking
{
    public class ITKHandTracker : MonoBehaviour
    {
        public ITKHand.Handedness type;
        private MixedRealityPose MRTKPose;
        private ITKHand.Pose target = new ITKHand.Pose(ITKHand.NumJoints);
        private ITKHand.Pose buffer = new ITKHand.Pose(ITKHand.NumJoints);
        private ITKHand.Pose pose = new ITKHand.Pose(ITKHand.NumJoints);

        public bool Tracking;
        public ITKGestures gestures;
        public ITKHandPhysics physicsHand;
        public ITKHandNonPhysics nonPhysicsHand;
        public ITKHandModel hand;

        private bool frozen = false; // True when tracking is lost but tracking is still enabled
        private bool frozenOutOfFrame = false; // True when frozen hand has been out of frame (may just be loss of tracking in front of you)
        private int frozenFrameTimer = 0;

        private void Start()
        {
            Disable(true);
        }

        private void Enable()
        {
            if (gestures != null)
                gestures.Enable();
            if (physicsHand != null)
                physicsHand.Enable();
            if (nonPhysicsHand != null)
                nonPhysicsHand.Enable();
            if (hand != null)
                hand.Enable();
        }

        private void Disable(bool forceDisable = false)
        {
            if (target.positions == null || target.rotations == null || Camera.main == null) return;

            Vector3 handDir = target.positions[ITKHand.Root] - Camera.main.transform.position;
            Vector3 cameraDir = Camera.main.transform.rotation * Vector3.forward; //TODO:: enable support for not main camera
            // Only disable if hand is behind you, otherwise to keep physics smooth allow hand tracking to be lost whilst its within 180 fov
            if (forceDisable || Vector3.Dot(cameraDir, handDir) < 0)
            {
                frozen = false;

                if (gestures != null)
                    gestures.Disable();
                if (physicsHand != null)
                    physicsHand.Disable();
                if (nonPhysicsHand != null)
                    nonPhysicsHand.Disable();
                if (hand != null)
                    hand.Disable();
            }
            // Object will not be disabled but is still physically active
            else if (!frozen)
            {
                frozen = true;
                frozenOutOfFrame = false;
                frozenFrameTimer = 5; // give 5 frame delay for hand tracking to catch up
            }
            else if (frozen)
            {
                // Ensure that hand has been out of frame with frozenOutOfFrame to prevent hand dissapearing if tracking is lost in front of you
                if (Vector3.Angle(cameraDir, handDir) > 40) frozenOutOfFrame = true;
                if (frozenOutOfFrame)
                {
                    if (Vector3.Angle(cameraDir, handDir) < 30)
                    {
                        if (frozenFrameTimer < 0)
                        {
                            frozen = false;
                            Disable(true);
                        }
                        else --frozenFrameTimer;
                    }
                }
            }
        }

        private void OnDisable()
        {
            Disable(true);
        }

        private void OnEnable()
        {
            Enable();
        }

        private void FixedUpdate()
        {
            Tracking = true;
            for (int i = 0; i < ITKHand.MRTKJoints.Length; i++)
            {
                Handedness handedness = type == ITKHand.Handedness.Left ? Handedness.Left : Handedness.Right;
                if (HandJointUtils.TryGetJointPose(ITKHand.MRTKJoints[i], handedness, out MRTKPose))
                {
                    buffer.positions[i] = MRTKPose.Position;
                    buffer.rotations[i] = MRTKPose.Rotation;
                }
                else Tracking = false;
            }

            if (Tracking) // On successful track swap buffers
            {
                ITKHand.Pose temp = buffer;
                buffer = target;
                target = temp;
            }

            // Interpolate to prevent jitter
            pose.Interpolate(target);

            // Enable or Disable based on tracking
            if (Tracking)
                Enable();
            else
                Disable();

            // Send tracking data to components
            if (gestures != null)
            {
#if UNITY_EDITOR
                if (gestures.type != type)
                    Debug.LogWarning("Tracked hand type does not match the type of the gesture script.");
#endif
                gestures.Track(pose);
            }
            if (physicsHand != null)
            {
#if UNITY_EDITOR
                if (physicsHand.type != type)
                    Debug.LogWarning("Tracked hand type does not match the type of the physics hand.");
#endif
                physicsHand.Track(pose, frozen);
            }
            if (nonPhysicsHand != null)
            {
#if UNITY_EDITOR
                if (nonPhysicsHand.type != type)
                    Debug.LogWarning("Tracked hand type does not match the type of the non-physics hand.");
#endif
                nonPhysicsHand.Track(pose);
            }
            if (hand != null)
            {
#if UNITY_EDITOR
                if (hand.type != type)
                    Debug.LogError("Tracked hand type does not match the type of the physics hand.");
#endif
                hand.Track(pose);
            }
        }
    }
}