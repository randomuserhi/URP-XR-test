using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.Input;

namespace InteractionTK.HandTracking
{
    public class ITKPoseProviderMRTK : ITKPoseProvider
    {
        private MixedRealityPose MRTKPose;
        private ITKHand.Pose pose = new ITKHand.Pose(ITKHand.NumJoints);
        private ITKHand.Pose buffer = new ITKHand.Pose(ITKHand.NumJoints);

        public override ITKHand.Pose GetPose(out bool tracking)
        {
            tracking = true;
            for (int i = 0; i < ITKHand.MRTKJoints.Length; i++)
            {
                Handedness handedness = type == ITKHand.Handedness.Left ? Handedness.Left : Handedness.Right;
                if (HandJointUtils.TryGetJointPose(ITKHand.MRTKJoints[i], handedness, out MRTKPose))
                {
                    buffer.positions[i] = MRTKPose.Position;
                    buffer.rotations[i] = MRTKPose.Rotation;
                }
                else tracking = false;
            }

            if (tracking) // On successful track swap buffers
            {
                ITKHand.Pose temp = buffer;
                buffer = pose;
                pose = temp;
            }

            return pose;
        }
    }
}
