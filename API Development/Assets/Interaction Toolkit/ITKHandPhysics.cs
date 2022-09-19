using InteractionTK.HandTracking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using UnityEngine;
using VirtualRealityTK;
using static OVRPlugin;

namespace InteractionTK.HandTracking
{
    public static partial class ITKHand
    {
        // TODO:: add a far max velocity where if the hand is far away it can move faster
        // (maybe increase maxforce as well, may need a far max force variable then??)
        //
        // TODO:: a lot of features use a "scale" variable with handedness, add this to ITKHandUtils and refactor
        public struct HandSettings
        {
            public Handedness defaultHandedness;
            public Vector3 handednessScale;

            public int solverIterations;
            public int solverVelocityIterations;

            public bool safeMode;

            public float maxVelocity;
            public float maxAngularVelocity;
            public float maxDepenetrationVelocity;

            public float maxError;
        }

        // TODO:: support mesh and compound colliders
        public struct SkeletonDescription
        {
            public static readonly JointDrive rootRotationDrive = new JointDrive()
            {
                positionSpring = 1e+20f,
                positionDamper = 1e+18f,
                maximumForce = 5f
            };
            public static readonly JointDrive rootPositionDrive = new JointDrive()
            {
                positionSpring = 1e+20f,
                positionDamper = 5e+18f,
                maximumForce = 20f
            };

            public struct NodeCollider
            {
                public enum Type
                {
                    None,
                    Capsule,
                    Box,
                    Sphere
                }

                public Type type;
                public Vector3 position;
                public Quaternion rotation;
                public Vector3 size;
                public float radius;
                public float height;
            }

            // TODO:: create a collider struct for the colliders and make it an array
            public struct Node
            {
                public Quaternion rightDefaultRotation; // right hand rotation of the joint upon instantiation - in local space
                public Quaternion leftDefaultRotation; // left hand rotation of the joint upon instantiation - in local space

                public float mass;
                public Vector3 centerOfMass;

                public Joint joint;
                public Joint toJoint; // Used when joint is the root, so there will be no rotation

                public NodeCollider[] colliders;

                public Vector3 anchor;
                public Vector3 connectedAnchor;

                public JointDrive rotationDrive;

                public Node[] children;
            }

            public HandSettings settings;
            public Node nodeTree;
        }

        // TODO:: tweak drives, forces and mass of thumb until thumb stops getting mad pushed when two hand holding a rod
        //        -> Found out that its the mass that determines stability, increase mass of each joint down the line and test the stability
        //        -> instability might be because the join isn't offset from the root => This is *partially the reason*, mass seems to be the main contributor

        // NOTE:: box colliders are used on the fingers instead of capsule colliders due to a known bug on hololens where capsule colliders with joints do not
        //        collider with other capsule colliders properly
        public static SkeletonDescription handSkeletonBox = new SkeletonDescription()
        {
            settings = new HandSettings()
            {
                defaultHandedness = Handedness.Left,
                handednessScale = new Vector3(-1, 1, 1),
                safeMode = true,
                solverIterations = 20,
                solverVelocityIterations = 10,
                maxVelocity = 5,
                maxAngularVelocity = 2,
                maxDepenetrationVelocity = 1,
                maxError = 1f
            },
            nodeTree = new SkeletonDescription.Node()
            {
                // Root node does not need a drive specified
                mass = 0.255f,
                centerOfMass = Vector3.zero,
                joint = Joint.Wrist,
                colliders = new SkeletonDescription.NodeCollider[]
                {
                    new SkeletonDescription.NodeCollider()
                    {
                        type = SkeletonDescription.NodeCollider.Type.Box,
                        position = new Vector3(0, 0.01f, 0f),
                        rotation = Quaternion.identity,
                        size = new Vector3(0.06f, 0.015f, 0.07f)
                    },
                    new SkeletonDescription.NodeCollider()
                    {
                        type = SkeletonDescription.NodeCollider.Type.Box,
                        position = new Vector3(0, -0.01f, -0.02f),
                        rotation = Quaternion.identity,
                        size = new Vector3(0.06f, 0.03f, 0.035f)
                    }
                },
                anchor = new Vector3(0.002f, -0.001f, -0.045f),
                connectedAnchor = Vector3.zero,
                children = new SkeletonDescription.Node[]
                {
                    // THUMB
                    new SkeletonDescription.Node()
                    {
                        leftDefaultRotation = Quaternion.Euler(0, 90, 0),
                        rightDefaultRotation = Quaternion.Euler(0, -90, 0),
                        mass = 0.225f,
                        centerOfMass = Vector3.zero,
                        joint = Joint.Wrist,
                        toJoint = Joint.ThumbMetacarpal,
                        colliders = new SkeletonDescription.NodeCollider[]
                        {
                            new SkeletonDescription.NodeCollider()
                            {
                                type = SkeletonDescription.NodeCollider.Type.Box,
                                position = Vector3.zero,
                                rotation = Quaternion.identity,
                                size = new Vector3(0.018f, 0.018f, 0.01f)
                                //radius = 0.009f,
                                //height = 0.01f,
                            }
                        },
                        anchor = new Vector3(0, 0f, -0.005f),
                        connectedAnchor = new Vector3(0.016f, 0.003f, -0.035f),
                        rotationDrive = new JointDrive()
                        {
                            positionSpring = 10f,
                            positionDamper = 0.1f,
                            maximumForce = 20f
                        },
                        children = new SkeletonDescription.Node[]
                        {
                            new SkeletonDescription.Node()
                            {
                                mass = 0.225f,
                                centerOfMass = Vector3.zero,
                                joint = Joint.ThumbMetacarpal,
                                colliders = new SkeletonDescription.NodeCollider[]
                                {
                                    new SkeletonDescription.NodeCollider()
                                    {
                                        type = SkeletonDescription.NodeCollider.Type.Box,
                                        position = Vector3.zero,
                                        rotation = Quaternion.identity,
                                        size = new Vector3(0.018f, 0.018f, 0.05f)
                                        //radius = 0.009f,
                                        //height = 0.05f,
                                    }
                                },
                                anchor = new Vector3(0, 0f, -0.025f),
                                connectedAnchor = new Vector3(0f, 0f, 0.005f),
                                rotationDrive = new JointDrive()
                                {
                                    positionSpring = 10f,
                                    positionDamper = 0.1f,
                                    maximumForce = 20f
                                },
                                children = new SkeletonDescription.Node[]
                                {
                                    new SkeletonDescription.Node()
                                    {
                                        mass = 0.03f,
                                        centerOfMass = Vector3.zero,
                                        joint = Joint.ThumbProximal,
                                        colliders = new SkeletonDescription.NodeCollider[]
                                        {
                                            new SkeletonDescription.NodeCollider()
                                            {
                                                type = SkeletonDescription.NodeCollider.Type.Box,
                                                position = Vector3.zero,
                                                rotation = Quaternion.identity,
                                                size = new Vector3(0.018f, 0.018f, 0.045f)
                                                //radius = 0.009f,
                                                //height = 0.045f,
                                            }
                                        },
                                        anchor = new Vector3(0, 0f, -0.022f),
                                        connectedAnchor = new Vector3(0f, 0f, 0.02f),
                                        rotationDrive = new JointDrive()
                                        {
                                            positionSpring = 10f,
                                            positionDamper = 0.1f,
                                            maximumForce = 20f
                                        },
                                        children = new SkeletonDescription.Node[]
                                        {
                                            new SkeletonDescription.Node()
                                            {
                                                mass = 0.03f,
                                                centerOfMass = Vector3.zero,
                                                joint = Joint.ThumbDistal,
                                                colliders = new SkeletonDescription.NodeCollider[]
                                                {
                                                    new SkeletonDescription.NodeCollider()
                                                    {
                                                        type = SkeletonDescription.NodeCollider.Type.Box,
                                                        position = Vector3.zero,
                                                        rotation = Quaternion.identity,
                                                        size = new Vector3(0.016f, 0.016f, 0.035f)
                                                        //radius = 0.008f,
                                                        //height = 0.035f,
                                                    }
                                                },
                                                anchor = new Vector3(0, 0f, -0.0075f),
                                                connectedAnchor = new Vector3(0f, 0f, 0.02f),
                                                rotationDrive = new JointDrive()
                                                {
                                                    positionSpring = 10f,
                                                    positionDamper = 0.1f,
                                                    maximumForce = 20f
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    },
                    // INDEX
                    new SkeletonDescription.Node()
                    {
                        mass = 0.03f,
                        centerOfMass = Vector3.zero,
                        joint = Joint.IndexKnuckle,
                        colliders = new SkeletonDescription.NodeCollider[]
                        {
                            new SkeletonDescription.NodeCollider()
                            {
                                type = SkeletonDescription.NodeCollider.Type.Box,
                                position = Vector3.zero,
                                rotation = Quaternion.identity,
                                size = new Vector3(0.018f, 0.018f, 0.055f)
                                //radius = 0.009f,
                                //height = 0.055f,
                            }
                        },
                        anchor = new Vector3(0, 0f, -0.0275f),
                        connectedAnchor = new Vector3(0.022f, 0.014f, 0.025f),
                        rotationDrive = new JointDrive()
                        {
                            positionSpring = 10f,
                            positionDamper = 0.1f,
                            maximumForce = 3f
                        },
                        children = new SkeletonDescription.Node[]
                        {
                            new SkeletonDescription.Node()
                            {
                                mass = 0.03f,
                                centerOfMass = Vector3.zero,
                                joint = Joint.IndexMiddle,
                                colliders = new SkeletonDescription.NodeCollider[]
                                {
                                    new SkeletonDescription.NodeCollider()
                                    {
                                        type = SkeletonDescription.NodeCollider.Type.Box,
                                        position = Vector3.zero,
                                        rotation = Quaternion.identity,
                                        size = new Vector3(0.016f, 0.016f, 0.04f)
                                        //radius = 0.008f,
                                        //height = 0.04f,
                                    }
                                },
                                anchor = new Vector3(0, 0f, -0.012f),
                                connectedAnchor = new Vector3(0f, 0f, 0.0275f),
                                rotationDrive = new JointDrive()
                                {
                                    positionSpring = 10f,
                                    positionDamper = 0.1f,
                                    maximumForce = 3f
                                },
                                children = new SkeletonDescription.Node[]
                                {
                                    new SkeletonDescription.Node()
                                    {
                                        mass = 0.03f,
                                        centerOfMass = Vector3.zero,
                                        joint = Joint.IndexDistal,
                                        colliders = new SkeletonDescription.NodeCollider[]
                                        {
                                            new SkeletonDescription.NodeCollider()
                                            {
                                                type = SkeletonDescription.NodeCollider.Type.Box,
                                                position = Vector3.zero,
                                                rotation = Quaternion.identity,
                                                size = new Vector3(0.015f, 0.015f, 0.03f)
                                                //radius = 0.0075f,
                                                //height = 0.03f,
                                            }
                                        },
                                        anchor = new Vector3(0, 0f, -0.015f),
                                        connectedAnchor = new Vector3(0f, 0f, 0.012f),
                                        rotationDrive = new JointDrive()
                                        {
                                            positionSpring = 10f,
                                            positionDamper = 0.1f,
                                            maximumForce = 3f
                                        }
                                    }
                                }
                            }
                        }
                    },
                    // MIDDLE
                    new SkeletonDescription.Node()
                    {
                        mass = 0.03f,
                        centerOfMass = Vector3.zero,
                        joint = Joint.MiddleKnuckle,
                        colliders = new SkeletonDescription.NodeCollider[]
                        {
                            new SkeletonDescription.NodeCollider()
                            {
                                type = SkeletonDescription.NodeCollider.Type.Box,
                                position = Vector3.zero,
                                rotation = Quaternion.identity,
                                size = new Vector3(0.018f, 0.018f, 0.06f)
                                //radius = 0.009f,
                                //height = 0.06f,
                            }
                        },
                        anchor = new Vector3(0, 0f, -0.03f),
                        connectedAnchor = new Vector3(0f, 0.014f, 0.025f),
                        rotationDrive = new JointDrive()
                        {
                            positionSpring = 10f,
                            positionDamper = 0.1f,
                            maximumForce = 3f
                        },
                        children = new SkeletonDescription.Node[]
                        {
                            new SkeletonDescription.Node()
                            {
                                mass = 0.03f,
                                centerOfMass = Vector3.zero,
                                joint = Joint.MiddleMiddle,
                                colliders = new SkeletonDescription.NodeCollider[]
                                {
                                    new SkeletonDescription.NodeCollider()
                                    {
                                        type = SkeletonDescription.NodeCollider.Type.Box,
                                        position = Vector3.zero,
                                        rotation = Quaternion.identity,
                                        size = new Vector3(0.016f, 0.016f, 0.04f)
                                        //radius = 0.008f,
                                        //height = 0.04f,
                                    }
                                },
                                anchor = new Vector3(0, 0f, -0.012f),
                                connectedAnchor = new Vector3(0f, 0f, 0.03f),
                                rotationDrive = new JointDrive()
                                {
                                    positionSpring = 10f,
                                    positionDamper = 0.1f,
                                    maximumForce = 3f
                                },
                                children = new SkeletonDescription.Node[]
                                {
                                    new SkeletonDescription.Node()
                                    {
                                        mass = 0.03f,
                                        centerOfMass = Vector3.zero,
                                        joint = Joint.MiddleDistal,
                                        colliders = new SkeletonDescription.NodeCollider[]
                                        {
                                            new SkeletonDescription.NodeCollider()
                                            {
                                                type = SkeletonDescription.NodeCollider.Type.Box,
                                                position = Vector3.zero,
                                                rotation = Quaternion.identity,
                                                size = new Vector3(0.015f, 0.015f, 0.03f)
                                                //radius = 0.0075f,
                                                //height = 0.03f,
                                            }
                                        },
                                        anchor = new Vector3(0, 0f, -0.015f),
                                        connectedAnchor = new Vector3(0f, 0f, 0.012f),
                                        rotationDrive = new JointDrive()
                                        {
                                            positionSpring = 10f,
                                            positionDamper = 0.1f,
                                            maximumForce = 3f
                                        }
                                    }
                                }
                            }
                        }
                    },
                    // RING
                    new SkeletonDescription.Node()
                    {
                        mass = 0.03f,
                        centerOfMass = Vector3.zero,
                        joint = Joint.RingKnuckle,
                        colliders = new SkeletonDescription.NodeCollider[]
                        {
                            new SkeletonDescription.NodeCollider()
                            {
                                type = SkeletonDescription.NodeCollider.Type.Box,
                                position = Vector3.zero,
                                rotation = Quaternion.identity,
                                size = new Vector3(0.018f, 0.018f, 0.055f)
                                //radius = 0.009f,
                                //height = 0.055f,
                            }
                        },
                        anchor = new Vector3(0, 0f, -0.0275f),
                        connectedAnchor = new Vector3(-0.022f, 0.014f, 0.025f),
                        rotationDrive = new JointDrive()
                        {
                            positionSpring = 10f,
                            positionDamper = 0.1f,
                            maximumForce = 3f
                        },
                        children = new SkeletonDescription.Node[]
                        {
                            new SkeletonDescription.Node()
                            {
                                mass = 0.03f,
                                centerOfMass = Vector3.zero,
                                joint = Joint.RingMiddle,
                                colliders = new SkeletonDescription.NodeCollider[]
                                {
                                    new SkeletonDescription.NodeCollider()
                                    {
                                        type = SkeletonDescription.NodeCollider.Type.Box,
                                        position = Vector3.zero,
                                        rotation = Quaternion.identity,
                                        size = new Vector3(0.016f, 0.016f, 0.04f)
                                        //radius = 0.008f,
                                        //height = 0.04f,
                                    }
                                },
                                anchor = new Vector3(0, 0f, -0.012f),
                                connectedAnchor = new Vector3(0f, 0f, 0.0275f),
                                rotationDrive = new JointDrive()
                                {
                                    positionSpring = 10f,
                                    positionDamper = 0.1f,
                                    maximumForce = 3f
                                },
                                children = new SkeletonDescription.Node[]
                                {
                                    new SkeletonDescription.Node()
                                    {
                                        mass = 0.03f,
                                        centerOfMass = Vector3.zero,
                                        joint = Joint.RingDistal,
                                        colliders = new SkeletonDescription.NodeCollider[]
                                        {
                                            new SkeletonDescription.NodeCollider()
                                            {
                                                type = SkeletonDescription.NodeCollider.Type.Box,
                                                position = Vector3.zero,
                                                rotation = Quaternion.identity,
                                                size = new Vector3(0.015f, 0.015f, 0.03f)
                                                //radius = 0.0075f,
                                                //height = 0.03f,
                                            }
                                        },
                                        anchor = new Vector3(0, 0f, -0.015f),
                                        connectedAnchor = new Vector3(0f, 0f, 0.012f),
                                        rotationDrive = new JointDrive()
                                        {
                                            positionSpring = 10f,
                                            positionDamper = 0.1f,
                                            maximumForce = 3f
                                        }
                                    }
                                }
                            }
                        }
                    },
                    // PINKY
                    new SkeletonDescription.Node()
                    {
                        mass = 0.03f,
                        centerOfMass = Vector3.zero,
                        joint = Joint.PinkyMetacarpal,
                        colliders = new SkeletonDescription.NodeCollider[]
                        {
                            new SkeletonDescription.NodeCollider()
                            {
                                type = SkeletonDescription.NodeCollider.Type.Box,
                                position = Vector3.zero,
                                rotation = Quaternion.identity,
                                size = new Vector3(0.016f, 0.016f, 0.06f)
                                //radius = 0.008f,
                                //height = 0.06f,
                            }
                        },
                        anchor = new Vector3(0f, 0f, -0.03f),
                        connectedAnchor = new Vector3(-0.02f, 0.014f, -0.035f),
                        rotationDrive = new JointDrive()
                        {
                            positionSpring = 10f,
                            positionDamper = 0.1f,
                            maximumForce = 20f
                        },
                        children = new SkeletonDescription.Node[]
                        {
                            new SkeletonDescription.Node()
                            {
                                mass = 0.03f,
                                centerOfMass = Vector3.zero,
                                joint = Joint.PinkyKnuckle,
                                colliders = new SkeletonDescription.NodeCollider[]
                                {
                                    new SkeletonDescription.NodeCollider()
                                    {
                                        type = SkeletonDescription.NodeCollider.Type.Box,
                                        position = Vector3.zero,
                                        rotation = Quaternion.identity,
                                        size = new Vector3(0.014f, 0.014f, 0.05f)
                                        //radius = 0.007f,
                                        //height = 0.05f,
                                    }
                                },
                                anchor = new Vector3(0, 0f, -0.025f),
                                connectedAnchor = new Vector3(0f, 0f, 0.02f),
                                rotationDrive = new JointDrive()
                                {
                                    positionSpring = 10f,
                                    positionDamper = 0.1f,
                                    maximumForce = 20f
                                },
                                children = new SkeletonDescription.Node[]
                                {
                                    new SkeletonDescription.Node()
                                    {
                                        mass = 0.03f,
                                        centerOfMass = Vector3.zero,
                                        joint = Joint.PinkyMiddle,
                                        colliders = new SkeletonDescription.NodeCollider[]
                                        {
                                            new SkeletonDescription.NodeCollider()
                                            {
                                                type = SkeletonDescription.NodeCollider.Type.Box,
                                                position = Vector3.zero,
                                                rotation = Quaternion.identity,
                                                size = new Vector3(0.014f, 0.014f, 0.03f)
                                                //radius = 0.007f,
                                                //height = 0.03f,
                                            }
                                        },
                                        anchor = new Vector3(0, 0f, -0.015f),
                                        connectedAnchor = new Vector3(0f, 0f, 0.018f),
                                        rotationDrive = new JointDrive()
                                        {
                                            positionSpring = 10f,
                                            positionDamper = 0.1f,
                                            maximumForce = 20f
                                        },
                                        children = new SkeletonDescription.Node[]
                                        {
                                            new SkeletonDescription.Node()
                                            {
                                                mass = 0.03f,
                                                centerOfMass = Vector3.zero,
                                                joint = Joint.PinkyDistal,
                                                colliders = new SkeletonDescription.NodeCollider[]
                                                {
                                                    new SkeletonDescription.NodeCollider()
                                                    {
                                                        type = SkeletonDescription.NodeCollider.Type.Box,
                                                        position = Vector3.zero,
                                                        rotation = Quaternion.identity,
                                                        size = new Vector3(0.014f, 0.014f, 0.03f)
                                                        //radius = 0.007f,
                                                        //height = 0.03f,
                                                    }
                                                },
                                                anchor = new Vector3(0, 0f, -0.01f),
                                                connectedAnchor = new Vector3(0f, 0f, 0.013f),
                                                rotationDrive = new JointDrive()
                                                {
                                                    positionSpring = 10f,
                                                    positionDamper = 0.1f,
                                                    maximumForce = 20f
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        // Capsule version of hand skeleton
        public static SkeletonDescription handSkeleton = new SkeletonDescription()
        {
            settings = new HandSettings()
            {
                defaultHandedness = Handedness.Left,
                handednessScale = new Vector3(-1, 1, 1),
                safeMode = true,
                solverIterations = 20,
                solverVelocityIterations = 10,
                maxVelocity = 5,
                maxAngularVelocity = 2,
                maxDepenetrationVelocity = 1f,
                maxError = 1f
            },
            nodeTree = new SkeletonDescription.Node()
            {
                // Root node does not need a drive specified
                mass = 0.255f,
                centerOfMass = Vector3.zero,
                joint = Joint.Wrist,
                colliders = new SkeletonDescription.NodeCollider[]
                {
                    new SkeletonDescription.NodeCollider()
                    {
                        type = SkeletonDescription.NodeCollider.Type.Box,
                        position = new Vector3(0, 0.01f, 0f),
                        rotation = Quaternion.identity,
                        size = new Vector3(0.06f, 0.015f, 0.07f)
                    },
                    new SkeletonDescription.NodeCollider()
                    {
                        type = SkeletonDescription.NodeCollider.Type.Box,
                        position = new Vector3(0, -0.01f, -0.02f),
                        rotation = Quaternion.identity,
                        size = new Vector3(0.06f, 0.03f, 0.035f)
                    }
                },
                anchor = new Vector3(0.002f, -0.001f, -0.045f),
                connectedAnchor = Vector3.zero,
                children = new SkeletonDescription.Node[]
                {
                    // THUMB
                    new SkeletonDescription.Node()
                    {
                        leftDefaultRotation = Quaternion.Euler(0, 90, 0),
                        rightDefaultRotation = Quaternion.Euler(0, -90, 0),
                        mass = 0.225f,
                        centerOfMass = Vector3.zero,
                        joint = Joint.Wrist,
                        toJoint = Joint.ThumbMetacarpal,
                        colliders = new SkeletonDescription.NodeCollider[]
                        {
                            new SkeletonDescription.NodeCollider()
                            {
                                type = SkeletonDescription.NodeCollider.Type.Capsule,
                                position = Vector3.zero,
                                rotation = Quaternion.identity,
                                radius = 0.009f,
                                height = 0.01f,
                            }
                        },
                        anchor = new Vector3(0, 0f, -0.005f),
                        connectedAnchor = new Vector3(0.016f, 0.003f, -0.035f),
                        rotationDrive = new JointDrive()
                        {
                            positionSpring = 10f,
                            positionDamper = 0.1f,
                            maximumForce = 20f
                        },
                        children = new SkeletonDescription.Node[]
                        {
                            new SkeletonDescription.Node()
                            {
                                mass = 0.225f,
                                centerOfMass = Vector3.zero,
                                joint = Joint.ThumbMetacarpal,
                                colliders = new SkeletonDescription.NodeCollider[]
                                {
                                    new SkeletonDescription.NodeCollider()
                                    {
                                        type = SkeletonDescription.NodeCollider.Type.Capsule,
                                        position = Vector3.zero,
                                        rotation = Quaternion.identity,
                                        radius = 0.009f,
                                        height = 0.05f,
                                    }
                                },
                                anchor = new Vector3(0, 0f, -0.025f),
                                connectedAnchor = new Vector3(0f, 0f, 0.005f),
                                rotationDrive = new JointDrive()
                                {
                                    positionSpring = 10f,
                                    positionDamper = 0.1f,
                                    maximumForce = 20f
                                },
                                children = new SkeletonDescription.Node[]
                                {
                                    new SkeletonDescription.Node()
                                    {
                                        mass = 0.03f,
                                        centerOfMass = Vector3.zero,
                                        joint = Joint.ThumbProximal,
                                        colliders = new SkeletonDescription.NodeCollider[]
                                        {
                                            new SkeletonDescription.NodeCollider()
                                            {
                                                type = SkeletonDescription.NodeCollider.Type.Capsule,
                                                position = Vector3.zero,
                                                rotation = Quaternion.identity,
                                                radius = 0.009f,
                                                height = 0.045f,
                                            }
                                        },
                                        anchor = new Vector3(0, 0f, -0.022f),
                                        connectedAnchor = new Vector3(0f, 0f, 0.02f),
                                        rotationDrive = new JointDrive()
                                        {
                                            positionSpring = 10f,
                                            positionDamper = 0.1f,
                                            maximumForce = 20f
                                        },
                                        children = new SkeletonDescription.Node[]
                                        {
                                            new SkeletonDescription.Node()
                                            {
                                                mass = 0.03f,
                                                centerOfMass = Vector3.zero,
                                                joint = Joint.ThumbDistal,
                                                colliders = new SkeletonDescription.NodeCollider[]
                                                {
                                                    new SkeletonDescription.NodeCollider()
                                                    {
                                                        type = SkeletonDescription.NodeCollider.Type.Capsule,
                                                        position = Vector3.zero,
                                                        rotation = Quaternion.identity,
                                                        radius = 0.008f,
                                                        height = 0.035f,
                                                    }
                                                },
                                                anchor = new Vector3(0, 0f, -0.0075f),
                                                connectedAnchor = new Vector3(0f, 0f, 0.02f),
                                                rotationDrive = new JointDrive()
                                                {
                                                    positionSpring = 10f,
                                                    positionDamper = 0.1f,
                                                    maximumForce = 20f
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    },
                    // INDEX
                    new SkeletonDescription.Node()
                    {
                        mass = 0.03f,
                        centerOfMass = Vector3.zero,
                        joint = Joint.IndexKnuckle,
                        colliders = new SkeletonDescription.NodeCollider[]
                        {
                            new SkeletonDescription.NodeCollider()
                            {
                                type = SkeletonDescription.NodeCollider.Type.Capsule,
                                position = Vector3.zero,
                                rotation = Quaternion.identity,
                                radius = 0.009f,
                                height = 0.055f,
                            }
                        },
                        anchor = new Vector3(0, 0f, -0.0275f),
                        connectedAnchor = new Vector3(0.022f, 0.014f, 0.03f),
                        rotationDrive = new JointDrive()
                        {
                            positionSpring = 10f,
                            positionDamper = 0.1f,
                            maximumForce = 3f
                        },
                        children = new SkeletonDescription.Node[]
                        {
                            new SkeletonDescription.Node()
                            {
                                mass = 0.03f,
                                centerOfMass = Vector3.zero,
                                joint = Joint.IndexMiddle,
                                colliders = new SkeletonDescription.NodeCollider[]
                                {
                                    new SkeletonDescription.NodeCollider()
                                    {
                                        type = SkeletonDescription.NodeCollider.Type.Capsule,
                                        position = Vector3.zero,
                                        rotation = Quaternion.identity,
                                        radius = 0.008f,
                                        height = 0.04f,
                                    }
                                },
                                anchor = new Vector3(0, 0f, -0.012f),
                                connectedAnchor = new Vector3(0f, 0f, 0.0275f),
                                rotationDrive = new JointDrive()
                                {
                                    positionSpring = 10f,
                                    positionDamper = 0.1f,
                                    maximumForce = 3f
                                },
                                children = new SkeletonDescription.Node[]
                                {
                                    new SkeletonDescription.Node()
                                    {
                                        mass = 0.03f,
                                        centerOfMass = Vector3.zero,
                                        joint = Joint.IndexDistal,
                                        colliders = new SkeletonDescription.NodeCollider[]
                                        {
                                            new SkeletonDescription.NodeCollider()
                                            {
                                                type = SkeletonDescription.NodeCollider.Type.Capsule,
                                                position = Vector3.zero,
                                                rotation = Quaternion.identity,
                                                radius = 0.0075f,
                                                height = 0.03f,
                                            }
                                        },
                                        anchor = new Vector3(0, 0f, -0.015f),
                                        connectedAnchor = new Vector3(0f, 0f, 0.012f),
                                        rotationDrive = new JointDrive()
                                        {
                                            positionSpring = 10f,
                                            positionDamper = 0.1f,
                                            maximumForce = 3f
                                        }
                                    }
                                }
                            }
                        }
                    },
                    // MIDDLE
                    new SkeletonDescription.Node()
                    {
                        mass = 0.03f,
                        centerOfMass = Vector3.zero,
                        joint = Joint.MiddleKnuckle,
                        colliders = new SkeletonDescription.NodeCollider[]
                        {
                            new SkeletonDescription.NodeCollider()
                            {
                                type = SkeletonDescription.NodeCollider.Type.Capsule,
                                position = Vector3.zero,
                                rotation = Quaternion.identity,
                                radius = 0.009f,
                                height = 0.06f,
                            }
                        },
                        anchor = new Vector3(0, 0f, -0.03f),
                        connectedAnchor = new Vector3(0f, 0.014f, 0.03f),
                        rotationDrive = new JointDrive()
                        {
                            positionSpring = 10f,
                            positionDamper = 0.1f,
                            maximumForce = 3f
                        },
                        children = new SkeletonDescription.Node[]
                        {
                            new SkeletonDescription.Node()
                            {
                                mass = 0.03f,
                                centerOfMass = Vector3.zero,
                                joint = Joint.MiddleMiddle,
                                colliders = new SkeletonDescription.NodeCollider[]
                                {
                                    new SkeletonDescription.NodeCollider()
                                    {
                                        type = SkeletonDescription.NodeCollider.Type.Capsule,
                                        position = Vector3.zero,
                                        rotation = Quaternion.identity,
                                        radius = 0.008f,
                                        height = 0.04f,
                                    }
                                },
                                anchor = new Vector3(0, 0f, -0.012f),
                                connectedAnchor = new Vector3(0f, 0f, 0.03f),
                                rotationDrive = new JointDrive()
                                {
                                    positionSpring = 10f,
                                    positionDamper = 0.1f,
                                    maximumForce = 3f
                                },
                                children = new SkeletonDescription.Node[]
                                {
                                    new SkeletonDescription.Node()
                                    {
                                        mass = 0.03f,
                                        centerOfMass = Vector3.zero,
                                        joint = Joint.MiddleDistal,
                                        colliders = new SkeletonDescription.NodeCollider[]
                                        {
                                            new SkeletonDescription.NodeCollider()
                                            {
                                                type = SkeletonDescription.NodeCollider.Type.Capsule,
                                                position = Vector3.zero,
                                                rotation = Quaternion.identity,
                                                radius = 0.0075f,
                                                height = 0.03f,
                                            }
                                        },
                                        anchor = new Vector3(0, 0f, -0.015f),
                                        connectedAnchor = new Vector3(0f, 0f, 0.012f),
                                        rotationDrive = new JointDrive()
                                        {
                                            positionSpring = 10f,
                                            positionDamper = 0.1f,
                                            maximumForce = 3f
                                        }
                                    }
                                }
                            }
                        }
                    },
                    // RING
                    new SkeletonDescription.Node()
                    {
                        mass = 0.03f,
                        centerOfMass = Vector3.zero,
                        joint = Joint.RingKnuckle,
                        colliders = new SkeletonDescription.NodeCollider[]
                        {
                            new SkeletonDescription.NodeCollider()
                            {
                                type = SkeletonDescription.NodeCollider.Type.Capsule,
                                position = Vector3.zero,
                                rotation = Quaternion.identity,
                                radius = 0.009f,
                                height = 0.055f,
                            }
                        },
                        anchor = new Vector3(0, 0f, -0.0275f),
                        connectedAnchor = new Vector3(-0.022f, 0.014f, 0.03f),
                        rotationDrive = new JointDrive()
                        {
                            positionSpring = 10f,
                            positionDamper = 0.1f,
                            maximumForce = 3f
                        },
                        children = new SkeletonDescription.Node[]
                        {
                            new SkeletonDescription.Node()
                            {
                                mass = 0.03f,
                                centerOfMass = Vector3.zero,
                                joint = Joint.RingMiddle,
                                colliders = new SkeletonDescription.NodeCollider[]
                                {
                                    new SkeletonDescription.NodeCollider()
                                    {
                                        type = SkeletonDescription.NodeCollider.Type.Capsule,
                                        position = Vector3.zero,
                                        rotation = Quaternion.identity,
                                        radius = 0.008f,
                                        height = 0.04f,
                                    }
                                },
                                anchor = new Vector3(0, 0f, -0.012f),
                                connectedAnchor = new Vector3(0f, 0f, 0.0275f),
                                rotationDrive = new JointDrive()
                                {
                                    positionSpring = 10f,
                                    positionDamper = 0.1f,
                                    maximumForce = 3f
                                },
                                children = new SkeletonDescription.Node[]
                                {
                                    new SkeletonDescription.Node()
                                    {
                                        mass = 0.03f,
                                        centerOfMass = Vector3.zero,
                                        joint = Joint.RingDistal,
                                        colliders = new SkeletonDescription.NodeCollider[]
                                        {
                                            new SkeletonDescription.NodeCollider()
                                            {
                                                type = SkeletonDescription.NodeCollider.Type.Capsule,
                                                position = Vector3.zero,
                                                rotation = Quaternion.identity,
                                                radius = 0.0075f,
                                                height = 0.03f,
                                            }
                                        },
                                        anchor = new Vector3(0, 0f, -0.015f),
                                        connectedAnchor = new Vector3(0f, 0f, 0.012f),
                                        rotationDrive = new JointDrive()
                                        {
                                            positionSpring = 10f,
                                            positionDamper = 0.1f,
                                            maximumForce = 3f
                                        }
                                    }
                                }
                            }
                        }
                    },
                    // PINKY
                    new SkeletonDescription.Node()
                    {
                        mass = 0.03f,
                        centerOfMass = Vector3.zero,
                        joint = Joint.PinkyMetacarpal,
                        colliders = new SkeletonDescription.NodeCollider[]
                        {
                            new SkeletonDescription.NodeCollider()
                            {
                                type = SkeletonDescription.NodeCollider.Type.Capsule,
                                position = Vector3.zero,
                                rotation = Quaternion.identity,
                                radius = 0.008f,
                                height = 0.06f,
                            }
                        },
                        anchor = new Vector3(0f, 0f, -0.03f),
                        connectedAnchor = new Vector3(-0.02f, 0.014f, -0.035f),
                        rotationDrive = new JointDrive()
                        {
                            positionSpring = 10f,
                            positionDamper = 0.1f,
                            maximumForce = 20f
                        },
                        children = new SkeletonDescription.Node[]
                        {
                            new SkeletonDescription.Node()
                            {
                                mass = 0.03f,
                                centerOfMass = Vector3.zero,
                                joint = Joint.PinkyKnuckle,
                                colliders = new SkeletonDescription.NodeCollider[]
                                {
                                    new SkeletonDescription.NodeCollider()
                                    {
                                        type = SkeletonDescription.NodeCollider.Type.Capsule,
                                        position = Vector3.zero,
                                        rotation = Quaternion.identity,
                                        radius = 0.007f,
                                        height = 0.05f,
                                    }
                                },
                                anchor = new Vector3(0, 0f, -0.025f),
                                connectedAnchor = new Vector3(0f, 0f, 0.02f),
                                rotationDrive = new JointDrive()
                                {
                                    positionSpring = 10f,
                                    positionDamper = 0.1f,
                                    maximumForce = 20f
                                },
                                children = new SkeletonDescription.Node[]
                                {
                                    new SkeletonDescription.Node()
                                    {
                                        mass = 0.03f,
                                        centerOfMass = Vector3.zero,
                                        joint = Joint.PinkyMiddle,
                                        colliders = new SkeletonDescription.NodeCollider[]
                                        {
                                            new SkeletonDescription.NodeCollider()
                                            {
                                                type = SkeletonDescription.NodeCollider.Type.Capsule,
                                                position = Vector3.zero,
                                                rotation = Quaternion.identity,
                                                radius = 0.007f,
                                                height = 0.03f,
                                            }
                                        },
                                        anchor = new Vector3(0, 0f, -0.015f),
                                        connectedAnchor = new Vector3(0f, 0f, 0.018f),
                                        rotationDrive = new JointDrive()
                                        {
                                            positionSpring = 10f,
                                            positionDamper = 0.1f,
                                            maximumForce = 20f
                                        },
                                        children = new SkeletonDescription.Node[]
                                        {
                                            new SkeletonDescription.Node()
                                            {
                                                mass = 0.03f,
                                                centerOfMass = Vector3.zero,
                                                joint = Joint.PinkyDistal,
                                                colliders = new SkeletonDescription.NodeCollider[]
                                                {
                                                    new SkeletonDescription.NodeCollider()
                                                    {
                                                        type = SkeletonDescription.NodeCollider.Type.Capsule,
                                                        position = Vector3.zero,
                                                        rotation = Quaternion.identity,
                                                        radius = 0.007f,
                                                        height = 0.03f,
                                                    }
                                                },
                                                anchor = new Vector3(0, 0f, -0.01f),
                                                connectedAnchor = new Vector3(0f, 0f, 0.013f),
                                                rotationDrive = new JointDrive()
                                                {
                                                    positionSpring = 10f,
                                                    positionDamper = 0.1f,
                                                    maximumForce = 20f
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };
    }

    public class ITKSkeleton
    {
        public class Node
        {
            public Node parent;
            public Node root;
            public ITKHand.Joint joint;
            public ITKHand.Joint toJoint;

            public Rigidbody rb;
            public ConfigurableJoint j;
            public Collider[] colliders;

            public Node[] children;

            private Quaternion defaultRotation;
            private readonly float mass;
            private readonly Vector3 inertiaTensor;

            public Node(ITKHand.Handedness type, ITKHand.HandSettings settings, ITKHand.SkeletonDescription.Node node, Node root = null, Node parentNode = null, Transform parent = null, Rigidbody body = null, PhysicMaterial material = null, bool isRoot = false)
            {
                if (root == null) root = this; this.root = root;
                this.parent = parentNode;
                joint = node.joint;
                toJoint = node.toJoint;

                defaultRotation = type == ITKHand.Handedness.Left ? node.leftDefaultRotation : node.rightDefaultRotation;

                GameObject container = new GameObject();
                string name = (root != null && node.joint == ITKHand.Root) ? node.toJoint.ToString() : node.joint.ToString();
                container.name = name;
                int layer = LayerMask.NameToLayer("ITKHand");
                if (layer > -1) container.layer = layer;
                else Debug.LogError("Could not find ITKHand layer, please create it in the editor.");
                container.transform.parent = parent;

                rb = container.AddComponent<Rigidbody>();
                rb.solverIterations = settings.solverIterations;
                rb.solverVelocityIterations = settings.solverVelocityIterations;
                rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                rb.maxDepenetrationVelocity = settings.maxDepenetrationVelocity;
                mass = node.mass;
                rb.mass = node.mass;
                rb.centerOfMass = node.centerOfMass;
                rb.useGravity = false;
                rb.drag = 0;
                rb.angularDrag = 0;

                j = container.AddComponent<ConfigurableJoint>();

                if (isRoot)
                {
                    j.xDrive = ITKHand.SkeletonDescription.rootPositionDrive;
                    j.yDrive = ITKHand.SkeletonDescription.rootPositionDrive;
                    j.zDrive = ITKHand.SkeletonDescription.rootPositionDrive;

                    j.slerpDrive = ITKHand.SkeletonDescription.rootRotationDrive;
                }
                else
                {
                    j.connectedBody = body;

                    j.xMotion = ConfigurableJointMotion.Locked;
                    j.yMotion = ConfigurableJointMotion.Locked;
                    j.zMotion = ConfigurableJointMotion.Locked;

                    j.xDrive = new JointDrive() { positionDamper = 0, positionSpring = 0, maximumForce = 0 };
                    j.yDrive = new JointDrive() { positionDamper = 0, positionSpring = 0, maximumForce = 0 };
                    j.zDrive = new JointDrive() { positionDamper = 0, positionSpring = 0, maximumForce = 0 };

                    j.slerpDrive = node.rotationDrive;
                }

                j.rotationDriveMode = RotationDriveMode.Slerp;

                j.autoConfigureConnectedAnchor = false;

                Vector3 scale = Vector3.one;
                if (type != settings.defaultHandedness) scale = settings.handednessScale;
                j.anchor = Vector3.Scale(node.anchor, scale);
                j.connectedAnchor = Vector3.Scale(node.connectedAnchor, scale);

                colliders = new Collider[node.colliders.Length];
                for (int i = 0; i < colliders.Length; ++i)
                {
                    ITKHand.SkeletonDescription.NodeCollider col = node.colliders[i];
                    GameObject colObject = new GameObject(name + " collider");
                    colObject.layer = layer;
                    colObject.transform.parent = container.transform;
                    colObject.transform.localPosition = col.position;
                    colObject.transform.localRotation = col.rotation;
                    switch (col.type)
                    {
                        case ITKHand.SkeletonDescription.NodeCollider.Type.Sphere:
                            {
                                SphereCollider c = colObject.AddComponent<SphereCollider>();
                                c.radius = col.radius;
                                colliders[i] = c;
                            }
                            break;
                        case ITKHand.SkeletonDescription.NodeCollider.Type.Capsule:
                            {
                                CapsuleCollider c = colObject.AddComponent<CapsuleCollider>();
                                c.direction = 2;
                                c.radius = col.radius;
                                c.height = col.height;
                                colliders[i] = c;
                            }
                            break;
                        case ITKHand.SkeletonDescription.NodeCollider.Type.Box:
                            {
                                BoxCollider c = colObject.AddComponent<BoxCollider>();
                                c.size = col.size;
                                colliders[i] = c;
                            }
                            break;
                    }
                    if (colliders[i])
                        colliders[i].material = material;
                }

                inertiaTensor = rb.inertiaTensor;
            }

            public void Enable()
            {
                if (colliders == null) return;

                for (int i = 0; i < colliders.Length; ++i)
                    colliders[i].enabled = true;
            }
            public void Disable()
            {
                if (colliders == null) return;

                for (int i = 0; i < colliders.Length; ++i)
                    colliders[i].enabled = false;

                rb.inertiaTensor = inertiaTensor; // reset inertia tensor to maintain rotaions
            }

            public void DeleteColliders()
            {
                for (int i = 0; i < colliders.Length; ++i)
                    UnityEngine.Object.Destroy(colliders[i]);

                colliders = null;
                rb.inertiaTensor = inertiaTensor; // reset inertia tensor to maintain rotaions
            }

            public void Reset()
            {
                rb.transform.localRotation = defaultRotation;
            }

            public void FixedUpdate(ITKHand.HandSettings settings, float massWeight = 1)
            {
                if (settings.safeMode)
                {
                    rb.mass = mass * Mathf.Clamp(massWeight, 0.01f, 1f);
                    rb.velocity = Vector3.ClampMagnitude(rb.velocity, settings.maxVelocity) * massWeight;
                    rb.angularVelocity = Vector3.ClampMagnitude(rb.velocity, settings.maxAngularVelocity) * massWeight;
                    rb.maxDepenetrationVelocity = settings.maxDepenetrationVelocity;
                }
            }

            public void Track(ITKHand.Pose pose, Quaternion currentRotation)
            {
                Quaternion worldSpaceTarget = pose.rotations[joint];

                if (parent == null) // We are the root
                    j.connectedAnchor = pose.positions[joint];

                if (parent != null && joint == ITKHand.Root)
                {
                    if (toJoint != 0)
                    {
                        Vector3 dir = pose.positions[toJoint] - pose.positions[ITKHand.Root];
                        if (dir == Vector3.zero) dir = Vector3.forward;
                        Quaternion localRotation = Quaternion.LookRotation(dir, pose.rotations[ITKHand.Root] * Vector3.up);
                        localRotation = Quaternion.Inverse(pose.rotations[ITKHand.Root]) * localRotation;
                        j.targetRotation = Quaternion.Inverse(localRotation);
                        currentRotation *= localRotation;
                    }
                    else // toJoint has not been set yet
                    {
                        Debug.LogWarning("toJoint was not set.");
                    }
                }
                else
                {
                    Quaternion localRotation = Quaternion.Inverse(currentRotation) * worldSpaceTarget;
                    j.targetRotation = Quaternion.Inverse(localRotation);
                    currentRotation *= localRotation;
                }

                if (children != null) for (int i = 0; i < children.Length; ++i) children[i].Track(pose, currentRotation);
            }

            public void Teleport(Vector3 position)
            {
                rb.transform.position = position;
            }
        }

        public ITKHand.Handedness type;
        public ITKHand.HandSettings settings;
        public Node root;
        public Node[] nodes;
        public Node[] joints;
        public PhysicMaterial material;

        public ITKSkeleton(ITKHand.Handedness type, Transform parent, ITKHand.SkeletonDescription descriptor, PhysicMaterial material)
        {
            joints = new Node[ITKHand.NumJoints];

            List<Node> temp = new List<Node>();

            this.type = type;
            this.material = material;
            settings = descriptor.settings;

            root = new Node(type, descriptor.settings, descriptor.nodeTree, parent: parent, isRoot: true);
            RecursiveGenerateNodes(temp, type, descriptor.settings, root, root, descriptor.nodeTree.children);

            nodes = temp.ToArray();

            // Disable internal collisions
            for (int i = 0; i < nodes.Length; ++i)
                for (int j = 0; j < nodes.Length; ++j)
                    for (int k = 0; k < nodes[i].colliders.Length; ++k)
                        for (int l = 0; l < nodes[j].colliders.Length; ++l)
                            Physics.IgnoreCollision(nodes[i].colliders[k], nodes[j].colliders[l]);
        }

        private void RecursiveGenerateNodes(List<Node> nodes, ITKHand.Handedness type, ITKHand.HandSettings settings, Node root, Node current, ITKHand.SkeletonDescription.Node[] children)
        {
            if (joints[current.joint] == null) joints[current.joint] = current;
            nodes.Add(current);
            if (children == null) return;

            current.children = new Node[children.Length];
            for (int i = 0; i < children.Length; ++i)
            {
                current.children[i] = new Node(type, settings, children[i], root, current, current.j.transform, current.rb, material);
                RecursiveGenerateNodes(nodes, type, settings, root, current.children[i], children[i].children);
            }
        }
    }

    public class ITKHandPhysics : MonoBehaviour
    {
        public PhysicMaterial material;

        public ITKHand.Handedness type;

        public ITKHandModel model;

        public ITKSkeleton skeleton { private set; get; }

        private bool safeEnable = true;
        private int safeEnableFrame = 5; // Enable after 5 frames

        private bool frozen;
        private float massWeight = 1f;

        private bool _active = true;
        public bool active
        {
            set
            {
                _active = value;
                if (_active) Enable();
                else Disable();
            }
            get => _active;
        }

        public int movingAverageFrameRange = 0;
        private int movingAverageCount = 0;
        private int movingAverageIndex = 0;
        private ITKHand.Pose[] movingAverage;

        private void Start()
        {
            // Check if ignore layer exists
            if (LayerMask.NameToLayer("ITKHandIgnore") < 0)
            {
                Debug.LogError("Could not find ITKHandIgnore layer, please create it in the editor.");
            }

            movingAverage = new ITKHand.Pose[movingAverageFrameRange];
            // Due to bug on hololens, capsule colliders don't behave properly, so box colliders are used instead
            ITKHand.SkeletonDescription description = VRTK.device == VRTK.Device.Hololens2 ? ITKHand.handSkeletonBox : ITKHand.handSkeleton;
            skeleton = new ITKSkeleton(type, transform, description, material);
            Disable(true);
        }

        public void Enable()
        {
            if (safeEnable) return; // Wait till safe enable finishes

            if (_active) return;
            _active = true;

            model?.Enable();

            // If we were frozen lerp velocity to prevent jitter
            if (frozen)
            {
                massWeight = -0.01f;
                frozen = false;
            }
        }

        public void Enable(ITKHand.Pose pose, bool forceEnable = false)
        {
            if (safeEnable) return; // Wait till safe enable finishes

            // Only enable if hand is not inside an object or forceEnable is set to true
            if (forceEnable || !Physics.CheckSphere(pose.positions[ITKHand.Root], 0.1f, ~LayerMask.GetMask("ITKHand", "ITKHandIgnore")))
            {
                if (!_active)
                    // Check if a teleport is actually needed
                    if (Vector3.Distance(skeleton.root.rb.position, pose.positions[ITKHand.Root]) > 1f)
                        Teleport(pose.positions[ITKHand.Root]);
                Enable();
            }
        }

        public void Disable(bool forceDisable = false)
        {
            if (!_active) return;
            _active = false;

            model?.Disable();
        }

        public void IgnoreCollision(Collider collider, bool ignore = true)
        {
            for (int i = 0; i < skeleton.nodes.Length; ++i)
                for (int j = 0; j < skeleton.nodes[i].colliders.Length; ++j)
                    Physics.IgnoreCollision(skeleton.nodes[i].colliders[j], collider, ignore);
        }

        public void IgnoreCollision(Collider[] colliders, bool ignore = true)
        {
            for (int i = 0; i < skeleton.nodes.Length; ++i)
                for (int j = 0; j < skeleton.nodes[i].colliders.Length; ++j)
                    for (int k = 0; k < colliders.Length; ++k)
                        Physics.IgnoreCollision(skeleton.nodes[i].colliders[j], colliders[k], ignore);
        }

        public void Teleport(Vector3 position)
        {
            for (int i = 0; i < skeleton.nodes.Length; ++i)
            {
                ITKSkeleton.Node n = skeleton.nodes[i];
                n.rb.velocity = Vector3.zero;
                n.rb.angularVelocity = Vector3.zero;
                n.Teleport(position);
            }
        }

        public void Track(ITKHand.Pose pose, bool frozen = false)
        {
            ITKSkeleton.Node root = skeleton.root;

            this.frozen = frozen;

            if (movingAverage != null && movingAverage.Length > 0) // Calculate moving average
            {
                if (movingAverage[movingAverageIndex].positions == null) movingAverage[movingAverageIndex].positions = new Vector3[ITKHand.NumJoints];
                if (movingAverage[movingAverageIndex].rotations == null) movingAverage[movingAverageIndex].rotations = new Quaternion[ITKHand.NumJoints];
                movingAverage[movingAverageIndex].Copy(pose);
                movingAverageIndex = (movingAverageIndex + 1) % movingAverage.Length;
                if (movingAverageCount < movingAverage.Length) ++movingAverageCount;

                if (movingAverageCount > 0)
                {
                    for (int i = 0; i < ITKHand.NumJoints; ++i)
                    {
                        Vector3 averagePos = Vector3.zero;
                        Vector4 cumulative = new Vector4(0, 0, 0, 0);
                        for (int j = 0; j < movingAverageCount; ++j)
                        {
                            averagePos += movingAverage[j].positions[i];
                            VRTKUtils.AverageQuaternion_Internal(ref cumulative, movingAverage[j].rotations[i], movingAverage[0].rotations[i]);
                        }
                        float addDet = 1f / movingAverageCount;
                        float x = cumulative.x * addDet;
                        float y = cumulative.y * addDet;
                        float z = cumulative.z * addDet;
                        float w = cumulative.w * addDet;
                        //note: if speed is an issue, you can skip the normalization step
                        pose.rotations[i] = VRTKUtils.NormalizeQuaternion(new Quaternion(x, y, z, w));
                        pose.positions[i] = averagePos / movingAverageCount;
                    }
                }
            }

            // Track joints
            root.Track(pose, Quaternion.identity);

            // Make sure the root doesn't overshoot
            Vector3 anchor = root.rb.position + root.rb.rotation * root.j.anchor;
            Vector3 dir = pose.positions[ITKHand.Root] - anchor;
            if (root.rb.velocity.magnitude > 0.1f && Vector3.Dot(root.rb.velocity, dir) < 0)
            {
                for (int i = 0; i < skeleton.nodes.Length; ++i)
                {
                    skeleton.nodes[i].rb.velocity *= 0.5f;
                    skeleton.nodes[i].rb.angularVelocity *= 0.5f;
                }
            }

            // Update nodes
            for (int i = 0; i < skeleton.nodes.Length; ++i)
            {
                skeleton.nodes[i].FixedUpdate(skeleton.settings, massWeight);
            }

            // teleport and set joints velocity to zero if unstable (far from target)
            if (Vector3.Distance(root.rb.position + root.rb.rotation * root.j.anchor, root.j.connectedAnchor) > skeleton.settings.maxError)
            {
                Teleport(pose.positions[ITKHand.Root]);
            }

            // safely enable when we are tracked properly - TODO:: check if hand is not inside of anything before enabling
            if (safeEnable && safeEnableFrame <= 0 && !Physics.CheckSphere(root.rb.position, 0.1f, ~LayerMask.GetMask("ITKHand", "ITKHandIgnore")) && Vector3.Distance(root.rb.position, pose.positions[ITKHand.Root]) < 0.1f)
            {
                safeEnable = false;
                for (int i = 0; i < skeleton.nodes.Length; ++i)
                {
                    skeleton.nodes[i].Reset();
                }
                Enable();
            }
            else if (safeEnableFrame > 0) --safeEnableFrame;

            // Lerp velocity weight to 1
            massWeight = Mathf.Lerp(massWeight, 1, 0.5f);

            model?.Track(pose, skeleton);
        }
    }
}
