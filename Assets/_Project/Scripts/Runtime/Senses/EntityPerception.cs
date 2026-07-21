using System;
using HollerHorror.Player;
using UnityEngine;

namespace HollerHorror.Senses
{
    public enum AwarenessState { Unaware, Suspicious, Alert }

    /// <summary>
    /// Generic hearing + vision model for entities. Consumes NoiseBus events and
    /// performs cone/line-of-sight vision checks against registered players,
    /// accumulating a single suspicion value with decay:
    ///   Unaware --(suspicion &gt;= suspiciousThreshold)--&gt; Suspicious --(&gt;= alertThreshold)--&gt; Alert.
    /// Entity AI (M3+) reads State / LastHeardPosition / VisibleTarget and reacts;
    /// this component never moves the entity itself.
    /// </summary>
    public sealed class EntityPerception : MonoBehaviour
    {
        [Header("Hearing")]
        [SerializeField, Tooltip("Scales every noise event's audible radius for this entity.")]
        private float hearingSensitivity = 1f;
        [SerializeField, Tooltip("Audible radius multiplier when terrain blocks the direct path.")]
        private float occlusionPenalty = 0.55f;
        [SerializeField] private LayerMask occlusionMask = ~0;

        [Header("Vision")]
        [SerializeField] private float visionRange = 26f;
        [SerializeField, Range(10f, 360f)] private float visionFovDegrees = 120f;
        [SerializeField] private float eyeHeight = 1.7f;
        [SerializeField, Tooltip("Suspicion gained per second staring at a sprinting player at point-blank range.")]
        private float visionGainPerSecond = 1.6f;
        [SerializeField, Tooltip("How much harder a crouched player is to spot.")]
        private float crouchVisibilityMultiplier = 0.45f;

        [Header("Suspicion")]
        [SerializeField] private float suspiciousThreshold = 0.35f;
        [SerializeField] private float alertThreshold = 1f;
        [SerializeField] private float maxSuspicion = 1.5f;
        [SerializeField] private float decayPerSecond = 0.12f;
        [SerializeField, Tooltip("No decay for this long after the last stimulus.")]
        private float decayDelay = 2.5f;

        private float lastStimulusTime = float.NegativeInfinity;

        public AwarenessState State { get; private set; } = AwarenessState.Unaware;
        public float Suspicion { get; private set; }
        public Vector3? LastHeardPosition { get; private set; }
        public FirstPersonController VisibleTarget { get; private set; }
        public Vector3 EyePosition => transform.position + Vector3.up * eyeHeight;

        /// <summary>Raised when a noise was actually heard (post-occlusion) by this entity.</summary>
        public event Action<NoiseEvent> Heard;

        private void OnEnable() => NoiseBus.OnNoise += HandleNoise;
        private void OnDisable() => NoiseBus.OnNoise -= HandleNoise;

        private void HandleNoise(NoiseEvent noiseEvent)
        {
            float distance = Vector3.Distance(EyePosition, noiseEvent.Position);
            float effectiveRadius = noiseEvent.Radius * hearingSensitivity;

            if (IsOccluded(noiseEvent.Position, noiseEvent.Source))
                effectiveRadius *= occlusionPenalty;

            if (distance > effectiveRadius || effectiveRadius <= 0f)
                return;

            // Close, loud noises spike suspicion; edge-of-range noises barely register.
            float proximity = 1f - Mathf.Clamp01(distance / effectiveRadius);
            float gain = noiseEvent.Loudness * Mathf.Lerp(0.15f, 1f, proximity);

            AddSuspicion(gain);
            LastHeardPosition = noiseEvent.Position;
            Heard?.Invoke(noiseEvent);
        }

        private bool IsOccluded(Vector3 point, Transform source)
        {
            Vector3 target = point + Vector3.up * 0.5f;
            if (!Physics.Linecast(EyePosition, target, out RaycastHit hit, occlusionMask, QueryTriggerInteraction.Ignore))
                return false;

            // The emitter's own body doesn't muffle its own noise.
            return source == null || (hit.transform != source && !hit.transform.IsChildOf(source));
        }

        private void Update()
        {
            UpdateVision();
            UpdateDecayAndState();
        }

        private void UpdateVision()
        {
            VisibleTarget = null;
            float bestGain = 0f;

            foreach (var player in PlayerRegistry.All)
            {
                Vector3 head = player.Head != null ? player.Head.position : player.transform.position + Vector3.up * 1.6f;
                Vector3 toPlayer = head - EyePosition;
                float distance = toPlayer.magnitude;
                if (distance > visionRange)
                    continue;

                if (Vector3.Angle(transform.forward, toPlayer) > visionFovDegrees * 0.5f)
                    continue;

                if (Physics.Linecast(EyePosition, head, out RaycastHit hit, occlusionMask, QueryTriggerInteraction.Ignore)
                    && hit.transform != player.transform && !hit.transform.IsChildOf(player.transform))
                    continue;

                float visibility = 1f - Mathf.Clamp01(distance / visionRange);
                if (player.CurrentStance == Stance.Crouched)
                    visibility *= crouchVisibilityMultiplier;
                // Moving players catch the eye; a still, crouched player is nearly invisible at range.
                visibility *= Mathf.Lerp(0.35f, 1f, player.CurrentSpeedNormalized);

                float gain = visibility * visionGainPerSecond * Time.deltaTime;
                if (gain > bestGain)
                {
                    bestGain = gain;
                    VisibleTarget = player;
                }
            }

            if (VisibleTarget != null)
            {
                AddSuspicion(bestGain);
                LastHeardPosition = VisibleTarget.transform.position; // sight updates the investigate point too
            }
        }

        private void AddSuspicion(float amount)
        {
            Suspicion = Mathf.Min(maxSuspicion, Suspicion + amount);
            lastStimulusTime = Time.time;
        }

        private void UpdateDecayAndState()
        {
            if (Time.time - lastStimulusTime > decayDelay)
                Suspicion = Mathf.Max(0f, Suspicion - decayPerSecond * Time.deltaTime);

            State = Suspicion >= alertThreshold ? AwarenessState.Alert
                : Suspicion >= suspiciousThreshold ? AwarenessState.Suspicious
                : AwarenessState.Unaware;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.8f, 0.2f, 0.6f);
            Vector3 eye = Application.isPlaying ? EyePosition : transform.position + Vector3.up * eyeHeight;
            Vector3 left = Quaternion.Euler(0f, -visionFovDegrees * 0.5f, 0f) * transform.forward;
            Vector3 right = Quaternion.Euler(0f, visionFovDegrees * 0.5f, 0f) * transform.forward;
            Gizmos.DrawLine(eye, eye + left * visionRange);
            Gizmos.DrawLine(eye, eye + right * visionRange);
        }
    }
}
