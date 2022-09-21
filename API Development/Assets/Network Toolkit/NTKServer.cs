using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static NetworkToolkit.NTK;

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
            Console.WriteLine(ip + ", has connected.");
            connectedDevices.Add(ip);
        }

        protected override void OnTimeout(IPEndPoint ip)
        {
            Console.WriteLine(ip + ", has timed out.");
            connectedDevices.Remove(ip);
        }

        protected override void OnPacketReconstructionTimeout(PacketIdentifier packet)
        {
            Console.WriteLine("Failed to reconstruct packet from: " + packet.ip + ", with id: " + packet.id);
        }

        protected override void OnAcknowledgeFail(IPEndPoint ip, ushort sequence)
        {
            Console.WriteLine("Failed to acknowledged: " + sequence);
        }

        protected override void OnAcknowledge(IPEndPoint ip, ushort sequence)
        {
            Console.WriteLine("Acknowledged: " + sequence);
        }

        protected override void OnReceive(IPEndPoint ip, NTK.Packet packet)
        {
            Console.WriteLine(packet.ReadString());
        }
    }
}