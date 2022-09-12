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
    }
}
