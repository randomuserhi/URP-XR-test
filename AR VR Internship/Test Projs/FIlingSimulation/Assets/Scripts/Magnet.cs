using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Magnet : MonoBehaviour
{
    public enum Pole
    {
        north = 0,
        south = 1,
    }

    public float magnetForce = 1;
    public Pole pole;
    public Rigidbody rb;
    public bool filing;

    private float p = 0.05f;
    private float maxF = 10000f;

    private void Start()
    {
        rb = transform.parent.GetComponent<Rigidbody>();
        if (!filing)
            FilingManager.magnets.Add(this);
    }

    private void FixedUpdate()
    {
        if (!filing)
            return;

        if (rb == null)
            return;

        Vector3 f1 = Vector3.zero;

        for (int j = 0; j < FilingManager.magnets.Count; j++)
        {
            Magnet m2 = FilingManager.magnets[j];

            if (m2.magnetForce < 5.0f)
                continue;

            if (transform.parent == m2.transform.parent)
                continue;

            Vector3 f = CalculateForce(this, m2);

            f1 += f * magnetForce * m2.magnetForce;
        }

        f1 = (f1.magnitude > maxF) ? f1.normalized * maxF : f1;

        rb.AddForceAtPosition(f1, transform.position);
    }

    private Vector3 CalculateForce(Magnet m1, Magnet m2)
    {
        Vector3 r = m2.transform.position - m1.transform.position;
        float dist = r.magnitude;
        float p0 = p * m1.magnetForce * m2.magnetForce;
        float p1 = 4 * Mathf.PI * dist;
        float f = p0 / p1;
        if (m1.pole == m2.pole)
            f = -f;

        return f * r.normalized;
    }
}
