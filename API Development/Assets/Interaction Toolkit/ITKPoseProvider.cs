using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InteractionTK.HandTracking
{
    public abstract class ITKPoseProvider : MonoBehaviour
    {
        public ITKHand.Handedness type;
        public abstract ITKHand.Pose GetPose(out bool tracking);
    }
}
