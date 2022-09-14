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
        public ITKHandUtils.Handedness type;
        private MixedRealityPose MRTKPose;
        private ITKHandUtils.Pose pose = new ITKHandUtils.Pose(ITKHandUtils.NumJoints);

        public bool Tracking;
        public ITKHand hand;

        private void FixedUpdate()
        {

            Tracking = true;
            for (int i = 0; i < ITKHandUtils.MRTKJoints.Length; i++)
            {
                Handedness handedness = type == ITKHandUtils.Handedness.Left ? Handedness.Left : Handedness.Right;
                if (HandJointUtils.TryGetJointPose(ITKHandUtils.MRTKJoints[i], handedness, out MRTKPose))
                {
                    pose.positions[i] = MRTKPose.Position;
                    pose.rotations[i] = MRTKPose.Rotation;
                }
                else Tracking = false;
            }

            if (hand.type != type)
            {
                Debug.LogError("Tracked hand type does not match the type of the ITKHand.");
                return;
            }

            if (Tracking)
            {
                hand.Enable();
                hand.Track(pose);
            }
            else
            {
                hand.Disable();
            }
        }
    }
}