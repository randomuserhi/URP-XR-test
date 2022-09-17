using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using static OVRPlugin;

namespace InteractionTK.HandTracking
{
    public class ITKGestures : MonoBehaviour
    {
        public ITKHand.Handedness type;
        public ITKHand.Pose pose { private set; get; }

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
        // TODO:: make overloads that take in a single Collider or ITKInteractable
        public float Distance(Collider[] colliders)
        {
            float closest = float.PositiveInfinity;
            if (pose.positions != null && pose.rotations != null)
            {
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
            }
            return closest;
        }

        public Vector3 ClosestPointFromJoint(Collider[] colliders, ITKHand.Joint joint)
        {
            float closest = float.PositiveInfinity;
            Vector3 point = Vector3.zero;
            if (pose.positions != null && pose.rotations != null)
            {
                for (int i = 0; i < colliders.Length; ++i)
                {
                    Collider c = colliders[i];
                    if (c.enabled)
                    {
                        Vector3 position = pose.positions[joint];
                        Vector3 closestPoint = c.ClosestPoint(position);
                        float dist = Vector3.Distance(closestPoint, position);
                        if (dist < closest)
                        {
                            closest = dist;
                            point = closestPoint;
                        }
                    }
                }
            }
            return point;
        }

        public bool active
        {
            get => _active;
            set
            {
                _active = value;
                if (_active) Enable();
                else Disable();
            }
        }

        private bool _active = true;
        public void Enable()
        {
            _active = true;
        }
        public void Disable()
        {
            _active = false;

            _intention = 0;
            _grasp = 0;
            _pinch = 0;
        }

        public void Track(ITKHand.Pose pose)
        {
            if (!active) return;

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
            float totalWeighting = 0;
            for (int i = 0; i < ITKHand.fingerTips.Length; i++)
            {
                ITKHand.Joint joint = ITKHand.fingerTips[i];
                if (joint != ITKHand.Palm)
                {
                    float weighting = joint == ITKHand.IndexTip ? 1f : 0.1f;
                    totalWeighting += weighting;
                    averageDistanceFromPalm += Vector3.Distance(pose.positions[joint], pose.positions[ITKHand.Palm]) * weighting;
                }
            }
            averageDistanceFromPalm /= totalWeighting;
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
