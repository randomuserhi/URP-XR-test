using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InteractionTK;
using InteractionTK.HandTracking;

namespace VirtualRealityTK
{
    public class VRTKPlayer : MonoBehaviour
    {
        public bool teleport;

        public ITKHandTracker left;
        public ITKHandTracker right;

        public ITKPoseProvider clientLeftPoseProvider;
        public ITKPoseProvider clientRightPoseProvider;

        public GameObject main;
    }
}