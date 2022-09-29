using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI.Table;

namespace InteractionTK.HandTracking
{
    public static partial class ITKHand
    {
        public static Joint[][] structure = new Joint[][]
        {
            new Joint[] { Joint.Palm },
            new Joint[] { Joint.ThumbMetacarpal, Joint.ThumbProximal, Joint.ThumbDistal, Joint.ThumbTip },
            new Joint[] { Joint.IndexMetacarpal, Joint.IndexKnuckle, Joint.IndexMiddle, Joint.IndexDistal, Joint.IndexTip },
            new Joint[] { Joint.MiddleMetacarpal, Joint.MiddleKnuckle, Joint.MiddleMiddle, Joint.MiddleDistal, Joint.MiddleTip },
            new Joint[] { Joint.RingMetacarpal, Joint.RingKnuckle, Joint.RingMiddle, Joint.RingDistal, Joint.RingTip },
            new Joint[] { Joint.PinkyMetacarpal, Joint.PinkyKnuckle, Joint.PinkyMiddle, Joint.PinkyDistal, Joint.PinkyTip }
        };

        public static float[][] distances = new float[][]
        {
            new float[] { 0 },
            new float[] { 0, 0.05f, 0.05f, 0.05f },
            new float[] { 0, 0.05f, 0.05f, 0.05f, 0.05f },
            new float[] { 0, 0.05f, 0.05f, 0.05f, 0.05f },
            new float[] { 0, 0.05f, 0.05f, 0.05f, 0.05f },
            new float[] { 0, 0.05f, 0.05f, 0.05f, 0.05f }
        };
    }

    public class ITKDebugHand : MonoBehaviour
    {
        public ITKHand.Handedness type;
        private ITKHand.Pose pose = new ITKHand.Pose(ITKHand.NumJoints);

        public ITKHandPhysics hand;
        public GameObject debugPoint;

        public bool Tracking = true;

        private GameObject[] joints = new GameObject[ITKHand.NumJoints];
        private GameObject[] interactable = new GameObject[ITKHand.NumJoints];
        private static Vector3[][] defaultPositions = new Vector3[][] // positions are the local positions from unity
        {
            new Vector3[] { new Vector3(0, 0, 4.65f) },
            new Vector3[] { new Vector3(2.77f, -1.8f, 2.8f), new Vector3(1.13f, 1.2f, 2.95f), new Vector3(0.7f, 0.5f, 3f), new Vector3(0.7f, 0.5f, 3f), new Vector3(0.53f, -0.6f, 2.2f) },
            new Vector3[] { new Vector3(1.26f, 0, 2.72f), new Vector3(0.5f, 0, 5f), new Vector3(0.15f, -0.6f, 4.44f), new Vector3(0.65f, -0.87f, 3.25f), new Vector3(0.06f, -1.5f, 1.67f) },
            new Vector3[] { new Vector3(0, 0, 2.74f), new Vector3(0, 0, 5.43f), new Vector3(0, -0.58f, 4.78f), new Vector3(0, -0.77f, 2.83f), new Vector3(0, -0.86f, 2f) },
            new Vector3[] { new Vector3(-1.26f, 0, 2.72f), new Vector3(-0.5f, 0, 5f), new Vector3(-0.15f, -0.6f, 4.44f), new Vector3(-0.65f, -0.87f, 3.25f), new Vector3(-0.06f, -1.5f, 1.67f) },
            new Vector3[] { new Vector3(-2.75f, 0, 2.78f), new Vector3(-1.26f, -0.27f, 4.8f), new Vector3(-0.63f, -0.69f, 2.86f), new Vector3(-0.56f, -1.08f, 2.36f), new Vector3(-0.42f, -0.89f, 1.93f) }
        };
        private void Start()
        {
            Vector3 scale = new Vector3(type == ITKHand.Handedness.Left ? 1 : -1, 1, 1);

            interactable[ITKHand.Root] = Instantiate(debugPoint);
            interactable[ITKHand.Root].transform.parent = transform;
            interactable[ITKHand.Root].name = ((ITKHand.Joint)ITKHand.Root).ToString();

            joints[ITKHand.Root] = new GameObject();
            joints[ITKHand.Root].transform.parent = transform;
            joints[ITKHand.Root].name = "_" + ((ITKHand.Joint)ITKHand.Root).ToString();
            for (int i = 0; i < ITKHand.structure.Length; ++i)
            {
                Transform root = joints[ITKHand.Root].transform;
                Transform interRoot = interactable[ITKHand.Root].transform;
                for (int j = 0; j < ITKHand.structure[i].Length; ++j)
                {
                    ITKHand.Joint curr = ITKHand.structure[i][j];

                    GameObject o = new GameObject();
                    o.transform.parent = root;
                    o.name = "_" + ITKHand.structure[i][j].ToString();

                    GameObject inter = Instantiate(debugPoint);
                    inter.transform.parent = interRoot;
                    inter.name = ITKHand.structure[i][j].ToString();

                    // Position joints to default location
                    inter.transform.localPosition = root.transform.position + Vector3.Scale(defaultPositions[i][j], scale);

                    interactable[curr] = inter;
                    joints[curr] = o;

                    interRoot = inter.transform;
                    root = o.transform;
                }
            }

            // Rotate thumb
            interactable[ITKHand.ThumbMetacarpal].transform.localRotation = Quaternion.Euler(0, 0, type == ITKHand.Handedness.Left ? 270 : 90);

            pose.positions = new Vector3[ITKHand.NumJoints];
            pose.rotations = new Quaternion[ITKHand.NumJoints];
        }

        private void FixedUpdate()
        {
            if (hand.type != type)
            {
                Debug.LogError("Tracked hand type does not match the type of the ITKHand.");
                return;
            }

            joints[ITKHand.Root].transform.position = interactable[ITKHand.Root].transform.position;
            joints[ITKHand.Root].transform.rotation = interactable[ITKHand.Root].transform.rotation;

            for (int i = 0; i < interactable.Length; ++i)
            {
                if (i != ITKHand.Root) interactable[i].transform.localRotation = Quaternion.Euler(0, 0, interactable[i].transform.localRotation.eulerAngles.z);
                joints[i].transform.rotation = Quaternion.Euler(
                    joints[i].transform.rotation.eulerAngles.x, 
                    joints[i].transform.rotation.eulerAngles.y, 
                    interactable[i].transform.rotation.eulerAngles.z);
            }

            for (int i = 0; i < ITKHand.structure.Length; ++i)
            {
                for (int j = 0; j < ITKHand.structure[i].Length - 1; ++j)
                {
                    ITKHand.Joint curr = ITKHand.structure[i][j];
                    ITKHand.Joint next = ITKHand.structure[i][j + 1];
                    Vector3 dir = interactable[next].transform.position - interactable[curr].transform.position;
                    if (dir != Vector3.zero) joints[curr].transform.rotation = Quaternion.LookRotation(dir, joints[curr].transform.rotation * Vector3.up);
                    joints[curr].transform.position = interactable[curr].transform.position;
                }
            }

            for (int i = 0; i < joints.Length; ++i)
            {
                pose.positions[i] = joints[i].transform.position;
                pose.rotations[i] = joints[i].transform.rotation;
            }

            if (Tracking)
            {
                hand.Enable();
                hand.Track(pose);
            }
            else
            {
                hand.Disable();
            }
        }
    }
}
