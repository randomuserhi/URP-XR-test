using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;

// NOTE:: System.Net and System.Net.Sockets are not available in older UWP versions and thus
//        this library may not work with Hololens 1 or versions less than UWP 10

namespace NetworkToolkit
{
    public class NTKSocket
    {
        protected byte[] buffer;
        private EndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);

        private Socket socket = null;

        private int line = 0;
        private string[] _log = new string[100];
        protected enum Tag
        {
            Warning,
            Fatal
        }
        protected void Log(Tag tag, string text)
        {
            string message = string.Empty;
            switch (tag)
            { 
                case Tag.Warning:
                    message = "Warning";
                    break;
                case Tag.Fatal:
                    message = "Fatal";
                    break;
                default:
                    message = "None";
                    break;
            }
            Log("[ " + message + " ] " + text);
        }
        public void Log(string text)
        {
            _log[line++] = text;
            line = line % _log.Length;
            logChange = true;
        }
        private bool logChange = false;
        private string _logBuffer = string.Empty;
        public string log 
        {
            get
            {
                if (logChange)
                {
                    _logBuffer = string.Join(Environment.NewLine, _log);
                    logChange = false;
                }
                return _logBuffer;
            }
        }

        protected virtual void OnError(Exception e) { }

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
                        packet.CopyTo(data, group * NTK.bufferSizeNoHeader, copySize);

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
            try
            {
                if (socket == null)
                {
                    Log(Tag.Warning, "Socket was not initialized yet.");
                    return;
                }

                UpdateReconstructedPackets();
                UpdateConnections();
            }
            catch (Exception e)
            {
                Log(Tag.Fatal, "Failed to process socket tick: " + e.Message);
                Disconnect();

                OnError(e); throw e; // Throw exception again so try - catch blocks outside can handle them
            }

            OnTick();
        }
        protected virtual void OnTick() { }

        public NTKSocket()
        {
            buffer = new byte[NTK.bufferSize];
        }
        private void InitializeSocket()
        {
            Log("Initializeing Socket...");
            try
            {
                if (socket != null) socket.Dispose();
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);
                try
                {
                    //https://stackoverflow.com/questions/38191968/c-sharp-udp-an-existing-connection-was-forcibly-closed-by-the-remote-host
                    socket.IOControl(
                        (IOControlCode)(-1744830452),
                        new byte[] { 0, 0, 0, 0 },
                        null
                    ); //Ignores UDP exceptions - can cause problems for other platforms (e.g Oculus and android), so wrap in a try catch and post warning.
                }
                catch (Exception e)
                {
                    Log(Tag.Warning, "socket.IOControl failed: " + e.Message);
                }
                Log("Socket initialized successfully.");
            }
            catch (Exception e)
            {
                Log(Tag.Fatal, "Socket failed to initialize: " + e.Message);
                Disconnect();

                OnError(e); throw e; // Throw exception again so try - catch blocks outside can handle them
            }
        }

        protected void Connect(IPAddress address, int port)
        {
            try
            {
                InitializeSocket();
                Log("Connecting to " + address + ":" + port + "...");
                lock (connectionLock)
                {
                    connections.Clear();
                    IPEndPoint ip = new IPEndPoint(address, port);
                    if (!connections.ContainsKey(ip))
                    {
                        connections.Add(ip, new Connection(ip));
                        OnConnect(ip);
                    }
                }
                lock (packetLock)
                {
                    packets.Clear();
                }
                socket.Connect(address, port);
                BeginReceive();
                Log("Socket connected successfully.");
            }
            catch (Exception e)
            {
                Log(Tag.Fatal, "Socket failed to connect: " + e.Message);
                Disconnect();

                OnError(e); throw e;
            }
        }
        protected void Bind(IPEndPoint ip)
        {
            try
            {
                InitializeSocket();
                Log("Binding to " + ip + "...");
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
            catch (Exception e)
            {
                Log(Tag.Fatal, "Socket failed to bind: " + e.Message);
                Disconnect();

                OnError(e); throw e;
            }
        }
        protected void Disconnect()
        {
            Log("Disconnecting and disposing socket...");
            socket.Dispose();
            socket = null;

            packets.Clear();
            connections.Clear();
        }

        protected void BeginReceive()
        {
            try
            {
                if (socket == null)
                {
                    Log(Tag.Warning, "Socket was not initialized yet.");
                    return;
                }

                socket.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref endPoint, ReceiveCallback, null);
            }
            catch (Exception e)
            {
                Log(Tag.Fatal, "Socket failed to start receiving: " + e.Message);
                Disconnect();

                OnError(e); throw e;
            }
        }

        private void ReceiveCallback(IAsyncResult result)
        {
            try
            {
                NTK.Packet packet = NTK.Packet.Create(NTK.bufferSize);
                IPEndPoint ip = null;
                Connection connection = null;
                lock (connectionLock)
                {
                    if (socket == null) return;
                    int numBytes = socket.EndReceiveFrom(result, ref endPoint);
                    packet.Copy(buffer, numBytes);

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
            catch (Exception e)
            {
                Log(Tag.Fatal, "(ReceiveCallback) Failed to process incoming packet: " + e.Message);
                Disconnect();

                OnError(e); throw e;
            }
        }

        public bool Send(NTK.Packet packet, NTK.PacketHeader header, out ushort sequence)
        {
            try
            {
                if (socket == null)
                {
                    Log(Tag.Warning, "Socket was not initialized yet.");
                    sequence = 0;
                    return false;
                }

                IPEndPoint ip = socket.RemoteEndPoint as IPEndPoint;
                if (!connections.ContainsKey(ip)) connections.Add(ip, new Connection(ip));
                Connection connection = connections[ip];
                byte[][] bytes;
                sequence = connection.sequence;
                if (packet.GetBytes(header, out bytes))
                {
                    if (bytes.Length == 0) return false;

                    for (int i = 0; i < bytes.Length; ++i)
                        socket.BeginSend(bytes[i], 0, bytes[i].Length, SocketFlags.None, null, null);

                    connections[ip].sent.Add(sequence);
                    return true;
                }
            }
            catch (Exception e)
            {
                Log(Tag.Fatal, "Failed to send packet: " + e.Message);
                Disconnect();

                OnError(e); throw e;
            }
            return false;
        }

        public bool SendTo(NTK.Packet packet, NTK.PacketHeader header, IPEndPoint destination, out ushort sequence)
        {
            try
            {
                if (socket == null)
                {
                    Log(Tag.Warning, "Socket was not initialized yet.");
                    sequence = 0;
                    return false;
                }

                if (!connections.ContainsKey(destination)) connections.Add(destination, new Connection(destination));
                Connection connection = connections[destination];
                byte[][] bytes;
                sequence = connection.sequence;
                if (packet.GetBytes(header, out bytes))
                {
                    if (bytes.Length == 0) return false;

                    for (int i = 0; i < bytes.Length; ++i)
                        socket.BeginSendTo(bytes[i], 0, bytes[i].Length, SocketFlags.None, destination, null, null);

                    connections[destination].sent.Add(sequence);
                    return true;
                }
            }
            catch (Exception e)
            {
                Log(Tag.Fatal, "Failed to send packet: " + e.Message);
                Disconnect();

                OnError(e); throw e;
            }
            return false;
        }

        public bool Send(NTK.Packet packet, out ushort sequence)
        {
            try
            {
                if (socket == null)
                {
                    Log(Tag.Warning, "Socket was not initialized yet.");
                    sequence = 0;
                    return false;
                }

                IPEndPoint ip = socket.RemoteEndPoint as IPEndPoint;
                if (!connections.ContainsKey(ip)) connections.Add(ip, new Connection(ip));
                Connection connection = connections[ip];
                byte[][] bytes;
                sequence = connection.sequence;
                NTK.PacketHeader header = new NTK.PacketHeader()
                {
                    sequence = sequence,
                    ack = connections[ip].ack,
                    ackBitfield = connections[ip].ackBitField
                };
                if (packet.GetBytes(header, out bytes))
                {
                    if (bytes.Length == 0) return false;

                    for (int i = 0; i < bytes.Length; ++i)
                        socket.BeginSend(bytes[i], 0, bytes[i].Length, SocketFlags.None, null, null);

                    connections[ip].sent.Add(sequence);
                    return true;
                }
            }
            catch (Exception e)
            {
                Log(Tag.Fatal, "Failed to send packet: " + e.Message);
                Disconnect();

                OnError(e); throw e;
            }
            return false;
        }

        public bool SendTo(NTK.Packet packet, IPEndPoint destination, out ushort sequence)
        {
            try
            {
                if (socket == null)
                {
                    Log(Tag.Warning, "Socket was not initialized yet.");
                    sequence = 0;
                    return false;
                }

                if (!connections.ContainsKey(destination)) connections.Add(destination, new Connection(destination));
                Connection connection = connections[destination];
                byte[][] bytes;
                sequence = connection.sequence;
                NTK.PacketHeader header = new NTK.PacketHeader()
                {
                    sequence = sequence,
                    ack = connections[destination].ack,
                    ackBitfield = connections[destination].ackBitField
                };
                if (packet.GetBytes(header, out bytes))
                {
                    if (bytes.Length == 0) return false;

                    for (int i = 0; i < bytes.Length; ++i)
                        socket.BeginSendTo(bytes[i], 0, bytes[i].Length, SocketFlags.None, destination, null, null);

                    connections[destination].sent.Add(sequence);
                    return true;
                }
            }
            catch (Exception e)
            {
                Log(Tag.Fatal, "Failed to send packet: " + e.Message);
                Disconnect();

                OnError(e); throw e;
            }
            return false;
        }

        public void Dispose()
        {
            Log("Disposing socket...");

            socket.Dispose();
            socket = null;
            OnDispose();
        }

        protected virtual void OnDispose() { }
    }
}
