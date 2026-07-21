using System.Collections.Generic;
using HollerHorror.Senses;
using UnityEngine;

namespace HollerHorror.Debugging
{
    /// <summary>
    /// Placeholder audible footsteps until the real audio pass (M7): plays a short
    /// synthesized thump for every footstep NoiseEvent, spatialized at the event
    /// position, volume/pitch driven by loudness and surface. Because it's fed by
    /// the same events the entities hear, "how loud it sounds" always matches
    /// "how far the monster hears it" — useful while tuning.
    /// </summary>
    public sealed class PlaceholderFootstepAudio : MonoBehaviour
    {
        [SerializeField, Range(0f, 1f)] private float volume = 0.5f;

        private AudioClip thump;
        private readonly Queue<AudioSource> pool = new();

        private void Awake()
        {
            thump = SynthesizeThump();
        }

        private void OnEnable() => NoiseBus.OnNoise += HandleNoise;
        private void OnDisable() => NoiseBus.OnNoise -= HandleNoise;

        private void HandleNoise(NoiseEvent noiseEvent)
        {
            if (noiseEvent.Kind != NoiseKind.Footstep)
                return;

            var source = GetSource();
            source.transform.position = noiseEvent.Position;
            source.volume = volume * Mathf.Lerp(0.25f, 1f, noiseEvent.Loudness);
            // Softer surfaces read duller/lower; loud crunchy ones brighter.
            source.pitch = Random.Range(0.92f, 1.08f) * Mathf.Lerp(0.75f, 1.15f, noiseEvent.Loudness);
            source.PlayOneShot(thump);
        }

        private AudioSource GetSource()
        {
            if (pool.Count > 0 && !pool.Peek().isPlaying)
                return Recycle(pool.Dequeue());

            var go = new GameObject("FootstepAudio");
            go.transform.SetParent(transform);
            var source = go.AddComponent<AudioSource>();
            source.spatialBlend = 1f;
            source.rolloffMode = AudioRolloffMode.Linear;
            source.minDistance = 1f;
            source.maxDistance = 25f;
            return Recycle(source);
        }

        private AudioSource Recycle(AudioSource source)
        {
            pool.Enqueue(source);
            return source;
        }

        private static AudioClip SynthesizeThump()
        {
            const int sampleRate = 44100;
            const float duration = 0.09f;
            int samples = (int)(sampleRate * duration);
            var data = new float[samples];

            var rng = new System.Random(12345);
            float previous = 0f;
            for (int i = 0; i < samples; i++)
            {
                // Low-passed noise burst with a sharp exponential decay ≈ a soft thud.
                float white = (float)(rng.NextDouble() * 2.0 - 1.0);
                previous = Mathf.Lerp(previous, white, 0.18f);
                float envelope = Mathf.Exp(-i / (samples * 0.22f));
                data[i] = previous * envelope;
            }

            var clip = AudioClip.Create("FootstepThump", samples, 1, sampleRate, stream: false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
