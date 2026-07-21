using UnityEngine;

namespace HollerHorror.Senses
{
    /// <summary>
    /// Solo-testing stand-in for voice noise: samples the real microphone locally
    /// (no Steam, no networking) and emits Voice NoiseEvents when you speak.
    /// Lets the senses scene answer "does talking give me away?" by itself.
    /// Netcode scenes use PlayerVoice instead — don't put both on one player.
    /// </summary>
    public sealed class MicrophoneNoiseProbe : MonoBehaviour
    {
        [SerializeField, Tooltip("RMS mic level above which you count as 'talking'.")]
        private float talkThreshold = 0.02f;
        [SerializeField] private float quietVoiceRadius = 8f;
        [SerializeField] private float loudVoiceRadius = 18f;

        private const int SampleRate = 16000;
        private const int WindowSamples = 512;

        private AudioClip micClip;
        private string device;
        private readonly float[] window = new float[WindowSamples];
        private float lastEmitTime;

        public float CurrentLevel { get; private set; }

        private void OnEnable()
        {
            if (Microphone.devices.Length == 0)
            {
                Debug.LogWarning("[MicNoiseProbe] No microphone devices found.");
                enabled = false;
                return;
            }

            device = Microphone.devices[0];
            micClip = Microphone.Start(device, loop: true, lengthSec: 1, frequency: SampleRate);
        }

        private void OnDisable()
        {
            if (device != null)
                Microphone.End(device);
        }

        private void Update()
        {
            if (micClip == null)
                return;

            int position = Microphone.GetPosition(device);
            int start = position - WindowSamples;
            if (start < 0)
                return; // not enough recorded yet this loop

            micClip.GetData(window, start);

            float sum = 0f;
            for (int i = 0; i < WindowSamples; i++)
                sum += window[i] * window[i];
            CurrentLevel = Mathf.Sqrt(sum / WindowSamples);

            if (CurrentLevel < talkThreshold || Time.time - lastEmitTime < 0.33f)
                return;

            lastEmitTime = Time.time;
            float intensity = Mathf.InverseLerp(talkThreshold, talkThreshold * 8f, CurrentLevel);
            float radius = Mathf.Lerp(quietVoiceRadius, loudVoiceRadius, intensity);
            NoiseBus.Emit(new NoiseEvent(transform.position, radius,
                Mathf.Lerp(0.4f, 0.9f, intensity), NoiseKind.Voice, transform));
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 340, 360, 44), GUI.skin.box);
            GUILayout.Label($"Mic level: {CurrentLevel:F3} {(CurrentLevel >= talkThreshold ? "— TALKING (audible!)" : "")}");
            GUILayout.EndArea();
        }
    }
}
