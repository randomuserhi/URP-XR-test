using System.Collections;
using System.Collections.Generic;
using UnityEditor.XR.LegacyInputHelpers;
using UnityEngine;

public class VRSpawner : MonoBehaviour
{
    public CameraOffset XRRig;

    private float offset = 0;
    private void Update()
    {
        if (XRRig == null) return;
        if (offset == 0)
        {
            offset = -Camera.main.transform.localPosition.y;
            XRRig.transform.position = transform.position + new Vector3(0, offset, 0);
        }
    }
}
