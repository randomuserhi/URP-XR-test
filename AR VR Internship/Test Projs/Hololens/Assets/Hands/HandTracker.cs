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
    public GameObject PhysicsTrackers;
    public GameObject SoftBodyParticle;

    [System.Serializable]
    private class TrackerSettings
    {
        [Range(0f, 10f)]
        public float f = 1;
        [Range(0f, 2f)]
        public float z = 1;
        [Range(-5f, 5f)]
        public float r = 0;
    }

    [SerializeField]
    private TrackerSettings trackerSettings;

    private class Hand
    {
        public bool setOffset = true;
        public Vector3 offset;
        public Quaternion rotOffset;
        public PositionTracker current;

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
            TrackedHandJoint.Palm,
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

        public void Init(GameObject trackerPrefab, GameObject physicalTrackerPrefab, GameObject particlePrefab, Transform parent, TrackerSettings settings)
        {
            rawTrackers = new RawTracker[trackedJoints.Length];
            physicalTrackers = new PositionTracker[trackedJoints.Length];
            for (int i = 0; i < trackedJoints.Length; i++)
            {
                GameObject tracker = Instantiate(trackerPrefab, parent);
                rawTrackers[i] = tracker.GetComponent<RawTracker>();

                GameObject physical = Instantiate(physicalTrackerPrefab, parent);
                physicalTrackers[i] = physical.GetComponent<PositionTracker>();

                physicalTrackers[i].target = rawTrackers[i];
                physicalTrackers[i].f = settings.f;
                physicalTrackers[i].r = settings.r;
                physicalTrackers[i].z = settings.z;
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

    private Hand LHand;
    private Hand RHand;

    private FilingRenderer f;

    private void Start()
    {
        f = GetComponent<FilingRenderer>();

        LHand = new Hand(Handedness.Left);
        RHand = new Hand(Handedness.Right);

        LHand.Init(RawTracker, PhysicsTrackers, SoftBodyParticle, transform, trackerSettings);
        RHand.Init(RawTracker, PhysicsTrackers, SoftBodyParticle, transform, trackerSettings);
    }

    private void Pinch(Hand h)
    {
        Vector3 dir = (h.rawTrackers[2].transform.position - h.rawTrackers[6].transform.position);
        float dist = dir.sqrMagnitude;
        if (dist < 0.04f * 0.04f)
        {
            if (h.current == null)
            {
                GameObject contact = null;
                for (int i = 0; i < f.magnets.Length; i++)
                {
                    Vector3 o = f.magnets[i].transform.position - h.rawTrackers[2].transform.position;
                    float d = o.sqrMagnitude;
                    if (d < 0.1f * 0.1f)
                    {
                        contact = f.magnets[i];
                        if (h.setOffset)
                        {
                            h.setOffset = false;
                            h.offset = o;

                            h.rotOffset = Quaternion.Inverse(f.magnets[i].transform.rotation) * h.rawTrackers[0].transform.rotation;
                        }
                        break;
                    }
                }
                if (contact != null) h.current = contact.GetComponent<PositionTracker>();
            }
            else
            {
                h.current.target.transform.position = h.rawTrackers[2].transform.position + h.offset;
                h.current.target.transform.rotation = h.rawTrackers[0].transform.rotation * Quaternion.Inverse(h.rotOffset);

                h.current.target.tracked = true;
            }
        }
        else if (h.current != null)
        {
            h.setOffset = true;
            h.current.target.tracked = false;
            h.current = null;
        }
    }

    private void FixedUpdate()
    {
        LHand.Pose();
        RHand.Pose();

        Pinch(LHand);
        Pinch(RHand);
    }
}
