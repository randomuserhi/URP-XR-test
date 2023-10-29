using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

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
            Debug.Log(ip + " has connected.");
            connectedDevices.Add(ip);
        }

        protected override void OnTimeout(IPEndPoint ip)
        {
            connectedDevices.Remove(ip);
        }

        protected override void OnPacketReconstructionTimeout(PacketIdentifier packet)
        {
        }

        protected override void OnAcknowledgeFail(IPEndPoint ip, ushort sequence)
        {
        }

        protected override void OnAcknowledge(IPEndPoint ip, ushort sequence)
        {
        }

        protected override void OnReceive(IPEndPoint ip, NTK.Packet packet)
        {
            Debug.Log(packet.ReadString());
        }
    }
}