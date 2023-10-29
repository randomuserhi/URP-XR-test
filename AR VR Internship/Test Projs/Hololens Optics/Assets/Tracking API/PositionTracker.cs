using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

// https://www.youtube.com/watch?v=KPoeNZZ6H4s

public class PositionTracker : MonoBehaviour
{
    public RawTracker target;

    [Range(0f, 10f)]
    public float f = 1;
    [Range(0f, 2f)]
    public float z = 1;
    [Range(-5f, 5f)]
    public float r = 0;

    [Range(0f, 0.5f)]
    public float tolerance = 0.05f;
    public bool teleport = true; // teleport when object regains tracking

    private bool reTrack = false;

    private Vector3 xp = Vector3.zero;
    private Vector3 y = Vector3.zero;
    private Vector3 yd = Vector3.zero;
    private float _w, _z, _d, k1, k2, k3;

    private Vector3? xd;

    private Rigidbody rb;

    private RotationTracker rotationTracker;

    private float sqrMagnitude;

    public bool alwaysTrack = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }

        rotationTracker = GetComponent<RotationTracker>();

        xp = transform.position;
        y = xp;

        _w = 2 * Mathf.PI * f;
        _z = z;
        _d = _w * Mathf.Sqrt(Mathf.Abs(z * z - 1));

        k1 = z / (Mathf.PI * f);
        k2 = 1f / ((2f * Mathf.PI * f) * (2f * Mathf.PI * f));
        k3 = r * z / (2f * Mathf.PI * f);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (target == null) return;

#if UNITY_EDITOR

        _w = 2 * Mathf.PI * f;
        _z = z;
        _d = _w * Mathf.Sqrt(Mathf.Abs(z * z - 1));

        k1 = z / (Mathf.PI * f);
        k2 = 1f / ((2f * Mathf.PI * f) * (2f * Mathf.PI * f));
        k3 = r * z / (2f * Mathf.PI * f);

#endif

        sqrMagnitude = (target.transform.position - transform.position).sqrMagnitude;

        if (target.tracked)
        {
            if (!alwaysTrack) Track();

            if (reTrack && teleport)
            {
                reTrack = false;
                if (sqrMagnitude > tolerance * tolerance)
                {
                    y = target.transform.position;
                    transform.position = target.transform.position;
                    rotationTracker?.SetRotation();
                }
            }
        }
        else
        {
            reTrack = true;
        }

        if (alwaysTrack) Track();
    }

    private void Track()
    {
        if (rb != null) y = transform.position;

        float T = Time.fixedDeltaTime;
        Vector3 x = target.transform.position;

        //if (xd == null) // Commented out as most tracking objects dont have velocity
        //{
        xd = (x - xp) / T;
        xp = x;
        //}

        float k1Stable, k2Stable;
        if (_w * T < _z) //Clamp k2 for stability and prevent jitter
        {
            k1Stable = k1;
            k2Stable = Mathf.Max(k2, T * T / 2 + T * k1 / 2, T * k1);
        }
        else
        {
            float t1 = Mathf.Exp(-_z * _w * T);
            float alpha = 2 * t1 * (_z <= 1 ? Mathf.Cos(T * _d) : math.cosh(T * _d));
            float beta = t1 * t1;
            float t2 = T / (1 + beta - alpha);
            k1Stable = (1 - beta) * t2;
            k2Stable = T * t2;
        }

        if (rb == null || rb.isKinematic) y += T * yd;
        yd += T * (x + k3 * xd.Value - y - k1 * yd) / k2Stable;

        if (rb == null || rb.isKinematic) transform.position = y;
        else rb.velocity = yd;
    }
}
