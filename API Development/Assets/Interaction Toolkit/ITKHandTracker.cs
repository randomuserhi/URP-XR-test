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
        public HandUtils.Handedness handedness;
        private MixedRealityPose MRTKPose;
        private HandUtils.Pose pose = new HandUtils.Pose(HandUtils.NumJoints);

        public bool Tracking;
        public ITKHand hand;

        private void FixedUpdate()
        {
            Tracking = true;
            for (int i = 0; i < HandUtils.MRTKJoints.Length; i++)
            {
                Handedness handedness = this.handedness == HandUtils.Handedness.Left ? Handedness.Left : Handedness.Right;
                if (HandJointUtils.TryGetJointPose(HandUtils.MRTKJoints[i], handedness, out MRTKPose))
                {
                    pose.positions[i] = MRTKPose.Position;
                    pose.rotations[i] = MRTKPose.Rotation;
                }
                else Tracking = false;
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