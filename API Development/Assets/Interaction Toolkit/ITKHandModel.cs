using JetBrains.Annotations;
using Microsoft.MixedReality.OpenXR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VirtualRealityTK;

namespace InteractionTK.HandTracking
{
    public static partial class ITKHand
    {
        public struct HandModelPoseOffsets
        {
            public Vector3 poseWristPositionOffset; // Offset when handling pose
            public Vector3 wristPositionOffset;
            public Quaternion wristRotationOffset;
            public Quaternion[] rotationOffsets;
        }

        // TODO:: package these values in a single struct for hand setup and throw into a JSON
        public static HandModelPoseOffsets leftModelOffsetsHololens = new HandModelPoseOffsets()
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
        public static HandModelPoseOffsets rightModelOffsetsHololens = new HandModelPoseOffsets()
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

        public static HandModelPoseOffsets leftModelOffsetsOculus = new HandModelPoseOffsets()
        {
            wristPositionOffset = new Vector3(0.06f, -0.01f, 0),
            poseWristPositionOffset = new Vector3(0.02f, -0.02f, 0),
            wristRotationOffset = Quaternion.Euler(355, 272, 180),
            rotationOffsets = new Quaternion[]
            {
                Quaternion.Euler(312, 274, 186),
                Quaternion.Euler(255, 278, 173),
                Quaternion.identity,
                Quaternion.identity,
                Quaternion.identity,
                Quaternion.Euler(12, 276, 182),
                Quaternion.Euler(265, 267, 180),
                Quaternion.Euler(0, 268, 171),
                Quaternion.Euler(356, 264, 165),
                Quaternion.Euler(4, 269, 167),
                Quaternion.Euler(356, 264, 170),
                Quaternion.Euler(-87.7f, 275, 185),
                Quaternion.Euler(-1, 273, 183),
                Quaternion.Euler(0, 274, 181),
                Quaternion.Euler(0, 274, 186),
                Quaternion.Euler(0, 268, 171),
                Quaternion.identity,
                Quaternion.Euler(359, 267, 167),
                Quaternion.Euler(0, 267, 171),
                Quaternion.Euler(0, 267, 176),
                Quaternion.Euler(0, 268, 176),
                Quaternion.identity,
                Quaternion.identity,
            }
        };
        public static HandModelPoseOffsets rightModelOffsetsOculus = new HandModelPoseOffsets()
        {
            wristPositionOffset = new Vector3(-0.06f, 0.01f, 0),
            poseWristPositionOffset = new Vector3(-0.02f, 0.02f, 0),
            wristRotationOffset = Quaternion.Euler(355, 272, 0),
            rotationOffsets = new Quaternion[]
            {
                Quaternion.Euler(0, 81, 164.3f),
                Quaternion.Euler(80, 258, 0),
                Quaternion.identity,
                Quaternion.identity,
                Quaternion.identity,
                Quaternion.Euler(350, 269, 359),
                Quaternion.Euler(85, 267, 0),
                Quaternion.Euler(0, 268, -9),
                Quaternion.Euler(356, 264, -15),
                Quaternion.Euler(4, 269, -13),
                Quaternion.Euler(356, 264, -10),
                Quaternion.Euler(97.7f, 275, 5),
                Quaternion.Euler(-1, 273, 3),
                Quaternion.Euler(0, 274, 1),
                Quaternion.Euler(0, 274, 6),
                Quaternion.Euler(0, 268, -9),
                Quaternion.identity,
                Quaternion.Euler(359, 267, -13),
                Quaternion.Euler(0, 267, -9),
                Quaternion.Euler(0, 267, -4),
                Quaternion.Euler(0, 268, -4),
            }
        };
    }

    public class ITKHandModel : MonoBehaviour
    {
        public ITKHand.Handedness type;

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
            Disable();
        }

        public void Track(ITKHand.Pose pose)
        {
            ITKHand.HandModelPoseOffsets offsets = type == ITKHand.Handedness.Left ? ITKHand.leftModelOffsetsHololens : ITKHand.rightModelOffsetsHololens;

            transform.rotation = pose.rotations[ITKHand.Wrist] * offsets.wristRotationOffset;
            transform.position = pose.positions[ITKHand.Wrist] + transform.rotation *  offsets.poseWristPositionOffset;

            Vector3 dir = pose.positions[ITKHand.ThumbMetacarpal] - pose.positions[ITKHand.Root];
            ThumbWristToMetacarpal.rotation = Quaternion.LookRotation(dir, Vector3.up) * offsets.rotationOffsets[ITKHand.Wrist];

            ThumbMetacarpal.rotation = pose.rotations[ITKHand.ThumbMetacarpal] * offsets.rotationOffsets[ITKHand.ThumbMetacarpal];
            ThumbProximal.rotation = pose.rotations[ITKHand.ThumbProximal] * offsets.rotationOffsets[ITKHand.ThumbProximal];
            ThumbDistal.rotation = pose.rotations[ITKHand.ThumbDistal] * offsets.rotationOffsets[ITKHand.ThumbProximal];
            IndexKnuckle.rotation = pose.rotations[ITKHand.IndexKnuckle] * offsets.rotationOffsets[ITKHand.IndexKnuckle];
            IndexMiddle.rotation = pose.rotations[ITKHand.IndexMiddle] * offsets.rotationOffsets[ITKHand.IndexMiddle];
            IndexDistal.rotation = pose.rotations[ITKHand.IndexDistal] * offsets.rotationOffsets[ITKHand.IndexDistal];
            MiddleKnuckle.rotation = pose.rotations[ITKHand.MiddleKnuckle] * offsets.rotationOffsets[ITKHand.MiddleKnuckle];
            MiddleMiddle.rotation = pose.rotations[ITKHand.MiddleMiddle] * offsets.rotationOffsets[ITKHand.MiddleMiddle];
            MiddleDistal.rotation = pose.rotations[ITKHand.MiddleDistal] * offsets.rotationOffsets[ITKHand.MiddleDistal];
            RingKnuckle.rotation = pose.rotations[ITKHand.RingKnuckle] * offsets.rotationOffsets[ITKHand.RingKnuckle];
            RingMiddle.rotation = pose.rotations[ITKHand.RingMiddle] * offsets.rotationOffsets[ITKHand.RingMiddle];
            RingDistal.rotation = pose.rotations[ITKHand.RingDistal] * offsets.rotationOffsets[ITKHand.RingDistal];
            PinkyMetacarpal.rotation = pose.rotations[ITKHand.PinkyMetacarpal] * offsets.rotationOffsets[ITKHand.PinkyMetacarpal];
            PinkyKnuckle.rotation = pose.rotations[ITKHand.PinkyKnuckle] * offsets.rotationOffsets[ITKHand.PinkyKnuckle];
            PinkyMiddle.rotation = pose.rotations[ITKHand.PinkyMiddle] * offsets.rotationOffsets[ITKHand.PinkyMiddle];
            PinkyDistal.rotation = pose.rotations[ITKHand.PinkyDistal] * offsets.rotationOffsets[ITKHand.PinkyDistal];
        }

        public void Track(ITKSkeleton skeleton)
        {
            if (skeleton.type != type)
            {
                Debug.LogError("ITKSkeleton hand type does not match the type of the ITKHandModel.");
                return;
            }

            ITKHand.HandModelPoseOffsets offsets;

            switch (VRTK.device)
            {
                case VRTK.Device.Oculus:
                    offsets = type == ITKHand.Handedness.Left ? ITKHand.leftModelOffsetsOculus : ITKHand.rightModelOffsetsOculus;
                    break;
                case VRTK.Device.Hololens2:
                default:
                    offsets = type == ITKHand.Handedness.Left ? ITKHand.leftModelOffsetsHololens : ITKHand.rightModelOffsetsHololens;
                    break;
            }

            ITKSkeleton.Node root = skeleton.root;
            transform.rotation = root.rb.rotation * offsets.wristRotationOffset;
            transform.position = root.rb.position + transform.rotation * offsets.wristPositionOffset;

            for (int i = 0; i < skeleton.nodes.Length; ++i)
            {
                ITKSkeleton.Node node = skeleton.nodes[i];
                Quaternion rot = node.rb.rotation;
                switch (node.joint)
                {
                    case ITKHand.Wrist:
                        if (node.parent != null) ThumbWristToMetacarpal.rotation = rot * offsets.rotationOffsets[ITKHand.Wrist];
                        break;
                    case ITKHand.ThumbMetacarpal:
                        ThumbMetacarpal.rotation = rot * offsets.rotationOffsets[ITKHand.ThumbMetacarpal];
                        break;
                    case ITKHand.ThumbProximal:
                        ThumbProximal.rotation = rot * offsets.rotationOffsets[ITKHand.ThumbProximal];
                        break;
                    case ITKHand.ThumbDistal:
                        ThumbDistal.rotation = rot * offsets.rotationOffsets[ITKHand.ThumbProximal];
                        break;

                    case ITKHand.IndexKnuckle:
                        IndexKnuckle.rotation = rot * offsets.rotationOffsets[ITKHand.IndexKnuckle];
                        break;
                    case ITKHand.IndexMiddle:
                        IndexMiddle.rotation = rot * offsets.rotationOffsets[ITKHand.IndexMiddle];
                        break;
                    case ITKHand.IndexDistal:
                        IndexDistal.rotation = rot * offsets.rotationOffsets[ITKHand.IndexDistal];
                        break;

                    case ITKHand.MiddleKnuckle:
                        MiddleKnuckle.rotation = rot * offsets.rotationOffsets[ITKHand.MiddleKnuckle];
                        break;
                    case ITKHand.MiddleMiddle:
                        MiddleMiddle.rotation = rot * offsets.rotationOffsets[ITKHand.MiddleMiddle];
                        break;
                    case ITKHand.MiddleDistal:
                        MiddleDistal.rotation = rot * offsets.rotationOffsets[ITKHand.MiddleDistal];
                        break;

                    case ITKHand.RingKnuckle:
                        RingKnuckle.rotation = rot * offsets.rotationOffsets[ITKHand.RingKnuckle];
                        break;
                    case ITKHand.RingMiddle:
                        RingMiddle.rotation = rot * offsets.rotationOffsets[ITKHand.RingMiddle];
                        break;
                    case ITKHand.RingDistal:
                        RingDistal.rotation = rot * offsets.rotationOffsets[ITKHand.RingDistal];
                        break;

                    case ITKHand.PinkyMetacarpal:
                        PinkyMetacarpal.rotation = rot * offsets.rotationOffsets[ITKHand.PinkyMetacarpal];
                        break;
                    case ITKHand.PinkyKnuckle:
                        PinkyKnuckle.rotation = rot * offsets.rotationOffsets[ITKHand.PinkyKnuckle];
                        break;
                    case ITKHand.PinkyMiddle:
                        PinkyMiddle.rotation = rot * offsets.rotationOffsets[ITKHand.PinkyMiddle];
                        break;
                    case ITKHand.PinkyDistal:
                        PinkyDistal.rotation = rot * offsets.rotationOffsets[ITKHand.PinkyDistal];
                        break;
                }
            }
        }
    }
}