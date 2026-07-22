using System;
using UnityEngine;

namespace HollerHorror.Entities
{
    /// <summary>
    /// The escalation clock (GDD §3). The night advances through phases; the
    /// entity grows bolder each one. Waiting for certainty at the board has a
    /// cost — that cost is measured here. A wrong ritual jerks the clock to the
    /// last phase (ForcePhase); a banishment stops it at Dawn.
    /// </summary>
    public sealed class NightClock : MonoBehaviour
    {
        public enum Phase { Dusk, EarlyNight, MidNight, LateNight, Dawn }

        public static NightClock Instance { get; private set; }

        [SerializeField, Tooltip("Seconds spent in Dusk, Early, Mid before reaching Late. Late runs until dawn or resolution.")]
        private float[] phaseDurations = { 75f, 90f, 90f };

        [SerializeField] private Light sun;

        private float phaseElapsed;

        public Phase Current { get; private set; } = Phase.Dusk;
        public bool Stopped { get; private set; }

        /// <summary>Raised when the phase advances. Escalation logic listens here.</summary>
        public event Action<Phase> PhaseChanged;

        private void Awake() => Instance = this;

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        private void Start() => ApplyPhaseAtmosphere(Current);

        private void Update()
        {
            if (Stopped || Current >= Phase.LateNight)
                return;

            phaseElapsed += Time.deltaTime;
            int index = (int)Current;
            if (index < phaseDurations.Length && phaseElapsed >= phaseDurations[index])
                Advance((Phase)(index + 1));
        }

        private void Advance(Phase next)
        {
            Current = next;
            phaseElapsed = 0f;
            ApplyPhaseAtmosphere(next);
            Debug.Log($"[NightClock] The night deepens: {next}");
            PhaseChanged?.Invoke(next);
        }

        /// <summary>Backfire: skip straight to the worst of the night.</summary>
        public void ForcePhase(Phase phase)
        {
            if (phase <= Current && phase != Phase.Dawn)
                return;
            Advance(phase);
        }

        /// <summary>Banishment: stop the clock; hold at dawn.</summary>
        public void ResolveToDawn()
        {
            Stopped = true;
            Advance(Phase.Dawn);
        }

        public float PhaseProgress()
        {
            int index = (int)Current;
            if (index >= phaseDurations.Length)
                return 1f;
            return Mathf.Clamp01(phaseElapsed / phaseDurations[index]);
        }

        private void ApplyPhaseAtmosphere(Phase phase)
        {
            if (sun == null)
                return;

            // Warm dusk light collapses toward cold, thin moonlight as night deepens.
            (Color color, float intensity, float fog) = phase switch
            {
                Phase.Dusk => (new Color(1.0f, 0.72f, 0.50f), 0.9f, 0.015f),
                Phase.EarlyNight => (new Color(0.55f, 0.55f, 0.72f), 0.55f, 0.02f),
                Phase.MidNight => (new Color(0.40f, 0.42f, 0.62f), 0.35f, 0.028f),
                Phase.LateNight => (new Color(0.30f, 0.30f, 0.5f), 0.22f, 0.038f),
                Phase.Dawn => (new Color(1.0f, 0.85f, 0.65f), 1.5f, 0.004f),
                _ => (Color.white, 0.5f, 0.02f),
            };
            sun.color = color;
            sun.intensity = intensity;
            RenderSettings.fogColor = Color.Lerp(RenderSettings.fogColor, color * 0.4f, 0.6f);
            RenderSettings.fogDensity = fog;
        }
    }
}
