using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEditor.PackageManager;
using UnityEngine;
using VirtualRealityTK;

namespace NetworkToolkit
{
    public class NetworkToolkit : MonoBehaviour
    {
        private static NetworkToolkit instance;

        private NTKServer server;
        public GameObject test;
        private NTK.Packet p;

        public void Start()
        {
            if (instance != null)
            {
                Debug.LogWarning("An instance of NTK already exists.");
                Destroy(this);
                return;
            }

            instance = this;

            p = NTK.Packet.Create(NTK.bufferSize);
            server = new NTKServer();
            server.Bind(26950);
        }

        public void FixedUpdate()
        {
            p.Reset();
            p.Write(test.transform.position);
            p.Write(test.transform.rotation);
            foreach (IPEndPoint ip in server.connectedDevices)
                server.SendTo(p, ip);
            
            server.Tick();
        }

        public void OnApplicationExit()
        {
            server.Dispose();
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
