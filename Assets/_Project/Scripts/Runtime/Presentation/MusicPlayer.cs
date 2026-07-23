using UnityEngine;

namespace HollerHorror.Presentation
{
    /// <summary>
    /// Plays a looping music bed with a gentle fade-in. Used for the licensed
    /// menu track ("Shepherd" by Plainride — in-game use only; promo/trailer
    /// rights are separate, GDD §9). Menu-scoped: loading a case scene destroys
    /// it, so gameplay begins in ambient near-silence, which is where the dread
    /// lives.
    /// </summary>
    public sealed class MusicPlayer : MonoBehaviour
    {
        [SerializeField] private AudioClip track;
        [SerializeField, Range(0f, 1f)] private float volume = 0.55f;
        [SerializeField] private float fadeInSeconds = 2.5f;

        private AudioSource source;

        private void Start()
        {
            if (track == null)
                return;

            source = gameObject.AddComponent<AudioSource>();
            source.clip = track;
            source.loop = true;
            source.spatialBlend = 0f;
            source.volume = 0f;
            source.Play();
        }

        private void Update()
        {
            if (source == null || source.volume >= volume)
                return;
            float step = volume / Mathf.Max(0.01f, fadeInSeconds);
            source.volume = Mathf.MoveTowards(source.volume, volume, step * Time.unscaledDeltaTime);
        }
    }
}
