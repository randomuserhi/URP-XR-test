using Microsoft.MixedReality.Toolkit.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InteractionTK.HandTracking
{
    public static partial class ITKHand
    {
        public const int Wrist = 0;

        public const int ThumbMetacarpal = 1;
        public const int IndexMetacarpal = 2;
        public const int MiddleMetacarpal = 3;
        public const int RingMetacarpal = 4;
        public const int PinkyMetacarpal = 5;

        public const int ThumbProximal = 6;
        public const int IndexKnuckle = 7;
        public const int MiddleKnuckle = 8;
        public const int PinkyKnuckle = 9;
        public const int RingKnuckle = 10;

        public const int ThumbDistal = 11;
        public const int IndexMiddle = 12;
        public const int MiddleMiddle = 13;
        public const int RingMiddle = 14;
        public const int PinkyMiddle = 15;

        public const int ThumbTip = 16;
        public const int IndexDistal = 17;
        public const int MiddleDistal = 18;
        public const int RingDistal = 19;
        public const int PinkyDistal = 20;

        public const int IndexTip = 21;
        public const int MiddleTip = 22;
        public const int RingTip = 23;
        public const int PinkyTip = 24;

        public const int Palm = 25;

        public const int NumJoints = 26;

        public const int Metacarpals = 1;
        public const int NumMetacarpals = 5;

        public const int Knuckles = 6;
        public const int NumKnuckles = 5;

        public const int Middles = 10;
        public const int NumMiddles = 5;

        public const int Distals = 15;
        public const int NumDistals = 5;

        public const int Tips = 20;
        public const int NumTips = 4;

        public const int Root = Wrist;

        public static Joint[] fingerTips = new Joint[]
        {
            ThumbTip,
            IndexTip,
            MiddleTip,
            RingTip,
            PinkyTip
        };

        public static TrackedHandJoint[] MRTKJoints = new TrackedHandJoint[] {
            TrackedHandJoint.Wrist,
            TrackedHandJoint.ThumbMetacarpalJoint,
            TrackedHandJoint.IndexMetacarpal,
            TrackedHandJoint.MiddleMetacarpal,
            TrackedHandJoint.RingMetacarpal,
            TrackedHandJoint.PinkyMetacarpal,

            TrackedHandJoint.ThumbProximalJoint,
            TrackedHandJoint.IndexKnuckle,
            TrackedHandJoint.MiddleKnuckle,
            TrackedHandJoint.PinkyKnuckle,
            TrackedHandJoint.RingKnuckle,

            TrackedHandJoint.ThumbDistalJoint,
            TrackedHandJoint.IndexMiddleJoint,
            TrackedHandJoint.MiddleMiddleJoint,
            TrackedHandJoint.RingMiddleJoint,
            TrackedHandJoint.PinkyMiddleJoint,

            TrackedHandJoint.ThumbTip,
            TrackedHandJoint.IndexDistalJoint,
            TrackedHandJoint.MiddleDistalJoint,
            TrackedHandJoint.RingDistalJoint,
            TrackedHandJoint.PinkyDistalJoint,

            TrackedHandJoint.IndexTip,
            TrackedHandJoint.MiddleTip,
            TrackedHandJoint.RingTip,
            TrackedHandJoint.PinkyTip,

            TrackedHandJoint.Palm
        };
        public static TrackedHandJoint[] ToMRTKJoint = MRTKJoints;
        public static Joint[] FromMRTKJoint = new Joint[] {
            Wrist, // MRTK doesn't have a value for 0
            Wrist,
            Palm,
            ThumbMetacarpal,
            ThumbProximal,
            ThumbDistal,
            ThumbTip,
            IndexMetacarpal,
            IndexKnuckle,
            IndexMiddle,
            IndexDistal,
            IndexTip,
            MiddleMetacarpal,
            MiddleKnuckle,
            MiddleMiddle,
            MiddleDistal,
            MiddleTip,
            RingMetacarpal,
            RingKnuckle,
            RingMiddle,
            RingDistal,
            RingTip,
            PinkyMetacarpal,
            PinkyKnuckle,
            PinkyMiddle,
            PinkyDistal,
            PinkyTip
        };

        public enum Handedness
        {
            Left,
            Right
        }

        public struct Pose
        {
            public Vector3[] positions;
            public Quaternion[] rotations;

            public Pose(int NumJoints)
            {
                positions = new Vector3[NumJoints];
                rotations = new Quaternion[NumJoints];

                // Set all quaternions to identity (unity default value for quaternion is invalid)
                for (int i = 0; i < rotations.Length; ++i)
                    rotations[i] = Quaternion.identity;
            }

            public void Copy(Pose other)
            {
                Array.Copy(other.positions, positions, NumJoints);
                Array.Copy(other.rotations, rotations, NumJoints);
            }
        }

        public struct Joint
        {
            readonly int joint;
            public Joint(int joint)
            {
                this.joint = joint;
            }

            public const int Wrist = 0;

            public const int ThumbMetacarpal = 1;
            public const int IndexMetacarpal = 2;
            public const int MiddleMetacarpal = 3;
            public const int RingMetacarpal = 4;
            public const int PinkyMetacarpal = 5;

            public const int ThumbProximal = 6;
            public const int IndexKnuckle = 7;
            public const int MiddleKnuckle = 8;
            public const int PinkyKnuckle = 9;
            public const int RingKnuckle = 10;

            public const int ThumbDistal = 11;
            public const int IndexMiddle = 12;
            public const int MiddleMiddle = 13;
            public const int RingMiddle = 14;
            public const int PinkyMiddle = 15;

            public const int ThumbTip = 16;
            public const int IndexDistal = 17;
            public const int MiddleDistal = 18;
            public const int RingDistal = 19;
            public const int PinkyDistal = 20;

            public const int IndexTip = 21;
            public const int MiddleTip = 22;
            public const int RingTip = 23;
            public const int PinkyTip = 24;

            public const int Palm = 25;

            public static implicit operator int(Joint value)
            {
                return value.joint;
            }

            public static implicit operator Joint(int value)
            {
                return new Joint(value);
            }

            private static string[] names =
            {
                "Wrist",

                "ThumbMetacarpal",
                "IndexMetacarpal",
                "MiddleMetacarpal",
                "RingMetacarpal",
                "PinkyMetacarpal",

                "ThumbProximal",
                "IndexKnuckle",
                "MiddleKnuckle",
                "PinkyKnuckle",
                "RingKnuckle",

                "ThumbDistal",
                "IndexMiddle",
                "MiddleMiddle",
                "RingMiddle",
                "PinkyMiddle",

                "ThumbTip",
                "IndexDistal",
                "MiddleDistal",
                "RingDistal",
                "PinkyDistal",

                "IndexTip",
                "MiddleTip",
                "RingTip",
                "PinkyTip",

                "Palm"
            };
            public override string ToString()
            {
                return names[joint];
            }
        }
    }
}