using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NetworkToolkit
{
    public class NTKClient : NTKSocket
    {
        public NTKClient() { }

        public void Connect(string address, int port)
        {
            Connect(IPAddress.Parse(address), port);
        }

        protected override void OnConnect(IPEndPoint ip)
        {
            Console.WriteLine(ip + ", has connected.");
        }

        protected override void OnTimeout(IPEndPoint ip)
        {
            Console.WriteLine(ip + ", has timed out.");
            Disconnect();
        }

        protected override void OnAcknowledgeFail(IPEndPoint endPoint, ushort sequence)
        {
            Console.WriteLine("Failed to acknowledged: " + sequence);
        }

        protected override void OnAcknowledge(IPEndPoint endPoint, ushort sequence)
        {
            //Console.WriteLine("Acknowledged: " + sequence);
        }

        protected override void OnReceive(IPEndPoint endPoint, NTK.Packet packet)
        {
            //Console.WriteLine(packet.ReadString());
        }
    }
}
