using InteractionTK.HandTracking;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TrackerSim : MonoBehaviour
{
    /*
    TrackedHandJoint.Palm, 0
    TrackedHandJoint.Wrist, 1

    TrackedHandJoint.ThumbTip, 2
    TrackedHandJoint.ThumbDistalJoint, 3
    TrackedHandJoint.ThumbProximalJoint, 4
    TrackedHandJoint.ThumbMetacarpalJoint, 5

    TrackedHandJoint.IndexTip, 6
    TrackedHandJoint.IndexDistalJoint, 7
    TrackedHandJoint.IndexMiddleJoint, 8
    TrackedHandJoint.IndexKnuckle, 9
    TrackedHandJoint.IndexMetacarpal, 10

    TrackedHandJoint.MiddleTip, 11
    TrackedHandJoint.MiddleDistalJoint, 12
    TrackedHandJoint.MiddleMiddleJoint, 13
    TrackedHandJoint.MiddleKnuckle, 14
    TrackedHandJoint.MiddleMetacarpal, 15

    TrackedHandJoint.RingTip, 16
    TrackedHandJoint.RingDistalJoint, 17
    TrackedHandJoint.RingMiddleJoint, 18
    TrackedHandJoint.RingKnuckle, 19
    TrackedHandJoint.RingMetacarpal, 20

    TrackedHandJoint.PinkyTip, 21
    TrackedHandJoint.PinkyDistalJoint, 22
    TrackedHandJoint.PinkyMiddleJoint, 23
    TrackedHandJoint.PinkyKnuckle, 24
    TrackedHandJoint.PinkyMetacarpal 25
    */

    public GameObject rawTracker;

    [SerializeField]
    public string file;

    private GameObject[] trackers = new GameObject[25];

    private Vector3[] positions;
    private Quaternion[] rotations;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < trackers.Length; i++)
        {
            GameObject t = Instantiate(rawTracker, transform);
            trackers[i] = t;
        }

        string[] lines = File.ReadAllLines(file);
        numFrames = lines.Length;
        positions = new Vector3[lines.Length * 25];
        rotations = new Quaternion[lines.Length * 25];

        for (int i = 0; i < lines.Length; i++)
        {
            string[] values = lines[i].Split(',');
            for (int j = 0, idx = 0; j < (values.Length - 1) / 7; j++) //-1 to remove trailing comma
            {
                positions[i * 25 + j] = new Vector3(float.Parse(values[idx++]), float.Parse(values[idx++]), float.Parse(values[idx++]));
                rotations[i * 25 + j] = new Quaternion(float.Parse(values[idx++]), float.Parse(values[idx++]), float.Parse(values[idx++]), float.Parse(values[idx++]));
            }
        }
    }

    private int frame = 100;
    private int numFrames = 0;
    private float timer = 0;
    public bool play = false;

    public ITKHandPhysics hand;

    // Update is called once per frame
    void FixedUpdate()
    {
        for (int i = 0; i < trackers.Length; i++)
        {
            trackers[i].transform.localPosition = positions[frame * 25 + i];
            trackers[i].transform.localRotation = rotations[frame * 25 + i];
        }

        if (timer >= 0.02f && play)
        {
            frame = (frame + 1) % numFrames;
            timer = 0;
        }
        else timer += Time.fixedDeltaTime;

        if (hand == null) return;

        Vector3[] pos = new Vector3[ITKHand.NumJoints];
        Quaternion[] rot = new Quaternion[ITKHand.NumJoints];
        int index = 0;
        //pos[HandUtils.Palm] = trackers[index++].transform.position;
        pos[ITKHand.Wrist] = trackers[index++].transform.position;
        pos[ITKHand.ThumbTip] = trackers[index++].transform.position;
        pos[ITKHand.ThumbDistal] = trackers[index++].transform.position;
        pos[ITKHand.ThumbProximal] = trackers[index++].transform.position;
        pos[ITKHand.ThumbMetacarpal] = trackers[index++].transform.position;
        pos[ITKHand.IndexTip] = trackers[index++].transform.position;
        pos[ITKHand.IndexDistal] = trackers[index++].transform.position;
        pos[ITKHand.IndexMiddle] = trackers[index++].transform.position;
        pos[ITKHand.IndexKnuckle] = trackers[index++].transform.position;
        pos[ITKHand.IndexMetacarpal] = trackers[index++].transform.position;
        pos[ITKHand.MiddleTip] = trackers[index++].transform.position;
        pos[ITKHand.MiddleDistal] = trackers[index++].transform.position;
        pos[ITKHand.MiddleMiddle] = trackers[index++].transform.position;
        pos[ITKHand.MiddleKnuckle] = trackers[index++].transform.position;
        pos[ITKHand.MiddleMetacarpal] = trackers[index++].transform.position;
        pos[ITKHand.RingTip] = trackers[index++].transform.position;
        pos[ITKHand.RingDistal] = trackers[index++].transform.position;
        pos[ITKHand.RingMiddle] = trackers[index++].transform.position;
        pos[ITKHand.RingKnuckle] = trackers[index++].transform.position;
        pos[ITKHand.RingMetacarpal] = trackers[index++].transform.position;
        pos[ITKHand.PinkyTip] = trackers[index++].transform.position;
        pos[ITKHand.PinkyDistal] = trackers[index++].transform.position;
        pos[ITKHand.PinkyMiddle] = trackers[index++].transform.position;
        pos[ITKHand.PinkyKnuckle] = trackers[index++].transform.position;
        pos[ITKHand.PinkyMetacarpal] = trackers[index++].transform.position;

        index = 0;
        //rot[HandUtils.Palm] = trackers[index++].transform.rotation;
        rot[ITKHand.Wrist] = trackers[index++].transform.rotation;
        rot[ITKHand.ThumbTip] = trackers[index++].transform.rotation;
        rot[ITKHand.ThumbDistal] = trackers[index++].transform.rotation;
        rot[ITKHand.ThumbProximal] = trackers[index++].transform.rotation;
        rot[ITKHand.ThumbMetacarpal] = trackers[index++].transform.rotation;
        rot[ITKHand.IndexTip] = trackers[index++].transform.rotation;
        rot[ITKHand.IndexDistal] = trackers[index++].transform.rotation;
        rot[ITKHand.IndexMiddle] = trackers[index++].transform.rotation;
        rot[ITKHand.IndexKnuckle] = trackers[index++].transform.rotation;
        rot[ITKHand.IndexMetacarpal] = trackers[index++].transform.rotation;
        rot[ITKHand.MiddleTip] = trackers[index++].transform.rotation;
        rot[ITKHand.MiddleDistal] = trackers[index++].transform.rotation;
        rot[ITKHand.MiddleMiddle] = trackers[index++].transform.rotation;
        rot[ITKHand.MiddleKnuckle] = trackers[index++].transform.rotation;
        rot[ITKHand.MiddleMetacarpal] = trackers[index++].transform.rotation;
        rot[ITKHand.RingTip] = trackers[index++].transform.rotation;
        rot[ITKHand.RingDistal] = trackers[index++].transform.rotation;
        rot[ITKHand.RingMiddle] = trackers[index++].transform.rotation;
        rot[ITKHand.RingKnuckle] = trackers[index++].transform.rotation;
        rot[ITKHand.RingMetacarpal] = trackers[index++].transform.rotation;
        rot[ITKHand.PinkyTip] = trackers[index++].transform.rotation;
        rot[ITKHand.PinkyDistal] = trackers[index++].transform.rotation;
        rot[ITKHand.PinkyMiddle] = trackers[index++].transform.rotation;
        rot[ITKHand.PinkyKnuckle] = trackers[index++].transform.rotation;
        rot[ITKHand.PinkyMetacarpal] = trackers[index++].transform.rotation;

        hand.Enable();
        hand.Track(new ITKHand.Pose()
        {
            positions = pos,
            rotations = rot
        });
    }
}
