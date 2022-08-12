using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

// Light Toolkit

namespace LightTK
{
    public struct LightRayHit
    {
        public Vector3 point;
        public Vector3 normal;
        public Curve curve;
    }

    public partial class LTK
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetRelativeIntersection(Vector3 origin, Vector3 dir, Surface curve, LightRayHit[] points)
        {
            return GetIntersection(origin, dir, curve.curve, points, true);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetRelativeIntersection(Vector3 origin, Vector3 dir, Curve curve, LightRayHit[] points)
        {
            return GetIntersection(origin, dir, curve, points, true);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetIntersection(LightRay ray, Surface curve, LightRayHit[] points, bool relative = false)
        {
            return GetIntersection(ray.position, ray.direction, curve.curve, points, relative);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetIntersection(Vector3 origin, Vector3 dir, Surface curve, LightRayHit[] points, bool relative = false)
        {
            return GetIntersection(origin, dir, curve.curve, points, relative);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetIntersection(LightRay ray, Curve curve, LightRayHit[] points, bool relative = false)
        {
            return GetIntersection(ray.position, ray.direction, curve, points, relative);
        }

        private struct partialEquation
        {
            public float a;
            public float b;

            public float d;
            public float e;
            public float f;
        }

        public static int GetIntersection(Vector3 origin, Vector3 dir, Curve curve, LightRayHit[] points, bool relative = false)
        {
            if (!relative)
            {
                origin = curve.rotation * (origin - curve.position);
                dir = curve.rotation * dir;
            }

            if (curve.j == 0 &&
                curve.k == 0 &&
                curve.l == 0 &&
                curve.m == 0 &&
                curve.n == 0 &&
                curve.o == 0) 
                return 0;

            partialEquation[] l = new partialEquation[3]
            {
                new partialEquation()
                {
                    a = origin.x, // a
                    b = dir.x, // d
                    d = curve.j,
                    e = curve.g,
                    f = curve.m
                },
                new partialEquation()
                {
                    a = origin.y, // b
                    b = dir.y, // e
                    d = curve.k,
                    e = curve.h,
                    f = curve.n
                },
                new partialEquation()
                {
                    a = origin.z, // c
                    b = dir.z, // f
                    d = curve.l,
                    e = curve.i,
                    f = curve.o
                }
            };
            int rel = 0;
            for (int i = 0; i < l.Length && l[rel].b == 0; i++, rel++) { }
            if (l[rel].b == 0)
            {
                Debug.LogWarning("Direction is Vector.zero!");
                return 0;
            }

            ref partialEquation r0 = ref l[rel];
            ref partialEquation r1 = ref l[(rel + 1) % l.Length];
            ref partialEquation r2 = ref l[(rel + 2) % l.Length];

            float ed = r1.b / r0.b;
            float fd = r2.b / r0.b;
            float aq = r0.d + r1.d * ed * ed + r2.d * fd * fd;

            float bead = r1.a - r0.a * ed;
            float cfad = r2.a - r0.a * fd;
            float bq = -2f * r0.d * r0.e + r1.d * (2f * ed * bead - 2f * r1.e * ed) + r2.d * (2f * fd * cfad - 2f * r2.e * fd) + r1.f * ed + r2.f * fd + curve.m;

            float cq = r0.d * r0.e * r0.e + r1.d * (bead * bead - 2f * r1.e * bead + r1.e * r1.e) + r2.d * (cfad * cfad - 2f * r2.e * cfad + r2.e * r2.e) + r1.f * bead + r2.f * cfad + curve.p;

            float[] solutions = new float[2];
            int count;
            if (aq == 0) //Solving linear equation or constant
            {
                if (bq != 0) //Solving linear equation, 0x^2 + ax + b = cy
                {
                    solutions[0] = curve.o * -cq / bq;
                    count = 1;
                }
                else if (curve.o != 0) //Solving constant, 0x^2 + 0x + a = cy
                {
                    solutions[0] = curve.o * -cq;
                    count = 1;
                }
                else count = 0; //Equation is null, 0x^2 + 0x + 0 = 0y
            }
            else count = Quadratic(aq, bq, cq, solutions); // Solving quadratic, ax^2 + bx + c = dy
            for (int i = 0, temp = count, j = 0; i < temp; i++, j++)
            {
                float v = solutions[i];
                ref LightRayHit hit = ref points[j];
                hit.curve = curve;
                switch (rel)
                {
                    case 0:
                        hit.point = new Vector3(
                            v,
                            ed * v + bead,
                            fd * v + cfad);
                        break;
                    case 1:
                        hit.point = new Vector3(
                            fd * v + cfad,
                            v,
                            ed * v + bead);
                        break;
                    case 2:
                        hit.point = new Vector3(
                            ed * v + bead,
                            fd * v + cfad,
                            v);
                        break;
                    default:
                        Debug.LogError("rel is not within bounds!");
                        break;
                }

                hit.normal = new Vector3(
                    2f * curve.j * hit.point.x - 2f * curve.j * curve.g + curve.m,
                    2f * curve.k * hit.point.y - 2f * curve.k * curve.h + curve.n,
                    2f * curve.l * hit.point.z - 2f * curve.l * curve.i + curve.o
                    );

                if ((curve.radial == 0 || hit.point.sqrMagnitude < curve.radial * curve.radial) &&
                    ((hit.point.x > curve.maximum.x) || (hit.point.x < curve.minimum.x) ||
                    (hit.point.y > curve.maximum.y) || (hit.point.y < curve.minimum.y) ||
                    (hit.point.z > curve.maximum.z) || (hit.point.z < curve.minimum.z)))
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

    public struct RefractionSettings
    {
        public enum Type
        {
            Single,
            Edge,
            Reflective
        }
        public Type type;
        public RefractionEquation single;
        public RefractionEquation positive;
        public RefractionEquation negative;
        public float refractiveIndex(float waveLength, float sign = 0)
        {
            switch (type)
            {
                case Type.Single:
                    return single.index(waveLength);
                case Type.Edge:
                    if (sign > 0)
                        return positive.index(waveLength);
                    else
                        return negative.index(waveLength);
                default:
                    return single.index(waveLength);
            }
        }

        public static implicit operator RefractionSettings(float refractiveIndex)
        {
            return new RefractionSettings()
            {
                type = Type.Single,
                single = refractiveIndex
            };
        }
        public static implicit operator RefractionSettings(RefractionEquation refractiveIndex)
        {
            return new RefractionSettings()
            {
                type = Type.Single,
                single = refractiveIndex
            };
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

        public static implicit operator float(RefractionEquation equation)
        {
            return equation.refractionIndex;
        }

        public static implicit operator RefractionEquation(float refractiveIndex)
        {
            return new RefractionEquation()
            {
                isFixed = true,
                refractionIndex = refractiveIndex
            };
        }

        public static RefractionEquation air = new RefractionEquation() { m = 0.0277f, c = 0.9726f, refractionIndex = 1.000293f };
        public static RefractionEquation crownGlass = new RefractionEquation() { m = 0.0163f, c = 1.4835f, refractionIndex = 1.523f };
    }

    public struct Curve
    {
        public RefractionSettings refractionSettings;

        /// <summary>
        /// j(x-g)^2+k(y-h)^2+l(z-i)^2+mx+ny+oz+p=0
        /// </summary>

        public Vector3 position;
        public Quaternion rotation;

        public Vector3 minimum;
        public Vector3 maximum;
        public float radial;

        public float g;
        public float h;
        public float i;
        public float j;
        public float k;
        public float l;
        public float m;
        public float n;
        public float o;
        public float p;
    }
}
