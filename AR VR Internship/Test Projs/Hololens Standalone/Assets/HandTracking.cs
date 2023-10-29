using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Microsoft;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.Input;

// DEPRECATED

public class HandTracking : MonoBehaviour
{
    public GameObject sphereMarker;

    private GameObject[] rightFingerObjects = new GameObject[5];
    private Renderer[] rightFingerObjectRenderers = new Renderer[5];
    private GameObject[] leftFingerObjects = new GameObject[5];
    private Renderer[] leftFingerObjectRenderers = new Renderer[5];

    private TrackedHandJoint[] fingerTips = new TrackedHandJoint[5] { TrackedHandJoint.ThumbTip, 
                                                                      TrackedHandJoint.IndexTip,
                                                                      TrackedHandJoint.MiddleTip,
                                                                      TrackedHandJoint.RingTip,
                                                                      TrackedHandJoint.PinkyTip };

    private MixedRealityPose rightPose;
    private MixedRealityPose leftPose;

    private void Start()
    {
        for (int i = 0; i < rightFingerObjects.Length; i++)
        {
            rightFingerObjects[i] = Instantiate(sphereMarker);
            rightFingerObjectRenderers[i] = rightFingerObjects[i].GetComponent<Renderer>();
            leftFingerObjects[i] = Instantiate(sphereMarker);
            leftFingerObjectRenderers[i] = leftFingerObjects[i].GetComponent<Renderer>();
        }
    }

    private void Update()
    {
        for (int i = 0; i < fingerTips.Length; i++)
        {
            if (HandJointUtils.TryGetJointPose(fingerTips[i], Handedness.Right, out rightPose))
            {
                rightFingerObjectRenderers[i].enabled = true;
                rightFingerObjectRenderers[i].GetComponent<RawTracker>().OnDetection();
                rightFingerObjects[i].transform.position = rightPose.Position;
            } else
            {
                rightFingerObjectRenderers[i].enabled = false;
                rightFingerObjectRenderers[i].GetComponent<RawTracker>().OnLoss();
            }

            if (HandJointUtils.TryGetJointPose(fingerTips[i], Handedness.Left, out leftPose))
            {
                leftFingerObjectRenderers[i].enabled = true;
                leftFingerObjectRenderers[i].GetComponent<RawTracker>().OnDetection();
                leftFingerObjects[i].transform.position = leftPose.Position;
            }
            else
            {
                leftFingerObjectRenderers[i].enabled = false;
                leftFingerObjectRenderers[i].GetComponent<RawTracker>().OnDetection();
            }
        }
    }
}
