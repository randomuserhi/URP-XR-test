using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using UnityEngine;

using Microsoft;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using InteractionTK.HandTracking;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public class ITKHandTracker : MonoBehaviour
{
    public class Hand
    {
        public Handedness handedness;
        private MixedRealityPose pose;

        private TrackedHandJoint[] trackedJoints = new TrackedHandJoint[] {
            TrackedHandJoint.Palm, // 0
            TrackedHandJoint.Wrist, // 1

            TrackedHandJoint.ThumbTip, // 2
            TrackedHandJoint.ThumbDistalJoint, // 3
            TrackedHandJoint.ThumbProximalJoint, // 4
            TrackedHandJoint.ThumbMetacarpalJoint, // 5

            TrackedHandJoint.IndexTip, // 6
            TrackedHandJoint.IndexDistalJoint, // 7
            TrackedHandJoint.IndexMiddleJoint, // 8
            TrackedHandJoint.IndexKnuckle, // 9
            TrackedHandJoint.IndexMetacarpal, // 10

            TrackedHandJoint.MiddleTip, // 11
            TrackedHandJoint.MiddleDistalJoint, // 12
            TrackedHandJoint.MiddleMiddleJoint, // 13
            TrackedHandJoint.MiddleKnuckle, // 14
            TrackedHandJoint.MiddleMetacarpal, // 15

            TrackedHandJoint.RingTip, // 16
            TrackedHandJoint.RingDistalJoint, // 17
            TrackedHandJoint.RingMiddleJoint, // 18
            TrackedHandJoint.RingKnuckle, // 19
            TrackedHandJoint.RingMetacarpal, // 20

            TrackedHandJoint.PinkyTip, // 21
            TrackedHandJoint.PinkyDistalJoint, // 22
            TrackedHandJoint.PinkyMiddleJoint, // 23
            TrackedHandJoint.PinkyKnuckle, // 24
            TrackedHandJoint.PinkyMetacarpal // 25
        };
        public GameObject[] trackers;

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
                for (int i = 0; i < trackers.Length; i++)
                {
                    s.Append(trackers[i].transform.position.x);
                    s.Append(",");
                    s.Append(trackers[i].transform.position.y);
                    s.Append(",");
                    s.Append(trackers[i].transform.position.z);
                    s.Append(",");
                    s.Append(trackers[i].transform.rotation.x);
                    s.Append(",");
                    s.Append(trackers[i].transform.rotation.y);
                    s.Append(",");
                    s.Append(trackers[i].transform.rotation.z);
                    s.Append(",");
                    s.Append(trackers[i].transform.rotation.w);
                    s.Append(",");
                }
                sw.WriteLine(s.ToString());
            }
        }

        public void Init(GameObject trackerPrefab, Transform parent)
        {
            trackers = new GameObject[trackedJoints.Length];
            for (int i = 0; i < trackedJoints.Length; i++)
            {
                GameObject tracker = Instantiate(trackerPrefab, parent);
                trackers[i] = tracker;
            }
        }
        public void Pose()
        {
            for (int i = 0; i < trackedJoints.Length; i++)
            {
                if (HandJointUtils.TryGetJointPose(trackedJoints[i], handedness, out pose))
                {
                    trackers[i].transform.position = pose.Position;
                    trackers[i].transform.rotation = pose.Rotation;
                }
            }
        }

        public void Update()
        {
            Pose();
        }
    }

    private Hand LHand;
    private Hand RHand;

    public GameObject trackObj;
    public ITKHand ITKLHand;
    //public ITKHand ITKRHand;

    private void Start()
    {
        LHand = new Hand(Handedness.Left);
        RHand = new Hand(Handedness.Right);

        LHand.Init(trackObj, transform);
        RHand.Init(trackObj, transform);
    }

    private void FixedUpdate()
    {
        LHand.Update();
        RHand.Update();

        Vector3[] pos = new Vector3[HandUtils.NumJoints];
        Quaternion[] rot = new Quaternion[HandUtils.NumJoints];
        int index = 0;
        pos[HandUtils.Palm] = LHand.trackers[index++].transform.position;
        pos[HandUtils.Wrist] = LHand.trackers[index++].transform.position;
        pos[HandUtils.ThumbTip] = LHand.trackers[index++].transform.position;
        pos[HandUtils.ThumbDistal] = LHand.trackers[index++].transform.position;
        pos[HandUtils.ThumbProximal] = LHand.trackers[index++].transform.position;
        pos[HandUtils.ThumbMetacarpal] = LHand.trackers[index++].transform.position;
        pos[HandUtils.IndexTip] = LHand.trackers[index++].transform.position;
        pos[HandUtils.IndexDistal] = LHand.trackers[index++].transform.position;
        pos[HandUtils.IndexMiddle] = LHand.trackers[index++].transform.position;
        pos[HandUtils.IndexKnuckle] = LHand.trackers[index++].transform.position;
        pos[HandUtils.IndexMetacarpal] = LHand.trackers[index++].transform.position;
        pos[HandUtils.MiddleTip] = LHand.trackers[index++].transform.position;
        pos[HandUtils.MiddleDistal] = LHand.trackers[index++].transform.position;
        pos[HandUtils.MiddleMiddle] = LHand.trackers[index++].transform.position;
        pos[HandUtils.MiddleKnuckle] = LHand.trackers[index++].transform.position;
        pos[HandUtils.MiddleMetacarpal] = LHand.trackers[index++].transform.position;
        pos[HandUtils.RingTip] = LHand.trackers[index++].transform.position;
        pos[HandUtils.RingDistal] = LHand.trackers[index++].transform.position;
        pos[HandUtils.RingMiddle] = LHand.trackers[index++].transform.position;
        pos[HandUtils.RingKnuckle] = LHand.trackers[index++].transform.position;
        pos[HandUtils.RingMetacarpal] = LHand.trackers[index++].transform.position;
        pos[HandUtils.PinkyTip] = LHand.trackers[index++].transform.position;
        pos[HandUtils.PinkyDistal] = LHand.trackers[index++].transform.position;
        pos[HandUtils.PinkyMiddle] = LHand.trackers[index++].transform.position;
        pos[HandUtils.PinkyKnuckle] = LHand.trackers[index++].transform.position;
        pos[HandUtils.PinkyMetacarpal] = LHand.trackers[index++].transform.position;

        index = 0;
        rot[HandUtils.Palm] = LHand.trackers[index++].transform.rotation;
        rot[HandUtils.Wrist] = LHand.trackers[index++].transform.rotation;
        rot[HandUtils.ThumbTip] = LHand.trackers[index++].transform.rotation;
        rot[HandUtils.ThumbDistal] = LHand.trackers[index++].transform.rotation;
        rot[HandUtils.ThumbProximal] = LHand.trackers[index++].transform.rotation;
        rot[HandUtils.ThumbMetacarpal] = LHand.trackers[index++].transform.rotation;
        rot[HandUtils.IndexTip] = LHand.trackers[index++].transform.rotation;
        rot[HandUtils.IndexDistal] = LHand.trackers[index++].transform.rotation;
        rot[HandUtils.IndexMiddle] = LHand.trackers[index++].transform.rotation;
        rot[HandUtils.IndexKnuckle] = LHand.trackers[index++].transform.rotation;
        rot[HandUtils.IndexMetacarpal] = LHand.trackers[index++].transform.rotation;
        rot[HandUtils.MiddleTip] = LHand.trackers[index++].transform.rotation;
        rot[HandUtils.MiddleDistal] = LHand.trackers[index++].transform.rotation;
        rot[HandUtils.MiddleMiddle] = LHand.trackers[index++].transform.rotation;
        rot[HandUtils.MiddleKnuckle] = LHand.trackers[index++].transform.rotation;
        rot[HandUtils.MiddleMetacarpal] = LHand.trackers[index++].transform.rotation;
        rot[HandUtils.RingTip] = LHand.trackers[index++].transform.rotation;
        rot[HandUtils.RingDistal] = LHand.trackers[index++].transform.rotation;
        rot[HandUtils.RingMiddle] = LHand.trackers[index++].transform.rotation;
        rot[HandUtils.RingKnuckle] = LHand.trackers[index++].transform.rotation;
        rot[HandUtils.RingMetacarpal] = LHand.trackers[index++].transform.rotation;
        rot[HandUtils.PinkyTip] = LHand.trackers[index++].transform.rotation;
        rot[HandUtils.PinkyDistal] = LHand.trackers[index++].transform.rotation;
        rot[HandUtils.PinkyMiddle] = LHand.trackers[index++].transform.rotation;
        rot[HandUtils.PinkyKnuckle] = LHand.trackers[index++].transform.rotation;
        rot[HandUtils.PinkyMetacarpal] = LHand.trackers[index++].transform.rotation;

        ITKLHand.Track(new HandUtils.Pose()
        {
            positions = pos,
            rotations = rot
        });
    }
}
