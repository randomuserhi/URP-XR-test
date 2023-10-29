using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace NetworkToolkit
{
    public class NTKSocket
    {
        protected byte[] buffer;
        private EndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);

        private Socket socket = null;

        protected struct PacketIdentifier
        {
            public IPEndPoint ip;
            public int id;

            public override bool Equals(object Obj)
            {
                return Obj is PacketIdentifier && this == (PacketIdentifier)Obj;
            }
            public override int GetHashCode()
            {
                int Hash = 27;
                Hash = (13 * Hash) + ip.GetHashCode();
                Hash = (13 * Hash) + id.GetHashCode();
                return Hash;
            }
            public static bool operator ==(PacketIdentifier a, PacketIdentifier b)
            {
                if (ReferenceEquals(a, null) && ReferenceEquals(b, null))
                    return true;
                else if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
                    return false;
                return a.id == b.id && a.ip.Equals(b.ip);
            }
            public static bool operator !=(PacketIdentifier a, PacketIdentifier b)
            {
                if (ReferenceEquals(a, null) && ReferenceEquals(b, null))
                    return false;
                else if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
                    return true;
                return a.id != b.id || !a.ip.Equals(b.ip);
            }
        }

        private class PacketReconstructor
        {
            private readonly object reconstructorLock = new object();

            public int lifeTime = 0;

            public byte[] data { get; private set; }
            private HashSet<int> groups = new HashSet<int>();
            private int totalGroups;

            private bool _completed = false;
            public bool completed { get => _completed; }

            public PacketReconstructor(int size, int totalGroups)
            {
                this.totalGroups = totalGroups;
                data = new byte[size];
            }

            public void Reconstruct(NTK.Packet packet, int size, int group, int totalGroups)
            {
                lock (reconstructorLock)
                {
                    if (totalGroups <= 0) return; // TODO:: Log warning, packet cannot have total group <= 0, should be > 0
                    if (size != data.Length || totalGroups != this.totalGroups) return; // TODO:: Log warning, packet doesn't match

                    if (!groups.Contains(group))
                    {
                        int copySize = NTK.bufferSizeNoHeader;
                        if (group == totalGroups - 1) copySize = data.Length - group * NTK.bufferSizeNoHeader;
                        Array.Copy(packet.data, packet.index, data, group * NTK.bufferSizeNoHeader, copySize);

                        groups.Add(group);
                        _completed = groups.Count == totalGroups;
                    }
                }
            }
        }
        private readonly object packetLock = new object();
        private Dictionary<PacketIdentifier, PacketReconstructor> packets = new Dictionary<PacketIdentifier, PacketReconstructor>();
        private void UpdateReconstructedPackets()
        {
            lock (packetLock)
            {
                PacketIdentifier[] p = packets.Keys.ToArray();
                for (int i = 0; i < p.Length; ++i)
                {
                    if (++packets[p[i]].lifeTime > NTK.tickRate)
                    {
                        packets.Remove(p[i]);
                        OnPacketReconstructionTimeout(p[i]);
                    }
                }
            }
        }
        protected virtual void OnPacketReconstructionTimeout(PacketIdentifier packet) { }

        private class Connection
        {
            public readonly IPEndPoint ip;

            public int timeSinceResponse = 0;

            private ushort _sequence = 0;
            public ushort sequence
            {
                get
                {
                    return _sequence++;
                }
            }

            private ushort _ack;
            public ushort ack 
            { 
                get
                {
                    return _ack;
                }
                set
                {
                    init = true;
                    _ack = value;
                }
            }
            public int ackBitField = 0;

            public bool init { get; private set; } = false;
            public bool active = true;

            public HashSet<ushort> sent = new HashSet<ushort>();

            public Connection(IPEndPoint ip)
            {
                this.ip = ip;
            }

            private readonly object acknowledgeLock = new object();
            public void Acknowledge(NTK.Packet packet, NTKSocket socket)
            {
                timeSinceResponse = 0;

                lock (acknowledgeLock)
                {
                    ushort sequence = packet.ReadUShort();
                    ushort ack = packet.ReadUShort();
                    int ackBitField = packet.ReadInt();

                    if (sent.Contains(ack))
                    {
                        sent.Remove(ack);
                        socket.OnAcknowledge(ip, ack);
                    }
                    for (int i = 0; i < 32; ++i)
                    {
                        int mask = 1 << i;
                        if ((ackBitField & mask) == mask)
                        {
                            ushort redundantAck = ack;
                            redundantAck -= 1;
                            redundantAck -= (ushort)i;
                            if (sent.Contains(redundantAck))
                            {
                                sent.Remove(redundantAck);
                                socket.OnAcknowledge(ip, redundantAck);
                            }
                        }
                    }

                    ushort[] temp = sent.ToArray();
                    for (int i = 0; i < temp.Length; ++i)
                    {
                        if (NTK.isSequenceGreater(ack, temp[i]))
                        {
                            int delta = ack > temp[i] ? ack - temp[i] : ushort.MaxValue - temp[i] + ack + 1;
                            if (delta > 32)
                            {
                                sent.Remove(temp[i]);
                                socket.OnAcknowledgeFail(ip, temp[i]);
                            }
                        }
                    }

                    if (!init)
                    {
                        this.ack = sequence;
                        socket.OnReceive(ip, packet);
                    }
                    else if (NTK.isSequenceGreater(sequence, this.ack))
                    {
                        int delta = sequence > this.ack ? sequence - this.ack : ushort.MaxValue - this.ack + sequence + 1;
                        this.ackBitField = this.ackBitField << delta;
                        this.ackBitField |= 1 << (--delta);

                        this.ack = sequence;

                        socket.OnReceive(ip, packet);
                    }
                    else if (NTK.isSequenceGreater(this.ack, sequence))
                    {
                        int delta = this.ack > sequence ? this.ack - sequence : ushort.MaxValue - sequence + this.ack + 1;
                        this.ackBitField |= 1 << (--delta);
                    }
                }
            }
        }
        private readonly object connectionLock = new object();
        private Dictionary<IPEndPoint, Connection> connections = new Dictionary<IPEndPoint, Connection>();
        private void UpdateConnections()
        {
            lock (connectionLock)
            {
                IPEndPoint[] ips = connections.Keys.ToArray();
                for (int i = 0; i < ips.Length; ++i)
                {
                    if (++connections[ips[i]].timeSinceResponse > NTK.timeout)
                    {
                        connections.Remove(ips[i]);
                        OnTimeout(ips[i]);
                    }
                }
            }
        }
        protected virtual void OnTimeout(IPEndPoint ip) { }
        protected virtual void OnConnect(IPEndPoint ip) { }

        // NOTE:: OnReceive, OnAcknowledge and OnAcknowledgeFail can be called concurrently, so make sure they are thread safe
        protected virtual void OnReceive(IPEndPoint ip, NTK.Packet packet) { }
        protected virtual void OnAcknowledge(IPEndPoint ip, ushort sequence) { }
        protected virtual void OnAcknowledgeFail(IPEndPoint ip, ushort sequence) { }

        public void Tick()
        {
            if (socket == null) return;

            UpdateReconstructedPackets();
            UpdateConnections();

            OnTick();
        }
        protected virtual void OnTick() { }

        public NTKSocket()
        {
            buffer = new byte[NTK.bufferSize];
        }
        private void InitializeSocket()
        {
            if (socket != null) socket.Dispose();
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);
            //https://stackoverflow.com/questions/38191968/c-sharp-udp-an-existing-connection-was-forcibly-closed-by-the-remote-host
            socket.IOControl(
                (IOControlCode)(-1744830452),
                new byte[] { 0, 0, 0, 0 },
                null
            ); //Ignores UDP exceptions
        }

        protected void Connect(IPAddress address, int port)
        {
            InitializeSocket();
            lock (connectionLock)
            {
                IPEndPoint ip = new IPEndPoint(address, port);
                if (!connections.ContainsKey(ip))
                {
                    connections.Add(ip, new Connection(ip));
                    OnConnect(ip);
                }
                connections.Clear();
            }
            lock (packetLock)
            {
                packets.Clear();
            }
            socket.Connect(address, port);
            BeginReceive();
        }
        protected void Bind(IPEndPoint ip)
        {
            InitializeSocket();
            lock (connectionLock)
            {
                connections.Clear();
            }
            lock (packetLock)
            {
                packets.Clear();
            }
            socket.Bind(ip);
            BeginReceive();
        }
        protected void Disconnect()
        {
            socket.Dispose();
            socket = null;

            packets.Clear();
            connections.Clear();
        }

        protected void BeginReceive()
        {
            if (socket == null) return;

            socket.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref endPoint, ReceiveCallback, null);
        }

        private void ReceiveCallback(IAsyncResult result)
        {
            UnityEngine.Debug.Log("ReceiveCallback");

            NTK.Packet packet = NTK.Packet.Create(NTK.bufferSize);
            IPEndPoint ip = null;
            Connection connection = null;
            lock (connectionLock)
            {
                if (socket == null) return;
                int numBytes = socket.EndReceiveFrom(result, ref endPoint);
                Array.Copy(buffer, packet.data, numBytes);

                ip = endPoint as IPEndPoint;
                if (!connections.ContainsKey(ip))
                {
                    connections.Add(ip, new Connection(ip));
                    OnConnect(ip);
                }
                connection = connections[ip];
            }
            if (socket == null) return;
            socket.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref endPoint, ReceiveCallback, null);

            packet.index = 0;

            int id = packet.ReadUShort();
            int size = packet.ReadUShort();
            int group = packet.ReadByte();
            int totalGroups = packet.ReadByte();

            PacketIdentifier packetIdentifier = new PacketIdentifier()
            { 
                ip = ip,
                id = id
            };

            lock (packetLock)
            {
                if (!packets.ContainsKey(packetIdentifier))
                    packets.Add(packetIdentifier, new PacketReconstructor(size, totalGroups));
            }

            PacketReconstructor reconstructor = packets[packetIdentifier];
            reconstructor.Reconstruct(packet, size, group, totalGroups);
            if (reconstructor.completed)
            {
                lock (packetLock) packets.Remove(packetIdentifier);
                connection.Acknowledge(NTK.Packet.Create(reconstructor.data), this);
            }
        }

        public bool Send(NTK.Packet packet, NTK.PacketHeader header)
        {
            if (socket == null) return false;

            IPEndPoint ip = socket.RemoteEndPoint as IPEndPoint;
            if (!connections.ContainsKey(ip)) connections.Add(ip, new Connection(ip));
            Connection connection = connections[ip];
            byte[][] bytes;
            ushort seq = connection.sequence;
            if (packet.GetBytes(header, out bytes))
            {
                if (bytes.Length == 0) return false;

                for (int i = 0; i < bytes.Length; ++i)
                    socket.BeginSend(bytes[i], 0, bytes[i].Length, SocketFlags.None, null, null);

                connections[ip].sent.Add(seq);
                return true;
            }
            return false;
        }

        public bool SendTo(NTK.Packet packet, NTK.PacketHeader header, IPEndPoint destination)
        {
            if (socket == null) return false;

            if (!connections.ContainsKey(destination)) connections.Add(destination, new Connection(destination));
            Connection connection = connections[destination];
            byte[][] bytes;
            ushort seq = connection.sequence;
            if (packet.GetBytes(header, out bytes))
            {
                if (bytes.Length == 0) return false;

                for (int i = 0; i < bytes.Length; ++i)
                    socket.BeginSendTo(bytes[i], 0, bytes[i].Length, SocketFlags.None, destination, null, null);

                connections[destination].sent.Add(seq);
                return true;
            }
            return false;
        }

        public bool Send(NTK.Packet packet)
        {
            if (socket == null) return false;

            IPEndPoint ip = socket.RemoteEndPoint as IPEndPoint;
            if (!connections.ContainsKey(ip)) connections.Add(ip, new Connection(ip));
            Connection connection = connections[ip];
            byte[][] bytes;
            ushort seq = connection.sequence;
            NTK.PacketHeader header = new NTK.PacketHeader()
            {
                sequence = seq,
                ack = connections[ip].ack,
                ackBitfield = connections[ip].ackBitField
            };
            if (packet.GetBytes(header, out bytes))
            {
                if (bytes.Length == 0) return false;

                for (int i = 0; i < bytes.Length; ++i)
                    socket.BeginSend(bytes[i], 0, bytes[i].Length, SocketFlags.None, null, null);

                connections[ip].sent.Add(seq);
                return true;
            }
            return false;
        }

        public bool SendTo(NTK.Packet packet, IPEndPoint destination)
        {
            if (socket == null) return false;

            if (!connections.ContainsKey(destination)) connections.Add(destination, new Connection(destination));
            Connection connection = connections[destination];
            byte[][] bytes;
            ushort seq = connection.sequence;
            NTK.PacketHeader header = new NTK.PacketHeader()
            {
                sequence = seq,
                ack = connections[destination].ack,
                ackBitfield = connections[destination].ackBitField
            };
            if (packet.GetBytes(header, out bytes))
            {
                if (bytes.Length == 0) return false;

                for (int i = 0; i < bytes.Length; ++i)
                    socket.BeginSendTo(bytes[i], 0, bytes[i].Length, SocketFlags.None, destination, null, null);

                connections[destination].sent.Add(seq);
                return true;
            }
            return false;
        }

        private void SendToCallback(IAsyncResult result)
        {
            int numBytesSent = socket.EndSendTo(result);
            OnSendTo(numBytesSent);
        }

        protected virtual void OnSendTo(int numBytesSent) { }

        public void Dispose()
        {
            socket.Dispose();
            socket = null;
            OnDispose();
        }

        protected virtual void OnDispose() { }
    }
}
