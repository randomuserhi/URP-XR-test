using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VirtualRealityTK;

namespace NetworkToolkit
{
    public class PlayerNetworkSync : NetworkSync
    {
        public VRTKPlayer master;
        public GameObject head;
        public NTKPoseProvider leftPoseProvider;
        public NTKPoseProvider rightPoseProvider;

        public override void Parse(NTK.Packet data)
        {
            head.transform.position = data.ReadVector3();
            head.transform.rotation = data.ReadQuaternion();

            leftPoseProvider.ParsePacket(data, head.transform.position);
            rightPoseProvider.ParsePacket(data, head.transform.position);

            master.teleport = data.ReadBool();
        }
    }
}
