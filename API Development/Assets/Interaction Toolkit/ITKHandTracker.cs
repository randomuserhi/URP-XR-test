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
        private ITKHand.Pose pose = new ITKHand.Pose(ITKHand.NumJoints);
        private ITKHand.Pose buffer = new ITKHand.Pose(ITKHand.NumJoints);

        public bool Tracking;
        public ITKHandPhysics physicsHand;
        public ITKHandModel hand;

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
                buffer = pose;
                pose = temp;
            }

            if (physicsHand != null)
            {
                if (physicsHand.type != type)
                {
                    Debug.LogError("Tracked hand type does not match the type of the physics hand.");
                    return;
                }

                if (Tracking)
                    physicsHand.Enable(pose);
                else
                    physicsHand.Disable();

                physicsHand.Track(pose);
            }
            if (hand != null)
            {
                if (hand.type != type)
                {
                    Debug.LogError("Tracked hand type does not match the type of the physics hand.");
                    return;
                }

                if (Tracking)
                    hand.Enable();
                else
                    hand.Disable();
                
                hand.Track(pose);
            }
        }
    }
}