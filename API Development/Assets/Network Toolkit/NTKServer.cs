using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Utilities;
using VirtualRealityTK;

namespace NetworkToolkit
{
    public class NTKServer : NTKSocket
    {
        public HashSet<IPEndPoint> connectedDevices = new HashSet<IPEndPoint>();

        public NTKServer() { }

        public void Bind(int Port)
        {
            Bind(new IPEndPoint(IPAddress.Any, Port));
        }

        protected override void OnConnect(IPEndPoint ip)
        {
            Log(ip + ", has connected.");
            connectedDevices.Add(ip);
        }

        protected override void OnTimeout(IPEndPoint ip)
        {
            Log(ip + ", has timed out.");
            connectedDevices.Remove(ip);
        }

        protected override void OnAcknowledgeFail(IPEndPoint ip, ushort sequence)
        {
        }

        protected override void OnAcknowledge(IPEndPoint ip, ushort sequence)
        {
        }

        protected override void OnTick()
        {
        }

        protected override void OnPacketReconstructionTimeout(PacketIdentifier packet)
        {
        }

        protected override void OnReceive(IPEndPoint ip, NTK.Packet packet)
        {
        }

        protected override void OnError(Exception e)
        {
            Debug.Log(e.Message);
        }

        protected override void OnDispose()
        {
        }
    }
}