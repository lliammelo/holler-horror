using Steamworks;
using UnityEngine;

namespace HollerHorror.Voice
{
    /// <summary>
    /// Streaming spatialized audio sink for decompressed Steam Voice PCM.
    /// Uses a looping streamed AudioClip fed from a float ring buffer;
    /// zero-fills on underrun so silence, not garbage, plays between packets.
    /// </summary>
    public sealed class VoiceOutput : MonoBehaviour
    {
        private const int RingSeconds = 4;

        private float[] ring;
        private int writePos;
        private int readPos;
        private int buffered;
        private readonly object gate = new();

        private AudioSource source;

        public static VoiceOutput CreateOn(Transform anchor, float maxDistance)
        {
            var go = new GameObject("VoiceOutput");
            go.transform.SetParent(anchor, false);
            var output = go.AddComponent<VoiceOutput>();
            output.Configure(maxDistance);
            return output;
        }

        private void Configure(float maxDistance)
        {
            int sampleRate = (int)SteamUser.OptimalSampleRate;
            ring = new float[sampleRate * RingSeconds];

            source = gameObject.AddComponent<AudioSource>();
            source.clip = AudioClip.Create("SteamVoice", sampleRate, 1, sampleRate, stream: true, OnAudioRead);
            source.loop = true;
            source.spatialBlend = 1f;
            source.rolloffMode = AudioRolloffMode.Linear;
            source.minDistance = 1.5f;
            source.maxDistance = maxDistance;
            source.Play();
        }

        /// <summary>Push little-endian 16-bit mono PCM into the ring.</summary>
        public void EnqueuePcm16(byte[] pcm, int byteCount)
        {
            lock (gate)
            {
                int samples = byteCount / 2;
                for (int i = 0; i < samples; i++)
                {
                    short s = (short)(pcm[i * 2] | (pcm[i * 2 + 1] << 8));
                    ring[writePos] = s / 32768f;
                    writePos = (writePos + 1) % ring.Length;
                    if (buffered < ring.Length)
                        buffered++;
                    else
                        readPos = (readPos + 1) % ring.Length; // overwrite oldest
                }
            }
        }

        private void OnAudioRead(float[] data)
        {
            lock (gate)
            {
                for (int i = 0; i < data.Length; i++)
                {
                    if (buffered > 0)
                    {
                        data[i] = ring[readPos];
                        readPos = (readPos + 1) % ring.Length;
                        buffered--;
                    }
                    else
                    {
                        data[i] = 0f;
                    }
                }
            }
        }
    }
}
