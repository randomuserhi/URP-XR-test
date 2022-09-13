using InteractionTK.HandTracking;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ITKDebugHand : MonoBehaviour
{
    public ITKHandUtils.Handedness handedness;
    private ITKHandUtils.Pose pose = new ITKHandUtils.Pose(ITKHandUtils.NumJoints);

    public ITKHand hand;
    public GameObject debugPoint;

    public bool Tracking = true;

    private GameObject wrist;
    private GameObject[][] skeleton;

    private void Start()
    {
        GenerateSkeleton();
    }

    // TODO:: make meta carpals face wrist point to immitate actual track
    // TODO:: auto position and rotate joints into a default state at the start
    private void GenerateSkeleton()
    {
        Vector3 scale = new Vector3(handedness == ITKHandUtils.Handedness.Left ? 1 : -1, 1, 1);

        wrist = debugPoint ? Instantiate(debugPoint) : new GameObject();
        wrist.name = new ITKHandUtils.Joint(ITKHandUtils.Wrist).ToString();
        wrist.transform.parent = transform;
        skeleton = new GameObject[ITKHandUtils.StructureCount.Length][];
        for (int i = 0; i < ITKHandUtils.StructureCount.Length; i++)
        {
            Transform p = wrist.transform;
            skeleton[i] = new GameObject[ITKHandUtils.StructureCount[i]];
            for (int j = 0; j < ITKHandUtils.StructureCount[i]; j++)
            {
                GameObject joint = debugPoint ? Instantiate(debugPoint) : new GameObject();
                joint.transform.parent = p;
                joint.name = new ITKHandUtils.Joint(ITKHandUtils.Structure[i][j]).ToString();

                joint.transform.localPosition = Vector3.Scale(Vector3.Scale(ITKHandUtils.LocalTransformStructure[i][j].position, scale), 
                    new Vector3(
                        1f / wrist.transform.localScale.x,
                        1f / wrist.transform.localScale.y,
                        1f / wrist.transform.localScale.z
                    ));

                skeleton[i][j] = joint;
                p = joint.transform;
            }
        }
    }

    private void FixedUpdate()
    {
        pose.positions[ITKHandUtils.Wrist] = wrist.transform.position;
        pose.rotations[ITKHandUtils.Wrist] = wrist.transform.rotation;

        for (int i = 0; i < ITKHandUtils.StructureCount.Length; i++)
        {
            for (int j = 0; j < ITKHandUtils.StructureCount[i]; j++)
            {
                ITKHandUtils.Joint joint = ITKHandUtils.Structure[i][j];
                pose.positions[joint] = skeleton[i][j].transform.position;
                pose.rotations[joint] = skeleton[i][j].transform.rotation;
            }
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
