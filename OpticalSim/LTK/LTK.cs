using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

// Light Toolkit

//TODO:: Assess use of class for surfaces instead of struct (might be better performance wise, not sure)

namespace LightTK
{
    public struct LightRayHit
    {
        public Vector3 point;
        public Vector3 relPoint;
        public Vector3 relDir;
        public Vector3 normal;
        public Surface surface;
    }

    public partial class LTK
    {
        private const float FL_EPSILON = 1e-10f;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool SafeCompare(float a, float b)
        {
            return Mathf.Abs(a - b) <= FL_EPSILON;
        }

        public static bool ClosestPointsOnTwoLines(out Vector3 p0, out Vector3 p1, Vector3 origin0, Vector3 dir0, Vector3 origin1, Vector3 dir1)
        {
            p0 = Vector3.zero;
            p1 = Vector3.zero;

            float a = Vector3.Dot(dir0, dir0);
            float b = Vector3.Dot(dir0, dir1);
            float e = Vector3.Dot(dir1, dir1);

            float d = a * e - b * b;

            //lines are not parallel
            if (d != 0.0f)
            {

                Vector3 r = origin0 - origin1;
                float c = Vector3.Dot(dir0, r);
                float f = Vector3.Dot(dir1, r);

                float s = (b * f - c * e) / d;
                float t = (a * f - c * b) / d;

                p0 = origin0 + dir0 * s;
                p1 = origin1 + dir1 * t;

                return true;
            }

            else
            {
                return false;
            }
        }

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
        public static int GetRelativeIntersection(Vector3 origin, Vector3 dir, AbstractSurface curve, LightRayHit[] points, bool bounded = true)
        {
            return GetIntersection(origin, dir, curve.surface, points, true, bounded);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetRelativeIntersection(Vector3 origin, Vector3 dir, Surface curve, LightRayHit[] points, bool bounded = true)
        {
            return GetIntersection(origin, dir, curve, points, true, bounded);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetIntersection(LightRay ray, AbstractSurface curve, LightRayHit[] points, bool relative = false, bool bounded = true)
        {
            return GetIntersection(ray.position, ray.direction, curve.surface, points, relative, bounded);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetIntersection(Vector3 origin, Vector3 dir, AbstractSurface curve, LightRayHit[] points, bool relative = false, bool bounded = true)
        {
            return GetIntersection(origin, dir, curve.surface, points, relative, bounded);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetIntersection(LightRay ray, Surface curve, LightRayHit[] points, bool relative = false, bool bounded = true)
        {
            return GetIntersection(ray.position, ray.direction, curve, points, relative, bounded);
        }

        private struct partialEquation
        {
            public float a;
            public float b;

            public float d;
            public float e;
            public float f;
        }

        /*private const int precision = 4;
        private static void Round(ref Vector3 v)
        {
            v.x = (float)Math.Round(v.x, precision);
            v.y = (float)Math.Round(v.y, precision);
            v.z = (float)Math.Round(v.z, precision);
        }*/

        public static int GetIntersection(Vector3 origin, Vector3 dir, Surface curve, LightRayHit[] points, bool relative = false, bool bounded = true)
        {
            if (!relative)
            {
                origin = Quaternion.Inverse(curve.rotation) * (origin - curve.position);
                dir = Quaternion.Inverse(curve.rotation) * dir;
                //Round(ref origin);
                //Round(ref dir);
            }

            dir.Normalize();

            if (curve.surface.j == 0 &&
                curve.surface.k == 0 &&
                curve.surface.l == 0 &&
                curve.surface.m == 0 &&
                curve.surface.n == 0 &&
                curve.surface.o == 0)
                return 0;

            partialEquation[] l = new partialEquation[3]
            {
                new partialEquation()
                {
                    a = origin.x, // a
                    b = dir.x, // d
                    d = curve.surface.j,
                    e = curve.surface.g,
                    f = curve.surface.m
                },
                new partialEquation()
                {
                    a = origin.y, // b
                    b = dir.y, // e
                    d = curve.surface.k,
                    e = curve.surface.h,
                    f = curve.surface.n
                },
                new partialEquation()
                {
                    a = origin.z, // c
                    b = dir.z, // f
                    d = curve.surface.l,
                    e = curve.surface.i,
                    f = curve.surface.o
                }
            };

            //To keep maths stable make sure to pick the largest b value, otherwise ed and fd will blow up
            int rel = 0;
            float curr = l[0].b;
            for (int i = 1; i < l.Length; i++) if (Mathf.Abs(curr) < Mathf.Abs(l[i].b)) { curr = l[i].b; rel = i; }
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
            float bq = -2f * r0.d * r0.e +
                r1.d * (2f * ed * bead - 2f * r1.e * ed) +
                r2.d * (2f * fd * cfad - 2f * r2.e * fd) +
                r1.f * ed + r2.f * fd + r0.f;

            float cq = r0.d * r0.e * r0.e + r1.d * (bead * bead - 2f * r1.e * bead + r1.e * r1.e) +
                r2.d * (cfad * cfad - 2f * r2.e * cfad + r2.e * r2.e) +
                r1.f * bead + r2.f * cfad + curve.surface.p;

            float[] solutions = new float[2];
            int count;
            if (SafeCompare(aq, 0)) //SafeCompare to prevent floating point errors
            {
                if (!SafeCompare(bq, 0)) // Solve Linear equation 0x^2 + bx + c = 0
                {
                    solutions[0] = -cq / bq;
                    count = 1;
                }
                else count = 0; // Invalid equation 0x^2 + 0x + c = 0
            }
            else count = Quadratic(aq, bq, cq, solutions); // Solving quadratic, ax^2 + bx + c = 0
            for (int i = 0, temp = count, j = 0; i < temp; i++, j++)
            {
                float v = solutions[i];
                ref LightRayHit hit = ref points[j];
                hit.surface = curve;
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
                    2f * curve.surface.j * hit.point.x - 2f * curve.surface.j * curve.surface.g + curve.surface.m,
                    2f * curve.surface.k * hit.point.y - 2f * curve.surface.k * curve.surface.h + curve.surface.n,
                    2f * curve.surface.l * hit.point.z - 2f * curve.surface.l * curve.surface.i + curve.surface.o
                    );

                if (bounded &&
                    ((curve.radial != 0 && hit.point.sqrMagnitude > curve.radial * curve.radial) ||
                    ((hit.point.x > curve.maximum.x) || (hit.point.x < curve.minimum.x) ||
                    (hit.point.y > curve.maximum.y) || (hit.point.y < curve.minimum.y) ||
                    (hit.point.z > curve.maximum.z) || (hit.point.z < curve.minimum.z))))
                {
                    count -= 1;
                    j -= 1;
                    continue;
                }

                hit.relPoint = hit.point;
                hit.relDir = dir;
                if (!relative)
                {
                    hit.normal = curve.rotation * hit.normal;
                    hit.point = curve.rotation * hit.point + curve.position;
                }

                hit.normal.Normalize();
            }
            return count;
        }
    }

    public struct RefractionSettings
    {
        public enum Type
        {
            Single,
            Edge
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

    [System.Serializable]
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

        public static RefractionEquation air = new RefractionEquation() { isFixed = true, m = 0.0277f, c = 0.9726f, refractionIndex = 1.000293f };
        public static RefractionEquation crownGlass = new RefractionEquation() { isFixed = true, m = 0.0163f, c = 1.4835f, refractionIndex = 1.523f };
    }

    [System.Serializable]
    public struct SurfaceSettings
    {
        public enum SurfaceType
        {
            None,
            Block,
            Refractive,
            Reflective,
            IdealLens
        }
        public SurfaceType type;

        public void setFocalLength(float focalLength, bool concave = true)
        {
            refractionSettings = 1f / (2f * focalLength) + RefractionEquation.air;
            if (concave)
            {
                invR1 = 1f;
                invR2 = -1f;
            }
            else
            {
                invR1 = -1f;
                invR2 = 1f;
            }
        }
        public float focalLength(ref LightRay r)
        {
            return 1f / ((refractionSettings.refractiveIndex(r.wavelength) - r.refractiveIndex) * (invR1 - invR2));
        }

        public float invR1;
        public float R1
        {
            get { return 1f / invR1; }
            set { invR1 = 1f / value; }
        }
        public float invR2;
        public float R2
        {
            get { return 1f / invR2; }
            set { invR2 = 1f / value; }
        }

        public RefractionSettings refractionSettings;

        public static implicit operator SurfaceSettings(RefractionEquation refractiveIndex)
        {
            return new SurfaceSettings()
            {
                type = SurfaceType.Refractive,
                refractionSettings = refractiveIndex
            };
        }
    }

    [System.Serializable]
    public struct Equation
    {
        /// <summary>
        /// j(x-g)^2+k(y-h)^2+l(z-i)^2+mx+ny+oz+p=0
        /// </summary>

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

        public static Equation Sphere = new Equation()
        {
            j = 1f,
            k = 1f,
            l = 1f,
            p = -1f
        };

        public static Equation Plane = new Equation()
        {
            o = 1f
        };

        public static Equation Paraboloid = new Equation()
        {
            j = 1f,
            k = 1f,
            o = -1f
        };

        public static Equation Hyperboloid = new Equation()
        {
            j = 1f,
            k = 1f,
            l = -1f,
            p = 1f
        };

        public static Equation Ellipsoid = new Equation()
        {
            j = 1f,
            k = 1f,
            l = 1f,
            p = -1f
        };

        public static Equation Cylinder = new Equation()
        {
            j = 1f,
            k = 1f,
            p = -1f
        };
    }

    [System.Serializable]
    public struct Surface
    {
        public SurfaceSettings settings;

        public Vector3 position;
        public Quaternion rotation;

        public Vector3 minimum;
        public Vector3 maximum;
        public float radial;

        public Equation surface;
    }
}
