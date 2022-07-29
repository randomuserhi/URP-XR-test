using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Filing : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private Transform[] poles;

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        poles = GetComponentsInChildren<Transform>();
    }

    private void Update()
    {
        float dist = Vector3.Distance(poles[0].position, poles[1].position);
        Vector3 dir = (poles[1].position - poles[0].position).normalized;
        Vector3 offset = dir * dist * 0.5f;
        lineRenderer.SetPosition(0, poles[0].position - offset);
        lineRenderer.SetPosition(1, poles[1].position - offset);
    }

}