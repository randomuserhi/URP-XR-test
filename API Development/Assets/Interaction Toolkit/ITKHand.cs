using JetBrains.Annotations;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

using VirtualRealityTK;

namespace InteractionTK.HandTracking
{
    public static partial class HandUtils
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

        public struct ArticulationDriveXYZ
        {
            public ArticulationJointType type;
            public ArticulationDrive zDrive;
            public ArticulationDrive yDrive;
            public ArticulationDrive xDrive;
            public ArticulationDofLock swingZ;
            public ArticulationDofLock swingY;
            public ArticulationDofLock swingX;
        }

        public struct JointTransform
        {
            public Vector3 position;

            public JointTransform(float x, float y, float z)
            {
                position = new Vector3(x, y, z);
            }

            public JointTransform(float x, float y, float z, Vector3 fromDir)
            {
                position = new Vector3(x, y, z);
            }
        }

        public struct ColliderJoint
        {
            public float height;
            public float radius;
        }

        public struct Pose
        {
            public Vector3[] positions;
            public Quaternion[] rotations;

            public Pose(int NumJoints = NumJoints)
            {
                positions = new Vector3[NumJoints];
                rotations = new Quaternion[NumJoints];
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

        // TODO:: package these values in a single struct for hand setup and throw into a JSON
        public const float stiffness = 100000f;
        public const float damping = 10;
        public const float forceLimit = float.MaxValue;
        public static int[] StructureCount = new int[] { 4, 5, 5, 5, 5 };
        public static Joint[][] Structure = new Joint[][]
        {
            new Joint[] { Joint.ThumbMetacarpal, Joint.ThumbProximal, Joint.ThumbDistal, Joint.ThumbTip },
            new Joint[] { Joint.IndexMetacarpal, Joint.IndexKnuckle, Joint.IndexMiddle, Joint.IndexDistal, Joint.IndexTip },
            new Joint[] { Joint.MiddleMetacarpal, Joint.MiddleKnuckle, Joint.MiddleMiddle, Joint.MiddleDistal, Joint.MiddleTip },
            new Joint[] { Joint.RingMetacarpal, Joint.RingKnuckle, Joint.RingMiddle, Joint.RingDistal, Joint.RingTip },
            new Joint[] { Joint.PinkyMetacarpal, Joint.PinkyKnuckle, Joint.PinkyMiddle, Joint.PinkyDistal, Joint.PinkyTip }
        };
        public static ArticulationDriveXYZ[][] DriveStructure = new ArticulationDriveXYZ[][]
        {
            new ArticulationDriveXYZ[] {
                new ArticulationDriveXYZ() {
                    type = ArticulationJointType.SphericalJoint,
                    zDrive = new ArticulationDrive() { lowerLimit = -180f, upperLimit = 180f, stiffness = stiffness, damping = damping, forceLimit = forceLimit },
                    yDrive = new ArticulationDrive() { lowerLimit = -180f, upperLimit = 180f, stiffness = stiffness, damping = damping, forceLimit = forceLimit },
                    xDrive = new ArticulationDrive() { lowerLimit = -180f, upperLimit = 180f, stiffness = stiffness, damping = damping, forceLimit = forceLimit },
                    swingZ = ArticulationDofLock.LimitedMotion,
                    swingY = ArticulationDofLock.LimitedMotion,
                    swingX = ArticulationDofLock.LimitedMotion
                },
                new ArticulationDriveXYZ() {
                    type = ArticulationJointType.SphericalJoint,
                    zDrive = new ArticulationDrive() { lowerLimit = -180f, upperLimit = 180f, stiffness = stiffness, damping = damping, forceLimit = forceLimit },
                    yDrive = new ArticulationDrive() { lowerLimit = -50f, upperLimit = 50f, stiffness = stiffness, damping = damping, forceLimit = forceLimit },
                    xDrive = new ArticulationDrive() { lowerLimit = -50f, upperLimit = 50f, stiffness = stiffness, damping = damping, forceLimit = forceLimit },
                    swingZ = ArticulationDofLock.LimitedMotion,
                    swingY = ArticulationDofLock.LimitedMotion,
                    swingX = ArticulationDofLock.LimitedMotion
                },
                new ArticulationDriveXYZ() {
                    type = ArticulationJointType.SphericalJoint,
                    zDrive = new ArticulationDrive() { lowerLimit = 0f, upperLimit = 0f, stiffness = stiffness, damping = damping, forceLimit = forceLimit },
                    yDrive = new ArticulationDrive() { lowerLimit = 0f, upperLimit = 0f, stiffness = stiffness, damping = damping, forceLimit = forceLimit },
                    xDrive = new ArticulationDrive() { lowerLimit = -50f, upperLimit = 90f, stiffness = stiffness, damping = damping, forceLimit = forceLimit },
                    swingZ = ArticulationDofLock.LockedMotion,
                    swingY = ArticulationDofLock.LockedMotion,
                    swingX = ArticulationDofLock.LimitedMotion
                },
                new ArticulationDriveXYZ() {
                    type = ArticulationJointType.SphericalJoint,
                    zDrive = new ArticulationDrive() { lowerLimit = 0f, upperLimit = 0f, stiffness = stiffness, damping = damping, forceLimit = forceLimit },
                    yDrive = new ArticulationDrive() { lowerLimit = 0f, upperLimit = 0f, stiffness = stiffness, damping = damping, forceLimit = forceLimit },
                    xDrive = new ArticulationDrive() { lowerLimit = -10f, upperLimit = 90f, stiffness = stiffness, damping = damping, forceLimit = forceLimit },
                    swingZ = ArticulationDofLock.LockedMotion,
                    swingY = ArticulationDofLock.LockedMotion,
                    swingX = ArticulationDofLock.LimitedMotion
                }
            },
            new ArticulationDriveXYZ[] {
                new ArticulationDriveXYZ() {
                    type = ArticulationJointType.SphericalJoint,
                    zDrive = new ArticulationDrive() { lowerLimit = -80, upperLimit = 80, stiffness = stiffness, damping = damping, forceLimit = forceLimit },
                    yDrive = new ArticulationDrive() { lowerLimit = -40, upperLimit = 40, stiffness = stiffness, damping = damping, forceLimit = forceLimit },
                    xDrive = new ArticulationDrive() { lowerLimit = -90, upperLimit = 90, stiffness = stiffness, damping = damping, forceLimit = forceLimit },
                    swingZ = ArticulationDofLock.LimitedMotion,
                    swingY = ArticulationDofLock.LimitedMotion,
                    swingX = ArticulationDofLock.LimitedMotion
                },
                new ArticulationDriveXYZ() {
                    type = ArticulationJointType.SphericalJoint,
                    zDrive = new ArticulationDrive() { lowerLimit = -40, upperLimit = 40, stiffness = stiffness, damping = damping, forceLimit = forceLimit },
                    yDrive = new ArticulationDrive() { lowerLimit = -60f, upperLimit = 60f, stiffness = stiffness, damping = damping, forceLimit = forceLimit },
                    xDrive = new ArticulationDrive() { lowerLimit = -40f, upperLimit = 40f, stiffness = stiffness, damping = damping, forceLimit = forceLimit },
                    swingZ = ArticulationDofLock.LimitedMotion,
                    swingY = ArticulationDofLock.LimitedMotion,
                    swingX = ArticulationDofLock.LimitedMotion
                },
                new ArticulationDriveXYZ() {
                    type = ArticulationJointType.SphericalJoint,
                    zDrive = new ArticulationDrive() { lowerLimit = -50f, upperLimit = 50f, stiffness = stiffness, damping = damping, forceLimit = forceLimit },
                    yDrive = new ArticulationDrive() { lowerLimit = -50f, upperLimit = 50f, stiffness = stiffness, damping = damping, forceLimit = forceLimit },
                    xDrive = new ArticulationDrive() { lowerLimit = -50f, upperLimit = 90f, stiffness = stiffness, damping = damping, forceLimit = forceLimit },
                    swingZ = ArticulationDofLock.LimitedMotion,
                    swingY = ArticulationDofLock.LimitedMotion,
                    swingX = ArticulationDofLock.LimitedMotion
                },
                new ArticulationDriveXYZ() {
                    type = ArticulationJointType.SphericalJoint,
                    zDrive = new ArticulationDrive() { lowerLimit = 0f, upperLimit = 0f, stiffness = stiffness, damping = damping, forceLimit = forceLimit },
                    yDrive = new ArticulationDrive() { lowerLimit = 0f, upperLimit = 0f, stiffness = stiffness, damping = damping, forceLimit = forceLimit },
                    xDrive = new ArticulationDrive() { lowerLimit = 0f, upperLimit = 120f, stiffness = stiffness, damping = damping, forceLimit = forceLimit },
                    swingZ = ArticulationDofLock.LockedMotion,
                    swingY = ArticulationDofLock.LockedMotion,
                    swingX = ArticulationDofLock.LimitedMotion
                },
                new ArticulationDriveXYZ() {
                    type = ArticulationJointType.SphericalJoint,
                    zDrive = new ArticulationDrive() { lowerLimit = 0f, upperLimit = 0f, stiffness = stiffness, damping = damping, forceLimit = forceLimit },
                    yDrive = new ArticulationDrive() { lowerLimit = 0f, upperLimit = 0f, stiffness = stiffness, damping = damping, forceLimit = forceLimit },
                    xDrive = new ArticulationDrive() { lowerLimit = -10f, upperLimit = 90f, stiffness = stiffness, damping = damping, forceLimit = forceLimit },
                    swingZ = ArticulationDofLock.LockedMotion,
                    swingY = ArticulationDofLock.LockedMotion,
                    swingX = ArticulationDofLock.LimitedMotion
                }
            },
            new ArticulationDriveXYZ[] {
                new ArticulationDriveXYZ() {
                    type = ArticulationJointType.SphericalJoint,
                    zDrive = new ArticulationDrive() { lowerLimit = -80, upperLimit = 80, stiffness = stiffness, damping = damping, forceLimit = forceLimit },
                    yDrive = new ArticulationDrive() { lowerLimit = -40, upperLimit = 40, stiffness = stiffness, damping = damping, forceLimit = forceLimit },
                    xDrive = new ArticulationDrive() { lowerLimit = -90, upperLimit = 90, stiffness = stiffness, damping = damping, forceLimit = forceLimit },
                    swingZ = ArticulationDofLock.LimitedMotion,
                    swingY = ArticulationDofLock.LimitedMotion,
                    swingX = ArticulationDofLock.LimitedMotion
                },
                new ArticulationDriveXYZ() {
                    type = ArticulationJointType.SphericalJoint,
                    zDrive = new ArticulationDrive() { lowerLimit = -40, upperLimit = 40, stiffness = stiffness, damping = damping, forceLimit = forceLimit },
                    yDrive = new ArticulationDrive() { lowerLimit = -60f, upperLimit = 60f, stiffness = stiffness, damping = damping, forceLimit = forceLimit },
                    xDrive = new ArticulationDrive() { lowerLimit = -40f, upperLimit = 40f, stiffness = stiffness, damping = damping, forceLimit = forceLimit },
                    swingZ = ArticulationDofLock.LimitedMotion,
                    swingY = ArticulationDofLock.LimitedMotion,
                    swingX = ArticulationDofLock.LimitedMotion
                },
                new ArticulationDriveXYZ() {
                    type = ArticulationJointType.SphericalJoint,
                    zDrive = new ArticulationDrive() { lowerLimit = -50f, upperLimit = 50f, stiffness = stiffness, damping = damping, forceLimit = forceLimit },
                    yDrive = new ArticulationDrive() { lowerLimit = -50f, upperLimit = 50f, stiffness = stiffness, damping = damping, forceLimit = forceLimit },
                    xDrive = new ArticulationDrive() { lowerLimit = -50f, upperLimit = 90f, stiffness = stiffness, damping = damping, forceLimit = forceLimit },
                    swingZ = ArticulationDofLock.LimitedMotion,
                    swingY = ArticulationDofLock.LimitedMotion,
                    swingX = ArticulationDofLock.LimitedMotion
                },
                new ArticulationDriveXYZ() {
                    type = ArticulationJointType.SphericalJoint,
                    zDrive = new ArticulationDrive() { lowerLimit = 0f, upperLimit = 0f, stiffness = stiffness, damping = damping, forceLimit = forceLimit },
                    yDrive = new ArticulationDrive() { lowerLimit = 0f, upperLimit = 0f, stiffness = stiffness, damping = damping, forceLimit = forceLimit },
                    xDrive = new ArticulationDrive() { lowerLimit = 0f, upperLimit = 120f, stiffness = stiffness, damping = damping, forceLimit = forceLimit },
                    swingZ = ArticulationDofLock.LockedMotion,
                    swingY = ArticulationDofLock.LockedMotion,
                    swingX = ArticulationDofLock.LimitedMotion
                },
                new ArticulationDriveXYZ() {
                    type = ArticulationJointType.SphericalJoint,
                    zDrive = new ArticulationDrive() { lowerLimit = 0f, upperLimit = 0f, stiffness = stiffness, damping = damping, forceLimit = forceLimit },
                    yDrive = new ArticulationDrive() { lowerLimit = 0f, upperLimit = 0f, stiffness = stiffness, damping = damping, forceLimit = forceLimit },
                    xDrive = new ArticulationDrive() { lowerLimit = -10f, upperLimit = 90f, stiffness = stiffness, damping = damping, forceLimit = forceLimit },
                    swingZ = ArticulationDofLock.LockedMotion,
                    swingY = ArticulationDofLock.LockedMotion,
                    swingX = ArticulationDofLock.LimitedMotion
                }
            },
            new ArticulationDriveXYZ[] {
                new ArticulationDriveXYZ() {
                    type = ArticulationJointType.SphericalJoint,
                    zDrive = new ArticulationDrive() { lowerLimit = -80, upperLimit = 80, stiffness = stiffness, damping = damping, forceLimit = forceLimit },
                    yDrive = new ArticulationDrive() { lowerLimit = -40, upperLimit = 40, stiffness = stiffness, damping = damping, forceLimit = forceLimit },
                    xDrive = new ArticulationDrive() { lowerLimit = -90, upperLimit = 90, stiffness = stiffness, damping = damping, forceLimit = forceLimit },
                    swingZ = ArticulationDofLock.LimitedMotion,
                    swingY = ArticulationDofLock.LimitedMotion,
                    swingX = ArticulationDofLock.LimitedMotion
                },
                new ArticulationDriveXYZ() {
                    type = ArticulationJointType.SphericalJoint,
                    zDrive = new ArticulationDrive() { lowerLimit = -40, upperLimit = 40, stiffness = stiffness, damping = damping, forceLimit = forceLimit },
                    yDrive = new ArticulationDrive() { lowerLimit = -60f, upperLimit = 60f, stiffness = stiffness, damping = damping, forceLimit = forceLimit },
                    xDrive = new ArticulationDrive() { lowerLimit = -40f, upperLimit = 40f, stiffness = stiffness, damping = damping, forceLimit = forceLimit },
                    swingZ = ArticulationDofLock.LimitedMotion,
                    swingY = ArticulationDofLock.LimitedMotion,
                    swingX = ArticulationDofLock.LimitedMotion
                },
                new ArticulationDriveXYZ() {
                    type = ArticulationJointType.SphericalJoint,
                    zDrive = new ArticulationDrive() { lowerLimit = -50f, upperLimit = 50f, stiffness = stiffness, damping = damping, forceLimit = forceLimit },
                    yDrive = new ArticulationDrive() { lowerLimit = -50f, upperLimit = 50f, stiffness = stiffness, damping = damping, forceLimit = forceLimit },
                    xDrive = new ArticulationDrive() { lowerLimit = -50f, upperLimit = 90f, stiffness = stiffness, damping = damping, forceLimit = forceLimit },
                    swingZ = ArticulationDofLock.LimitedMotion,
                    swingY = ArticulationDofLock.LimitedMotion,
                    swingX = ArticulationDofLock.LimitedMotion
                },
                new ArticulationDriveXYZ() {
                    type = ArticulationJointType.SphericalJoint,
                    zDrive = new ArticulationDrive() { lowerLimit = 0f, upperLimit = 0f, stiffness = stiffness, damping = damping, forceLimit = forceLimit },
                    yDrive = new ArticulationDrive() { lowerLimit = 0f, upperLimit = 0f, stiffness = stiffness, damping = damping, forceLimit = forceLimit },
                    xDrive = new ArticulationDrive() { lowerLimit = 0f, upperLimit = 120f, stiffness = stiffness, damping = damping, forceLimit = forceLimit },
                    swingZ = ArticulationDofLock.LockedMotion,
                    swingY = ArticulationDofLock.LockedMotion,
                    swingX = ArticulationDofLock.LimitedMotion
                },
                new ArticulationDriveXYZ() {
                    type = ArticulationJointType.SphericalJoint,
                    zDrive = new ArticulationDrive() { lowerLimit = 0f, upperLimit = 0f, stiffness = stiffness, damping = damping, forceLimit = forceLimit },
                    yDrive = new ArticulationDrive() { lowerLimit = 0f, upperLimit = 0f, stiffness = stiffness, damping = damping, forceLimit = forceLimit },
                    xDrive = new ArticulationDrive() { lowerLimit = -10f, upperLimit = 90f, stiffness = stiffness, damping = damping, forceLimit = forceLimit },
                    swingZ = ArticulationDofLock.LockedMotion,
                    swingY = ArticulationDofLock.LockedMotion,
                    swingX = ArticulationDofLock.LimitedMotion
                }
            },
            new ArticulationDriveXYZ[] {
                new ArticulationDriveXYZ() {
                    type = ArticulationJointType.SphericalJoint,
                    zDrive = new ArticulationDrive() { lowerLimit = -80, upperLimit = 80, stiffness = stiffness, damping = damping, forceLimit = forceLimit },
                    yDrive = new ArticulationDrive() { lowerLimit = -70, upperLimit = 70, stiffness = stiffness, damping = damping, forceLimit = forceLimit },
                    xDrive = new ArticulationDrive() { lowerLimit = -90, upperLimit = 90, stiffness = stiffness, damping = damping, forceLimit = forceLimit },
                    swingZ = ArticulationDofLock.LimitedMotion,
                    swingY = ArticulationDofLock.LimitedMotion,
                    swingX = ArticulationDofLock.LimitedMotion
                },
                new ArticulationDriveXYZ() {
                    type = ArticulationJointType.SphericalJoint,
                    zDrive = new ArticulationDrive() { lowerLimit = -40, upperLimit = 40, stiffness = stiffness, damping = damping, forceLimit = forceLimit },
                    yDrive = new ArticulationDrive() { lowerLimit = -60f, upperLimit = 60f, stiffness = stiffness, damping = damping, forceLimit = forceLimit },
                    xDrive = new ArticulationDrive() { lowerLimit = -40f, upperLimit = 40f, stiffness = stiffness, damping = damping, forceLimit = forceLimit },
                    swingZ = ArticulationDofLock.LimitedMotion,
                    swingY = ArticulationDofLock.LimitedMotion,
                    swingX = ArticulationDofLock.LimitedMotion
                },
                new ArticulationDriveXYZ() {
                    type = ArticulationJointType.SphericalJoint,
                    zDrive = new ArticulationDrive() { lowerLimit = -50f, upperLimit = 50f, stiffness = stiffness, damping = damping, forceLimit = forceLimit },
                    yDrive = new ArticulationDrive() { lowerLimit = -50f, upperLimit = 50f, stiffness = stiffness, damping = damping, forceLimit = forceLimit },
                    xDrive = new ArticulationDrive() { lowerLimit = -50f, upperLimit = 90f, stiffness = stiffness, damping = damping, forceLimit = forceLimit },
                    swingZ = ArticulationDofLock.LimitedMotion,
                    swingY = ArticulationDofLock.LimitedMotion,
                    swingX = ArticulationDofLock.LimitedMotion
                },
                new ArticulationDriveXYZ() {
                    type = ArticulationJointType.SphericalJoint,
                    zDrive = new ArticulationDrive() { lowerLimit = 0f, upperLimit = 0f, stiffness = stiffness, damping = damping, forceLimit = forceLimit },
                    yDrive = new ArticulationDrive() { lowerLimit = 0f, upperLimit = 0f, stiffness = stiffness, damping = damping, forceLimit = forceLimit },
                    xDrive = new ArticulationDrive() { lowerLimit = 0f, upperLimit = 120f, stiffness = stiffness, damping = damping, forceLimit = forceLimit },
                    swingZ = ArticulationDofLock.LockedMotion,
                    swingY = ArticulationDofLock.LockedMotion,
                    swingX = ArticulationDofLock.LimitedMotion
                },
                new ArticulationDriveXYZ() {
                    type = ArticulationJointType.SphericalJoint,
                    zDrive = new ArticulationDrive() { lowerLimit = 0f, upperLimit = 0f, stiffness = stiffness, damping = damping, forceLimit = forceLimit },
                    yDrive = new ArticulationDrive() { lowerLimit = 0f, upperLimit = 0f, stiffness = stiffness, damping = damping, forceLimit = forceLimit },
                    xDrive = new ArticulationDrive() { lowerLimit = -10f, upperLimit = 90f, stiffness = stiffness, damping = damping, forceLimit = forceLimit },
                    swingZ = ArticulationDofLock.LockedMotion,
                    swingY = ArticulationDofLock.LockedMotion,
                    swingX = ArticulationDofLock.LimitedMotion
                }
            }
        }; //TODO:: fix limits (mostly Y and Z limits are wrong)
        public static JointTransform[][] LocalTransformStructure = new JointTransform[][]
        {
            new JointTransform[] {
                new JointTransform(0, 0, 0.025f),
                new JointTransform(0, 0, 0.045f), new JointTransform(0, 0, 0.04f), new JointTransform(0, 0, 0.02f)
            },
            new JointTransform[] {
                new JointTransform(0, 0, 0.018f),
                new JointTransform(0, 0, 0.07f), new JointTransform(0, 0, 0.045f), new JointTransform(0, 0, 0.025f), new JointTransform(0, 0, 0.02f)
            },
            new JointTransform[] {
                new JointTransform(0, 0, 0.015f),
                new JointTransform(0, 0, 0.07f), new JointTransform(0, 0, 0.055f), new JointTransform(0, 0, 0.025f), new JointTransform(0, 0, 0.02f)
            },
            new JointTransform[] {
                new JointTransform(0, 0, 0.016f),
                new JointTransform(0, 0, 0.07f), new JointTransform(0, 0, 0.045f), new JointTransform(0, 0, 0.025f), new JointTransform(0, 0, 0.02f)
            },
            new JointTransform[] {
                new JointTransform(0, 0, 0.02f),
                new JointTransform(0, 0, 0.065f), new JointTransform(0, 0, 0.035f), new JointTransform(0, 0, 0.02f), new JointTransform(0, 0, 0.02f)
            }
        };
        public static ColliderJoint[][] ColliderStructure = new ColliderJoint[][]
        {
            new ColliderJoint[] {
                new ColliderJoint() { height = 0.02f, radius = 0.007f },
                new ColliderJoint() { height = 0.045f, radius = 0.007f },
                new ColliderJoint() { height = 0.04f, radius = 0.006f },
                new ColliderJoint() { height = 0.02f, radius = 0.005f }
            },
            new ColliderJoint[] {
                new ColliderJoint() { height = 0.01f, radius = 0.007f },
                new ColliderJoint() { height = 0.068f, radius = 0.008f },
                new ColliderJoint() { height = 0.045f, radius = 0.007f },
                new ColliderJoint() { height = 0.025f, radius = 0.006f },
                new ColliderJoint() { height = 0.02f, radius = 0.005f }
            },
            new ColliderJoint[] {
                new ColliderJoint() { height = 0.015f, radius = 0.007f },
                new ColliderJoint() { height = 0.068f, radius = 0.008f },
                new ColliderJoint() { height = 0.05f, radius = 0.007f },
                new ColliderJoint() { height = 0.025f, radius = 0.006f },
                new ColliderJoint() { height = 0.02f, radius = 0.005f }
            },
            new ColliderJoint[] {
                new ColliderJoint() { height = 0.01f, radius = 0.007f },
                new ColliderJoint() { height = 0.068f, radius = 0.008f },
                new ColliderJoint() { height = 0.045f, radius = 0.007f },
                new ColliderJoint() { height = 0.025f, radius = 0.006f },
                new ColliderJoint() { height = 0.02f, radius = 0.005f }
            },
            new ColliderJoint[] {
                new ColliderJoint() { height = 0.02f, radius = 0.007f },
                new ColliderJoint() { height = 0.068f, radius = 0.008f },
                new ColliderJoint() { height = 0.035f, radius = 0.007f },
                new ColliderJoint() { height = 0.02f, radius = 0.006f },
                new ColliderJoint() { height = 0.02f, radius = 0.005f }
            }
        };
    }

    public class ITKSkeletonNode
    {
        public struct Joint //TODO:: make it such that rotation order can change, xyz => zyx etc.... (Currently only xyz)
        {
            public ArticulationBody x;
            public ArticulationBody y;
            public ArticulationBody z;

            public ArticulationBody last
            {
                get
                {
                    if (z != null) return z;
                    if (x != null) return x;
                    return y;
                }
            }

            public ArticulationDrive xDrive
            {
                get => x.xDrive;
                set => x.xDrive = value;
            }
            public ArticulationDrive yDrive
            {
                get => y.xDrive;
                set => y.xDrive = value;
            }
            public ArticulationDrive zDrive
            {
                get => z.xDrive;
                set => z.xDrive = value;
            }

            public string name 
            { 
                set
                {
                    if (x) x.name = "x-" + value;
                    if (y) y.name = "y-" + value;
                    if (z) z.name = "z-" + value;
                } 
            }
            public CollisionDetectionMode collisionDetectionMode
            {
                set
                {
                    if (x) x.collisionDetectionMode = value;
                    if (y) y.collisionDetectionMode = value;
                    if (z) z.collisionDetectionMode = value;
                }
            }
            public int solverIterations
            {
                set
                {
                    if (x) x.solverIterations = value;
                    if (y) y.solverIterations = value;
                    if (z) z.solverIterations = value;
                }
            }
            public int solverVelocityIterations
            {
                set
                {
                    if (x) x.solverVelocityIterations = value;
                    if (y) y.solverVelocityIterations = value;
                    if (z) z.solverVelocityIterations = value;
                }
            }
            private float _mass;
            public float mass
            {
                set
                {
                    _mass = value;
                    if (x) x.mass = value;
                    if (y) y.mass = value;
                    if (z) z.mass = value;
                }
                get => _mass;
            }
            public float jointFriction
            {
                set
                {
                    if (x) x.jointFriction = value;
                    if (y) y.jointFriction = value;
                    if (z) z.jointFriction = value;
                }
            }
            public Vector3 localPosition
            {
                set
                {
                    if (x) x.transform.localPosition = value;
                    if (y) y.transform.localPosition = value;
                    if (z) z.transform.localPosition = value;
                }
            }
            public Quaternion localRotation
            {
                set
                {
                    if (x) x.transform.localRotation = value;
                    if (y) y.transform.localRotation = value;
                    if (z) z.transform.localRotation = value;
                }
            }
            public Vector3 position
            {
                get => last.transform.position;
            }
            public Quaternion rotation
            {
                get => last.transform.rotation;
            }
            public Vector3 velocity
            {
                get => last.velocity;
                set
                {
                    if (x) x.velocity = value;
                    if (y) y.velocity = value;
                    if (z) z.velocity = value;
                }
            }
            public Vector3 angularVelocity
            {
                get => last.angularVelocity;
                set
                {
                    if (x) x.angularVelocity = value;
                    if (y) y.angularVelocity = value;
                    if (z) z.angularVelocity = value;
                }
            }

            public void AddForce(Vector3 force)
            {
                if (x) x.AddForce(force);
                if (y) y.AddForce(force);
                if (z) z.AddForce(force);
            }

            public static implicit operator ArticulationBody(Joint j) => j.x;
        }

        public ITKSkeletonNode[] children;
        public Joint joint;

        private static GameObject CreateBody(Transform parent, out ArticulationBody body)
        {
            GameObject o = new GameObject();
            o.transform.parent = parent;
            o.transform.localPosition = Vector3.zero;
            body = o.AddComponent<ArticulationBody>();

            return o;
        }
        private static GameObject CreateBody(Transform parent, HandUtils.ColliderJoint joint, out ArticulationBody body, out CapsuleCollider collider)
        {
            GameObject o = new GameObject();
            o.transform.parent = parent;
            o.transform.localPosition = Vector3.zero;
            body = o.AddComponent<ArticulationBody>();
            collider = o.AddComponent<CapsuleCollider>();
            collider.radius = joint.radius;
            collider.height = joint.height;
            collider.direction = 2;
            collider.center = new Vector3(0, 0, -joint.height / 2f);

            return o;
        }

        public static ITKSkeletonNode HandSkeleton(Transform parent, HandUtils.Handedness type, out ITKSkeletonNode[][] skeleton, out ArticulationBody[] bodies, out CapsuleCollider[] capsuleColliders)
        {
            Vector3 scale = new Vector3(type == HandUtils.Handedness.Left ? 1 : -1, 1, 1);

            List<CapsuleCollider> colliders = new List<CapsuleCollider>();
            List<ArticulationBody> articulationBodies = new List<ArticulationBody>();
            ITKSkeletonNode wrist = new ITKSkeletonNode();
            CreateBody(parent, out wrist.joint.x);
            wrist.joint.name = "Wrist";
            wrist.children = new ITKSkeletonNode[HandUtils.StructureCount.Length];
            wrist.children.InitializeArray();
            wrist.joint.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            wrist.joint.solverIterations = 60;
            wrist.joint.solverVelocityIterations = 20;
            wrist.joint.mass = 100;
            wrist.joint.jointFriction = 10;

            skeleton = new ITKSkeletonNode[HandUtils.StructureCount.Length][];
            for (int i = 0; i < HandUtils.StructureCount.Length; ++i)
            {
                skeleton[i] = new ITKSkeletonNode[HandUtils.StructureCount[i]];
                Transform p = wrist.joint.last.transform;
                ITKSkeletonNode node = wrist.children[i];
                float mass = 50;
                for (int j = 0; j < HandUtils.StructureCount[i]; ++j, mass /= 2f)
                {
                    HandUtils.ArticulationDriveXYZ drive = HandUtils.DriveStructure[i][j];
                    HandUtils.ColliderJoint col = HandUtils.ColliderStructure[i][j];
                    if (drive.type == ArticulationJointType.FixedJoint)
                    {
                        CapsuleCollider collider;
                        CreateBody(p, col, out node.joint.x, out collider);
                        colliders.Add(collider);
                        articulationBodies.Add(node.joint);

                        node.joint.name = HandUtils.Structure[i][j].ToString();
                        node.joint.localPosition = Vector3.Scale(HandUtils.LocalTransformStructure[i][j].position, scale);
                        node.joint.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                        node.joint.mass = mass;
                        node.joint.jointFriction = 10;
                        node.joint.last.anchorPosition = node.joint.last.transform.InverseTransformPoint(p.transform.position);
                    }
                    else if (drive.type == ArticulationJointType.SphericalJoint)
                    {
                        Transform q = p;
                        int count = 0;
                        if (drive.swingY != ArticulationDofLock.LockedMotion)
                        {
                            ++count;
                            CreateBody(p, out node.joint.y);
                            node.joint.y.jointType = ArticulationJointType.RevoluteJoint;
                            node.joint.y.anchorRotation = q.rotation * Quaternion.LookRotation(Vector3.forward, Vector3.up) * Quaternion.Euler(new Vector3(0, 0, 90));

                            node.joint.y.twistLock = drive.swingY;
                            node.joint.y.xDrive = drive.yDrive;

                            p = node.joint.y.transform;
                            articulationBodies.Add(node.joint.y);
                        }
                        if (drive.swingX != ArticulationDofLock.LockedMotion)
                        {
                            ++count;
                            CreateBody(p, out node.joint.x);
                            node.joint.x.jointType = ArticulationJointType.RevoluteJoint;
                            node.joint.x.anchorRotation = q.rotation * Quaternion.LookRotation(Vector3.forward, Vector3.up);

                            node.joint.x.twistLock = drive.swingX;
                            node.joint.x.xDrive = drive.xDrive;

                            p = node.joint.x.transform;
                            articulationBodies.Add(node.joint.x);
                        }
                        if (drive.swingZ != ArticulationDofLock.LockedMotion)
                        {
                            ++count;
                            CreateBody(p, out node.joint.z);
                            node.joint.z.jointType = ArticulationJointType.RevoluteJoint;
                            node.joint.z.anchorRotation = q.rotation * Quaternion.LookRotation(Vector3.left, Vector3.up);

                            node.joint.z.twistLock = drive.swingZ;
                            node.joint.z.xDrive = drive.zDrive;

                            p = node.joint.z.transform;
                            articulationBodies.Add(node.joint.z);
                        }

                        node.joint.name = HandUtils.Structure[i][j].ToString();
                        Quaternion offset = count > 1 ? q.rotation : Quaternion.identity;
                        node.joint.last.transform.localPosition = offset * Vector3.Scale(HandUtils.LocalTransformStructure[i][j].position, scale);
                        node.joint.last.anchorPosition = node.joint.last.transform.InverseTransformPoint(q.transform.position);
                        node.joint.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                        node.joint.mass = mass;
                        node.joint.jointFriction = 10;

                        CapsuleCollider collider;
                        collider = node.joint.last.gameObject.AddComponent<CapsuleCollider>();
                        collider.radius = col.radius;
                        collider.height = col.height;
                        collider.direction = 2;
                        collider.center = new Vector3(0, 0, -col.height / 2f);
                        colliders.Add(collider);
                    }
                    else
                    {
                        Debug.LogError("Joint type not supported, please use FixedJoint or SphericalJoint");
                        bodies = null;
                        capsuleColliders = null;
                        return null;
                    }

                    p = node.joint.last.transform;
                    skeleton[i][j] = node;
                    node.children = new ITKSkeletonNode[1];
                    node.children.InitializeArray();
                    node = node.children[0];
                }
            }

            for (int i = 0; i < colliders.Count; ++i)
                for (int j = 0; j < colliders.Count; ++j)
                    if (i != j) Physics.IgnoreCollision(colliders[i], colliders[j], true);

            capsuleColliders = colliders.ToArray();
            bodies = articulationBodies.ToArray();
            return wrist;
        }
    }

    public class ITKHand : MonoBehaviour
    {
        ITKSkeletonNode wrist;
        ITKSkeletonNode[][] skeleton;
        public HandUtils.Handedness type;
        public ITKHandModel model;

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

        public bool hideModelOnDisable = true;

        public void Enable()
        {
            if (_active) return;

            wrist.joint.last.immovable = false;
            foreach (CapsuleCollider collider in capsuleColliders) collider.enabled = true;

            if (hideModelOnDisable) model.Enable();

            _active = true;
        }

        public void Disable()
        {
            if (!_active) return;

            wrist.joint.last.immovable = true;
            wrist.joint.velocity = Vector3.zero;
            wrist.joint.angularVelocity = Vector3.zero;
            foreach (CapsuleCollider collider in capsuleColliders) collider.enabled = false;
            for (int i = 0; i < articulationBodies.Length; i++)
            {
                articulationBodies[i].velocity = Vector3.zero;
                articulationBodies[i].angularVelocity = Vector3.zero;
                articulationBodies[i].jointPosition = new ArticulationReducedSpace(0f, 0f, 0f);
            }

            if (hideModelOnDisable) model.Disable();

            _active = false;
        }

        private void Start()
        {
            GenerateBody();
        }

        private void GenerateBody()
        {
            wrist = ITKSkeletonNode.HandSkeleton(transform, type, out skeleton, out articulationBodies, out capsuleColliders);
        }

        private CapsuleCollider[] capsuleColliders;
        private ArticulationBody[] articulationBodies;
        private int lastFrameTeleport;
        private bool ghosted = false;

        public void Track(HandUtils.Pose pose)
        {
            if (!active) return;

            model?.Track(wrist, skeleton);

            // Counter Gravity; force = mass * acceleration
            wrist.joint.AddForce(-Physics.gravity * wrist.joint.mass);
            foreach (ArticulationBody body in articulationBodies)
            {
                body.AddForce(-Physics.gravity * body.mass);
            }

            // Solve position constraint
            wrist.joint.velocity *= 0.05f;
            Vector3 velocity = (pose.positions[HandUtils.Wrist] - wrist.joint.position) * 0.95f / Time.fixedDeltaTime;
            //TODO:: make the velocity clamp only when hand is in contact with an object => clamp is to prevent joints from breaking when high velocities are applied against an object
            wrist.joint.velocity += Vector3.ClampMagnitude(velocity, 0.5f);

            // Solve rotation constraint
            Quaternion rotation = pose.rotations[HandUtils.Wrist] * Quaternion.Inverse(wrist.joint.rotation);
            Vector3 rot;
            float speed;
            rotation.ToAngleAxis(out speed, out rot);
            wrist.joint.angularVelocity = rot * speed * Mathf.Deg2Rad / Time.fixedDeltaTime;

            // Rotate joints
            for (int i = 0; i < HandUtils.StructureCount.Length; ++i)
            {
                HandUtils.Joint prevJoint = HandUtils.Wrist;
                Quaternion currentRotation = Quaternion.identity;
                for (int j = 0; j < HandUtils.StructureCount[i]; ++j)
                {
                    HandUtils.Joint currJoint = HandUtils.Structure[i][j];
                    if (i != 0 && j > 2)
                    {
                        HandUtils.Joint prevPrevJoint = HandUtils.Structure[i][j - 2];
                        if (skeleton[i][j].joint.x)
                        {
                            ArticulationDrive drive = skeleton[i][j].joint.xDrive;
                            float angle = Vector3.SignedAngle(
                                pose.rotations[prevPrevJoint] * Vector3.forward,
                                pose.positions[currJoint] - pose.positions[prevJoint],
                                pose.rotations[prevPrevJoint] * Vector3.right
                                );
                            drive.target = angle > 180 ? angle - 360 : angle;
                            skeleton[i][j].joint.xDrive = drive;
                        }
                    }
                    else
                    {
                        Quaternion localRotation;
                        if (j == 0)
                        {
                            Quaternion r = Quaternion.LookRotation(pose.positions[currJoint] - pose.positions[prevJoint], pose.rotations[prevJoint] * Vector3.up);
                            localRotation = Quaternion.Inverse(pose.rotations[prevJoint]) * r;
                            currentRotation = r;
                        }
                        else
                        {
                            localRotation = Quaternion.Inverse(currentRotation) * pose.rotations[prevJoint];
                            currentRotation *= localRotation;
                        }

                        if (skeleton[i][j].joint.y)
                        {
                            ArticulationDrive drive = skeleton[i][j].joint.yDrive;
                            float angle = localRotation.eulerAngles.y;
                            drive.target = angle > 180 ? angle - 360 : angle;
                            skeleton[i][j].joint.yDrive = drive;
                        }
                        if (skeleton[i][j].joint.x)
                        {
                            ArticulationDrive drive = skeleton[i][j].joint.xDrive;
                            float angle = localRotation.eulerAngles.x;
                            drive.target = angle > 180 ? angle - 360 : angle;
                            skeleton[i][j].joint.xDrive = drive;
                        }
                        if (skeleton[i][j].joint.z)
                        {
                            ArticulationDrive drive = skeleton[i][j].joint.zDrive;
                            float angle = localRotation.eulerAngles.z;
                            drive.target = angle > 180 ? angle - 360 : angle;
                            skeleton[i][j].joint.zDrive = drive;
                        }
                    }

                    prevJoint = currJoint;
                }
            }

            // Fix the hand if it gets into a bad situation by teleporting and holding in place until its bad velocities disappear
            // TODO:: dont just check the wrist if it goes out of wack, check all joints
            if (Vector3.Distance(wrist.joint.position, pose.positions[HandUtils.Joint.Wrist]) > 0.2f)
            {
                wrist.joint.last.immovable = true;
                wrist.joint.last.TeleportRoot(pose.positions[HandUtils.Joint.Wrist], pose.rotations[HandUtils.Joint.Wrist]);
                wrist.joint.velocity = Vector3.zero;
                wrist.joint.angularVelocity = Vector3.zero;
                lastFrameTeleport = Time.frameCount;
                foreach (CapsuleCollider collider in capsuleColliders) collider.enabled = false;
                for (int i = 0; i < articulationBodies.Length; i++)
                {
                    articulationBodies[i].velocity = Vector3.zero;
                    articulationBodies[i].angularVelocity = Vector3.zero;
                    // Reset joint position in case they got fucked up
                    articulationBodies[i].jointPosition = new ArticulationReducedSpace(0f, 0f, 0f);
                }
                ghosted = true;
            }
            if (Time.frameCount - lastFrameTeleport >= 1)
            {
                wrist.joint.last.immovable = false;
            }
            if (Time.frameCount - lastFrameTeleport >= 2 && ghosted
                && !Physics.CheckSphere(wrist.joint.last.worldCenterOfMass, 0.1f) // Check that hand isn't being teleported inside of something
                )
            {
                foreach (CapsuleCollider collider in capsuleColliders) collider.enabled = true;
                ghosted = false;
            }
        }
    }
}