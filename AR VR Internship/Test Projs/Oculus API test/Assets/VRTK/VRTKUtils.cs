using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualRealityTK
{
    public static partial class VRTKUtils
    {
        public static void InitializeArray<T>(this T[] self) where T : new()
        {
            for (int i = 0; i < self.Length; ++i)
            {
                self[i] = new T();
            }
        }

        // See: https://forum.unity.com/threads/shortest-rotation-between-two-quaternions.812346/
        public static Quaternion ShortestRotation(Quaternion a, Quaternion b)
        {
            if (Quaternion.Dot(a, b) < 0)
            {
                return a * Quaternion.Inverse(Multiply(b, -1));
            }
            else return a * Quaternion.Inverse(b);
        }

        public static Quaternion Multiply(Quaternion input, float scalar)
        {
            return new Quaternion(input.x * scalar, input.y * scalar, input.z * scalar, input.w * scalar);
        }
    }
}
