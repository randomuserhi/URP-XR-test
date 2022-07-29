using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Filing : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private Transform[] poles;

    public Vector3 relPos;

    public GameObject parent;

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        poles = GetComponentsInChildren<Transform>();
    }

    private Vector3 Snap(Vector3 v)
    {
        return new Vector3(Mathf.Round(v.x / 0.125f) * 0.125f, Mathf.Round(v.y / 0.125f) * 0.125f, Mathf.Round(v.z / 0.125f) * 0.125f);
    }

    private void Update()
    {
        float dist = Vector3.Distance(poles[0].position, poles[1].position);
        Vector3 dir = (poles[1].position - poles[0].position).normalized;
        Vector3 offset = dir * dist * 0.5f;
        lineRenderer.SetPosition(0, poles[0].position - offset);
        lineRenderer.SetPosition(1, poles[1].position - offset);

        transform.position = Snap(parent.transform.position + relPos);
    }

}