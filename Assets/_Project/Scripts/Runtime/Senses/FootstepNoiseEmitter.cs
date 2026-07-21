using HollerHorror.Player;
using UnityEngine;

namespace HollerHorror.Senses
{
    /// <summary>
    /// Converts player movement into footstep NoiseEvents: one event per stride,
    /// loudness from locomotion state, radius scaled by the surface underfoot.
    /// This is the primary input the Wendigo hunts by (GDD §4.1).
    /// </summary>
    [RequireComponent(typeof(FirstPersonController))]
    public sealed class FootstepNoiseEmitter : MonoBehaviour
    {
        [Header("Stride lengths (metres between footstep events)")]
        [SerializeField] private float crouchStride = 1.2f;
        [SerializeField] private float walkStride = 1.8f;
        [SerializeField] private float sprintStride = 2.3f;

        [Header("Noise")]
        [SerializeField, Tooltip("Audible radius in metres of a sprint footstep on a baseline (multiplier 1.0) surface.")]
        private float baseSprintRadius = 24f;
        [SerializeField] private float crouchLoudness = 0.2f;
        [SerializeField] private float walkLoudness = 0.55f;
        [SerializeField] private float sprintLoudness = 1.0f;
        [SerializeField] private LayerMask groundMask = ~0;

        private FirstPersonController controller;
        private Vector3 lastPosition;
        private float strideAccumulator;

        public SurfaceType CurrentSurface { get; private set; } = SurfaceType.Grass;
        public NoiseEvent? LastEmitted { get; private set; }

        /// <summary>Name of the collider under our feet — debug aid for surface tagging issues.</summary>
        public string GroundHitName { get; private set; } = "(none)";

        private void Awake()
        {
            controller = GetComponent<FirstPersonController>();
            lastPosition = transform.position;
        }

        private void Update()
        {
            // controller.enabled is false on remote network copies — never emit for them.
            if (!controller.enabled || !controller.IsGrounded)
            {
                lastPosition = transform.position;
                return;
            }

            Vector3 delta = transform.position - lastPosition;
            delta.y = 0f;
            lastPosition = transform.position;

            SampleSurface(); // continuous, so the HUD always shows what's underfoot

            if (controller.Locomotion == LocomotionState.Idle)
            {
                strideAccumulator = 0f;
                return;
            }

            strideAccumulator += delta.magnitude;
            float stride = controller.CurrentStance == Stance.Crouched
                ? crouchStride
                : controller.Locomotion == LocomotionState.Sprinting ? sprintStride : walkStride;

            if (strideAccumulator < stride)
                return;

            strideAccumulator = 0f;
            EmitFootstep();
        }

        private void SampleSurface()
        {
            if (Physics.SphereCast(transform.position + Vector3.up * 0.3f, 0.2f, Vector3.down,
                    out RaycastHit hit, 0.6f, groundMask, QueryTriggerInteraction.Ignore))
            {
                GroundHitName = hit.collider.name;
                var tag = hit.collider.GetComponentInParent<SurfaceTag>();
                if (tag != null && tag.Type != CurrentSurface)
                {
                    Debug.Log($"[Footsteps] Surface {CurrentSurface} -> {tag.Type} (stepped onto '{hit.collider.name}')");
                    CurrentSurface = tag.Type;
                }
                else if (tag != null)
                {
                    CurrentSurface = tag.Type;
                }
            }
            else
            {
                GroundHitName = "(nothing under feet)";
            }
        }

        private void EmitFootstep()
        {
            float loudness = controller.CurrentStance == Stance.Crouched
                ? crouchLoudness
                : controller.Locomotion == LocomotionState.Sprinting ? sprintLoudness : walkLoudness;

            float radius = baseSprintRadius * loudness * SurfaceAcoustics.NoiseMultiplier(CurrentSurface);

            var noiseEvent = new NoiseEvent(transform.position, radius, loudness, NoiseKind.Footstep, transform);
            LastEmitted = noiseEvent;
            NoiseBus.Emit(noiseEvent);
        }
    }
}
