using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldTest : MonoBehaviour
{
    public float sign = 1;
    public float step = 0.03f;
    public float tolerance = 1e-7f;

    Vector3[] buffer = new Vector3[100];

    // Update is called once per frame
    void FixedUpdate()
    {
    }
}
