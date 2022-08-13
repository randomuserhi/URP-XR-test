using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace LightTK
{
    public struct LightRay
    {
        public Vector3 position;
        public Vector3 prevDirection;
        public Vector3 direction;
        public Vector3 normal;

        public float wavelength;
        public float refractiveIndex;

        public static LightRay visibleLight = new LightRay() { wavelength = 0.64f, refractiveIndex = RefractionEquation.air };
    }

    public partial class LTK
    {
        public static Curve[] BakeCurves(Surface[] surfaces)
        {
            Curve[] curves = new Curve[surfaces.Length];
            for (int i = 0; i < surfaces.Length; i++)
            {
                curves[i] = surfaces[i].curve;
            }
            return curves;
        }
        public static Curve[] BakeCurves(List<Surface> surfaces)
        {
            Curve[] curves = new Curve[surfaces.Count];
            for (int i = 0; i < surfaces.Count; i++)
            {
                curves[i] = surfaces[i].curve;
            }
            return curves;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool SolveRay(ref LightRay l, LightRayHit p)
        {
            RefractionSettings refraction = p.curve.refractionSettings;

            float sign = Vector3.Dot(p.normal, l.direction);
            float refractiveIndex = refraction.refractiveIndex(l.wavelength, sign);
#if UNITY_EDITOR
            if (refraction.type != RefractionSettings.Type.Reflective && refractiveIndex == 0)
            {
                Debug.LogError("refractiveIndex is 0");
                return false;
            }
#endif
            if (sign > 0) p.normal = -p.normal;
            //if (p.curve.normalEquation.type == Equation.Type.Asymmetrical) p.normal = -p.normal;
            //else if (p.curve.normalEquation.type == Equation.Type.Symmetrical) p.normal.z = -p.normal.z;

            //Round(ref l.direction);
            l.direction = l.direction.normalized;
            l.prevDirection = l.direction;

            float AngleI = Mathf.Acos(Vector3.Dot(l.direction, -p.normal));

            //Debug.Log("----");
            //Debug.Log(l.direction);
            float ratio = l.refractiveIndex / refractiveIndex;
            float cosI = 1;// Vector3.Dot(-p.normal, l.direction);
            float sinT2 = ratio * ratio * (1f - cosI * cosI);
            if (refraction.type == RefractionSettings.Type.Reflective || sinT2 > 1.0f) // Total internal reflection
            {
                /*Vector3 perp = Vector3.Cross(l.direction, p.normal);
                Vector3 aligned = Vector3.Cross(p.normal, perp);
                Vector3 projection = Vector3.Project(l.direction, aligned);
                l.direction = -l.direction + projection * 2;*/

                l.direction = l.direction - 2f * Vector3.Dot(l.direction, p.normal) * p.normal;

                float AngleT = Mathf.Acos(Vector3.Dot(l.direction, p.normal));
                Debug.Log(l.refractiveIndex + ", " + refractiveIndex + " > " + Mathf.Rad2Deg * AngleI + ", " + Mathf.Rad2Deg * AngleT);
            }
            else
            {
                float cosT = Mathf.Sqrt(1f - sinT2);
                l.direction = ratio * l.direction + (ratio * cosI - cosT) * p.normal;

                float AngleT = Mathf.Acos(Vector3.Dot(l.direction, -p.normal));
                Debug.Log(l.refractiveIndex + ", " + refractiveIndex + " > " + AngleI + ", " + AngleT);
            }

            l.position = p.point;
            l.normal = p.normal;
            //Round(ref l.direction);

            if (refraction.type == RefractionSettings.Type.Edge)
                l.refractiveIndex = refractiveIndex;

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool SimulateRay(ref LightRay l, Curve[] curves)
        {
            if (l.direction == Vector3.zero) return false;
            LightRayHit[] hits = new LightRayHit[2];
            LightRayHit p = new LightRayHit();
            Vector3 pdir = Vector3.positiveInfinity;
            bool success = false;
            for (int i = 0; i < curves.Length; i++)
            {
                ref Curve c = ref curves[i];
                int count = GetIntersection(l, c, hits);
                for (int j = 0; j < count; j++)
                {
                    ref LightRayHit hit = ref hits[j];
                    Vector3 dir = hit.point - l.position;
                    if (Vector3.Dot(dir, l.direction) <= 0) continue;
                    if (hit.point != l.position && (!success || dir.sqrMagnitude < pdir.sqrMagnitude))
                    {
                        success = true;
                        p = hit;
                        pdir = p.point - l.position;
                    }
                }
            }
            if (!success) return false;

            return SolveRay(ref l, p);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool SimulateRay(ref LightRay l, List<Curve> curves)
        {
            if (l.direction == Vector3.zero) return false;
            LightRayHit[] hits = new LightRayHit[2];
            LightRayHit p = new LightRayHit();
            Vector3 pdir = Vector3.positiveInfinity;
            bool success = false;
            for (int i = 0; i < curves.Count; i++)
            {
                int count = GetIntersection(l, curves[i], hits);
                for (int j = 0; j < count; j++)
                {
                    ref LightRayHit hit = ref hits[j];
                    Vector3 dir = hit.point - l.position;
                    if (Vector3.Dot(dir, l.direction) <= 0) continue;
                    if (hit.point != l.position && (!success || dir.sqrMagnitude < pdir.sqrMagnitude))
                    {
                        success = true;
                        p = hit;
                        pdir = p.point - l.position;
                    }
                }
            }
            if (!success) return false;

            return SolveRay(ref l, p);
        }

        public static void SimulateRays(LightRay[] rays, Curve[] curves)
        {
            for (int i = 0; i < rays.Length; i++)
            {
                SimulateRay(ref rays[i], curves);
            }
        }

        public static void SimulateRays(LightRay[] rays, List<Curve> curves)
        {
            for (int i = 0; i < rays.Length; i++)
            {
                SimulateRay(ref rays[i], curves);
            }
        }
        public static void SimulateRays(List<LightRay> rays, Curve[] curves)
        {
            for (int i = 0; i < rays.Count; i++)
            {
                LightRay l = rays[i];
                SimulateRay(ref l, curves);
                rays[i] = l;
            }
        }

        public static void SimulateRays(List<LightRay> rays, List<Curve> curves)
        {
            for (int i = 0; i < rays.Count; i++)
            {
                LightRay l = rays[i];
                SimulateRay(ref l, curves);
                rays[i] = l;
            }
        }
    }
}
