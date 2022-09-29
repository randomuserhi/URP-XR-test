using InteractionTK.HandTracking;
using Microsoft.MixedReality.Toolkit.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VirtualRealityTK;

namespace NetworkToolkit
{
    public class NTKPoseProvider : ITKPoseProvider
    {
        private readonly object providerLock = new object();

        private bool receivedTrackingState = false;
        private ITKHand.Pose buffer = new ITKHand.Pose(ITKHand.NumJoints);
        private ITKHand.Pose pose = new ITKHand.Pose(ITKHand.NumJoints);

        public void ParsePacket(NTK.Packet data, Vector3 head)
        {
            lock (providerLock)
            {
                receivedTrackingState = data.ReadBool();

                if (receivedTrackingState)
                {
                    // Root
                    buffer.positions[ITKHand.Root] = head + data.ReadHalfVector3();
                    buffer.rotations[ITKHand.Root] = data.ReadHalfQuaternion();

                    // Palm
                    buffer.positions[ITKHand.Palm] = head + data.ReadHalfVector3();
                    buffer.rotations[ITKHand.Palm] = data.ReadHalfQuaternion();

                    // Read the rest of the joints as rotations (except metacarpals)
                    for (int i = 0; i < ITKHand.structure.Length; ++i)
                    {
                        if (i == 0) continue; // Skip palm
                        for (int j = 0; j < ITKHand.structure[i].Length; ++j)
                        {
                            ITKHand.Joint joint = ITKHand.structure[i][j];

                            if (j == 0) // Handle metacarpal
                            {
                                buffer.positions[joint] = head + data.ReadHalfVector3();
                                buffer.rotations[joint] = data.ReadHalfQuaternion();
                            }
                            else // Handle fingers
                            {
                                ITKHand.Joint prev = ITKHand.structure[i][j - 1];

                                buffer.rotations[joint] = data.ReadHalfQuaternion();
                                buffer.positions[joint] = buffer.positions[prev] + buffer.rotations[prev] * Vector3.forward * ITKHand.distances[i][j];
                            }
                        }
                    }
                }
            }
        }

        public override ITKHand.Pose GetPose(out bool tracking)
        {
            lock (providerLock)
            {
                tracking = receivedTrackingState;

                if (tracking) // On successful track swap buffers
                {
                    ITKHand.Pose temp = buffer;
                    buffer = pose;
                    pose = temp;
                }
            }

            return pose;
        }
    }
}
