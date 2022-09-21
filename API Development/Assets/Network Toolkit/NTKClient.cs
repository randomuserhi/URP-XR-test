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
    public class NTKClient : NTKSocket
    {
        public NTKClient() { }

        public bool connected { get; private set; } = false;

        public void Connect(string address, int port)
        {
            Connect(IPAddress.Parse(address), port);
        }

        protected override void OnConnect(IPEndPoint ip)
        {
            Log("Connected to " + ip + ".");
            connected = true;
        }

        protected override void OnTimeout(IPEndPoint ip)
        {
            Log("Timed out from " + ip + ".");
            Disconnect();
            connected = false;
        }

        protected override void OnAcknowledgeFail(IPEndPoint ip, ushort sequence)
        {
        }

        protected override void OnAcknowledge(IPEndPoint ip, ushort sequence)
        {
        }

        protected override void OnReceive(IPEndPoint ip, NTK.Packet packet)
        {
        }

        protected override void OnError(Exception e)
        {
            Debug.Log(e.Message);
        }
    }
}
