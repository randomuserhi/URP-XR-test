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

        private NTKClient client;
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
            client = new NTKClient();
            client.Connect("192.168.8.141", 26950);
        }

        public TextMeshProUGUI log;
        public void FixedUpdate()
        {
            log.text = client.log;

            if (client.connected)
            {
                p.Reset();
                p.Write("hello");
                client.Send(p);

                client.Tick();
            }
        }

        public void OnApplicationExit()
        {
            client.Dispose();
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
