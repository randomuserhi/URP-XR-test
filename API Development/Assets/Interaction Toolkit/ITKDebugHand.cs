using InteractionTK.HandTracking;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ITKDebugHand : MonoBehaviour
{
    public ITKHandUtils.Handedness type;
    private ITKHandUtils.Pose pose = new ITKHandUtils.Pose(ITKHandUtils.NumJoints);

    public ITKHand hand;
    public GameObject debugPoint;

    public bool Tracking = true;

    private GameObject wrist;
    private GameObject[][] skeleton;

    private void Start()
    {
        
    }

    private void FixedUpdate()
    {
        if (hand.type != type)
        {
            Debug.LogError("Tracked hand type does not match the type of the ITKHand.");
            return;
        }

        hand.Track(pose);
    }
}
