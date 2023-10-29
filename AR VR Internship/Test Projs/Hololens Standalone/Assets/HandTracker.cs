using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using UnityEngine;

using Microsoft;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.Input;

public class HandTracker : MonoBehaviour
{
    public GameObject RawTracker;

    private class Hand
    {
        public Handedness handedness;
        private MixedRealityPose pose;

        /*private bool[] isKinematic = new bool[] { 
            false,
            false,
            true,
            true,

            false,
            false,
            false,
            true,
            true,

            false,
            false,
            false,
            true,
            true,

            false,
            false,
            false,
            true,
            true,

            false,
            false,
            false,
            true,
            true,

            true,
            true
        };*/

        private TrackedHandJoint[] trackedJoints = new TrackedHandJoint[] {
            TrackedHandJoint.Wrist,

            TrackedHandJoint.ThumbTip,
            TrackedHandJoint.ThumbDistalJoint,
            TrackedHandJoint.ThumbProximalJoint,
            TrackedHandJoint.ThumbMetacarpalJoint,

            TrackedHandJoint.IndexTip,
            TrackedHandJoint.IndexDistalJoint,
            TrackedHandJoint.IndexMiddleJoint,
            TrackedHandJoint.IndexKnuckle,
            TrackedHandJoint.IndexMetacarpal,

            TrackedHandJoint.MiddleTip,
            TrackedHandJoint.MiddleDistalJoint,
            TrackedHandJoint.MiddleMiddleJoint,
            TrackedHandJoint.MiddleKnuckle,
            TrackedHandJoint.MiddleMetacarpal,

            TrackedHandJoint.RingTip,
            TrackedHandJoint.RingDistalJoint,
            TrackedHandJoint.RingMiddleJoint,
            TrackedHandJoint.RingKnuckle,
            TrackedHandJoint.RingMetacarpal,

            TrackedHandJoint.PinkyTip,
            TrackedHandJoint.PinkyDistalJoint,
            TrackedHandJoint.PinkyMiddleJoint,
            TrackedHandJoint.PinkyKnuckle,
            TrackedHandJoint.PinkyMetacarpal
        };
        public RawTracker[] rawTrackers;
        public PositionTracker[] physicalTrackers;

        public Hand(Handedness handedness)
        {
            this.handedness = handedness;
        }

        public void Save(string fileName)
        {
            if (!File.Exists(fileName))
                File.Create(fileName);

            using (StreamWriter sw = File.AppendText(fileName))
            {
                StringBuilder s = new StringBuilder();
                for (int i = 0; i < rawTrackers.Length; i++)
                {
                    s.Append(rawTrackers[i].transform.position.x);
                    s.Append(",");
                    s.Append(rawTrackers[i].transform.position.y);
                    s.Append(",");
                    s.Append(rawTrackers[i].transform.position.z);
                    s.Append(",");
                    s.Append(rawTrackers[i].transform.rotation.x);
                    s.Append(",");
                    s.Append(rawTrackers[i].transform.rotation.y);
                    s.Append(",");
                    s.Append(rawTrackers[i].transform.rotation.z);
                    s.Append(",");
                    s.Append(rawTrackers[i].transform.rotation.w);
                    s.Append(",");
                }
                sw.WriteLine(s.ToString());
            }
        }

        public void Init(GameObject trackerPrefab, Transform parent, PhysicsHand hand)
        {
            rawTrackers = new RawTracker[trackedJoints.Length];
            for (int i = 0; i < trackedJoints.Length; i++)
            {
                GameObject tracker = Instantiate(trackerPrefab, parent);
                rawTrackers[i] = tracker.GetComponent<RawTracker>();
            }

            if (hand != null)
            {
                hand.trackers[(int)PhysicsHand.Joints.thumbMetacarpalJoint] = rawTrackers[4].transform;
                hand.trackers[(int)PhysicsHand.Joints.thumbProximalJoint] = rawTrackers[3].transform;

                hand.trackers[(int)PhysicsHand.Joints.indexMetacarpal] = rawTrackers[9].transform;
                hand.trackers[(int)PhysicsHand.Joints.indexKnuckle] = rawTrackers[8].transform;

                hand.trackers[(int)PhysicsHand.Joints.middleMetacarpal] = rawTrackers[14].transform;
                hand.trackers[(int)PhysicsHand.Joints.middleKnuckle] = rawTrackers[13].transform;

                hand.trackers[(int)PhysicsHand.Joints.ringMetacarpal] = rawTrackers[19].transform;
                hand.trackers[(int)PhysicsHand.Joints.ringKnuckle] = rawTrackers[18].transform;

                hand.trackers[(int)PhysicsHand.Joints.pinkyMetacarpal] = rawTrackers[24].transform;
                hand.trackers[(int)PhysicsHand.Joints.pinkyKnuckle] = rawTrackers[23].transform;

                hand.trackers[(int)PhysicsHand.Joints.thumbDistalJoint] = rawTrackers[2].transform;
                hand.trackers[(int)PhysicsHand.Joints.thumbTip] = rawTrackers[1].transform;

                hand.trackers[(int)PhysicsHand.Joints.indexMiddleJoint] = rawTrackers[7].transform;
                hand.trackers[(int)PhysicsHand.Joints.indexDistalJoint] = rawTrackers[6].transform;
                hand.trackers[(int)PhysicsHand.Joints.indexTip] = rawTrackers[5].transform;

                hand.trackers[(int)PhysicsHand.Joints.middleMiddleJoint] = rawTrackers[12].transform;
                hand.trackers[(int)PhysicsHand.Joints.middleDistalJoint] = rawTrackers[11].transform;
                hand.trackers[(int)PhysicsHand.Joints.middleTip] = rawTrackers[10].transform;

                hand.trackers[(int)PhysicsHand.Joints.ringMiddleJoint] = rawTrackers[17].transform;
                hand.trackers[(int)PhysicsHand.Joints.ringDistalJoint] = rawTrackers[16].transform;
                hand.trackers[(int)PhysicsHand.Joints.ringTip] = rawTrackers[15].transform;

                hand.trackers[(int)PhysicsHand.Joints.pinkyMiddleJoint] = rawTrackers[22].transform;
                hand.trackers[(int)PhysicsHand.Joints.pinkyDistalJoint] = rawTrackers[21].transform;
                hand.trackers[(int)PhysicsHand.Joints.pinkyTip] = rawTrackers[20].transform;
            }
        }

        public void Pose()
        {
            for (int i = 0; i < trackedJoints.Length; i++)
            {
                if (HandJointUtils.TryGetJointPose(trackedJoints[i], handedness, out pose))
                {
                    rawTrackers[i].OnDetection();
                    rawTrackers[i].transform.position = pose.Position;
                    rawTrackers[i].transform.rotation = pose.Rotation;
                }
                else rawTrackers[i].OnLoss();
            }
        }
    }

    public PhysicsHand LPHand;
    public PhysicsHand RPHand;

    private Hand LHand;
    private Hand RHand;

    private void Start()
    {
        LHand = new Hand(Handedness.Left);
        RHand = new Hand(Handedness.Right);

        LHand.Init(RawTracker, transform, LPHand);
        RHand.Init(RawTracker, transform, RPHand);
    }

    private void FixedUpdate()
    {
        LHand.Pose();
        RHand.Pose();
    }
}
