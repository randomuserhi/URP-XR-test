using System.Collections;
using System.Collections.Generic;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using static OVRPlugin;

namespace InteractionTK.HandTracking
{
    public class ITKGestures : MonoBehaviour
    {
        private ITKHand.Pose pose;

        private float _intention;
        private float _grasp;
        private float _pinch;

        public float intention { get => _intention; }
        public float grasp { get => _grasp; }
        public float pinch { get => _pinch; }

        // returns smallest distance between a joint and the given colliders
        private static ITKHand.Joint[] validJoints = new ITKHand.Joint[]
            {
                ITKHand.Palm,
                ITKHand.ThumbTip,
                ITKHand.IndexTip,
                ITKHand.MiddleTip,
                ITKHand.RingTip,
                ITKHand.PinkyTip
            };
        public float Distance(Collider[] colliders)
        {
            float closest = float.PositiveInfinity;
            for (int i = 0; i < colliders.Length; ++i)
            {
                Collider c = colliders[i];
                if (c.enabled)
                {
                    float closestJoint = float.PositiveInfinity;
                    for (int j = 0; j < validJoints.Length; ++j)
                    {
                        Vector3 position = pose.positions[validJoints[j]];
                        float dist = Vector3.Distance(c.ClosestPoint(position), position);
                        if (dist < closestJoint)
                            closestJoint = dist;
                    }
                    if (closestJoint < closest)
                        closest = closestJoint;
                }
            }
            return closest;
        }

        public void Track(ITKHand.Pose pose)
        {
            this.pose = pose;

            // Gestures are weighted depending on whether you are facing it
            // this is to test if it was intentional or not, since you are probably looking at it if its intentional
            const float fov = 50f;
            Vector3 handDir = pose.positions[ITKHand.Root] - Camera.main.transform.position;
            Vector3 cameraDir = Camera.main.transform.rotation * Vector3.forward; //TODO:: enable support for not main camera
            float t = 1f - Mathf.Clamp(Vector3.Angle(cameraDir, handDir) / fov, 0f, 1f);
            float x = (t * 2f) - 1f;
            const float scale = 2f;
            const float rate = -9f;
            _intention = 1f / (1 + Mathf.Pow(scale, rate * x));

            // Grasp confidence
            float averageDistanceFromPalm = 0;
            for (int i = 0; i < ITKHand.fingerTips.Length; i++)
            {
                ITKHand.Joint joint = ITKHand.fingerTips[i];
                if (joint != ITKHand.Palm)
                {
                    averageDistanceFromPalm += Vector3.Distance(pose.positions[joint], pose.positions[ITKHand.Palm]);
                }
            }
            averageDistanceFromPalm /= ITKHand.fingerTips.Length;
            averageDistanceFromPalm -= 0.04f;
            float distance = Mathf.Clamp(averageDistanceFromPalm, 0, float.MaxValue);
            _grasp = Mathf.Clamp(1 - (distance / 0.08f), 0f, 1f);

            // Pinch confidence
            float thumbIndexDistance = Vector3.Distance(pose.positions[ITKHand.IndexTip], pose.positions[ITKHand.ThumbTip]);
            distance = Mathf.Clamp(thumbIndexDistance - 0.015f, 0, float.MaxValue);
            _pinch = Mathf.Clamp(1 - (distance / 0.08f), 0f, 1f);
        }
    }
}
