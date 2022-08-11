using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Light Toolkit

namespace LightTK
{
    public class LTK
    {
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

        public static int GetRelativeIntersection(Vector3 origin, Vector3 dir, Curve curve, RayHit[] points)
        {
            return GetIntersection(origin, dir, curve.parameters, points, true);
        }

        public static int GetRelativeIntersection(Vector3 origin, Vector3 dir, CurveParameter curve, RayHit[] points)
        {
            return GetIntersection(origin, dir, curve, points, true);
        }

        public static int GetIntersection(Vector3 origin, Vector3 dir, Curve curve, RayHit[] points, bool relative = false)
        {
            return GetIntersection(origin, dir, curve.parameters, points, relative);
        }
        public static int GetIntersection(Vector3 origin, Vector3 dir, CurveParameter curve, RayHit[] points, bool relative = false)
        {
            if (!relative)
            {
                origin = curve.rotation * (origin - curve.position);
                dir = curve.rotation * dir;
            }

            float a = origin.x;
            float b = origin.y;
            float c = origin.z;
            float d = dir.x;
            float e = dir.y;
            float f = dir.z;
            float[] solutions = new float[2];
            float aq, bq, cq;
            int count;

            if (f == 0)
            {
                aq = curve.l * curve.h * curve.p * curve.q / (curve.s * curve.s);
                bq = curve.q * (curve.p - 1f);
                cq = curve.l * (a * a + b * b) - curve.r;

                count = Quadratic(aq, bq, cq, solutions);
                if (count == 0) return 0;
                else for (int i = 0, j = 0, length = count; i < length; j++, i++)
                    {
                        float z = solutions[i];
                        ref RayHit hit = ref points[j];
                        hit.point = new Vector3(
                            a,
                            b,
                            z);

                        hit.normal = new Vector3(
                            2f * curve.l * hit.point.x,

                            2f * curve.l * hit.point.y,

                            2f * curve.l * curve.h * curve.p * curve.q / (curve.s * curve.s) * hit.point.z
                            + curve.q * (curve.p - 1)
                            );

                        bool isBound;
                        if (curve.radial != 0f)
                            isBound = hit.point.sqrMagnitude < curve.radial * curve.radial;
                        else
                            isBound = (hit.point.x >= curve.maximum.x) || (hit.point.x <= curve.minimum.x) ||
                                      (hit.point.y >= curve.maximum.y) || (hit.point.y <= curve.minimum.y) ||
                                      (hit.point.z >= curve.maximum.z) || (hit.point.z <= curve.minimum.z);
                        
                        if (isBound)
                        {
                            count -= 1;
                            j -= 1;
                            continue;
                        }

                        if (!relative)
                        {
                            hit.normal = Quaternion.Inverse(curve.rotation) * hit.normal;
                            hit.point = Quaternion.Inverse(curve.rotation) * hit.point + curve.position;
                        }
                    }

                return count;
            }

            float ddee = d * d + e * e;
            float adbe = a * d + b * e;
            float cf = c / f;
            
            aq = curve.l * ((curve.h * curve.p * curve.q) / (curve.s * curve.s) + ddee/(f * f));
            bq = curve.q * (curve.p - 1f) - (2f * curve.l / f) * (c / f * ddee + adbe);
            cq = curve.l * (cf * cf * ddee - 2f * cf * adbe + a * a + b * b) - curve.r;

            count = Quadratic(aq, bq, cq, solutions);
            if (count == 0) return 0;
            else for (int i = 0, j = 0, length = count; i < length; j++, i++)
                {
                    float z = solutions[i];
                    ref RayHit hit = ref points[j];
                    hit.point = new Vector3(
                        d / f * (z - c) + a,
                        e / f * (z - c) + b,
                        z);

                    hit.normal = new Vector3(
                        2f * curve.l * hit.point.x,

                        2f * curve.l * hit.point.y,

                        2f * curve.l * curve.h * curve.p * curve.q / (curve.s * curve.s) * hit.point.z
                        + curve.q * (curve.p - 1)
                        );

                    bool isBound;
                    if (curve.radial != 0f)
                        isBound = hit.point.sqrMagnitude < curve.radial * curve.radial;
                    else
                        isBound = (hit.point.x >= curve.maximum.x) || (hit.point.x <= curve.minimum.x) ||
                                  (hit.point.y >= curve.maximum.y) || (hit.point.y <= curve.minimum.y) ||
                                  (hit.point.z >= curve.maximum.z) || (hit.point.z <= curve.minimum.z);

                    if (isBound)
                    {
                        count -= 1;
                        j -= 1;
                        continue;
                    }

                    if (!relative)
                    {
                        hit.normal = Quaternion.Inverse(curve.rotation) * hit.normal;
                        hit.point = Quaternion.Inverse(curve.rotation) * hit.point + curve.position;
                    }
                }

            return count;
        }
    }

    public struct RefractionEquation
    {
        // refractive index = m * (1 / wavelength) + c
        //
        // find m and c using real life measurements and know that
        // refractive index is inversely proportional to wavelength:
        //    - Use excel to get line of best fit from graph of y-axis "refractive index" by x-axis "1 / wavelength"
        //    - Display equation and take m and c values
        //
        // wavelength is typically in micro meters

        public float m;
        public float c;

        // used to define a fixed refraction index across all wavelengths
        public bool isFixed;
        public float refractionIndex;

        public float index(float invWavelength)
        {
            if (isFixed) return refractionIndex;
            return m * invWavelength + c;
        }
    }

    public struct CurveParameter
    {
        /// <summary>
        /// l(x^2+y^2)+((lhpq)z^2)/s^2+q(p-1)z = r
        /// </summary>

        public Vector3 position;
        public Quaternion rotation;

        public enum Type
        {
            Single,
            Edge
        }
        public Type type;
        public RefractionEquation singleRefractiveIndex;
        public RefractionEquation frontRefractiveIndex;
        public RefractionEquation backRefractiveIndex;

        public Vector3 minimum;
        public Vector3 maximum;
        public float radial;

        public float l;
        public float h;
        public float q;
        public float p;
        public float r;
        public float s;

        public static CurveParameter Plane = new CurveParameter { l = 0f, p = 2, q = 1, r = 0 };
        public static CurveParameter Paraboloid = new CurveParameter { l = 1f, p = 0, q = 1, r = 0 };
        public static CurveParameter Cylinder = new CurveParameter { l = 1f, q = 0f, r = 1f };
        public static CurveParameter Elliptoid = new CurveParameter { l = 1f, p = 1f, s = 1f, q = 1f, h = 1f, r = 1f };
        public static CurveParameter Sphere = Elliptoid;
        public static CurveParameter Hyperboloid = new CurveParameter { l = 1f, p = 1f, s = 1f, q = 1f, h = -1f, r = -1f };
    }
}