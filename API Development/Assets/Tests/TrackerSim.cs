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

    public ITKHand hand;

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

        Vector3[] pos = new Vector3[ITKHandUtils.NumJoints];
        Quaternion[] rot = new Quaternion[ITKHandUtils.NumJoints];
        int index = 0;
        //pos[HandUtils.Palm] = trackers[index++].transform.position;
        pos[ITKHandUtils.Wrist] = trackers[index++].transform.position;
        pos[ITKHandUtils.ThumbTip] = trackers[index++].transform.position;
        pos[ITKHandUtils.ThumbDistal] = trackers[index++].transform.position;
        pos[ITKHandUtils.ThumbProximal] = trackers[index++].transform.position;
        pos[ITKHandUtils.ThumbMetacarpal] = trackers[index++].transform.position;
        pos[ITKHandUtils.IndexTip] = trackers[index++].transform.position;
        pos[ITKHandUtils.IndexDistal] = trackers[index++].transform.position;
        pos[ITKHandUtils.IndexMiddle] = trackers[index++].transform.position;
        pos[ITKHandUtils.IndexKnuckle] = trackers[index++].transform.position;
        pos[ITKHandUtils.IndexMetacarpal] = trackers[index++].transform.position;
        pos[ITKHandUtils.MiddleTip] = trackers[index++].transform.position;
        pos[ITKHandUtils.MiddleDistal] = trackers[index++].transform.position;
        pos[ITKHandUtils.MiddleMiddle] = trackers[index++].transform.position;
        pos[ITKHandUtils.MiddleKnuckle] = trackers[index++].transform.position;
        pos[ITKHandUtils.MiddleMetacarpal] = trackers[index++].transform.position;
        pos[ITKHandUtils.RingTip] = trackers[index++].transform.position;
        pos[ITKHandUtils.RingDistal] = trackers[index++].transform.position;
        pos[ITKHandUtils.RingMiddle] = trackers[index++].transform.position;
        pos[ITKHandUtils.RingKnuckle] = trackers[index++].transform.position;
        pos[ITKHandUtils.RingMetacarpal] = trackers[index++].transform.position;
        pos[ITKHandUtils.PinkyTip] = trackers[index++].transform.position;
        pos[ITKHandUtils.PinkyDistal] = trackers[index++].transform.position;
        pos[ITKHandUtils.PinkyMiddle] = trackers[index++].transform.position;
        pos[ITKHandUtils.PinkyKnuckle] = trackers[index++].transform.position;
        pos[ITKHandUtils.PinkyMetacarpal] = trackers[index++].transform.position;

        index = 0;
        //rot[HandUtils.Palm] = trackers[index++].transform.rotation;
        rot[ITKHandUtils.Wrist] = trackers[index++].transform.rotation;
        rot[ITKHandUtils.ThumbTip] = trackers[index++].transform.rotation;
        rot[ITKHandUtils.ThumbDistal] = trackers[index++].transform.rotation;
        rot[ITKHandUtils.ThumbProximal] = trackers[index++].transform.rotation;
        rot[ITKHandUtils.ThumbMetacarpal] = trackers[index++].transform.rotation;
        rot[ITKHandUtils.IndexTip] = trackers[index++].transform.rotation;
        rot[ITKHandUtils.IndexDistal] = trackers[index++].transform.rotation;
        rot[ITKHandUtils.IndexMiddle] = trackers[index++].transform.rotation;
        rot[ITKHandUtils.IndexKnuckle] = trackers[index++].transform.rotation;
        rot[ITKHandUtils.IndexMetacarpal] = trackers[index++].transform.rotation;
        rot[ITKHandUtils.MiddleTip] = trackers[index++].transform.rotation;
        rot[ITKHandUtils.MiddleDistal] = trackers[index++].transform.rotation;
        rot[ITKHandUtils.MiddleMiddle] = trackers[index++].transform.rotation;
        rot[ITKHandUtils.MiddleKnuckle] = trackers[index++].transform.rotation;
        rot[ITKHandUtils.MiddleMetacarpal] = trackers[index++].transform.rotation;
        rot[ITKHandUtils.RingTip] = trackers[index++].transform.rotation;
        rot[ITKHandUtils.RingDistal] = trackers[index++].transform.rotation;
        rot[ITKHandUtils.RingMiddle] = trackers[index++].transform.rotation;
        rot[ITKHandUtils.RingKnuckle] = trackers[index++].transform.rotation;
        rot[ITKHandUtils.RingMetacarpal] = trackers[index++].transform.rotation;
        rot[ITKHandUtils.PinkyTip] = trackers[index++].transform.rotation;
        rot[ITKHandUtils.PinkyDistal] = trackers[index++].transform.rotation;
        rot[ITKHandUtils.PinkyMiddle] = trackers[index++].transform.rotation;
        rot[ITKHandUtils.PinkyKnuckle] = trackers[index++].transform.rotation;
        rot[ITKHandUtils.PinkyMetacarpal] = trackers[index++].transform.rotation;

        hand.Enable();
        hand.Track(new ITKHandUtils.Pose()
        {
            positions = pos,
            rotations = rot
        });
    }
}
