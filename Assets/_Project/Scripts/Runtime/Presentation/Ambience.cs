using UnityEngine;

namespace HollerHorror.Presentation
{
    /// <summary>
    /// A single decaying "tension" value (0..1) any system can push. The ambient
    /// audio reads it to silence the insects as a threat nears — the GDD's
    /// insects-that-stop. Kept deliberately tiny and global so entities can raise
    /// it with one call and no wiring.
    /// </summary>
    public static class Ambience
    {
        private const float DecayTau = 3.5f; // seconds to fall to ~37%

        private static float peak;
        private static float peakTime = float.NegativeInfinity;

        /// <summary>Raise tension to at least <paramref name="level"/> (0..1).</summary>
        public static void Report(float level)
        {
            float current = Tension;
            peak = Mathf.Clamp01(Mathf.Max(current, level));
            peakTime = Time.time;
        }

        /// <summary>Current tension, decayed from the last report.</summary>
        public static float Tension
        {
            get
            {
                float dt = Time.time - peakTime;
                return dt < 0f ? peak : peak * Mathf.Exp(-dt / DecayTau);
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset()
        {
            peak = 0f;
            peakTime = float.NegativeInfinity;
        }
    }
}
