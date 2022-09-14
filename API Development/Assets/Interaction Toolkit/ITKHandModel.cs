using JetBrains.Annotations;
using Microsoft.MixedReality.OpenXR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI.Table;

namespace InteractionTK.HandTracking
{
    public static partial class ITKHandUtils
    {
        public struct HandModelPoseOffsets
        {
            public Vector3 poseWristPositionOffset; // Offset when handling pose
            public Vector3 wristPositionOffset;
            public Quaternion wristRotationOffset;
            public Quaternion[] rotationOffsets;
        }

        // TODO:: package these values in a single struct for hand setup and throw into a JSON
        public static HandModelPoseOffsets leftModelOffsets = new HandModelPoseOffsets()
        {
            wristPositionOffset = new Vector3(0.06f, -0.01f, 0),
            poseWristPositionOffset = new Vector3(0.02f, -0.02f, 0),
            wristRotationOffset = Quaternion.Euler(355, 272, 180),
            rotationOffsets = new Quaternion[]
            {
                Quaternion.Euler(312, 274, 186),
                Quaternion.Euler(345, 278, 173),
                Quaternion.identity,
                Quaternion.identity,
                Quaternion.identity,
                Quaternion.Euler(62, 276, 182),
                Quaternion.Euler(355, 267, 180),
                Quaternion.Euler(0, 268, 171),
                Quaternion.Euler(356, 264, 165),
                Quaternion.Euler(4, 269, 167),
                Quaternion.Euler(356, 264, 170),
                Quaternion.Euler(7.7f, 275, 185),
                Quaternion.Euler(-1, 273, 183),
                Quaternion.Euler(0, 274, 181),
                Quaternion.Euler(0, 274, 186),
                Quaternion.Euler(0, 268, 171),
                Quaternion.identity,
                Quaternion.Euler(359, 267, 167),
                Quaternion.Euler(0, 267, 171),
                Quaternion.Euler(0, 267, 176),
                Quaternion.Euler(0, 268, 176)
            }
        };
        public static HandModelPoseOffsets rightModelOffsets = new HandModelPoseOffsets()
        {
            wristPositionOffset = new Vector3(-0.06f, 0.01f, 0),
            poseWristPositionOffset = new Vector3(-0.02f, 0.02f, 0),
            wristRotationOffset = Quaternion.Euler(355, 272, 0),
            rotationOffsets = new Quaternion[]
            {
                Quaternion.Euler(312, 274, 6),
                Quaternion.Euler(345, 278, -7),
                Quaternion.identity,
                Quaternion.identity,
                Quaternion.identity,
                Quaternion.Euler(314, 273, 357),
                Quaternion.Euler(355, 267, 0),
                Quaternion.Euler(0, 268, -9),
                Quaternion.Euler(356, 264, -15),
                Quaternion.Euler(4, 269, -13),
                Quaternion.Euler(356, 264, -10),
                Quaternion.Euler(7.7f, 275, 5),
                Quaternion.Euler(-1, 273, 3),
                Quaternion.Euler(0, 274, 1),
                Quaternion.Euler(0, 274, 6),
                Quaternion.Euler(0, 268, -9),
                Quaternion.identity,
                Quaternion.Euler(359, 267, -13),
                Quaternion.Euler(0, 267, -9),
                Quaternion.Euler(0, 267, -4),
                Quaternion.Euler(0, 268, -4)
            }
        };
    }

    public class ITKHandModel : MonoBehaviour
    {
        public ITKHandUtils.Handedness type;

        private SkinnedMeshRenderer meshRenderer;

        public Transform wrist;

        public Transform ThumbWristToMetacarpal;
        public Transform ThumbMetacarpal;
        public Transform ThumbProximal;
        public Transform ThumbDistal;

        public Transform IndexKnuckle;
        public Transform IndexMiddle;
        public Transform IndexDistal;

        public Transform MiddleKnuckle;
        public Transform MiddleMiddle;
        public Transform MiddleDistal;

        public Transform RingKnuckle;
        public Transform RingMiddle;
        public Transform RingDistal;

        public Transform PinkyMetacarpal;
        public Transform PinkyKnuckle;
        public Transform PinkyMiddle;
        public Transform PinkyDistal;

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

        public void Track(ITKHandUtils.Pose pose)
        {
            ITKHandUtils.HandModelPoseOffsets offsets = type == ITKHandUtils.Handedness.Left ? ITKHandUtils.leftModelOffsets : ITKHandUtils.rightModelOffsets;

            transform.rotation = pose.rotations[ITKHandUtils.Wrist] * offsets.wristRotationOffset;
            transform.position = pose.positions[ITKHandUtils.Wrist] + transform.rotation *  offsets.poseWristPositionOffset;

            Vector3 dir = pose.positions[ITKHandUtils.ThumbMetacarpal] - pose.positions[ITKHandUtils.Root];
            ThumbWristToMetacarpal.rotation = Quaternion.LookRotation(dir, Vector3.up) * offsets.rotationOffsets[ITKHandUtils.Wrist];

            ThumbMetacarpal.rotation = pose.rotations[ITKHandUtils.ThumbMetacarpal] * offsets.rotationOffsets[ITKHandUtils.ThumbMetacarpal];
            ThumbProximal.rotation = pose.rotations[ITKHandUtils.ThumbProximal] * offsets.rotationOffsets[ITKHandUtils.ThumbProximal];
            ThumbDistal.rotation = pose.rotations[ITKHandUtils.ThumbDistal] * offsets.rotationOffsets[ITKHandUtils.ThumbProximal];
            IndexKnuckle.rotation = pose.rotations[ITKHandUtils.IndexKnuckle] * offsets.rotationOffsets[ITKHandUtils.IndexKnuckle];
            IndexMiddle.rotation = pose.rotations[ITKHandUtils.IndexMiddle] * offsets.rotationOffsets[ITKHandUtils.IndexMiddle];
            IndexDistal.rotation = pose.rotations[ITKHandUtils.IndexDistal] * offsets.rotationOffsets[ITKHandUtils.IndexDistal];
            MiddleKnuckle.rotation = pose.rotations[ITKHandUtils.MiddleKnuckle] * offsets.rotationOffsets[ITKHandUtils.MiddleKnuckle];
            MiddleMiddle.rotation = pose.rotations[ITKHandUtils.MiddleMiddle] * offsets.rotationOffsets[ITKHandUtils.MiddleMiddle];
            MiddleDistal.rotation = pose.rotations[ITKHandUtils.MiddleDistal] * offsets.rotationOffsets[ITKHandUtils.MiddleDistal];
            RingKnuckle.rotation = pose.rotations[ITKHandUtils.RingKnuckle] * offsets.rotationOffsets[ITKHandUtils.RingKnuckle];
            RingMiddle.rotation = pose.rotations[ITKHandUtils.RingMiddle] * offsets.rotationOffsets[ITKHandUtils.RingMiddle];
            RingDistal.rotation = pose.rotations[ITKHandUtils.RingDistal] * offsets.rotationOffsets[ITKHandUtils.RingDistal];
            PinkyMetacarpal.rotation = pose.rotations[ITKHandUtils.PinkyMetacarpal] * offsets.rotationOffsets[ITKHandUtils.PinkyMetacarpal];
            PinkyKnuckle.rotation = pose.rotations[ITKHandUtils.PinkyKnuckle] * offsets.rotationOffsets[ITKHandUtils.PinkyKnuckle];
            PinkyMiddle.rotation = pose.rotations[ITKHandUtils.PinkyMiddle] * offsets.rotationOffsets[ITKHandUtils.PinkyMiddle];
            PinkyDistal.rotation = pose.rotations[ITKHandUtils.PinkyDistal] * offsets.rotationOffsets[ITKHandUtils.PinkyDistal];
        }
        public void Track(ITKSkeleton skeleton)
        {
            if (skeleton.type != type)
            {
                Debug.LogError("ITKSkeleton hand type does not match the type of the ITKHandModel.");
                return;
            }

            ITKHandUtils.HandModelPoseOffsets offsets = type == ITKHandUtils.Handedness.Left ? ITKHandUtils.leftModelOffsets : ITKHandUtils.rightModelOffsets;

            ITKSkeleton.Node root = skeleton.root;
            transform.rotation = root.rb.rotation * offsets.wristRotationOffset;
            transform.position = root.rb.position + transform.rotation * offsets.wristPositionOffset;

            for (int i = 0; i < skeleton.nodes.Length; ++i)
            {
                ITKSkeleton.Node node = skeleton.nodes[i];
                Quaternion rot = node.rb.rotation;
                switch (node.joint)
                {
                    case ITKHandUtils.Wrist:
                        if (node.parent != null) ThumbWristToMetacarpal.rotation = rot * offsets.rotationOffsets[ITKHandUtils.Wrist];
                        break;
                    case ITKHandUtils.ThumbMetacarpal:
                        ThumbMetacarpal.rotation = rot * offsets.rotationOffsets[ITKHandUtils.ThumbMetacarpal];
                        break;
                    case ITKHandUtils.ThumbProximal:
                        ThumbProximal.rotation = rot * offsets.rotationOffsets[ITKHandUtils.ThumbProximal];
                        break;
                    case ITKHandUtils.ThumbDistal:
                        ThumbDistal.rotation = rot * offsets.rotationOffsets[ITKHandUtils.ThumbProximal];
                        break;

                    case ITKHandUtils.IndexKnuckle:
                        IndexKnuckle.rotation = rot * offsets.rotationOffsets[ITKHandUtils.IndexKnuckle];
                        break;
                    case ITKHandUtils.IndexMiddle:
                        IndexMiddle.rotation = rot * offsets.rotationOffsets[ITKHandUtils.IndexMiddle];
                        break;
                    case ITKHandUtils.IndexDistal:
                        IndexDistal.rotation = rot * offsets.rotationOffsets[ITKHandUtils.IndexDistal];
                        break;

                    case ITKHandUtils.MiddleKnuckle:
                        MiddleKnuckle.rotation = rot * offsets.rotationOffsets[ITKHandUtils.MiddleKnuckle];
                        break;
                    case ITKHandUtils.MiddleMiddle:
                        MiddleMiddle.rotation = rot * offsets.rotationOffsets[ITKHandUtils.MiddleMiddle];
                        break;
                    case ITKHandUtils.MiddleDistal:
                        MiddleDistal.rotation = rot * offsets.rotationOffsets[ITKHandUtils.MiddleDistal];
                        break;

                    case ITKHandUtils.RingKnuckle:
                        RingKnuckle.rotation = rot * offsets.rotationOffsets[ITKHandUtils.RingKnuckle];
                        break;
                    case ITKHandUtils.RingMiddle:
                        RingMiddle.rotation = rot * offsets.rotationOffsets[ITKHandUtils.RingMiddle];
                        break;
                    case ITKHandUtils.RingDistal:
                        RingDistal.rotation = rot * offsets.rotationOffsets[ITKHandUtils.RingDistal];
                        break;

                    case ITKHandUtils.PinkyMetacarpal:
                        PinkyMetacarpal.rotation = rot * offsets.rotationOffsets[ITKHandUtils.PinkyMetacarpal];
                        break;
                    case ITKHandUtils.PinkyKnuckle:
                        PinkyKnuckle.rotation = rot * offsets.rotationOffsets[ITKHandUtils.PinkyKnuckle];
                        break;
                    case ITKHandUtils.PinkyMiddle:
                        PinkyMiddle.rotation = rot * offsets.rotationOffsets[ITKHandUtils.PinkyMiddle];
                        break;
                    case ITKHandUtils.PinkyDistal:
                        PinkyDistal.rotation = rot * offsets.rotationOffsets[ITKHandUtils.PinkyDistal];
                        break;
                }
            }
        }
    }
}