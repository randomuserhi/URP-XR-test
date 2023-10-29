using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TrackerSim : MonoBehaviour
{
    /*
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
    */

    public GameObject rawTracker;

    [SerializeField]
    public PhysicsHand hand;

    [SerializeField]
    public string file;

    private RawTracker[] trackers = new RawTracker[25];

    private Vector3[] positions;
    private Quaternion[] rotations;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < trackers.Length; i++)
        {
            GameObject t = Instantiate(rawTracker, transform);
            trackers[i] = t.GetComponent<RawTracker>();
        }

        if (hand != null)
        {
            hand.trackers[(int)PhysicsHand.Joints.thumbMetacarpalJoint] = trackers[4].transform;
            hand.trackers[(int)PhysicsHand.Joints.thumbProximalJoint] = trackers[3].transform;

            hand.trackers[(int)PhysicsHand.Joints.indexMetacarpal] = trackers[9].transform;
            hand.trackers[(int)PhysicsHand.Joints.indexKnuckle] = trackers[8].transform;

            hand.trackers[(int)PhysicsHand.Joints.middleMetacarpal] = trackers[14].transform;
            hand.trackers[(int)PhysicsHand.Joints.middleKnuckle] = trackers[13].transform;

            hand.trackers[(int)PhysicsHand.Joints.ringMetacarpal] = trackers[19].transform;
            hand.trackers[(int)PhysicsHand.Joints.ringKnuckle] = trackers[18].transform;

            hand.trackers[(int)PhysicsHand.Joints.pinkyMetacarpal] = trackers[24].transform;
            hand.trackers[(int)PhysicsHand.Joints.pinkyKnuckle] = trackers[23].transform;

            hand.trackers[(int)PhysicsHand.Joints.thumbDistalJoint] = trackers[2].transform;
            hand.trackers[(int)PhysicsHand.Joints.thumbTip] = trackers[1].transform;

            hand.trackers[(int)PhysicsHand.Joints.indexMiddleJoint] = trackers[7].transform;
            hand.trackers[(int)PhysicsHand.Joints.indexDistalJoint] = trackers[6].transform;
            hand.trackers[(int)PhysicsHand.Joints.indexTip] = trackers[5].transform;

            hand.trackers[(int)PhysicsHand.Joints.middleMiddleJoint] = trackers[12].transform;
            hand.trackers[(int)PhysicsHand.Joints.middleDistalJoint] = trackers[11].transform;
            hand.trackers[(int)PhysicsHand.Joints.middleTip] = trackers[10].transform;

            hand.trackers[(int)PhysicsHand.Joints.ringMiddleJoint] = trackers[17].transform;
            hand.trackers[(int)PhysicsHand.Joints.ringDistalJoint] = trackers[16].transform;
            hand.trackers[(int)PhysicsHand.Joints.ringTip] = trackers[15].transform;

            hand.trackers[(int)PhysicsHand.Joints.pinkyMiddleJoint] = trackers[22].transform;
            hand.trackers[(int)PhysicsHand.Joints.pinkyDistalJoint] = trackers[21].transform;
            hand.trackers[(int)PhysicsHand.Joints.pinkyTip] = trackers[20].transform;
        }

        string[] lines = File.ReadAllLines(file);
        numFrames = lines.Length;
        positions = new Vector3[lines.Length * 25];
        rotations = new Quaternion[lines.Length * 25];

        for (int i = 0; i < lines.Length; i++)
        {
            string[] values = lines[i].Split(",");
            for (int j = 0, idx = 0; j < (values.Length - 1) / 7; j++) //-1 to remove trailing comma
            {
                positions[i * 25 + j] = new Vector3(float.Parse(values[idx++]), float.Parse(values[idx++]), float.Parse(values[idx++]));
                rotations[i * 25 + j] = new Quaternion(float.Parse(values[idx++]), float.Parse(values[idx++]), float.Parse(values[idx++]), float.Parse(values[idx++]));
            }
        }
    }

    private int frame = 0;
    private int numFrames = 0;

    // Update is called once per frame
    void FixedUpdate()
    {
        for (int i = 0; i < trackers.Length; i++)
        {
            trackers[i].transform.localPosition = positions[frame * 25 + i];
            trackers[i].transform.localRotation = rotations[frame * 25 + i];
        }

        frame = (frame + 1) % numFrames;
    }
}
