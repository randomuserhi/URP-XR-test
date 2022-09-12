using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using UnityEngine;
using VirtualRealityTK;
using static UnityEngine.GraphicsBuffer;

namespace InteractionTK.HandTracking_Deprecated
{
    public static class HandUtils
    {
        public readonly static int Wrist = 0;

        public readonly static int ThumbMetacarpal = 1;
        public readonly static int IndexMetacarpal = 2;
        public readonly static int MiddleMetacarpal = 3;
        public readonly static int RingMetacarpal = 4;
        public readonly static int PinkyMetacarpal = 5;

        public readonly static int ThumbProximal = 6;
        public readonly static int IndexKnuckle = 7;
        public readonly static int MiddleKnuckle = 8;
        public readonly static int PinkyKnuckle = 9;
        public readonly static int RingKnuckle = 10;

        public readonly static int ThumbDistal = 11;
        public readonly static int IndexMiddle = 12;
        public readonly static int MiddleMiddle = 13;
        public readonly static int RingMiddle = 14;
        public readonly static int PinkyMiddle = 15;

        public readonly static int ThumbTip = 16;
        public readonly static int IndexDistal = 17;
        public readonly static int MiddleDistal = 18;
        public readonly static int RingDistal = 19;
        public readonly static int PinkyDistal = 20;

        public readonly static int IndexTip = 21;
        public readonly static int MiddleTip = 22;
        public readonly static int RingTip = 23;
        public readonly static int PinkyTip = 24;

        public readonly static int Palm = 25;

        public static int NumJoints = 26;

        public static int Metacarpals = 1;
        public static int NumMetacarpals = 5;

        public static int Knuckles = 6;
        public static int NumKnuckles = 5;

        public static int Middles = 10;
        public static int NumMiddles = 5;

        public static int Distals = 15;
        public static int NumDistals = 5;

        public static int Tips = 20;
        public static int NumTips = 4;

        public struct Joint
        {
            readonly int joint;
            public Joint(int joint)
            {
                this.joint = joint;
            }

            public readonly static int Wrist = 0;

            public readonly static int ThumbMetacarpal = 1;
            public readonly static int IndexMetacarpal = 2;
            public readonly static int MiddleMetacarpal = 3;
            public readonly static int RingMetacarpal = 4;
            public readonly static int PinkyMetacarpal = 5;

            public readonly static int ThumbProximal = 6;
            public readonly static int IndexKnuckle = 7;
            public readonly static int MiddleKnuckle = 8;
            public readonly static int PinkyKnuckle = 9;
            public readonly static int RingKnuckle = 10;

            public readonly static int ThumbDistal = 11;
            public readonly static int IndexMiddle = 12;
            public readonly static int MiddleMiddle = 13;
            public readonly static int RingMiddle = 14;
            public readonly static int PinkyMiddle = 15;

            public readonly static int ThumbTip = 16;
            public readonly static int IndexDistal = 17;
            public readonly static int MiddleDistal = 18;
            public readonly static int RingDistal = 19;
            public readonly static int PinkyDistal = 20;

            public readonly static int IndexTip = 21;
            public readonly static int MiddleTip = 22;
            public readonly static int RingTip = 23;
            public readonly static int PinkyTip = 24;

            public readonly static int Palm = 25;

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

        public struct JointLimit
        {
            public float maximum;
            public float minimum;
        }

        public struct JointDriveXYZ
        {
            public JointLimit xLimit;
            public JointLimit yLimit;
            public JointLimit zLimit;
            public JointDrive drive;
            public ConfigurableJointMotion xMotion;
            public ConfigurableJointMotion yMotion;
            public ConfigurableJointMotion zMotion;
        }

        public struct JointTransform
        {
            public Vector3 position;
            private Vector3 fromDir;
            public Quaternion rotation(Vector3 scale)
            {
                if (fromDir == Vector3.zero) return Quaternion.identity;
                return Quaternion.FromToRotation(fromDir, Vector3.Scale(position, scale));
            }
            public Quaternion rotation(ITKSkeleton.Type type)
            {
                if (fromDir == Vector3.zero) return Quaternion.identity;
                Vector3 scale = new Vector3(type == ITKSkeleton.Type.Left ? 1 : -1, 1, 1);
                return Quaternion.FromToRotation(fromDir, Vector3.Scale(position, scale));
            }

            public JointTransform(float x, float y, float z)
            {
                position = new Vector3(x, y, z);
                fromDir = Vector3.zero;
            }

            public JointTransform(float x, float y, float z, Vector3 fromDir)
            {
                this.fromDir = fromDir;
                position = new Vector3(x, y, z);
            }
        }

        public class SphericalJoint
        {
            public readonly ConfigurableJoint xJoint;
            public readonly ConfigurableJoint yJoint;
            public readonly ConfigurableJoint zJoint;

            public SphericalJoint(ConfigurableJoint xJoint, ConfigurableJoint yJoint, ConfigurableJoint zJoint)
            {
                this.xJoint = xJoint;
                this.yJoint = yJoint;
                this.zJoint = zJoint;

                xJoint.axis = new Vector3(1, 0, 0);
                yJoint.axis = new Vector3(0, 1, 0);
                zJoint.axis = new Vector3(0, 0, 1);

                xJoint.xMotion = ConfigurableJointMotion.Locked;
                xJoint.yMotion = ConfigurableJointMotion.Locked;
                xJoint.zMotion = ConfigurableJointMotion.Locked;
                yJoint.xMotion = ConfigurableJointMotion.Locked;
                yJoint.yMotion = ConfigurableJointMotion.Locked;
                yJoint.zMotion = ConfigurableJointMotion.Locked;
                zJoint.xMotion = ConfigurableJointMotion.Locked;
                zJoint.yMotion = ConfigurableJointMotion.Locked;
                zJoint.zMotion = ConfigurableJointMotion.Locked;
            }

            public Rigidbody connectedBody
            {
                set
                {
                    xJoint.connectedBody = value;
                    yJoint.connectedBody = value;
                    zJoint.connectedBody = value;
                }
            }
            public bool autoConfigureConnectedAnchor
            {
                set
                {
                    xJoint.autoConfigureConnectedAnchor = value;
                    yJoint.autoConfigureConnectedAnchor = value;
                    zJoint.autoConfigureConnectedAnchor = value;
                }
            }
            public Vector3 connectedAnchor
            {
                set
                {
                    xJoint.connectedAnchor = value;
                    yJoint.connectedAnchor = value;
                    zJoint.connectedAnchor = value;
                }
            }
            public Vector3 anchor
            {
                set
                {
                    xJoint.anchor = value;
                    yJoint.anchor = value;
                    zJoint.anchor = value;
                }
            }
            public ConfigurableJointMotion angularXMotion
            {
                set
                {
                    xJoint.angularXMotion = value;
                }
            }
            public ConfigurableJointMotion angularYMotion
            {
                set
                {
                    yJoint.angularXMotion = value;
                }
            }
            public ConfigurableJointMotion angularZMotion
            {
                set
                {
                    zJoint.angularXMotion = value;
                }
            }
            public JointLimit angularXLimit
            {
                set
                {
                    xJoint.highAngularXLimit = new SoftJointLimit() { limit = -value.minimum };
                    xJoint.lowAngularXLimit = new SoftJointLimit() { limit = -value.maximum };
                }
            }
            public JointLimit angularYLimit
            {
                set
                {
                    yJoint.highAngularXLimit = new SoftJointLimit() { limit = -value.minimum };
                    yJoint.lowAngularXLimit = new SoftJointLimit() { limit = -value.maximum };
                }
            }
            public JointLimit angularZLimit
            {
                set
                {
                    zJoint.highAngularXLimit = new SoftJointLimit() { limit = -value.minimum };
                    zJoint.lowAngularXLimit = new SoftJointLimit() { limit = -value.maximum };
                }
            }
            public JointDrive angularXDrive
            {
                set
                {
                    xJoint.angularXDrive = value;
                }
            }
            public JointDrive angularYDrive
            {
                set
                {
                    yJoint.angularXDrive = value;
                }
            }
            public JointDrive angularZDrive
            {
                set
                {
                    zJoint.angularXDrive = value;
                }
            }

            private Quaternion _targetRotation = Quaternion.identity;
            public Quaternion targetRotation
            {
                set
                {
                    _targetRotation = value;

                    Vector3 axis;
                    float degree;
                    _targetRotation.ToAngleAxis(out degree, out axis);
                    Vector3 rot = axis * degree;
                    xJoint.targetRotation = Quaternion.Euler(new Vector3(rot.x, 0, 0));
                    yJoint.targetRotation = Quaternion.Euler(new Vector3(rot.y, 0, 0));
                    zJoint.targetRotation = Quaternion.Euler(new Vector3(rot.z, 0, 0));
                }
                get
                {
                    return _targetRotation;
                }
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
        }

        public static float positionSpring = 1000;
        public static float positionDamper = 100;
        public static float maxForce = 500;
        public static int[] StructureCount = new int[] { 4, 5, 5, 5, 5 };
        public static Joint[][] Structure = new Joint[][]
        {
            new Joint[] { Joint.ThumbMetacarpal, Joint.ThumbProximal, Joint.ThumbDistal, Joint.ThumbTip },
            new Joint[] { Joint.IndexMetacarpal, Joint.IndexKnuckle, Joint.IndexMiddle, Joint.IndexDistal, Joint.IndexTip },
            new Joint[] { Joint.MiddleMetacarpal, Joint.MiddleKnuckle, Joint.MiddleMiddle, Joint.MiddleDistal, Joint.MiddleTip },
            new Joint[] { Joint.RingMetacarpal, Joint.RingKnuckle, Joint.RingMiddle, Joint.RingDistal, Joint.RingTip },
            new Joint[] { Joint.PinkyMetacarpal, Joint.PinkyKnuckle, Joint.PinkyMiddle, Joint.PinkyDistal, Joint.PinkyTip }
        };
        public static JointDriveXYZ[][] DriveStructure = new JointDriveXYZ[][]
        {
            new JointDriveXYZ[] {
                new JointDriveXYZ() {
                    xMotion = ConfigurableJointMotion.Locked,
                    yMotion = ConfigurableJointMotion.Locked,
                    zMotion = ConfigurableJointMotion.Locked
                },
                new JointDriveXYZ() {
                    drive = new JointDrive() { positionSpring = positionSpring, positionDamper = positionDamper, maximumForce = maxForce },
                    xLimit = new JointLimit() { maximum = 90, minimum = -90 },
                    yLimit = new JointLimit() { maximum = 90, minimum = -90 },
                    zLimit = new JointLimit() { maximum = 90, minimum = -90 },
                    xMotion = ConfigurableJointMotion.Limited,
                    yMotion = ConfigurableJointMotion.Limited,
                    zMotion = ConfigurableJointMotion.Limited
                },
                new JointDriveXYZ() {
                    drive = new JointDrive() { positionSpring = positionSpring, positionDamper = positionDamper, maximumForce = maxForce },
                    xLimit = new JointLimit() { maximum = 50, minimum = -50 },
                    xMotion = ConfigurableJointMotion.Limited,
                    yMotion = ConfigurableJointMotion.Locked,
                    zMotion = ConfigurableJointMotion.Locked
                },
                new JointDriveXYZ() {
                    drive = new JointDrive() { positionSpring = positionSpring, positionDamper = positionDamper, maximumForce = maxForce },
                    xLimit = new JointLimit() { minimum = -10, maximum = 90 },
                    xMotion = ConfigurableJointMotion.Limited,
                    yMotion = ConfigurableJointMotion.Locked,
                    zMotion = ConfigurableJointMotion.Locked
                }
            },
            new JointDriveXYZ[] {
                new JointDriveXYZ() {
                    xMotion = ConfigurableJointMotion.Locked,
                    yMotion = ConfigurableJointMotion.Locked,
                    zMotion = ConfigurableJointMotion.Locked
                },
                new JointDriveXYZ() {
                    drive = new JointDrive() { positionSpring = positionSpring, positionDamper = positionDamper, maximumForce = maxForce },
                    xLimit = new JointLimit() { minimum = 0, maximum = 0 },
                    xMotion = ConfigurableJointMotion.Limited,
                    yMotion = ConfigurableJointMotion.Locked,
                    zMotion = ConfigurableJointMotion.Locked
                },
                new JointDriveXYZ() {
                    drive = new JointDrive() { positionSpring = positionSpring, positionDamper = positionDamper, maximumForce = maxForce },
                    xLimit = new JointLimit() { minimum = -50, maximum = 90 },
                    yLimit = new JointLimit() { minimum = -50, maximum = 50 },
                    xMotion = ConfigurableJointMotion.Limited,
                    yMotion = ConfigurableJointMotion.Limited,
                    zMotion = ConfigurableJointMotion.Locked
                },
                new JointDriveXYZ() {
                    drive = new JointDrive() { positionSpring = positionSpring, positionDamper = positionDamper, maximumForce = maxForce },
                    xLimit = new JointLimit() { minimum = 0, maximum = 90 },
                    xMotion = ConfigurableJointMotion.Limited,
                    yMotion = ConfigurableJointMotion.Locked,
                    zMotion = ConfigurableJointMotion.Locked
                },
                new JointDriveXYZ() {
                    drive = new JointDrive() { positionSpring = positionSpring, positionDamper = positionDamper, maximumForce = maxForce },
                     xLimit = new JointLimit() { minimum = -10, maximum = 90 },
                    xMotion = ConfigurableJointMotion.Limited,
                    yMotion = ConfigurableJointMotion.Locked,
                    zMotion = ConfigurableJointMotion.Locked
                }
            },
            new JointDriveXYZ[] {
                new JointDriveXYZ() {
                    xMotion = ConfigurableJointMotion.Locked,
                    yMotion = ConfigurableJointMotion.Locked,
                    zMotion = ConfigurableJointMotion.Locked
                },
                new JointDriveXYZ() {
                    drive = new JointDrive() { positionSpring = positionSpring, positionDamper = positionDamper, maximumForce = maxForce },
                    xLimit = new JointLimit() { minimum = 0, maximum = 0 },
                    xMotion = ConfigurableJointMotion.Limited,
                    yMotion = ConfigurableJointMotion.Locked,
                    zMotion = ConfigurableJointMotion.Locked
                },
                new JointDriveXYZ() {
                    drive = new JointDrive() { positionSpring = positionSpring, positionDamper = positionDamper, maximumForce = maxForce },
                    xLimit = new JointLimit() { minimum = -50, maximum = 90 },
                    yLimit = new JointLimit() { minimum = -50, maximum = 50 },
                    xMotion = ConfigurableJointMotion.Limited,
                    yMotion = ConfigurableJointMotion.Limited,
                    zMotion = ConfigurableJointMotion.Locked
                },
                new JointDriveXYZ() {
                    drive = new JointDrive() { positionSpring = positionSpring, positionDamper = positionDamper, maximumForce = maxForce },
                    xLimit = new JointLimit() { minimum = 0, maximum = 90 },
                    xMotion = ConfigurableJointMotion.Limited,
                    yMotion = ConfigurableJointMotion.Locked,
                    zMotion = ConfigurableJointMotion.Locked
                },
                new JointDriveXYZ() {
                    drive = new JointDrive() { positionSpring = positionSpring, positionDamper = positionDamper, maximumForce = maxForce },
                     xLimit = new JointLimit() { minimum = -10, maximum = 90 },
                    xMotion = ConfigurableJointMotion.Limited,
                    yMotion = ConfigurableJointMotion.Locked,
                    zMotion = ConfigurableJointMotion.Locked
                }
            },
            new JointDriveXYZ[] {
                new JointDriveXYZ() {
                    xMotion = ConfigurableJointMotion.Locked,
                    yMotion = ConfigurableJointMotion.Locked,
                    zMotion = ConfigurableJointMotion.Locked
                },
                new JointDriveXYZ() {
                    drive = new JointDrive() { positionSpring = positionSpring, positionDamper = positionDamper, maximumForce = maxForce },
                    xLimit = new JointLimit() { minimum = 0, maximum = 0 },
                    xMotion = ConfigurableJointMotion.Limited,
                    yMotion = ConfigurableJointMotion.Locked,
                    zMotion = ConfigurableJointMotion.Locked
                },
                new JointDriveXYZ() {
                    drive = new JointDrive() { positionSpring = positionSpring, positionDamper = positionDamper, maximumForce = maxForce },
                    xLimit = new JointLimit() { minimum = -50, maximum = 90 },
                    yLimit = new JointLimit() { minimum = -50, maximum = 50 },
                    xMotion = ConfigurableJointMotion.Limited,
                    yMotion = ConfigurableJointMotion.Limited,
                    zMotion = ConfigurableJointMotion.Locked
                },
                new JointDriveXYZ() {
                    drive = new JointDrive() { positionSpring = positionSpring, positionDamper = positionDamper, maximumForce = maxForce },
                    xLimit = new JointLimit() { minimum = 0, maximum = 90 },
                    xMotion = ConfigurableJointMotion.Limited,
                    yMotion = ConfigurableJointMotion.Locked,
                    zMotion = ConfigurableJointMotion.Locked
                },
                new JointDriveXYZ() {
                    drive = new JointDrive() { positionSpring = positionSpring, positionDamper = positionDamper, maximumForce = maxForce },
                     xLimit = new JointLimit() { minimum = -10, maximum = 90 },
                    xMotion = ConfigurableJointMotion.Limited,
                    yMotion = ConfigurableJointMotion.Locked,
                    zMotion = ConfigurableJointMotion.Locked
                }
            },
            new JointDriveXYZ[] {
                new JointDriveXYZ() {
                    xMotion = ConfigurableJointMotion.Locked,
                    yMotion = ConfigurableJointMotion.Locked,
                    zMotion = ConfigurableJointMotion.Locked
                },
                new JointDriveXYZ() {
                    drive = new JointDrive() { positionSpring = positionSpring, positionDamper = positionDamper, maximumForce = maxForce },
                    xLimit = new JointLimit() { minimum = 0, maximum = 0 },
                    xMotion = ConfigurableJointMotion.Limited,
                    yMotion = ConfigurableJointMotion.Locked,
                    zMotion = ConfigurableJointMotion.Locked
                },
                new JointDriveXYZ() {
                    drive = new JointDrive() { positionSpring = positionSpring, positionDamper = positionDamper, maximumForce = maxForce },
                    xLimit = new JointLimit() { minimum = -50, maximum = 90 },
                    yLimit = new JointLimit() { minimum = -50, maximum = 50 },
                    xMotion = ConfigurableJointMotion.Limited,
                    yMotion = ConfigurableJointMotion.Limited,
                    zMotion = ConfigurableJointMotion.Locked
                },
                new JointDriveXYZ() {
                    drive = new JointDrive() { positionSpring = positionSpring, positionDamper = positionDamper, maximumForce = maxForce },
                    xLimit = new JointLimit() { minimum = 0, maximum = 90 },
                    xMotion = ConfigurableJointMotion.Limited,
                    yMotion = ConfigurableJointMotion.Locked,
                    zMotion = ConfigurableJointMotion.Locked
                },
                new JointDriveXYZ() {
                    drive = new JointDrive() { positionSpring = positionSpring, positionDamper = positionDamper, maximumForce = maxForce },
                    xLimit = new JointLimit() { minimum = -10, maximum = 90 },
                    xMotion = ConfigurableJointMotion.Limited,
                    yMotion = ConfigurableJointMotion.Locked,
                    zMotion = ConfigurableJointMotion.Locked
                }
            }
        };
        public static JointTransform[][] LocalTransformStructure = new JointTransform[][]
        {
            new JointTransform[] {
                new JointTransform(0.02f, 0, 0.013f, Vector3.forward),
                new JointTransform(0, 0, 0.045f), new JointTransform(0, 0, 0.03f), new JointTransform(0, 0, 0.02f)
            },
            new JointTransform[] {
                new JointTransform(0.01f, 0, 0.015f),
                new JointTransform(0.01f, 0, 0.06f, Vector3.forward),
                new JointTransform(0, 0, 0.045f), new JointTransform(0, 0, 0.025f), new JointTransform(0, 0, 0.015f)
            },
            new JointTransform[] {
                new JointTransform(0, 0, 0.015f),
                new JointTransform(0, 0, 0.065f),
                new JointTransform(0, 0, 0.05f), new JointTransform(0, 0, 0.025f), new JointTransform(0, 0, 0.015f)
            },
            new JointTransform[] {
                new JointTransform(-0.005f, 0, 0.015f),
                new JointTransform(-0.01f, 0, 0.06f, Vector3.forward),
                new JointTransform(0, 0, 0.045f), new JointTransform(0, 0, 0.025f), new JointTransform(0, 0, 0.015f)
            },
            new JointTransform[] {
                new JointTransform(-0.015f, 0, 0.013f),
                new JointTransform(-0.015f, 0, 0.05f, Vector3.forward),
                new JointTransform(0, 0, 0.035f), new JointTransform(0, 0, 0.02f), new JointTransform(0, 0, 0.015f)
            }
        };
        public static ColliderJoint[][] ColliderStructure = new ColliderJoint[][]
        {
            new ColliderJoint[] {
                new ColliderJoint() { height = 0.017f, radius = 0.005f },
                new ColliderJoint() { height = 0.042f, radius = 0.005f },
                new ColliderJoint() { height = 0.027f, radius = 0.005f },
                new ColliderJoint() { height = 0.017f, radius = 0.005f }
            },
            new ColliderJoint[] {
                new ColliderJoint() { height = 0.007f, radius = 0.005f },
                new ColliderJoint() { height = 0.057f, radius = 0.005f },
                new ColliderJoint() { height = 0.042f, radius = 0.005f },
                new ColliderJoint() { height = 0.022f, radius = 0.005f },
                new ColliderJoint() { height = 0.012f, radius = 0.005f }
            },
            new ColliderJoint[] {
                new ColliderJoint() { height = 0.012f, radius = 0.005f },
                new ColliderJoint() { height = 0.062f, radius = 0.005f },
                new ColliderJoint() { height = 0.047f, radius = 0.005f },
                new ColliderJoint() { height = 0.022f, radius = 0.005f },
                new ColliderJoint() { height = 0.012f, radius = 0.005f }
            },
            new ColliderJoint[] {
                new ColliderJoint() { height = 0.007f, radius = 0.005f },
                new ColliderJoint() { height = 0.057f, radius = 0.005f },
                new ColliderJoint() { height = 0.042f, radius = 0.005f },
                new ColliderJoint() { height = 0.022f, radius = 0.005f },
                new ColliderJoint() { height = 0.012f, radius = 0.005f }
            },
            new ColliderJoint[] {
                new ColliderJoint() { height = 0.017f, radius = 0.005f },
                new ColliderJoint() { height = 0.047f, radius = 0.005f },
                new ColliderJoint() { height = 0.032f, radius = 0.005f },
                new ColliderJoint() { height = 0.017f, radius = 0.005f },
                new ColliderJoint() { height = 0.013f, radius = 0.005f }
            }
        };
    }

    public class ITKSkeleton
    {
        public enum Type
        {
            Left,
            Right
        }

        public ITKSkeleton[] children;
        public ITKSkeleton[][] tree;
        public Rigidbody body;
        public HandUtils.SphericalJoint joint;

        public CapsuleCollider[] colliders;
        public Rigidbody[] bodies;
        public HandUtils.SphericalJoint[] joints;

        private static GameObject CreateBody(Transform parent, out Rigidbody rb)
        {
            GameObject o = new GameObject();
            o.transform.parent = parent;
            o.transform.localPosition = Vector3.zero;
            rb = o.AddComponent<Rigidbody>();

            return o;
        }
        private static GameObject CreateBody(Transform parent, out Rigidbody rb, out HandUtils.SphericalJoint joint)
        {
            GameObject o = CreateBody(parent, out rb);
            joint = new HandUtils.SphericalJoint(o.AddComponent<ConfigurableJoint>(), o.AddComponent<ConfigurableJoint>(), o.AddComponent<ConfigurableJoint>());

            return o;
        }
        private static GameObject CreateBody(Transform parent, HandUtils.ColliderJoint j, out Rigidbody rb, out HandUtils.SphericalJoint joint, out CapsuleCollider collider)
        {
            GameObject o = CreateBody(parent, out rb, out joint);
            collider = o.AddComponent<CapsuleCollider>();
            collider.radius = j.radius;
            collider.height = j.height;
            collider.direction = 2;
            collider.center = new Vector3(0, 0, -j.height / 2f);

            return o;
        }

        public static ITKSkeleton HandSkeleton(Transform parent, Type type, out Rigidbody[] bodies, out HandUtils.SphericalJoint[] joints, out CapsuleCollider[] capsuleColliders)
        {
            Vector3 scale = new Vector3(type == Type.Left ? 1 : -1, 1, 1);

            List<CapsuleCollider> colliders = new List<CapsuleCollider>();
            List<Rigidbody> rigidbodies = new List<Rigidbody>();
            List<HandUtils.SphericalJoint> configurableJoints = new List<HandUtils.SphericalJoint>();

            ITKSkeleton wrist = new ITKSkeleton();
            CreateBody(parent, out wrist.body);
            wrist.body.name = "Wrist";
            wrist.body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            wrist.body.maxAngularVelocity = 0.075f * Mathf.PI / Time.fixedDeltaTime;
            wrist.body.solverIterations = 60;
            wrist.body.solverVelocityIterations = 20;
            wrist.body.mass = 1000;
            wrist.body.angularDrag = 10;

            wrist.children = new ITKSkeleton[HandUtils.StructureCount.Length];
            wrist.children.InitializeArray();

            wrist.tree = new ITKSkeleton[HandUtils.StructureCount.Length][];

            for (int i = 0; i < HandUtils.StructureCount.Length; ++i)
            {
                wrist.tree[i] = new ITKSkeleton[HandUtils.StructureCount[i]];

                Rigidbody b = wrist.body;
                Transform p = wrist.body.transform;
                ITKSkeleton node = wrist.children[i];
                float mass = 50;
                for (int j = 0; j < HandUtils.StructureCount[i]; ++j, mass /= 2f)
                {
                    CapsuleCollider collider;
                    CreateBody(p, HandUtils.ColliderStructure[i][j], out node.body, out node.joint, out collider);
                    colliders.Add(collider);
                    rigidbodies.Add(node.body);
                    configurableJoints.Add(node.joint);

                    node.body.name = HandUtils.Structure[i][j].ToString();
                    node.body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                    node.body.solverIterations = 60;
                    node.body.solverVelocityIterations = 20;
                    node.body.mass = mass;
                    node.body.angularDrag = 10;

                    node.body.transform.localPosition = Vector3.Scale(HandUtils.LocalTransformStructure[i][j].position, scale);
                    node.body.transform.localRotation = HandUtils.LocalTransformStructure[i][j].rotation(scale);

                    node.joint.connectedBody = b;
                    node.joint.autoConfigureConnectedAnchor = false;
                    node.joint.connectedAnchor = Vector3.zero;
                    node.joint.anchor = node.body.transform.InverseTransformPoint(p.transform.position);

                    HandUtils.JointDriveXYZ settings = HandUtils.DriveStructure[i][j];

                    node.joint.angularXMotion = settings.xMotion;
                    node.joint.angularYMotion = settings.yMotion;
                    node.joint.angularZMotion = settings.zMotion;

                    node.joint.angularXDrive = settings.drive;
                    node.joint.angularYDrive = settings.drive;
                    node.joint.angularZDrive = settings.drive;

                    node.joint.angularXLimit = settings.xLimit;
                    node.joint.angularYLimit = settings.yLimit;
                    node.joint.angularZLimit = settings.zLimit;

                    b = node.body;
                    p = node.body.transform;
                    wrist.tree[i][j] = node;
                    node.children = new ITKSkeleton[1];
                    node.children.InitializeArray();
                    node = node.children[0];
                }
            }

            for (int i = 0; i < colliders.Count; ++i)
                for (int j = 0; j < colliders.Count; ++j)
                    if (i != j) Physics.IgnoreCollision(colliders[i], colliders[j], true);

            capsuleColliders = colliders.ToArray();
            joints = configurableJoints.ToArray();
            bodies = rigidbodies.ToArray();
            return wrist;
        }
    }

    public class ITKHand_Deprecated : MonoBehaviour
    {
        ITKSkeleton wrist;
        public ITKSkeleton.Type type;
        public Vector3 scale
        {
            get { return new Vector3(type == ITKSkeleton.Type.Left ? 1 : -1, 1, 1); }
        }

        public GameObject debug;
        public GameObject pointer1;
        public GameObject pointer2;

        private void Start()
        {
            GenerateBody();
        }

        private void GenerateBody()
        {
            wrist = ITKSkeleton.HandSkeleton(transform, type, out bodies, out joints, out capsuleColliders);
        }

        private CapsuleCollider[] capsuleColliders;
        private HandUtils.SphericalJoint[] joints;
        private Rigidbody[] bodies;
        public int unstable = 0;
        public void Track(HandUtils.Pose pose)
        {
            pose.positions[HandUtils.Wrist] = debug.transform.position;
            pose.rotations[HandUtils.Wrist] = debug.transform.rotation;

            // Counteract gravity of children joints
            foreach (Rigidbody body in bodies)
            {
                body.AddForce(-Physics.gravity * body.mass);
            }

            // TODO smoothe out movement
            wrist.body.velocity *= 0.05f;
            Vector3 velocity = (pose.positions[HandUtils.Wrist] - wrist.body.position) * 0.95f / Time.fixedDeltaTime;
            wrist.body.velocity += velocity;

            // TODO smoothe out rotation
            Quaternion rotation = pose.rotations[HandUtils.Wrist] * Quaternion.Inverse(wrist.body.rotation);
            Vector3 rot;
            float speed;
            rotation.ToAngleAxis(out speed, out rot);
            wrist.body.angularVelocity = rot * speed * Mathf.Deg2Rad / Time.fixedDeltaTime;

            // Rotate joints
            for (int i = 0; i < HandUtils.StructureCount.Length; ++i)
            {
                for (int j = 1; j < HandUtils.StructureCount[i] - 1; ++j)
                {
                    HandUtils.Joint currJoint = HandUtils.Structure[i][j];
                    HandUtils.Joint nextJoint = HandUtils.Structure[i][j + 1];

                    Vector3 current = Quaternion.Inverse(pose.rotations[HandUtils.Wrist]) * (pose.positions[currJoint] - pose.positions[HandUtils.Wrist]);//pose.rotations[HandUtils.Wrist] * (pose.positions[currJoint] - pose.positions[HandUtils.Wrist]);
                    Vector3 next = Quaternion.Inverse(pose.rotations[HandUtils.Wrist]) * (pose.positions[nextJoint] - pose.positions[HandUtils.Wrist]);//pose.rotations[HandUtils.Wrist] * (pose.positions[nextJoint] - pose.positions[HandUtils.Wrist]);

                    Debug.DrawLine(next, current);
                    Debug.DrawRay(current, wrist.body.rotation * HandUtils.LocalTransformStructure[i][i == 0 ? 0 : 1].position, Color.red);

                    Quaternion target = Quaternion.FromToRotation(next - current, wrist.body.rotation * HandUtils.LocalTransformStructure[i][i == 0 ? 0 : 1].position);
                    wrist.tree[i][j].joint.targetRotation = target;
                }
            }

            // Teleport hand if it goes unstable
            if (Vector3.Distance(wrist.body.transform.position, pose.positions[HandUtils.Wrist]) > 0.2f) // TODO:: make a better mechanism for detecting unstable hand
            {
                unstable = 10;
                // TODO:: make this not be a frame count and instead keep correcting
                // until all joints are stable (give each joint a stable rate score based on their change in position each frame)
                // such that it only keeps hands gone until they are stable as opposed to a fixed 10 frames
            }
            if (unstable > 0)
            {
                wrist.body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                wrist.body.isKinematic = true;
                wrist.body.position = pose.positions[HandUtils.Wrist];
                wrist.body.rotation = pose.rotations[HandUtils.Wrist];
                wrist.body.velocity = Vector3.zero;
                wrist.body.angularVelocity = Vector3.zero;
                foreach (CapsuleCollider collider in capsuleColliders) collider.enabled = false;
                for (int i = 0; i < bodies.Length; i++)
                {
                    bodies[i].velocity = Vector3.zero;
                    bodies[i].angularVelocity = Vector3.zero;
                }
                --unstable;
            }
            else
            {
                wrist.body.isKinematic = false;
                wrist.body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                foreach (CapsuleCollider collider in capsuleColliders) collider.enabled = true;
            }
        }
    }
}