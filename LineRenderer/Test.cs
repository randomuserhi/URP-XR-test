using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    private void Update()
    {
        BetterLineRenderer.positions.Add(new Vector3[] { Vector3.zero, Vector3.one, Vector3.one*2 + Vector3.back});
    }
}
