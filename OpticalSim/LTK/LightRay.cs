﻿using System.Collections;
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
        public static Surface[] BakeCurves(AbstractSurface[] surfaces)
        {
            Surface[] curves = new Surface[surfaces.Length];
            for (int i = 0; i < surfaces.Length; i++)
            {
                curves[i] = surfaces[i].surface;
            }
            return curves;
        }
        public static Surface[] BakeCurves(List<AbstractSurface> surfaces)
        {
            Surface[] curves = new Surface[surfaces.Count];
            for (int i = 0; i < surfaces.Count; i++)
            {
                curves[i] = surfaces[i].surface;
            }
            return curves;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool SolveRay(ref LightRay l, LightRayHit p)
        {
            l.prevDirection = l.direction;
            l.position = p.point;
            l.normal = p.normal;

            switch (p.surface.type)
            {
                case SurfaceSettings.SurfaceType.Reflective:
                    return SolveRayReflection(ref l, p);
                case SurfaceSettings.SurfaceType.Refractive:
                    return SolveRayRefraction(ref l, p);
                case SurfaceSettings.SurfaceType.ThinLens:
                    return SolveRayIdealLens(ref l, p);
                default:
                    return true;
            }
        }

        public static bool SolveRayIdealLens(ref LightRay l, LightRayHit p)
        {
            RefractionSettings refraction = p.surface.refractionSettings;

            float sign = Vector3.Dot(p.normal, l.direction);
            float refractiveIndex = refraction.refractiveIndex(l.wavelength, sign);
#if UNITY_EDITOR
            if (refractiveIndex == 0)
            {
                Debug.LogError("refractiveIndex is 0");
                return false;
            }
            else if (refraction.type != RefractionSettings.Type.Single)
            {
                Debug.LogError("ideal lens can only have refractive type of single");
                return false;
            }
#endif
            //TODO:: maybe make oNormal and normal an array and surfaces contain an array of equations for normals
            //       This way infinitely thing combinations of surfaces can be made instead of just 2 surfaces
            //       normal and onormal

            if (sign > 0) p.normal = -p.normal;
            l.direction = l.direction.normalized;

            float AngleI = Mathf.Acos(Vector3.Dot(l.direction, -p.normal));

            float ratio = l.refractiveIndex / refractiveIndex;
            float cosI = Vector3.Dot(-p.normal, l.direction);
            float sinT2 = ratio * ratio * (1f - cosI * cosI);
            if (sinT2 > 1.0f) // Total internal reflection
            {
                SolveRayReflection(ref l, p);
            }
            else
            {
                float cosT = Mathf.Sqrt(1f - sinT2);
                l.direction = ratio * l.direction + (ratio * cosI - cosT) * p.normal;

                float AngleT = Mathf.Acos(Vector3.Dot(l.direction, -p.normal));
            }

            if (sign > 0) p.oNormal = -p.oNormal;
            l.direction = l.direction.normalized;

            AngleI = Mathf.Acos(Vector3.Dot(l.direction, -p.oNormal));

            ratio = refractiveIndex / l.refractiveIndex;
            cosI = Vector3.Dot(-p.oNormal, l.direction);
            sinT2 = ratio * ratio * (1f - cosI * cosI);
            if (sinT2 > 1.0f) // Total internal reflection
            {
                SolveRayReflection(ref l, p);
            }
            else
            {
                float cosT = Mathf.Sqrt(1f - sinT2);
                l.direction = ratio * l.direction + (ratio * cosI - cosT) * p.oNormal;

                float AngleT = Mathf.Acos(Vector3.Dot(l.direction, -p.oNormal));
            }

            return true;
        }

        public static bool SolveRayReflection(ref LightRay l, LightRayHit p)
        {
            l.direction = l.direction - 2f * Vector3.Dot(l.direction, p.normal) * p.normal;
            float AngleT = Mathf.Acos(Vector3.Dot(l.direction, p.normal));

            return true;
        }

        public static bool SolveRayRefraction(ref LightRay l, LightRayHit p)
        {
            RefractionSettings refraction = p.surface.refractionSettings;

            float sign = Vector3.Dot(p.normal, l.direction);
            float refractiveIndex = refraction.refractiveIndex(l.wavelength, sign);
#if UNITY_EDITOR
            if (refractiveIndex == 0)
            {
                Debug.LogError("refractiveIndex is 0");
                return false;
            }
#endif
            if (sign > 0) p.normal = -p.normal;
            l.direction = l.direction.normalized;

            float AngleI = Mathf.Acos(Vector3.Dot(l.direction, -p.normal));

            float ratio = l.refractiveIndex / refractiveIndex;
            float cosI = Vector3.Dot(-p.normal, l.direction);
            float sinT2 = ratio * ratio * (1f - cosI * cosI);
            if (sinT2 > 1.0f) // Total internal reflection
            {
                SolveRayReflection(ref l, p);
            }
            else
            {
                float cosT = Mathf.Sqrt(1f - sinT2);
                l.direction = ratio * l.direction + (ratio * cosI - cosT) * p.normal;

                float AngleT = Mathf.Acos(Vector3.Dot(l.direction, -p.normal));
            }

            if (refraction.type == RefractionSettings.Type.Edge)
                l.refractiveIndex = refractiveIndex;

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool SimulateRay(ref LightRay l, Surface[] curves)
        {
            if (l.direction == Vector3.zero) return false;
            LightRayHit[] hits = new LightRayHit[2];
            LightRayHit p = new LightRayHit();
            Vector3 pdir = Vector3.positiveInfinity;
            bool success = false;
            for (int i = 0; i < curves.Length; i++)
            {
                ref Surface c = ref curves[i];
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
        public static bool SimulateRay(ref LightRay l, List<Surface> curves)
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

        public static void SimulateRays(LightRay[] rays, Surface[] curves)
        {
            for (int i = 0; i < rays.Length; i++)
            {
                SimulateRay(ref rays[i], curves);
            }
        }

        public static void SimulateRays(LightRay[] rays, List<Surface> curves)
        {
            for (int i = 0; i < rays.Length; i++)
            {
                SimulateRay(ref rays[i], curves);
            }
        }
        public static void SimulateRays(List<LightRay> rays, Surface[] curves)
        {
            for (int i = 0; i < rays.Count; i++)
            {
                LightRay l = rays[i];
                SimulateRay(ref l, curves);
                rays[i] = l;
            }
        }

        public static void SimulateRays(List<LightRay> rays, List<Surface> curves)
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
