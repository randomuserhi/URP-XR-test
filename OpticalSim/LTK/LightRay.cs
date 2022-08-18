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
        public SurfaceSettings.SurfaceType surfaceType;

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
            l.direction.Normalize();
            l.prevDirection = l.direction;
            l.normal = p.normal;
            l.surfaceType = p.surface.settings.type;

            bool success = true;
            switch (p.surface.settings.type)
            {
                case SurfaceSettings.SurfaceType.Reflective:
                    success = SolveRayReflection(ref l, p);
                    break;
                case SurfaceSettings.SurfaceType.Refractive:
                    success = SolveRayRefraction(ref l, p);
                    break;
                case SurfaceSettings.SurfaceType.IdealLens:
                    success = SolveRayIdealLens(ref l, p);
                    break;
                case SurfaceSettings.SurfaceType.Block:
                    success = false;
                    break;
            }

            l.direction.Normalize();
            //l.position = p.point + l.direction * 0.001f; // => only applies if rounding is used for GetIntersection
            l.position = p.point;
            return success;
        }

        //NOTE:: Only works on Plane surfaces
        public static bool SolveRayIdealLens(ref LightRay l, LightRayHit p)
        {
            RefractionSettings refraction = p.surface.settings.refractionSettings;

            float sign = Vector3.Dot(p.normal, l.direction);
            float refractiveIndex = refraction.refractiveIndex(l.wavelength, sign);
#if UNITY_EDITOR
            if (refractiveIndex == 0)
            {
                Debug.LogError("refractiveIndex cannot be 0.");
                return false;
            }
            else if (p.surface.settings.refractionSettings.type != RefractionSettings.Type.Single)
            {
                Debug.LogError("Ideal lens surface can only have refractive type single.");
                return false;
            }
            else if (p.surface.settings.focalLength(ref l) == 0)
            {
                Debug.LogError("Focal length cannot be 0.");
                return false;
            }
#endif
            float focalLength;
            if (sign > 0) focalLength = -p.surface.settings.focalLength(ref l);
            else focalLength = p.surface.settings.focalLength(ref l);

            //TODO:: find a more effient way to implement this without focalPlane

            Surface focalPlane = new Surface()
            {
                surface = new Equation()
                {
                    o = 1f,
                    p = focalLength
                }
            };
            LightRayHit[] focalHit = new LightRayHit[2];
            int hits = GetRelativeIntersection(Vector3.zero, p.relDir, focalPlane, focalHit, bounded: false);
#if UNITY_EDITOR
            if (hits == 0)
            {
                Debug.LogError("Unable to hit focal plane. This should never happen.");
                return false;
            }
#endif
            l.direction = p.surface.rotation * focalHit[0].point + p.surface.position - p.point;

            return true;
        }

        public static bool SolveRayReflection(ref LightRay l, LightRayHit p)
        {
            //Debug.Log(p.normal.x + ", " + p.normal.y + ", " + p.normal.z);

            /*Vector3 perp = Vector3.Cross(l.direction, p.normal);
            Vector3 aligned = Vector3.Cross(p.normal, perp);
            Vector3 projection = Vector3.Project(l.direction, aligned);
            l.direction = -l.direction + projection * 2;*/

            float sign = Vector3.Dot(p.normal, l.direction);
            if (sign > 0) p.normal = -p.normal;

            l.direction = l.direction - 2f * Vector3.Dot(l.direction, p.normal) * p.normal;

            return true;
        }

        public static bool SolveRayRefraction(ref LightRay l, LightRayHit p)
        {
            RefractionSettings refraction = p.surface.settings.refractionSettings;

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool SimulateRay(ref LightRay l, List<LTKCollider> curves)
        {
            if (l.direction == Vector3.zero) return false;
            LightRayHit[] hits = new LightRayHit[2];
            LightRayHit p = new LightRayHit();
            Vector3 pdir = Vector3.positiveInfinity;
            bool success = false;
            for (int i = 0; i < curves.Count; i++)
            {
                if (curves[i].enableCollision)
                {
                    int count = GetIntersection(l, curves[i]._surface, hits);
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
            }
            if (!success) return false;

            return SolveRay(ref l, p);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool SimulateRay(ref LightRay l, LTKCollider[] curves)
        {
            if (l.direction == Vector3.zero) return false;
            LightRayHit[] hits = new LightRayHit[2];
            LightRayHit p = new LightRayHit();
            Vector3 pdir = Vector3.positiveInfinity;
            bool success = false;
            for (int i = 0; i < curves.Length; i++)
            {
                if (curves[i].enableCollision)
                {
                    int count = GetIntersection(l, curves[i]._surface, hits);
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
