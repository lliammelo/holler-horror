using UnityEngine;

namespace HollerHorror.Entities
{
    /// <summary>
    /// Placeholder Wendigo vocalizations, synthesized at runtime (real audio: M7).
    /// The scream doubles as game information: a loud close scream = it locked onto
    /// you; faint distant calls while it patrols are the GDD §4.1 "sign" that a
    /// Wendigo is the active entity.
    /// </summary>
    public static class WendigoAudio
    {
        private static AudioClip scream;

        public static void PlayScreamAt(Vector3 position, bool loud)
        {
            // The scream silences the night — insects stop (GDD §9).
            Presentation.Ambience.Report(loud ? 1f : 0.5f);

            if (scream == null)
                scream = SynthesizeScream();

            var go = new GameObject("WendigoScream");
            go.transform.position = position;
            var source = go.AddComponent<AudioSource>();
            source.clip = scream;
            source.spatialBlend = 1f;
            source.rolloffMode = AudioRolloffMode.Linear;
            source.minDistance = loud ? 6f : 3f;
            source.maxDistance = loud ? 120f : 70f;
            source.volume = loud ? 1f : 0.45f;
            source.pitch = Random.Range(0.9f, 1.05f);
            source.Play();
            Object.Destroy(go, scream.length + 0.3f);
        }

        private static AudioClip SynthesizeScream()
        {
            const int sampleRate = 44100;
            const float duration = 1.5f;
            int samples = (int)(sampleRate * duration);
            var data = new float[samples];

            var rng = new System.Random(667);
            float noiseLp = 0f;
            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)samples;
                // Descending shriek: 820Hz falling to 240Hz with rising vibrato.
                float freq = Mathf.Lerp(820f, 240f, Mathf.Pow(t, 0.7f));
                float vibrato = Mathf.Sin(t * 34f * Mathf.PI) * Mathf.Lerp(4f, 26f, t);
                float phaseFreq = freq + vibrato;
                float tone = Mathf.Sin(2f * Mathf.PI * phaseFreq * i / sampleRate);
                // Add rasp: low-passed noise amplitude-modulated by the tone.
                float white = (float)(rng.NextDouble() * 2.0 - 1.0);
                noiseLp = Mathf.Lerp(noiseLp, white, 0.35f);
                float rasp = noiseLp * 0.45f * Mathf.Abs(tone);

                // Envelope: fast attack, long throaty release.
                float envelope = Mathf.Clamp01(t * 12f) * Mathf.Pow(1f - t, 0.6f);
                data[i] = (tone * 0.6f + rasp) * envelope;
            }

            var clip = AudioClip.Create("WendigoScream", samples, 1, sampleRate, stream: false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
