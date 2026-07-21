using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HollerHorror.Player
{
    /// <summary>
    /// M3 down state: entities down players rather than instakill (GDD §5).
    /// Downing disables the controller (which also unregisters it from
    /// PlayerRegistry, so entities lose interest). Proper carry/revive by
    /// teammates comes with multiplayer wiring; for testing, hold R to get up.
    /// </summary>
    [RequireComponent(typeof(FirstPersonController))]
    public sealed class PlayerHealth : MonoBehaviour
    {
        [SerializeField, Tooltip("Seconds holding R to self-revive (playtest stand-in for teammate revive).")]
        private float selfReviveHoldSeconds = 2.5f;
        [SerializeField, Tooltip("Seconds after reviving during which entity vision ignores this player (hearing still works).")]
        private float reviveGraceSeconds = 6f;

        private FirstPersonController controller;
        private float reviveHeld;
        private float graceUntil = float.NegativeInfinity;

        public bool IsDowned { get; private set; }

        /// <summary>True briefly after standing up — entity vision skips us, hearing does not.</summary>
        public bool InReviveGrace => Time.time < graceUntil;

        public event Action<PlayerHealth> Downed;
        public event Action<PlayerHealth> Revived;

        private void Awake() => controller = GetComponent<FirstPersonController>();

        public void Down(Transform by)
        {
            if (IsDowned)
                return;

            IsDowned = true;
            reviveHeld = 0f;
            controller.enabled = false; // also unregisters from PlayerRegistry
            Downed?.Invoke(this);
            Debug.Log($"[PlayerHealth] {name} downed by {(by != null ? by.name : "unknown")}");
        }

        public void Revive()
        {
            if (!IsDowned)
                return;

            IsDowned = false;
            graceUntil = Time.time + reviveGraceSeconds;
            controller.enabled = true;
            Revived?.Invoke(this);
        }

        private void Update()
        {
            if (!IsDowned)
                return;

            if (Keyboard.current != null && Keyboard.current.rKey.isPressed)
            {
                reviveHeld += Time.deltaTime;
                if (reviveHeld >= selfReviveHoldSeconds)
                    Revive();
            }
            else
            {
                reviveHeld = 0f;
            }
        }

        private void OnGUI()
        {
            if (!IsDowned)
                return;

            var overlay = new Rect(0, 0, Screen.width, Screen.height);
            GUI.color = new Color(0.5f, 0f, 0f, 0.35f);
            GUI.DrawTexture(overlay, Texture2D.whiteTexture);
            GUI.color = Color.white;

            var style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 28,
                alignment = TextAnchor.MiddleCenter,
            };
            float progress = Mathf.Clamp01(reviveHeld / selfReviveHoldSeconds);
            GUI.Label(new Rect(0, Screen.height * 0.4f, Screen.width, 80),
                $"DOWNED\nhold [R] to get up  {(progress > 0f ? $"{progress:P0}" : "")}", style);
        }
    }
}
