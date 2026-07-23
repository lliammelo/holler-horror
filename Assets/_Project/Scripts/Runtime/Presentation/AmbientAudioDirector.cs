using UnityEngine;

namespace HollerHorror.Presentation
{
    /// <summary>
    /// Layered ambience (GDD §9): a constant low wind bed plus a night-insect
    /// layer that ducks toward silence as Ambience.Tension rises — the insects
    /// stopping is the tell that something is close. Fully synthesized so it
    /// needs no imported audio; the real audio pass replaces the clips.
    /// </summary>
    public sealed class AmbientAudioDirector : MonoBehaviour
    {
        [SerializeField, Range(0f, 1f)] private float windVolume = 0.35f;
        [SerializeField, Range(0f, 1f)] private float insectVolume = 0.5f;

        private AudioSource wind;
        private AudioSource insects;

        private void Start()
        {
            wind = MakeLoopSource("Wind", SynthesizeWind(), windVolume);
            insects = MakeLoopSource("Insects", SynthesizeInsects(), insectVolume);
        }

        private void Update()
        {
            // Insects fall silent as tension rises; wind swells slightly to fill.
            float tension = Ambience.Tension;
            insects.volume = Mathf.Lerp(insects.volume, insectVolume * (1f - tension), Time.deltaTime * 3f);
            wind.volume = Mathf.Lerp(wind.volume, windVolume * (1f + tension * 0.4f), Time.deltaTime * 2f);
        }

        private AudioSource MakeLoopSource(string name, AudioClip clip, float volume)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform);
            var source = go.AddComponent<AudioSource>();
            source.clip = clip;
            source.loop = true;
            source.spatialBlend = 0f; // 2D ambient bed
            source.volume = volume;
            source.Play();
            return source;
        }

        private static AudioClip SynthesizeWind()
        {
            const int rate = 44100;
            const int samples = rate * 4;
            var data = new float[samples];
            var rng = new System.Random(7);
            float lp = 0f, lp2 = 0f;
            for (int i = 0; i < samples; i++)
            {
                float white = (float)(rng.NextDouble() * 2.0 - 1.0);
                lp = Mathf.Lerp(lp, white, 0.02f);   // heavy low-pass → rumble
                lp2 = Mathf.Lerp(lp2, lp, 0.05f);
                float swell = 0.6f + 0.4f * Mathf.Sin(i / (float)samples * Mathf.PI * 2f); // gentle gusting
                data[i] = lp2 * swell;
            }
            LoopFade(data, rate / 4);
            return Clip("Wind", data, rate);
        }

        private static AudioClip SynthesizeInsects()
        {
            const int rate = 44100;
            const int samples = rate * 4;
            var data = new float[samples];
            var rng = new System.Random(31);

            // A field of chirring voices: band-passed noise (not pure tones, which
            // ring) gated into short pulses at staggered rates.
            for (int voice = 0; voice < 4; voice++)
            {
                float chirpHz = 9f + (float)rng.NextDouble() * 9f;
                float phase = (float)rng.NextDouble();
                float gain = 0.09f + (float)rng.NextDouble() * 0.05f;
                float lpCoeff = 0.35f + (float)rng.NextDouble() * 0.2f; // brightness of the chirr
                float lp = 0f, prev = 0f;
                for (int i = 0; i < samples; i++)
                {
                    float t = i / (float)rate;
                    float gate = Mathf.Pow(Mathf.Max(0f, Mathf.Sin((t + phase) * chirpHz * Mathf.PI * 2f)), 8f);
                    float white = (float)(rng.NextDouble() * 2.0 - 1.0);
                    lp = Mathf.Lerp(lp, white, lpCoeff); // low-pass the noise
                    float band = lp - prev;              // difference → rough band-pass, a dry rasp
                    prev = lp;
                    data[i] += band * gate * gain;
                }
            }

            // Soft-clip so overlapping voices never spike into a click/ring.
            for (int i = 0; i < samples; i++)
                data[i] = Mathf.Clamp(data[i], -0.9f, 0.9f);

            LoopFade(data, rate / 8);
            return Clip("Insects", data, rate);
        }

        /// <summary>Cross-fades the buffer ends so the loop has no click.</summary>
        private static void LoopFade(float[] data, int fade)
        {
            for (int i = 0; i < fade; i++)
            {
                float k = i / (float)fade;
                data[i] *= k;
                data[data.Length - 1 - i] *= k;
            }
        }

        private static AudioClip Clip(string name, float[] data, int rate)
        {
            var clip = AudioClip.Create(name, data.Length, 1, rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
