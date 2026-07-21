using UnityEngine;

namespace HollerHorror.Player
{
    /// <summary>
    /// Per-player stamina pool consumed by sprinting.
    /// Exhaustion locks sprint out until stamina recovers past a threshold,
    /// so players can't feather the sprint key while being chased.
    /// </summary>
    public sealed class PlayerStamina : MonoBehaviour
    {
        [SerializeField] private float maxStamina = 100f;
        [SerializeField] private float sprintDrainPerSecond = 18f;
        [SerializeField] private float regenPerSecond = 12f;
        [SerializeField, Tooltip("Seconds after draining stops before regen begins.")]
        private float regenDelay = 1.2f;
        [SerializeField, Tooltip("Once fully exhausted, sprint stays locked until stamina recovers to this value.")]
        private float exhaustionRecoveryThreshold = 25f;

        private float lastDrainTime = float.NegativeInfinity;
        private bool exhausted;

        public float Current { get; private set; }
        public float Max => maxStamina;
        public float Normalized => Current / maxStamina;

        /// <summary>True while the player is allowed to start or continue sprinting.</summary>
        public bool CanSprint => !exhausted && Current > 0f;

        private void Awake()
        {
            Current = maxStamina;
        }

        private void Update()
        {
            if (Current >= maxStamina || Time.time - lastDrainTime < regenDelay)
                return;

            Current = Mathf.Min(maxStamina, Current + regenPerSecond * Time.deltaTime);
            if (exhausted && Current >= exhaustionRecoveryThreshold)
                exhausted = false;
        }

        /// <summary>Called by the controller each frame it is sprinting.</summary>
        public void DrainForSprint()
        {
            Current = Mathf.Max(0f, Current - sprintDrainPerSecond * Time.deltaTime);
            lastDrainTime = Time.time;
            if (Current <= 0f)
                exhausted = true;
        }
    }
}
