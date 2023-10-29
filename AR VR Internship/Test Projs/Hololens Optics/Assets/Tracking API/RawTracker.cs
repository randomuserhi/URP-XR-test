using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RawTracker : MonoBehaviour
{
    public bool tracked = true;

    Vector3 pos;
    Quaternion rot;

    private void Start()
    {
        pos = transform.localPosition;
        rot = transform.localRotation;
    }

    public void OnDetection()
    {
        tracked = true;
        transform.localPosition = pos;
        transform.localRotation = rot;
    }

    public void OnLoss()
    {
        tracked = false;
    }
}
