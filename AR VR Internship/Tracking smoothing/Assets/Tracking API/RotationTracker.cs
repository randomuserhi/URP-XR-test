using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

// https://www.youtube.com/watch?v=KPoeNZZ6H4s

public class RotationTracker : MonoBehaviour
{
    [SerializeField]
    private RawTracker target;

    [Range(0f, 10f)]
    public float f = 1;
    [Range(0f, 2f)]
    public float z = 1;
    [Range(-5f, 5f)]
    public float r = 0;

    private Vector3 xp = Vector3.zero;
    private Vector3 y = Vector3.zero;
    private Vector3 yd = Vector3.zero;
    private float _w, _z, _d, k1, k2, k3;

    private Vector3? xd;

    void Start()
    {
        xp = Vector3.zero;
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

        if (target.tracked)
        {
            Track();
        }
    }

    public void SetRotation()
    {
        y = target.transform.rotation.eulerAngles;
    }

    void Track()
    {
        float T = Time.fixedDeltaTime;
        Vector3 x = target.transform.rotation.eulerAngles;

        // Angle logic to account for 0 -> 359 when rotating backwards or 359 -> 0 when rotating forwards and
        // to stop rotation from just looping all the way round.
        // y -= 360 * (xp - x - 180) / (xp - x - 180);
        if ((xp - x).x > 180) y.x -= 360;
        else if ((x - xp).x > 180) y.x += 360;

        if ((xp - x).y > 180) y.y -= 360;
        else if ((x - xp).y > 180) y.y += 360;

        if ((xp - x).z > 180) y.z -= 360;
        else if ((x - xp).z > 180) y.z += 360;

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

        y += T * yd;
        yd += T * (x + k3 * xd.Value - y - k1 * yd) / k2Stable;

        //Debug.Log(x + ", " + y);

        Quaternion rot = transform.rotation;
        rot.eulerAngles = y;
        transform.rotation = rot;
    }
}
