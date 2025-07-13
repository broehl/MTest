// Broadcast audio from the microphone over the local network using RTP

// Written by Bernie Roehl, July 2025

using System;
using System.Net;
using System.Net.Sockets;
using Unity.Netcode;
using UnityEngine;

namespace ConestogaMultiplayer
{
    public class RTPSender : NetworkBehaviour
    {
        [SerializeField] int basePort = 6000;  // this value + our network id is the port we use

        string microphoneDevice = "Headset Microphone (Oculus Virtual Audio Device)";

        AudioClip audioClip;         // this is where the microphone data is stored
        int lastpos = 0;             // last buffer position we read audio data from
        bool sending = false;

        // RTP variables:
        UInt16 sequenceNumber = 0;
        UInt32 timestamp = 0;

        UdpClient udpClient = new UdpClient();
        IPEndPoint remoteEndPoint;

        const int RTP_HEADER_LEN = 12;
        const int MAX_DATA_PER_PACKET = 1400;   // ethernet MTU is 1500, but we need to allow for headers (UDP=8 bytes, IP=40 bytes or more)

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (!IsOwner) return;
            udpClient.EnableBroadcast = true;
            int myBroadcastPort = basePort + (ushort)NetworkManager.Singleton.LocalClientId;
            print($"Broadcasting audio on port {myBroadcastPort}");
            remoteEndPoint = new IPEndPoint(IPAddress.Broadcast, myBroadcastPort);
            audioClip = Microphone.Start(microphoneDevice, true, 1, 44100);  // needs to be 44100 for RTP payload type 11
            if (audioClip == null)
            {
                Debug.Log("Failed to open specified mic, using default audio input");
                Debug.Log("Available audio devices are:");
                foreach (string devname in Microphone.devices) Debug.Log($"  {devname}");
                audioClip = Microphone.Start(null, true, 3, 44100);
                if (audioClip == null) Debug.Log("Failed to open audio input");
            }
            if (audioClip) sending = true;
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            if (!IsOwner) return;
            sending = false;
            Microphone.End(microphoneDevice);
        }

        void Write16(byte[] buffer, int offset, UInt16 value)
        {
            buffer[offset] = ((byte)((value >> 8) & 0xFF));
            buffer[offset + 1] = ((byte)(value & 0xFF));
        }

        void Send(float[] samples)
        {
            if (udpClient == null) return;
            if (samples == null || samples.Length == 0) return;

            // convert samples from floats (range -1 to +1) to signed 16-bit ints
            byte[] audioBuffer = new byte[samples.Length * sizeof(Int16)];
            int bufferIndex = 0;
            for (int sampleIndex = 0; sampleIndex < samples.Length; ++sampleIndex)
            {
                Int16 sample = (Int16)(samples[sampleIndex] * Int16.MaxValue);
                Write16(audioBuffer, bufferIndex, unchecked((UInt16)sample));
                bufferIndex += sizeof(Int16);
            }

            // send out the audio as a series of RTP packets
            int offset = 0;
            int dataLeftToSend = audioBuffer.Length;
            while (dataLeftToSend > 0)
            {
                int bodyLen = Math.Min(dataLeftToSend, MAX_DATA_PER_PACKET - RTP_HEADER_LEN);
                byte[] packetBuffer = new byte[RTP_HEADER_LEN + bodyLen];
                Write16(packetBuffer, 0, FirstTwoBytes(2, 0, 0, 0, 1, 11));
                Write16(packetBuffer, 2, sequenceNumber++);
                Write16(packetBuffer, 4, (UInt16)(timestamp >> 16));
                Write16(packetBuffer, 6, (UInt16)timestamp);
                Write16(packetBuffer, 8, 0);    // SSRC
                Write16(packetBuffer, 10, 0);   // SSRC
                Array.Copy(audioBuffer, offset, packetBuffer, RTP_HEADER_LEN, bodyLen);
                int dataSent = udpClient.Send(packetBuffer, packetBuffer.Length, remoteEndPoint);
                dataLeftToSend -= dataSent;
                offset += dataSent;
            }
            timestamp += (UInt32)Mathf.FloorToInt(Time.deltaTime * 1000);
        }

        UInt16 FirstTwoBytes(int rtpVersion, int rtpPadding, int rtpExtension, int rtpSrcCount, int rtpMarker, int rtpPayloadType)
        {
            byte firstByte = (byte)((rtpVersion << 6) | (rtpPadding << 5) | (rtpExtension << 4) | rtpSrcCount);
            byte secondByte = (byte)((rtpMarker << 7) | (rtpPayloadType & 0x7F));
            return (UInt16)((firstByte << 8) | secondByte);
        }

        public void FixedUpdate()
        {
            if (!sending) return;
            // pull data from the microphone's audioclip and send it
            int pos = Microphone.GetPosition(microphoneDevice);
            if (pos <= 0) return;
            if (lastpos > pos) lastpos = 0;  // wrap-around
            int len = pos - lastpos;  // number of bytes since the last time we read the buffer
            if (len <= 0) return;
            float[] samples = new float[len];
            audioClip.GetData(samples, lastpos);
            lastpos = pos;
            Send(samples);
        }
    }
}