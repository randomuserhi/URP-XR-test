using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.Utilities.Gltf.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using VirtualRealityTK;

namespace InteractionTK.HandTracking
{
    public static partial class ITKHandUtils
    {
        public struct HandSettings
        {
            public Handedness defaultHandedness;
            public Vector3 handednessScale;

            public bool safeMode;

            public float maxVelocity;
            public float maxAngularVelocity;
            public float maxDepenetrationVelocity;

            public float maxError;
        }

        // TODO:: support mesh and compound colliders
        public struct HandSkeletonDescription
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
                maximumForce = 12.5f
            };

            public struct Node
            {
                public enum Type
                {
                    Capsule,
                    Box,
                    Sphere
                }

                public Quaternion rightDefaultRotation; // right hand rotation of the joint upon instantiation - in local space
                public Quaternion leftDefaultRotation; // left hand rotation of the joint upon instantiation - in local space

                public float mass;
                public Vector3 centerOfMass;

                public Joint joint;
                public Joint toJoint; // Used when joint is the root, so there will be no rotation

                public Type type;
                public Vector3 size;
                public float radius;
                public float height;

                public Vector3 anchor;
                public Vector3 connectedAnchor;

                public JointDrive rotationDrive;

                public int[][] ignore; // index coordinate to node that collision should be ignored for
                public Node[] children;
            }

            public HandSettings settings;
            public Node nodeTree;
        }

        public static HandSkeletonDescription handSkeleton = new HandSkeletonDescription()
        {
            settings = new HandSettings()
            {
                defaultHandedness = Handedness.Left,
                handednessScale = new Vector3(-1, 1, 1),
                safeMode = true,
                maxVelocity = 5,
                maxAngularVelocity = 2,
                maxDepenetrationVelocity = 1,
                maxError = 0.5f
            },
            nodeTree = new HandSkeletonDescription.Node()
            {
                // Root node does not need a drive specified
                mass = 0.255f,
                centerOfMass = Vector3.zero,
                joint = Joint.Wrist,
                type = HandSkeletonDescription.Node.Type.Box,
                size = new Vector3(0.06f, 0.025f, 0.07f),
                anchor = new Vector3(0.002f, -0.001f, -0.045f),
                connectedAnchor = Vector3.zero,
                children = new HandSkeletonDescription.Node[]
                {
                    // THUMB
                    new HandSkeletonDescription.Node()
                    {
                        leftDefaultRotation = Quaternion.Euler(0, 90, 0),
                        rightDefaultRotation = Quaternion.Euler(0, -90, 0),
                        mass = 0.015f,
                        centerOfMass = Vector3.zero,
                        joint = Joint.Wrist,
                        toJoint = Joint.ThumbMetacarpal,
                        type = HandSkeletonDescription.Node.Type.Capsule,
                        radius = 0.009f,
                        height = 0.03f,
                        anchor = new Vector3(0, 0f, -0.015f),
                        connectedAnchor = new Vector3(0.002f, -0.001f, -0.045f),
                        rotationDrive = new JointDrive()
                        {
                            positionSpring = 10f,
                            positionDamper = 0.1f,
                            maximumForce = 12.5f
                        },
                        children = new HandSkeletonDescription.Node[]
                        {
                            new HandSkeletonDescription.Node()
                            {
                                mass = 0.015f,
                                centerOfMass = Vector3.zero,
                                joint = Joint.ThumbMetacarpal,
                                type = HandSkeletonDescription.Node.Type.Capsule,
                                radius = 0.009f,
                                height = 0.05f,
                                anchor = new Vector3(0, 0f, -0.025f),
                                connectedAnchor = new Vector3(0f, 0f, 0.015f),
                                rotationDrive = new JointDrive()
                                {
                                    positionSpring = 10f,
                                    positionDamper = 0.1f,
                                    maximumForce = 12.5f
                                },
                                ignore = new int[][] 
                                {
                                    new int[0] // Ignore root collider
                                },
                                children = new HandSkeletonDescription.Node[]
                                {
                                    new HandSkeletonDescription.Node()
                                    {
                                        mass = 0.015f,
                                        centerOfMass = Vector3.zero,
                                        joint = Joint.ThumbProximal,
                                        type = HandSkeletonDescription.Node.Type.Capsule,
                                        radius = 0.009f,
                                        height = 0.04f,
                                        anchor = new Vector3(0, 0f, -0.017f),
                                        connectedAnchor = new Vector3(0f, 0f, 0.02f),
                                        rotationDrive = new JointDrive()
                                        {
                                            positionSpring = 10f,
                                            positionDamper = 0.1f,
                                            maximumForce = 12.5f
                                        },
                                        children = new HandSkeletonDescription.Node[]
                                        {
                                            new HandSkeletonDescription.Node()
                                            {
                                                mass = 0.015f,
                                                centerOfMass = Vector3.zero,
                                                joint = Joint.ThumbDistal,
                                                type = HandSkeletonDescription.Node.Type.Capsule,
                                                radius = 0.008f,
                                                height = 0.03f,
                                                anchor = new Vector3(0, 0f, -0.007f),
                                                connectedAnchor = new Vector3(0f, 0f, 0.02f),
                                                rotationDrive = new JointDrive()
                                                {
                                                    positionSpring = 10f,
                                                    positionDamper = 0.1f,
                                                    maximumForce = 12.5f
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    },
                    // INDEX
                    new HandSkeletonDescription.Node()
                    {
                        mass = 0.015f,
                        centerOfMass = Vector3.zero,
                        joint = Joint.IndexKnuckle,
                        type = HandSkeletonDescription.Node.Type.Capsule,
                        radius = 0.009f,
                        height = 0.055f,
                        anchor = new Vector3(0, 0f, -0.0275f),
                        connectedAnchor = new Vector3(0.022f, -0.003f, 0.03f),
                        rotationDrive = new JointDrive()
                        {
                            positionSpring = 10f,
                            positionDamper = 0.1f,
                            maximumForce = 1f
                        },
                        children = new HandSkeletonDescription.Node[]
                        {
                            new HandSkeletonDescription.Node()
                            {
                                mass = 0.015f,
                                centerOfMass = Vector3.zero,
                                joint = Joint.IndexMiddle,
                                type = HandSkeletonDescription.Node.Type.Capsule,
                                radius = 0.008f,
                                height = 0.04f,
                                anchor = new Vector3(0, 0f, -0.012f),
                                connectedAnchor = new Vector3(0f, 0f, 0.0275f),
                                rotationDrive = new JointDrive()
                                {
                                    positionSpring = 10f,
                                    positionDamper = 0.1f,
                                    maximumForce = 1f
                                },
                                children = new HandSkeletonDescription.Node[]
                                {
                                    new HandSkeletonDescription.Node()
                                    {
                                        mass = 0.015f,
                                        centerOfMass = Vector3.zero,
                                        joint = Joint.IndexDistal,
                                        type = HandSkeletonDescription.Node.Type.Capsule,
                                        radius = 0.0075f,
                                        height = 0.03f,
                                        anchor = new Vector3(0, 0f, -0.015f),
                                        connectedAnchor = new Vector3(0f, 0f, 0.012f),
                                        rotationDrive = new JointDrive()
                                        {
                                            positionSpring = 10f,
                                            positionDamper = 0.1f,
                                            maximumForce = 1f
                                        }
                                    }
                                }
                            }
                        }
                    },
                    // MIDDLE
                    new HandSkeletonDescription.Node()
                    {
                        mass = 0.015f,
                        centerOfMass = Vector3.zero,
                        joint = Joint.MiddleKnuckle,
                        type = HandSkeletonDescription.Node.Type.Capsule,
                        radius = 0.009f,
                        height = 0.06f,
                        anchor = new Vector3(0, 0f, -0.03f),
                        connectedAnchor = new Vector3(0f, -0.003f, 0.03f),
                        rotationDrive = new JointDrive()
                        {
                            positionSpring = 10f,
                            positionDamper = 0.1f,
                            maximumForce = 1f
                        },
                        children = new HandSkeletonDescription.Node[]
                        {
                            new HandSkeletonDescription.Node()
                            {
                                mass = 0.015f,
                                centerOfMass = Vector3.zero,
                                joint = Joint.MiddleMiddle,
                                type = HandSkeletonDescription.Node.Type.Capsule,
                                radius = 0.008f,
                                height = 0.04f,
                                anchor = new Vector3(0, 0f, -0.012f),
                                connectedAnchor = new Vector3(0f, 0f, 0.03f),
                                rotationDrive = new JointDrive()
                                {
                                    positionSpring = 10f,
                                    positionDamper = 0.1f,
                                    maximumForce = 1f
                                },
                                children = new HandSkeletonDescription.Node[]
                                {
                                    new HandSkeletonDescription.Node()
                                    {
                                        mass = 0.015f,
                                        centerOfMass = Vector3.zero,
                                        joint = Joint.MiddleDistal,
                                        type = HandSkeletonDescription.Node.Type.Capsule,
                                        radius = 0.0075f,
                                        height = 0.03f,
                                        anchor = new Vector3(0, 0f, -0.015f),
                                        connectedAnchor = new Vector3(0f, 0f, 0.012f),
                                        rotationDrive = new JointDrive()
                                        {
                                            positionSpring = 10f,
                                            positionDamper = 0.1f,
                                            maximumForce = 1f
                                        }
                                    }
                                }
                            }
                        }
                    },
                    // RING
                    new HandSkeletonDescription.Node()
                    {
                        mass = 0.015f,
                        centerOfMass = Vector3.zero,
                        joint = Joint.RingKnuckle,
                        type = HandSkeletonDescription.Node.Type.Capsule,
                        radius = 0.009f,
                        height = 0.055f,
                        anchor = new Vector3(0, 0f, -0.0275f),
                        connectedAnchor = new Vector3(-0.022f, -0.003f, 0.03f),
                        rotationDrive = new JointDrive()
                        {
                            positionSpring = 10f,
                            positionDamper = 0.1f,
                            maximumForce = 1f
                        },
                        children = new HandSkeletonDescription.Node[]
                        {
                            new HandSkeletonDescription.Node()
                            {
                                mass = 0.015f,
                                centerOfMass = Vector3.zero,
                                joint = Joint.RingMiddle,
                                type = HandSkeletonDescription.Node.Type.Capsule,
                                radius = 0.008f,
                                height = 0.04f,
                                anchor = new Vector3(0, 0f, -0.012f),
                                connectedAnchor = new Vector3(0f, 0f, 0.0275f),
                                rotationDrive = new JointDrive()
                                {
                                    positionSpring = 10f,
                                    positionDamper = 0.1f,
                                    maximumForce = 1f
                                },
                                children = new HandSkeletonDescription.Node[]
                                {
                                    new HandSkeletonDescription.Node()
                                    {
                                        mass = 0.015f,
                                        centerOfMass = Vector3.zero,
                                        joint = Joint.RingDistal,
                                        type = HandSkeletonDescription.Node.Type.Capsule,
                                        radius = 0.0075f,
                                        height = 0.03f,
                                        anchor = new Vector3(0, 0f, -0.015f),
                                        connectedAnchor = new Vector3(0f, 0f, 0.012f),
                                        rotationDrive = new JointDrive()
                                        {
                                            positionSpring = 10f,
                                            positionDamper = 0.1f,
                                            maximumForce = 1f
                                        }
                                    }
                                }
                            }
                        }
                    },
                    // PINKY
                    new HandSkeletonDescription.Node()
                    {
                        mass = 0.015f,
                        centerOfMass = Vector3.zero,
                        joint = Joint.PinkyMetacarpal,
                        type = HandSkeletonDescription.Node.Type.Capsule,
                        radius = 0.008f,
                        height = 0.06f,
                        anchor = new Vector3(0f, 0f, -0.03f),
                        connectedAnchor = new Vector3(-0.02f, -0.005f, -0.035f),
                        rotationDrive = new JointDrive()
                        {
                            positionSpring = 10f,
                            positionDamper = 0.1f,
                            maximumForce = 12.5f
                        },
                        children = new HandSkeletonDescription.Node[]
                        {
                            new HandSkeletonDescription.Node()
                            {
                                mass = 0.015f,
                                centerOfMass = Vector3.zero,
                                joint = Joint.PinkyKnuckle,
                                type = HandSkeletonDescription.Node.Type.Capsule,
                                radius = 0.007f,
                                height = 0.05f,
                                anchor = new Vector3(0, 0f, -0.025f),
                                connectedAnchor = new Vector3(0f, 0f, 0.02f),
                                rotationDrive = new JointDrive()
                                {
                                    positionSpring = 10f,
                                    positionDamper = 0.1f,
                                    maximumForce = 12.5f
                                },
                                ignore = new int[][]
                                {
                                    new int[0] // Ignore root collider
                                },
                                children = new HandSkeletonDescription.Node[]
                                {
                                    new HandSkeletonDescription.Node()
                                    {
                                        mass = 0.015f,
                                        centerOfMass = Vector3.zero,
                                        joint = Joint.PinkyMiddle,
                                        type = HandSkeletonDescription.Node.Type.Capsule,
                                        radius = 0.007f,
                                        height = 0.03f,
                                        anchor = new Vector3(0, 0f, -0.015f),
                                        connectedAnchor = new Vector3(0f, 0f, 0.018f),
                                        rotationDrive = new JointDrive()
                                        {
                                            positionSpring = 10f,
                                            positionDamper = 0.1f,
                                            maximumForce = 12.5f
                                        },
                                        children = new HandSkeletonDescription.Node[]
                                        {
                                            new HandSkeletonDescription.Node()
                                            {
                                                mass = 0.015f,
                                                centerOfMass = Vector3.zero,
                                                joint = Joint.PinkyDistal,
                                                type = HandSkeletonDescription.Node.Type.Capsule,
                                                radius = 0.007f,
                                                height = 0.03f,
                                                anchor = new Vector3(0, 0f, -0.01f),
                                                connectedAnchor = new Vector3(0f, 0f, 0.013f),
                                                rotationDrive = new JointDrive()
                                                {
                                                    positionSpring = 10f,
                                                    positionDamper = 0.1f,
                                                    maximumForce = 12.5f
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
            public ITKHandUtils.Joint joint;
            public ITKHandUtils.Joint toJoint;

            public Rigidbody rb;
            public ConfigurableJoint j;
            public Collider collider;

            public Node[] children;

            private Quaternion defaultRotation;

            public Node(ITKHandUtils.Handedness type, ITKHandUtils.HandSettings settings, ITKHandUtils.HandSkeletonDescription.Node node, Node root = null, Node parentNode = null, Transform parent = null, Rigidbody body = null, PhysicMaterial material = null, bool isRoot = false)
            {
                if (root == null) root = this; this.root = root;
                this.parent = parentNode;
                joint = node.joint;
                toJoint = node.toJoint;

                defaultRotation = type == ITKHandUtils.Handedness.Left ? node.leftDefaultRotation : node.rightDefaultRotation;

                GameObject container = new GameObject();
                container.name = (root != null && node.joint == ITKHandUtils.Root) ? node.toJoint.ToString() : node.joint.ToString();
                int layer = LayerMask.NameToLayer("ITKHand");
                if (layer > -1) container.layer = layer;
                else Debug.LogError("Could not find ITKHand layer, please create it in the editor.");
                container.transform.parent = parent;

                rb = container.AddComponent<Rigidbody>();
                rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                rb.maxDepenetrationVelocity = settings.maxDepenetrationVelocity;
                rb.mass = node.mass;
                rb.centerOfMass = node.centerOfMass;
                rb.useGravity = false;
                rb.drag = 0;
                rb.angularDrag = 0;

                j = container.AddComponent<ConfigurableJoint>();

                if (isRoot)
                {
                    j.xDrive = ITKHandUtils.HandSkeletonDescription.rootPositionDrive;
                    j.yDrive = ITKHandUtils.HandSkeletonDescription.rootPositionDrive;
                    j.zDrive = ITKHandUtils.HandSkeletonDescription.rootPositionDrive;

                    j.slerpDrive = ITKHandUtils.HandSkeletonDescription.rootRotationDrive;
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

                collider = null;
                switch (node.type)
                {
                    case ITKHandUtils.HandSkeletonDescription.Node.Type.Sphere:
                        {
                            SphereCollider c = container.AddComponent<SphereCollider>();
                            c.radius = node.radius;
                            collider = c;
                        }
                        break;
                    case ITKHandUtils.HandSkeletonDescription.Node.Type.Capsule:
                        {
                            CapsuleCollider c = container.AddComponent<CapsuleCollider>();
                            c.direction = 2;
                            c.radius = node.radius;
                            c.height = node.height;
                            collider = c;
                        }
                        break;
                    case ITKHandUtils.HandSkeletonDescription.Node.Type.Box:
                        {
                            BoxCollider c = container.AddComponent<BoxCollider>();
                            c.size = node.size;
                            collider = c;
                        }
                        break;
                }
                if (collider)
                {
                    collider.material = material;

                    if (node.ignore != null) 
                        for (int i = 0; i < node.ignore.Length; ++i)
                        {
                            // TODO:: check if node.ignore[i] == null

                            // Ignore root
                            if (node.ignore[i].Length == 0)
                            {
                                Physics.IgnoreCollision(collider, root.collider);
                                continue;
                            }

                            // TODO:: error checking here (just simple bound checks)
                            Node curr = root;
                            for (int j = 0; j < node.ignore[i].Length; ++j)
                            {
                                curr = curr.children[node.ignore[i][j]];
                            }
                            Physics.IgnoreCollision(collider, curr.collider);
                        }
                }
            }

            public void Reset()
            {
                rb.transform.localRotation = defaultRotation;
            }

            public void FixedUpdate(ITKHandUtils.HandSettings settings)
            {
                if (settings.safeMode)
                {
                    rb.velocity = Vector3.ClampMagnitude(rb.velocity, settings.maxVelocity);
                    rb.angularVelocity = Vector3.ClampMagnitude(rb.velocity, settings.maxAngularVelocity);
                    rb.maxDepenetrationVelocity = settings.maxDepenetrationVelocity;

                    if (Vector3.Distance(rb.position, j.targetPosition) > settings.maxError)
                    {
                        rb.velocity = Vector3.zero;
                        rb.angularVelocity = Vector3.zero;
                    }
                }
            }

            public void Track(ITKHandUtils.Pose pose, Quaternion currentRotation)
            {
                Quaternion worldSpaceTarget = pose.rotations[joint];
                
                if (parent == null) // We are the root
                    j.connectedAnchor = pose.positions[joint];

                if (parent != null && joint == ITKHandUtils.Root)
                {
                    if (toJoint != 0)
                    {
                        Vector3 dir = pose.positions[toJoint] - pose.positions[ITKHandUtils.Root];
                        if (dir == Vector3.zero) dir = Vector3.forward;
                        Quaternion localRotation = Quaternion.LookRotation(dir, pose.rotations[ITKHandUtils.Root] * Vector3.up);
                        localRotation = Quaternion.Inverse(pose.rotations[ITKHandUtils.Root]) * localRotation;
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
        }

        public ITKHandUtils.Handedness type;
        public ITKHandUtils.HandSettings settings;
        public Node root;
        public Node[] nodes;
        public PhysicMaterial material;

        public ITKSkeleton(ITKHandUtils.Handedness type, Transform parent, ITKHandUtils.HandSkeletonDescription descriptor, PhysicMaterial material)
        {
            List<Node> temp = new List<Node>();
            
            this.type = type;
            this.material = material;
            settings = descriptor.settings;

            root = new Node(type, descriptor.settings, descriptor.nodeTree, parent: parent, isRoot: true);
            RecursiveGenerateNodes(temp, type, descriptor.settings, root, root, descriptor.nodeTree.children);

            nodes = temp.ToArray();
        }

        private void RecursiveGenerateNodes(List<Node> nodes, ITKHandUtils.Handedness type, ITKHandUtils.HandSettings settings, Node root, Node current, ITKHandUtils.HandSkeletonDescription.Node[] children)
        {
            nodes.Add(current);
            if (children == null) return;

            current.children = new Node[children.Length];
            for (int i = 0; i < children.Length; ++i)
            {
                current.children[i] = new Node(type, settings, children[i], root, current, current.j.transform, current.rb, material);
                RecursiveGenerateNodes(nodes, type, settings, root,  current.children[i], children[i].children);
            }
        }
    }

    public class ITKHand : MonoBehaviour
    {
        public PhysicMaterial material;

        public ITKHandUtils.Handedness type;

        public ITKHandModel physicsModel;
        public ITKHandModel realModel;

        private ITKSkeleton skeleton;

        private bool safeEnable = true;
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

        private void Start()
        {
            skeleton = new ITKSkeleton(type, transform, ITKHandUtils.handSkeleton, material);
            Disable();
        }

        public void Enable()
        {
            if (_active) return;
            _active = true;

            for (int i = 0; i < skeleton.nodes.Length; i++)
            {
                skeleton.nodes[i].collider.enabled = true;
            }
        }

        public void Disable()
        {
            if (!_active) return;
            _active = false;

            for (int i = 0; i < skeleton.nodes.Length; i++)
            {
                skeleton.nodes[i].collider.enabled = false;
            }
        }

        public void Track(ITKHandUtils.Pose pose)
        {
            ITKSkeleton.Node root = skeleton.root;
            
            // Track joints
            root.Track(pose, Quaternion.identity);

            // Update nodes
            for (int i = 0; i < skeleton.nodes.Length; ++i)
            {
                skeleton.nodes[i].FixedUpdate(skeleton.settings);
            }

            // TODO:: teleport and set joints velocity to zero if unstable (check distance from target)

            // safely enable when we are tracked properly - TODO:: check if hand is not inside of anything before enabling
            if (safeEnable && Physics.CheckSphere(root.rb.position, 0.1f) && Vector3.Distance(root.rb.position, pose.positions[ITKHandUtils.Root]) < 0.01f)
            {
                safeEnable = false;
                for (int i = 0; i < skeleton.nodes.Length; ++i)
                {
                    skeleton.nodes[i].Reset();
                }
                Enable();
            }

            physicsModel?.Track(skeleton);
            realModel?.Track(pose);
        }
    }
}