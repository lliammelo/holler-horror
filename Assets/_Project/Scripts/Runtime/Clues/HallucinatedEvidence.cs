using HollerHorror.Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HollerHorror.Clues
{
    /// <summary>
    /// A false clue conjured by a fraying mind (GDD §7). It looks like real
    /// evidence and offers an examine prompt — but examining it pins NOTHING; it
    /// dissolves. The gap between "what I saw" and "what actually pinned to the
    /// board" is itself information: the board only accepts what's real.
    /// </summary>
    public sealed class HallucinatedEvidence : MonoBehaviour
    {
        [SerializeField] private string fakeTitle = "Something in the dark";
        [SerializeField] private float examineDistance = 2.5f;
        [SerializeField, Tooltip("Vanishes on its own after this long even if never examined.")]
        private float lifetime = 22f;

        public void Configure(string title) => fakeTitle = title;

        private bool LocalPlayerNear()
        {
            foreach (var player in PlayerRegistry.All)
                if (Vector3.Distance(player.transform.position, transform.position) <= examineDistance)
                    return true;
            return false;
        }

        private void Update()
        {
            lifetime -= Time.deltaTime;
            if (lifetime <= 0f)
            {
                Destroy(gameObject);
                return;
            }

            // If sanity recovers, hallucinations fade — the real world reasserts.
            if (PlayerSanity.Local != null && PlayerSanity.Local.Tier == SanityTier.Calm)
            {
                Destroy(gameObject);
                return;
            }

            if (LocalPlayerNear() && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
                Dispel();
        }

        private void Dispel()
        {
            Debug.Log($"[Hallucination] Reached for '{fakeTitle}' — nothing was there. Nothing pinned.");
            Destroy(gameObject);
        }

        private void OnGUI()
        {
            if (!LocalPlayerNear())
                return;

            GUILayout.BeginArea(new Rect(Screen.width / 2f - 110, Screen.height * 0.56f, 220, 30), GUI.skin.box);
            GUILayout.Label($"[E] Examine: {fakeTitle}");
            GUILayout.EndArea();
        }
    }
}
