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

    public class Hand
    {
        public bool lingering = false;
        public float linger = 0;
        public Interactable current;

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
        public RawTracker[] rawTrackers;

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

        public void Init(GameObject trackerPrefab, Transform parent)
        {
            rawTrackers = new RawTracker[trackedJoints.Length];
            for (int i = 0; i < trackedJoints.Length; i++)
            {
                GameObject tracker = Instantiate(trackerPrefab, parent);
                rawTrackers[i] = tracker.GetComponent<RawTracker>();
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

        private static int[] validJoints = new int[]
        {
            0, 2, 3, 4,
            6, 7, 8, 9,
            11, 12, 13, 14,
            16, 17, 18, 19,
            21, 22, 23, 24
        };
        public InteractPoint InRange(Interactable obj)
        {
            InteractPoint p = new InteractPoint() { joint = -1, distance = float.PositiveInfinity };
            Vector3 closest = Vector3.positiveInfinity;
            for (int i = 0; i < validJoints.Length; i++)
            {
                Vector3 pos = rawTrackers[validJoints[i]].transform.position;
                if (obj.colliders != null && obj.colliders.Length > 0)
                {
                    float closestDist = float.PositiveInfinity;
                    for (int j = 0; j < obj.colliders.Length; j++)
                    {
                        Vector3 point = Physics.ClosestPoint(pos, obj.colliders[j], obj.transform.position, obj.transform.rotation);
                        float dist2 = (point - pos).sqrMagnitude;
                        if (dist2 < closestDist)
                        {
                            closestDist = dist2;
                            closest = point;
                        }
                    }
                }
                else
                {
                    closest = obj.transform.position;
                }
                Vector3 dir = pos - closest;
                if (dir.sqrMagnitude < p.distance)
                {
                    p.point = closest;
                    p.distance = dir.sqrMagnitude;
                    p.joint = validJoints[i];
                }
            }
            return p;
        }

        public bool isTracked 
        { 
            get
            {
                bool tracked = true;
                for (int i = 0; i < rawTrackers.Length; i++)
                    if (!rawTrackers[i].tracked) tracked = false;
                return tracked;
            }
        }

        public bool isGrabbing = false;
        private bool checkGrab()
        {
            if (!isTracked) return false;

            bool isGripping = false;
            
            //Only index finger
            Vector3 dir = (rawTrackers[2].transform.position - rawTrackers[6].transform.position);
            float dist = dir.sqrMagnitude;
            if (dist < 0.03f * 0.03f)
            {
                isGripping = true;
            }

            bool enclosed = true;
            bool distal = false;
            bool middle = false;
            bool distal2 = false;
            bool middle2 = false;
            bool proximal = false;
            for (int i = 6; i < rawTrackers.Length; i += 5)
            {
                //All fingers
                /*Vector3 dir = (rawTrackers[2].transform.position - rawTrackers[i].transform.position);
                float dist = dir.sqrMagnitude;
                if (dist < 0.04f * 0.04f)
                {
                    isGripping = true;
                }*/

                dir = (rawTrackers[0].transform.position - rawTrackers[i].transform.position);
                dist = dir.sqrMagnitude;
                if (dist > 0.1f * 0.1f)
                {
                    enclosed = false;
                }

                dir = (rawTrackers[4].transform.position - rawTrackers[i].transform.position);
                dist = dir.sqrMagnitude;
                if (dist < 0.04f * 0.04f)
                {
                    proximal = true;
                }

                dir = (rawTrackers[2].transform.position - rawTrackers[i + 1].transform.position);
                dist = dir.sqrMagnitude;
                if (dist < 0.1f * 0.1f)
                {
                    distal = false;
                }

                dir = (rawTrackers[2].transform.position - rawTrackers[i + 2].transform.position);
                dist = dir.sqrMagnitude;
                if (dist < 0.1f * 0.1f)
                {
                    middle = false;
                }

                dir = (rawTrackers[3].transform.position - rawTrackers[i + 1].transform.position);
                dist = dir.sqrMagnitude;
                if (dist < 0.1f * 0.1f)
                {
                    distal2 = false;
                }

                dir = (rawTrackers[3].transform.position - rawTrackers[i + 2].transform.position);
                dist = dir.sqrMagnitude;
                if (dist < 0.1f * 0.1f)
                {
                    middle2 = false;
                }
            }
            isGripping |= enclosed && (distal || proximal || middle || distal2 || middle2);

            return isGripping;
        }

        public void Update()
        {
            Pose();
            isGrabbing = checkGrab();
        }
    }

    public static Hand LHand;
    public static Hand RHand;

    private void Start()
    {
        LHand = new Hand(Handedness.Left);
        RHand = new Hand(Handedness.Right);

        LHand.Init(RawTracker, transform);
        RHand.Init(RawTracker, transform);
    }

    public static List<Interactable> objects = new List<Interactable>();

    private void Interact(Hand h)
    {
        for (int i = 0; i < h.rawTrackers.Length; i++)
        {
            h.rawTrackers[i].GetComponent<MeshRenderer>().material.color = h.isGrabbing ? Color.red : Color.white;
            h.rawTrackers[i].GetComponent<MeshRenderer>().enabled = h.isTracked;
        }

        if (!h.isTracked)
        {
            if (h.current != null)
            {
                h.current.disposeHands.Add(h);
            }

            h.current = null;
            h.lingering = false;

            return;
        }

        Interactable contact = null;
        InteractPoint interact = new InteractPoint();
        float d = float.PositiveInfinity;
        for (int i = 0; i < objects.Count; i++)
        {
            if (!objects[i].enabled) continue;
            InteractPoint temp = h.InRange(objects[i]);
            if (temp.distance < d)
            {
                interact = temp;
                d = temp.distance;
                if (temp.distance < 0.05f * 0.05f) contact = objects[i];
            }
        }

        float lingerTimer = 0.1f;
        if (contact != null)
        {
            if (!h.lingering && h.current != null && h.current != contact)
            {
                h.linger = lingerTimer;
                h.lingering = true;
            }
        }
        else if (!h.lingering && h.current != null)
        {
            h.linger = lingerTimer;
            h.lingering = true;
        }

        if (h.linger <= 0)
        {
            if (h.current != null && h.current != contact)
            {
                h.current.disposeHands.Add(h);
            }

            if (!h.isGrabbing)
            {
                h.current = contact;
                h.lingering = false;
            }
        }
        else h.linger -= Time.fixedDeltaTime;

        if (h.current != null)
        {
            h.current.disposeHands.Remove(h);
            if (!h.current.hands.ContainsKey(h))
            {
                h.current.hands.Add(h, interact);
            }
            h.current.hands[h] = interact;
        }

        /*Vector3 dir = (h.rawTrackers[2].transform.position - h.rawTrackers[6].transform.position);
        float dist = dir.sqrMagnitude;
        if (dist < 0.04f * 0.04f)
        {
            if (h.current == null)
            {
                GameObject contact = null;
                float d = float.PositiveInfinity;
                for (int i = 0; i < objects.Count; i++)
                {
                    Vector3 o = objects[i].transform.position - h.rawTrackers[2].transform.position;
                    float d2 = o.sqrMagnitude;
                    if (d2 < 0.1f * 0.1f && d2 < d)
                    {
                        d = d2;
                        contact = objects[i].gameObject;
                        if (h.setOffset)
                        {
                            h.setOffset = false;
                            h.offset = o;

                            h.rotOffset = Quaternion.Inverse(objects[i].transform.rotation) * h.rawTrackers[0].transform.rotation;
                        }
                    }
                }
                if (contact != null) h.current = contact;
            }
            else
            {
                h.current.transform.position = h.rawTrackers[2].transform.position + h.offset;
                h.current.transform.rotation = h.rawTrackers[0].transform.rotation * Quaternion.Inverse(h.rotOffset);
            }
        }
        else if (h.current != null)
        {
            h.setOffset = true;
            h.current = null;
        }*/
    }

    private void FixedUpdate()
    {
        LHand.Update();
        RHand.Update();

        Interact(LHand);
        Interact(RHand);

        //LHand.Save(@"C:\Users\Bboy1\Desktop\LHand.txt");
        //RHand.Save(@"C:\Users\Bboy1\Desktop\RHand.txt");
    }
}
