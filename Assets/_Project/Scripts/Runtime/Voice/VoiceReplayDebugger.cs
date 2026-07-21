using System.IO;
using Steamworks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HollerHorror.Voice
{
    /// <summary>
    /// M1 Fetch feasibility proof: press F5 to replay a garbled snippet of
    /// something a player actually said, from a random position nearby —
    /// exactly what the Fetch will do from the treeline in M5.
    /// Garbling = pitch drop + random dropout chunks, tuned to sound wrong.
    /// </summary>
    public sealed class VoiceReplayDebugger : MonoBehaviour
    {
        [SerializeField] private float snippetSeconds = 4f;
        [SerializeField] private Vector2 replayDistanceRange = new(6f, 14f);
        [SerializeField, Range(0.5f, 1f)] private float replayPitch = 0.9f;
        [SerializeField, Range(0f, 0.5f)] private float dropoutChance = 0.15f;

        private readonly MemoryStream decompressStream = new();

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.f5Key.wasPressedThisFrame)
                TryReplay();
        }

        private void TryReplay()
        {
            var snippet = VoiceEchoLibrary.GetRandomSnippet(snippetSeconds);
            if (snippet == null)
            {
                Debug.Log("[VoiceReplay] Nothing captured yet — talk on voice first, then F5.");
                return;
            }

            // Decompress the whole snippet into one PCM buffer.
            decompressStream.SetLength(0);
            var pcm = new MemoryStream();
            foreach (var packet in snippet.Value.packets)
            {
                decompressStream.SetLength(0);
                int bytes = SteamUser.DecompressVoice(packet.Compressed, decompressStream);
                if (bytes > 0)
                    pcm.Write(decompressStream.GetBuffer(), 0, bytes);
            }

            int sampleCount = (int)(pcm.Length / 2);
            if (sampleCount < 100)
            {
                Debug.LogWarning($"[VoiceReplay] Snippet decompressed to almost nothing ({sampleCount} samples) — skipping.");
                return;
            }

            var samples = new float[sampleCount];
            var buffer = pcm.GetBuffer();
            for (int i = 0; i < sampleCount; i++)
            {
                short s = (short)(buffer[i * 2] | (buffer[i * 2 + 1] << 8));
                samples[i] = s / 32768f;
            }

            GarbleInPlace(samples);
            PlayAtRandomOffset(samples);
            Debug.Log($"[VoiceReplay] Replaying {sampleCount / SteamUser.OptimalSampleRate:F1}s of client {snippet.Value.clientId}'s voice.");
        }

        private void GarbleInPlace(float[] samples)
        {
            int chunk = 2048;
            for (int start = 0; start < samples.Length; start += chunk)
            {
                if (Random.value < dropoutChance)
                {
                    int end = Mathf.Min(start + chunk, samples.Length);
                    for (int i = start; i < end; i++)
                        samples[i] = 0f;
                }
            }
        }

        private void PlayAtRandomOffset(float[] samples)
        {
            var listener = Camera.main != null ? Camera.main.transform : transform;
            Vector2 dir = Random.insideUnitCircle.normalized;
            float dist = Random.Range(replayDistanceRange.x, replayDistanceRange.y);
            Vector3 pos = listener.position + new Vector3(dir.x, 0f, dir.y) * dist;

            var go = new GameObject("FetchEcho (debug)");
            go.transform.position = pos;
            var source = go.AddComponent<AudioSource>();
            var clip = AudioClip.Create("FetchEcho", samples.Length, 1, (int)SteamUser.OptimalSampleRate, stream: false);
            clip.SetData(samples, 0);
            source.clip = clip;
            source.spatialBlend = 1f;
            source.rolloffMode = AudioRolloffMode.Linear;
            source.minDistance = 2f;
            source.maxDistance = 30f;
            source.pitch = replayPitch;
            source.Play();
            Destroy(go, clip.length / replayPitch + 0.5f);
        }
    }
}
