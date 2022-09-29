using InteractionTK.Menus;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using VirtualRealityTK;
using static NetworkToolkit.NTK;

namespace NetworkToolkit
{
    public static partial class NTK
    {
        [Serializable]
        public struct NetworkSyncGroup
        {
            public NetworkSync server;
            public NetworkSync client;
        }

        public static GameObject playerPrefab { get => NetworkToolkit.instance == null ? null : NetworkToolkit.instance.playerPrefab; }
        public static NetworkSyncGroup[] syncedObjects { get => NetworkToolkit.instance == null ? null : NetworkToolkit.instance.syncedObjects; }
    }

    public class NetworkToolkit : MonoBehaviour
    {
        public enum Type
        {
            Host,
            Client
        }

        internal static NetworkToolkit instance;

        public bool desktopMode = false;

        public GameObject playerPrefab;
        public NetworkSyncGroup[] syncedObjects;

        public MTKConsoleWindow console;
        public Type type = Type.Host;

        public string DesktopIP = "";
        public int DesktopPort = 26950;

        public UnityEvent<NTKServer> OnHost;
        public UnityEvent<NTKServer> OnHostEnd;
        public UnityEvent<NTKClient> OnConnect;
        public UnityEvent<NTKClient> OnDisconnect;

        public void Start()
        {
            if (instance != null)
            {
                Debug.LogWarning("An instance of NTK already exists.");
                Destroy(this);
                return;
            }

            instance = this;

            if (desktopMode)
                switch (type)
                {
                    case Type.Host:
                        SetupHost();
                        break;
                    case Type.Client:
                        SetupClient(IPAddress.Parse(DesktopIP), DesktopPort);
                        break;
                }
        }

        private NTKServer server;
        private NTKClient client;

        public void FixedUpdate()
        {
            if (!console) return;

            switch (type)
            {
                case Type.Host:
                    if (server == null) return;
                    console.tmp.text = server.log;
                    server.Tick();
                    break;
                case Type.Client:
                    if (client == null) return;
                    console.tmp.text = client.log;
                    client.Tick();
                    break;
            }
        }

        public void SetupHost()
        {
            type = Type.Host;
            
            server = new NTKServer();
            server.Bind(26950);

            OnHost?.Invoke(server);
        }

        public void DisconnnectHost()
        {
            if (type != Type.Host) return;

            OnHostEnd?.Invoke(server);

            server.Dispose();
            server = null;
        }

        public void SetupClient(IPAddress address, int port)
        {
            type = Type.Client;

            client = new NTKClient();
            client.Connect(address, port);

            OnConnect?.Invoke(client);
        }

        public void DisconnectClient()
        {
            if (type != Type.Client) return;

            OnDisconnect?.Invoke(client);

            client.Dispose();
            client = null;
        }

        public void OnApplicationExit()
        {
            if (server != null) server.Dispose();
            if (client != null) client.Dispose();
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
