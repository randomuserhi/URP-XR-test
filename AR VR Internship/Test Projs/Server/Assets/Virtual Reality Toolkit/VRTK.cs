using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualRealityTK
{
    public static partial class VRTK
    {
        public enum Device
        {
            Desktop,
            Hololens2,
            Oculus,
            OculusV2 // Second version for oculus for hand tracking V2 in OVR settings
        }

        public static Device device = Device.Hololens2;
    }
}
