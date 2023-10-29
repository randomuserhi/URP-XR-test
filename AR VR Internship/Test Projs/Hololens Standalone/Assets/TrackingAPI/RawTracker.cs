using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RawTracker : MonoBehaviour
{
    public bool tracked = true;

    public void OnDetection()
    {
        tracked = true;
    }

    public void OnLoss()
    {
        tracked = false;
    }
}
