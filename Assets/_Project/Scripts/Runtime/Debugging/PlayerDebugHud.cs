using UnityEngine;

namespace HollerHorror.Debugging
{
    /// <summary>
    /// Throwaway M0 on-screen readout for stamina and movement state.
    /// Replaced by the real HUD later; kept out of Player namespace on purpose.
    /// </summary>
    public sealed class PlayerDebugHud : MonoBehaviour
    {
        [SerializeField] private Player.FirstPersonController controller;
        [SerializeField] private Player.PlayerStamina stamina;
        [SerializeField] private Senses.FootstepNoiseEmitter noiseEmitter;

        private void OnGUI()
        {
            if (controller == null || stamina == null)
                return;

            GUILayout.BeginArea(new Rect(10, 10, 320, 120), GUI.skin.box);
            GUILayout.Label($"State: {controller.Locomotion} / {controller.CurrentStance}");
            GUILayout.Label($"Speed: {controller.CurrentSpeed:F2} m/s (norm {controller.CurrentSpeedNormalized:F2})");
            GUILayout.Label($"Stamina: {stamina.Current:F0} / {stamina.Max:F0}" + (stamina.CanSprint ? "" : "  [EXHAUSTED]"));
            if (noiseEmitter != null)
            {
                var last = noiseEmitter.LastEmitted;
                GUILayout.Label($"Surface: {noiseEmitter.CurrentSurface}  on [{noiseEmitter.GroundHitName}]" +
                                (last.HasValue ? $"  r={last.Value.Radius:F1}m" : ""));
            }
            GUILayout.EndArea();
        }
    }
}
