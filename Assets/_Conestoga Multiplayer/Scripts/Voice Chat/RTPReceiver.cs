// Receive audio data over RTP and play it back

// Written by Bernie Roehl, July 2025

using System;
using System.Net;
using System.Net.Sockets;
using Unity.Netcode;
using UnityEngine;

namespace ConestogaMultiplayer
{
    public class RTPReceiver : NetworkBehaviour
    {
        [SerializeField] int basePort = 6000;

        AudioSource audioSource;

        float startThreshold = 4000;  // number of samples to accumulate before starting

        UdpClient udpClient;
        IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

        const int RTP_HEADER_LEN = 12;
        const UInt16 TOP_BIT_16 = 1 << 15;   // we check the top bit to determine when the sequence number has wrapped around

        UInt16 lastSequenceNumber = 0;       // we use this for detecting dropped packets
        int outOfSequenceCount = 0;

        AudioClip audioClip;

        long absoluteWritePosition = 0;      // absolute offset into the incoming audio stream

        int previousTimeSamples = 0;         // we use this to determine when we've looped
        int playbackLoops = 0;               // number of times we've looped (we use this to compute absolute read position)

        private void Awake()
        {
            GetComponent<PlayerAvatar>().playerAvatarChangedEvent.AddListener(PlayerAvatarChanged);
        }
   
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            int myReceivePort = basePort + (ushort)OwnerClientId;
            udpClient = new UdpClient(myReceivePort);
            print($"Receiving audio on port {myReceivePort}");
            int clipSize = 44100 * 3;  // 44.1 khz times number of seconds to buffer
            audioClip = AudioClip.Create("Received", clipSize, 1, 44100, false);
            if (audioSource == null) audioSource = GetComponentInChildren<AudioSource>();
            if (audioSource) SetupAudioSource(audioSource);
            else Debug.LogError("RTP Receiver couldn't find an AudioSource");
        }
        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            udpClient.Close();
        }
   
        public void PlayerAvatarChanged(GameObject avatar)
        {
            audioSource = avatar.GetComponent<AvatarReferences>()?.mouthAudio;
            if (audioSource == null) audioSource = avatar.GetComponentInChildren<AudioSource>();
            if (audioSource) SetupAudioSource(audioSource);
            else Debug.Log("No audio source");
        }

        void SetupAudioSource(AudioSource audioSource)
        {
            audioSource.clip = audioClip;
            audioSource.loop = true;
            audioSource.spatialize = true;
            audioSource.spatialBlend = 1;
            previousTimeSamples = playbackLoops = 0;
            audioSource.Play();
        }

        UInt16 Read16(byte[] buffer, int offset) => (UInt16)((buffer[offset] << 8) | buffer[offset + 1]);

        void FixedUpdate()
        {
            if (udpClient == null) return;
            while (udpClient.Available > 0)
            {
                byte[] packet = udpClient.Receive(ref RemoteIpEndPoint);
                if ((packet[1] & 0x7F) != 11) return; // bad payload type
                UInt16 seq = Read16(packet, 2);
                if ((seq & TOP_BIT_16) == 0 && (lastSequenceNumber & TOP_BIT_16) != 0) lastSequenceNumber = seq; // wrap-around
                if (seq < lastSequenceNumber)
                {
                    if (++outOfSequenceCount > 5)
                    {
                        lastSequenceNumber = seq;
                        outOfSequenceCount = 0;
                    }
                    return;
                }
                else outOfSequenceCount = 0;
                lastSequenceNumber = seq;
                ProcessAudio(packet);
            }
            CheckIfOutOfData();
        }

        void ProcessAudio(byte[] packet)
        {
            // convert the incoming audio into an array of floating point values (range -1 to +1)
            float[] audioBuffer = new float[(packet.Length - RTP_HEADER_LEN) / sizeof(Int16)];
            int audioBufferIndex = 0;
            for (int packetIndex = RTP_HEADER_LEN; packetIndex < packet.Length; packetIndex += sizeof(Int16))
                audioBuffer[audioBufferIndex++] = unchecked((Int16)Read16(packet, packetIndex)) / (float)Int16.MaxValue;
 
            // write the data into the audio clip
            audioClip.SetData(audioBuffer, (int)(absoluteWritePosition % audioClip.samples));
            absoluteWritePosition += audioBuffer.Length;
        }

        void CheckIfOutOfData()
        {
            if (audioSource == null) return;
            if (audioSource.timeSamples < previousTimeSamples) ++playbackLoops;  // wrapped around the clip's internal buffer
            previousTimeSamples = audioSource.timeSamples;

            long absoluteReadPosition = playbackLoops * audioClip.samples + audioSource.timeSamples;
            if (audioSource.isPlaying && absoluteReadPosition >= absoluteWritePosition) audioSource.Stop();
            else if (!audioSource.isPlaying && (absoluteWritePosition - absoluteReadPosition) > startThreshold) audioSource.Play();
        }
    }
}
