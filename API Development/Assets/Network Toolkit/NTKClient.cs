using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VirtualRealityTK;
using InteractionTK.HandTracking;
using static NetworkToolkit.NTK;

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
        public new void Connect(IPAddress address, int port)
        {
            base.Connect(address, port);
        }

        protected override void OnConnect(IPEndPoint ip)
        {
            Log("Connected to " + ip + ".");
            connected = true;
        }

        protected override void OnTimeout(IPEndPoint ip)
        {
            Log("Timed out from " + ip + ".");
            connected = false;
        }

        private bool handshake = false;

        protected override void OnTick()
        {
            if (!handshake)
            {
                Packet packet = Packet.Create(1);
                packet.Write((byte)0);
                Send(packet, out _);
                return;
            }

            Packet playerData = Packet.Create(1000);
            playerData.Write((byte)1);
            // Head
            playerData.Write(VRTK.master.main.transform.position);
            playerData.Write(VRTK.master.main.transform.rotation);

            // Left Hand
            bool tracking;
            ITKHand.Pose pose = VRTK.master.clientLeftPoseProvider.GetPose(out tracking);
            playerData.Write(tracking);
            if (tracking)
            {
                // Root
                Vector3 relativeRootPosition = pose.positions[ITKHand.Root] - VRTK.master.main.transform.position;
                playerData.WriteHalf(relativeRootPosition);
                playerData.WriteHalf(pose.rotations[ITKHand.Root]);

                // Palm
                Vector3 relativePalmPosition = pose.positions[ITKHand.Palm] - VRTK.master.main.transform.position;
                playerData.WriteHalf(relativePalmPosition);
                playerData.WriteHalf(pose.rotations[ITKHand.Palm]);

                // Write the rest of the joints as rotations (except metacarpals)
                for (int i = 0; i < ITKHand.structure.Length; ++i)
                {
                    if (i == 0) continue; // Skip palm
                    for (int j = 0; j < ITKHand.structure[i].Length; ++j)
                    {
                        ITKHand.Joint joint = ITKHand.structure[i][j];

                        if (j == 0) // Handle metacarpal
                        {
                            Vector3 relativeMetacarpalPosition = pose.positions[joint] - VRTK.master.main.transform.position;
                            playerData.WriteHalf(relativeMetacarpalPosition);
                            playerData.WriteHalf(pose.rotations[joint]);
                        }
                        else playerData.WriteHalf(pose.rotations[joint]); // Handle fingers
                    }
                }
            }

            // Right Hand
            pose = VRTK.master.clientRightPoseProvider.GetPose(out tracking);
            playerData.Write(tracking);
            if (tracking)
            {
                // Root
                Vector3 relativeRootPosition = pose.positions[ITKHand.Root] - VRTK.master.main.transform.position;
                playerData.WriteHalf(relativeRootPosition);
                playerData.WriteHalf(pose.rotations[ITKHand.Root]);

                // Palm
                Vector3 relativePalmPosition = pose.positions[ITKHand.Palm] - VRTK.master.main.transform.position;
                playerData.WriteHalf(relativePalmPosition);
                playerData.WriteHalf(pose.rotations[ITKHand.Palm]);

                // Write the rest of the joints as rotations (except metacarpals)
                for (int i = 0; i < ITKHand.structure.Length; ++i)
                {
                    if (i == 0) continue; // Skip palm
                    for (int j = 0; j < ITKHand.structure[i].Length; ++j)
                    {
                        ITKHand.Joint joint = ITKHand.structure[i][j];

                        if (j == 0) // Handle metacarpal
                        {
                            Vector3 relativeMetacarpalPosition = pose.positions[joint] - VRTK.master.main.transform.position;
                            playerData.WriteHalf(relativeMetacarpalPosition);
                            playerData.WriteHalf(pose.rotations[joint]);
                        }
                        else playerData.WriteHalf(pose.rotations[joint]); // Handle fingers
                    }
                }
            }

            // Teleport state
            playerData.Write(VRTK.master.teleport);

            Send(playerData, out _);
        }

        protected override void OnAcknowledgeFail(IPEndPoint ip, ushort sequence)
        {
        }

        protected override void OnAcknowledge(IPEndPoint ip, ushort sequence)
        {
        }

        protected override void OnPacketReconstructionTimeout(PacketIdentifier packet)
        {
        }

        protected override void OnReceive(IPEndPoint ip, Packet packet)
        {
            byte code = packet.ReadByte();
            switch (code)
            {
                case 0:
                    {
                        Log("Server acknowledged connection.");
                        handshake = true;
                    }
                    break;
            }
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
