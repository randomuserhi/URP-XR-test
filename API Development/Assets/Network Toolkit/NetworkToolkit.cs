using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using VirtualRealityTK;

namespace NetworkToolkit
{
    public class NetworkToolkit : MonoBehaviour
    {
        public static NetworkToolkit instance;


        public void Start()
        {
            if (instance != null)
            {
                Debug.LogWarning("An instance of NTK already exists.");
                Destroy(this);
                return;
            }

            instance = this;
        }

        public void FixedUpdate()
        {
        }

        public void OnApplicationExit()
        {
        }

        [RuntimeInitializeOnLoadMethod]
        private static void Initialize()
        {
            NetworkToolkit instance = FindObjectOfType<NetworkToolkit>();
            if (instance == null)
            {
                Debug.LogWarning("NTK was not found, creating a new object.");
                GameObject o = new GameObject("Network Toolkit");
                NetworkToolkit.instance = o.AddComponent<NetworkToolkit>();
            }
        }
    }
}
