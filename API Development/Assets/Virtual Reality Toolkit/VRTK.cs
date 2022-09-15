using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualRealityTK
{
    public static partial class VRTK
    {
        public enum Device
        {
            Hololens2,
            Oculus
        }

        public static Device device = Device.Hololens2;
    }
}
