using HollerHorror.Voice;
using UnityEngine;

namespace HollerHorror.Entities
{
    /// <summary>
    /// The Fetch's "voice from the treeline" (GDD §4.2). If real player voice was
    /// captured this run (netcode scenes, M1), it garbles and replays that — your
    /// own words coming back wrong. Otherwise it falls back to a synthesized
    /// whisper so the effect still lands in solo/greybox.
    /// </summary>
    public static class FetchAudio
    {
        private static AudioClip whisper;

        public static void PlayVoiceFromTheTreeline(Vector3 position)
        {
            var snippet = VoiceEchoLibrary.GetRandomSnippet(3.5f, 1f);
            if (snippet != null && TryPlayCapturedVoice(position, snippet.Value.packets))
                return;

            PlayWhisper(position);
        }

        private static bool TryPlayCapturedVoice(Vector3 position, System.Collections.Generic.List<VoiceEchoLibrary.Packet> packets)
        {
            var pcm = new System.IO.MemoryStream();
            var scratch = new System.IO.MemoryStream();
            foreach (var packet in packets)
            {
                scratch.SetLength(0);
                int bytes = Steamworks.SteamUser.DecompressVoice(packet.Compressed, scratch);
                if (bytes > 0)
                    pcm.Write(scratch.GetBuffer(), 0, bytes);
            }

            int sampleCount = (int)(pcm.Length / 2);
            if (sampleCount < 200)
                return false;

            var samples = new float[sampleCount];
            var buffer = pcm.GetBuffer();
            for (int i = 0; i < sampleCount; i++)
            {
                short s = (short)(buffer[i * 2] | (buffer[i * 2 + 1] << 8));
                samples[i] = s / 32768f;
                if (Random.value < 0.12f) samples[i] = 0f; // dropout garble
            }

            var clip = AudioClip.Create("FetchEcho", sampleCount, 1, (int)Steamworks.SteamUser.OptimalSampleRate, false);
            clip.SetData(samples, 0);
            SpawnSource(position, clip, 0.88f);
            return true;
        }

        private static void PlayWhisper(Vector3 position)
        {
            if (whisper == null)
                whisper = SynthesizeWhisper();
            SpawnSource(position, whisper, Random.Range(0.95f, 1.05f));
        }

        private static void SpawnSource(Vector3 position, AudioClip clip, float pitch)
        {
            var go = new GameObject("FetchVoice");
            go.transform.position = position;
            var source = go.AddComponent<AudioSource>();
            source.clip = clip;
            source.spatialBlend = 1f;
            source.rolloffMode = AudioRolloffMode.Linear;
            source.minDistance = 3f;
            source.maxDistance = 45f;
            source.pitch = pitch;
            source.Play();
            Object.Destroy(go, clip.length / pitch + 0.3f);
        }

        private static AudioClip SynthesizeWhisper()
        {
            const int sampleRate = 44100;
            const float duration = 1.8f;
            int samples = (int)(sampleRate * duration);
            var data = new float[samples];
            var rng = new System.Random(991);

            float bandpass = 0f, prev = 0f;
            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)samples;
                float white = (float)(rng.NextDouble() * 2.0 - 1.0);
                // Breathy band-passed noise, amplitude wavering like half-formed words.
                bandpass = Mathf.Lerp(bandpass, white, 0.4f);
                float voiced = bandpass - prev;
                prev = bandpass;
                float syllable = Mathf.Pow(Mathf.Abs(Mathf.Sin(t * 11f * Mathf.PI)), 3f);
                float envelope = Mathf.Sin(t * Mathf.PI); // fade in and out
                data[i] = voiced * syllable * envelope * 0.5f;
            }

            var clip = AudioClip.Create("FetchWhisper", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
