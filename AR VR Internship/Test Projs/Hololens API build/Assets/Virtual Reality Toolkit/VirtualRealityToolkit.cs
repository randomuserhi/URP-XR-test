using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VirtualRealityTK;

namespace VirtualRealityTK
{
    public class VirtualRealityToolkit : MonoBehaviour
    {
        private static VirtualRealityToolkit instance;

        public VRTK.Device device = VRTK.Device.Hololens2;

        public void Start()
        {
            if (instance != null)
            {
                Debug.LogWarning("An instance of VRTK already exists.");
                Destroy(this);
                return;
            }

            instance = this;
        }

        public void FixedUpdate()
        {
            VRTK.device = device;
        }

        [RuntimeInitializeOnLoadMethod]
        private static void Initialize()
        {
            VirtualRealityToolkit instance = FindObjectOfType<VirtualRealityToolkit>();
            if (instance == null)
            {
                Debug.LogWarning("VRTK was not found, creating a new object.");
                GameObject o = new GameObject("VirtualReality Toolkit");
                instance = o.AddComponent<VirtualRealityToolkit>();
            }
        }
    }
}