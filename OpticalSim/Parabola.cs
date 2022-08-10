using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parabola : MonoBehaviour
{
    public float bound = 0f;
    public float s = 1f;
    public bool backface = false;

    private Lens l;
    public void Start()
    {
        l = GetComponentInParent<Lens>();
        if (l == null) Debug.LogError("No lens component");
    }

    private static int Quadratic(float a, float b, float c, float[] answers)
    {
        float det = b * b - 4 * a * c;
        if (det < 0) return 0;
        else
        {
            answers[0] = (-b + Mathf.Sqrt(det)) / (2f * a);
            answers[1] = (-b - Mathf.Sqrt(det)) / (2f * a);
            return det > 0 ? 2 : 1;
        }
    }

    public int Solve(Vector3 pos, Vector3 dir, RayHit[] hits)
    {
        if (dir == Vector3.zero) dir = Vector3.forward;

        Vector3 relPos = transform.rotation * (pos - transform.position);
        Vector3 relDir = transform.rotation * dir;

        float A = relPos.x;
        float B = relPos.y;
        float C = relPos.z;
        float D = relDir.x;
        float E = relDir.y;
        float F = relDir.z;

        if (D == 0 && E == 0)
        {
            ref RayHit hit = ref hits[0];
            hit.point = new Vector3(A, B, s * (A*A + B*B));
            hit.l = l;
            if (hit.point.z > bound) return 0;

            hit.normal = new Vector3(2f * s * hit.point.x, 2f * s * hit.point.y, -1f);

            hit.backface = backface;
            hit.normal = Quaternion.Inverse(transform.rotation) * hit.normal;
            hit.point = Quaternion.Inverse(transform.rotation) * hit.point + transform.position;
            return 1;
        }

        ref float a = ref (D == 0 ? ref B : ref A);
        ref float b = ref (D == 0 ? ref A : ref B);
        ref float c = ref C;
        ref float d = ref (D == 0 ? ref E : ref D);
        ref float e = ref (D == 0 ? ref D : ref E);
        ref float f = ref F;

        float aq = s * (e * e / d + d);
        float bq = 2 * s * (b * e - e * e * a / d) - f;
        float cq = s * (e * e * a * a / d - 2f * b * e * a + d * b * b) - d * c + f * a;

        float[] X = new float[2];
        int count = Quadratic(aq, bq, cq, X);
        for (int i = 0; i < count; i++)
        {
            ref RayHit hit = ref hits[i];
            hit.l = l;
            float comp = (X[i] - a) / d;
            if (D == 0)
                hit.point = new Vector3()
                {
                    x = e * comp + b,
                    y = X[i],
                    z = f * comp + c
                };
            else
                hit.point = new Vector3()
                {
                    x = X[i],
                    y = e * comp + b,
                    z = f * comp + c
                };

            if (hit.point.z > bound)
            {
                count -= 1;
                i -= 1;
                continue;
            }

            hit.normal = new Vector3(2f * s * hit.point.x, 2f * s * hit.point.y, -1f);

            hit.backface = backface;
            hit.normal = Quaternion.Inverse(transform.rotation) * hit.normal;
            hit.point = Quaternion.Inverse(transform.rotation) * hit.point + transform.position;
        }
        return count;
    }
}
