using Microsoft.MixedReality.OpenXR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static InteractionTK.HandTracking.HandUtils;

namespace InteractionTK.HandTracking
{
    public static partial class HandUtils
    {
        public struct HandModelPoseOffsets
        { 
            public Quaternion wristRotationOffset;
            public Quaternion[][] rotationOffsets;
        }

        // TODO:: package these values in a single struct for hand setup and throw into a JSON
        public static HandModelPoseOffsets leftModelOffsets = new HandModelPoseOffsets()
        {
            wristRotationOffset = Quaternion.Euler(343, 150, 255),
            rotationOffsets = new Quaternion[][]
            {
                new Quaternion[] {
                    Quaternion.identity,
                    Quaternion.Euler(359, 263, 185), Quaternion.Euler(359, 263, 180), Quaternion.Euler(359, 263, 180)
                },
                new Quaternion[] {
                    Quaternion.identity, Quaternion.identity,
                    Quaternion.Euler(359, 269, 170), Quaternion.Euler(359, 269, 175), Quaternion.Euler(359, 269, 193)
                },
                new Quaternion[] {
                    Quaternion.identity, Quaternion.identity,
                    Quaternion.Euler(359, 269, 170), Quaternion.Euler(359, 269, 175), Quaternion.Euler(359, 269, 193)
                },
                new Quaternion[] {
                    Quaternion.identity, Quaternion.identity,
                    Quaternion.Euler(359, 269, 170), Quaternion.Euler(359, 269, 175), Quaternion.Euler(359, 269, 193)
                },
                new Quaternion[] {
                    Quaternion.identity, Quaternion.identity,
                    Quaternion.Euler(359, 269, 170), Quaternion.Euler(359, 269, 175), Quaternion.Euler(359, 269, 193)
                },
            }
        };
        public static HandModelPoseOffsets rightModelOffsets = new HandModelPoseOffsets()
        {
            wristRotationOffset = Quaternion.Euler(343, 211, 100),
            rotationOffsets = new Quaternion[][]
            {
                new Quaternion[] {
                    Quaternion.identity,
                    Quaternion.Euler(0, 280, 5), Quaternion.Euler(0, 269, 0), Quaternion.Euler(0, 263, 0)
                },
                new Quaternion[] {
                    Quaternion.identity, Quaternion.identity,
                    Quaternion.Euler(0, 269, -10), Quaternion.Euler(0, 269, -5), Quaternion.Euler(0, 269, 13)
                },
                new Quaternion[] {
                    Quaternion.identity, Quaternion.identity,
                    Quaternion.Euler(0, 269, -10), Quaternion.Euler(0, 269, -5), Quaternion.Euler(0, 269, 13)
                },
                new Quaternion[] {
                    Quaternion.identity, Quaternion.identity,
                    Quaternion.Euler(0, 269, -10), Quaternion.Euler(0, 269, -5), Quaternion.Euler(0, 269, 13)
                },
                new Quaternion[] {
                    Quaternion.identity, Quaternion.identity,
                    Quaternion.Euler(0, 269, -10), Quaternion.Euler(0, 269, -5), Quaternion.Euler(0, 269, 13)
                },
            }
        };
    }

    public class ITKHandModel : MonoBehaviour
    {
        public HandUtils.Handedness type;

        private SkinnedMeshRenderer meshRenderer;

        public Transform wrist;

        public Transform ThumbProximal;
        public Transform ThumbDistal;
        public Transform ThumbTip;

        public Transform IndexMiddle;
        public Transform IndexDistal;
        public Transform IndexTip;

        public Transform MiddleMiddle;
        public Transform MiddleDistal;
        public Transform MiddleTip;

        public Transform RingMiddle;
        public Transform RingDistal;
        public Transform RingTip;

        //public Transform PinkyKnuckle;
        public Transform PinkyMiddle;
        public Transform PinkyDistal;
        public Transform PinkyTip;

        private bool _active = true;
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

        public void Enable()
        {
            if (_active) return;

            if (meshRenderer) meshRenderer.enabled = true;

            _active = true;
        }

        public void Disable()
        {
            if (!_active) return;

            if (meshRenderer) meshRenderer.enabled = false;

            _active = false;
        }

        private void Start()
        {
            meshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        }

        // TODO:: Implement Track function that uses HandUtils.Pose

        public void Track(ITKSkeletonNode wrist, ITKSkeletonNode[][] skeleton)
        {
            HandModelPoseOffsets pose = type == HandUtils.Handedness.Left ? HandUtils.leftModelOffsets : HandUtils.rightModelOffsets;

            // Move geometry to position
            transform.position = wrist.joint.position;
            transform.rotation = wrist.joint.rotation;

            // Move wrist to correct position
            this.wrist.position = wrist.joint.position;
            this.wrist.rotation = wrist.joint.rotation;
            this.wrist.localRotation *= pose.wristRotationOffset;

            for (int i = 0; i < HandUtils.StructureCount.Length; i++)
            {
                for (int j = 0; j < HandUtils.StructureCount[i]; j++)
                {
                    switch (HandUtils.Structure[i][j])
                    {
                        case HandUtils.ThumbProximal:
                            ThumbProximal.transform.rotation = skeleton[i][j].joint.rotation;
                            ThumbProximal.transform.localRotation *= pose.rotationOffsets[i][j];
                            break;
                        case HandUtils.ThumbDistal:
                            ThumbDistal.transform.rotation = skeleton[i][j].joint.rotation;
                            ThumbDistal.transform.localRotation *= pose.rotationOffsets[i][j];
                            break;
                        case HandUtils.ThumbTip:
                            ThumbTip.transform.rotation = skeleton[i][j].joint.rotation;
                            ThumbTip.transform.localRotation *= pose.rotationOffsets[i][j];
                            break;

                        case HandUtils.IndexMiddle:
                            IndexMiddle.transform.rotation = skeleton[i][j].joint.rotation;
                            IndexMiddle.transform.localRotation *= pose.rotationOffsets[i][j];
                            break;
                        case HandUtils.IndexDistal:
                            IndexDistal.transform.rotation = skeleton[i][j].joint.rotation;
                            IndexDistal.transform.localRotation *= pose.rotationOffsets[i][j];
                            break;
                        case HandUtils.IndexTip:
                            IndexTip.transform.rotation = skeleton[i][j].joint.rotation;
                            IndexTip.transform.localRotation *= pose.rotationOffsets[i][j];
                            break;

                        case HandUtils.MiddleMiddle:
                            MiddleMiddle.transform.rotation = skeleton[i][j].joint.rotation;
                            MiddleMiddle.transform.localRotation *= pose.rotationOffsets[i][j];
                            break;
                        case HandUtils.MiddleDistal:
                            MiddleDistal.transform.rotation = skeleton[i][j].joint.rotation;
                            MiddleDistal.transform.localRotation *= pose.rotationOffsets[i][j];
                            break;
                        case HandUtils.MiddleTip:
                            MiddleTip.transform.rotation = skeleton[i][j].joint.rotation;
                            MiddleTip.transform.localRotation *= pose.rotationOffsets[i][j];
                            break;

                        case HandUtils.RingMiddle:
                            RingMiddle.transform.rotation = skeleton[i][j].joint.rotation;
                            RingMiddle.transform.localRotation *= pose.rotationOffsets[i][j];
                            break;
                        case HandUtils.RingDistal:
                            RingDistal.transform.rotation = skeleton[i][j].joint.rotation;
                            RingDistal.transform.localRotation *= pose.rotationOffsets[i][j];
                            break;
                        case HandUtils.RingTip:
                            RingTip.transform.rotation = skeleton[i][j].joint.rotation;
                            RingTip.transform.localRotation *= pose.rotationOffsets[i][j];
                            break;

                        case HandUtils.PinkyMiddle:
                            PinkyMiddle.transform.rotation = skeleton[i][j].joint.rotation;
                            PinkyMiddle.transform.localRotation *= pose.rotationOffsets[i][j];
                            break;
                        case HandUtils.PinkyDistal:
                            PinkyDistal.transform.rotation = skeleton[i][j].joint.rotation;
                            PinkyDistal.transform.localRotation *= pose.rotationOffsets[i][j];
                            break;
                        case HandUtils.PinkyTip:
                            PinkyTip.transform.rotation = skeleton[i][j].joint.rotation;
                            PinkyTip.transform.localRotation *= pose.rotationOffsets[i][j];
                            break;
                    }
                }
            }
        }
    }
}