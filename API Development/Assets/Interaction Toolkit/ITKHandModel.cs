using JetBrains.Annotations;
using Microsoft.MixedReality.OpenXR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InteractionTK.HandTracking
{
    public static partial class ITKHandUtils
    {
        public struct HandModelPoseOffsets
        {
            public Vector3 wristPositionOffset;
            public Quaternion wristRotationOffset;
            public Quaternion[] rotationOffsets;
        }

        // TODO:: package these values in a single struct for hand setup and throw into a JSON
        public static HandModelPoseOffsets leftModelOffsets = new HandModelPoseOffsets()
        {
            wristPositionOffset = new Vector3(0.06f, -0.01f, 0),
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

        // TODO:: Implement Track function that uses HandUtils.Pose

        public Quaternion testRot = Quaternion.identity;
        public Vector3 testPos;

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