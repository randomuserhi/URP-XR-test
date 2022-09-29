using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using static UnityEngine.ParticleSystem;

namespace FieldsToolkit
{
    public static partial class FTK
    {
        [Serializable]
        public struct Settings
        {
            public int numLinesPerRing;
            public int numRings;

            public int iterationsPerFrame;
            public float step;
            public float tolerance;

            public static Settings defaultSettings = new Settings() { numLinesPerRing = 5, numRings = 5, iterationsPerFrame = 3, step = 0.03f, tolerance = 1e-7f };
        }
        public static Settings settings { get => FieldsToolkit.instance == null ? Settings.defaultSettings : FieldsToolkit.instance.settings; }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector3 CalculateForce(Vector3 m1, Vector3 m2, float f1, float f2)
        {
            // Scale forces to ensure that f doesnt become so small it truncates to 0
            f1 *= 1000f;
            f2 *= 1000f;
            
            Vector3 r = m2 - m1;
            float dist = r.magnitude;
            if (dist == 0) r = Vector3.up;
            const float p = 0.05f;
            float p0 = p * f1 * f2;
            float p1 = 4 * Mathf.PI * dist;
            float f = p0 / p1;
            if (dist == 0) f = 0;

            return f * r.normalized;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 FieldDir(Vector3 point, float strength, List<FTKPole> poles)
        {
            Vector3 dir = Vector3.zero;

            for (int i = 0; i < poles.Count; i++)
            {
                dir -= CalculateForce(point, poles[i].transform.position, strength, poles[i].strength);
            }

            return dir.normalized;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 ForceOnPole(FTKPole pole, List<FTKPole> poles)
        {
            Vector3 dir = Vector3.zero;

            for (int i = 0; i < poles.Count; i++)
            {
                if (poles[i] == pole) continue;
                dir -= CalculateForce(pole.transform.position, poles[i].transform.position, pole.strength, poles[i].strength);
            }

            return dir.normalized;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector3 RK4(Vector3 point, float sign, float step, List<FTKPole> poles)
        {
            Vector3 k1 = step * FieldDir(point, sign, poles);
            Vector3 k2 = step * FieldDir(point + 0.5f * k1, sign, poles);
            Vector3 k3 = step * FieldDir(point + 0.5f * k2, sign, poles);
            Vector3 k4 = step * FieldDir(point + k3, sign, poles);

            return point + (k1 + 2f * (k2 + k3) + k4) / 6f;
        }
        public static bool Solve(Vector3[] buffer, float sign, float step, float tolerance, List<FTKPole> poles, out float s, out int count, int start = 1, int iterations = int.MaxValue)
        {
            s = step;
            int iter = 0;
            for (count = start; iter < iterations && count < buffer.Length; ++count, ++iter)
            {
                if (count == 0) continue;

                // Calculate single and double RK4
                Vector3 single = RK4(buffer[count - 1], sign, s, poles);
                Vector3 half = RK4(buffer[count - 1], sign, 0.5f * s, poles);
                Vector3 full = RK4(half, sign, 0.5f * s, poles);
                Vector3 diff = full - single;
                if (diff.sqrMagnitude > tolerance)
                {
                    // Adjust step to fit within tolerance
                    while (diff.sqrMagnitude > tolerance)
                    {
                        s *= 0.5f;
                        single = RK4(buffer[count - 1], sign, s, poles);
                        half = RK4(buffer[count - 1], sign, 0.5f * s, poles);
                        full = RK4(half, sign, 0.5f * s, poles);
                        diff = full - single;
                    }
                    buffer[count] = single;
                }
                else // Adjust step to maximize tolerance
                {
                    buffer[count] = single;
                    s *= 2f;
                }

                // Check if line has reached a sink
                if ((buffer[count] - buffer[count - 1]).sqrMagnitude < 0.001f * 0.001f) return true;
            }
            return false;
        }
    }
}
