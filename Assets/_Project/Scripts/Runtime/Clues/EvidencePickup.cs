using HollerHorror.Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HollerHorror.Clues
{
    /// <summary>
    /// A piece of physical evidence in the world. Examine (E) to pin it to the
    /// board as an Evidence card; the prop stays but can't be collected twice.
    /// The full evidence system arrives in M4 — this closes the M3 loop:
    /// find evidence → clue card → new dialogue branches open.
    /// </summary>
    public sealed class EvidencePickup : MonoBehaviour
    {
        [SerializeField] private string title = "Evidence";
        [SerializeField, TextArea] private string body = "";
        [SerializeField] private string foundAt = "the holler";
        [SerializeField] private float examineDistance = 2.5f;

        private bool collected;

        public void Configure(string cardTitle, string cardBody, string location)
        {
            title = cardTitle;
            body = cardBody;
            foundAt = location;
        }

        private bool LocalPlayerNear()
        {
            foreach (var player in PlayerRegistry.All)
                if (Vector3.Distance(player.transform.position, transform.position) <= examineDistance)
                    return true;
            return false;
        }

        private void Update()
        {
            if (collected || ClueBoard.Instance == null || !LocalPlayerNear())
                return;

            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                collected = true;
                int n = ClueBoard.Instance.State.Cards.Count;
                var position = new Vector2(0.12f + (n % 5) * 0.19f, 0.84f - (n / 5) * 0.30f);
                ClueBoard.Instance.AddCard(title, body, ClueKind.Evidence, foundAt, position);
                Debug.Log($"[Evidence] Pinned: {title}");
            }
        }

        private void OnGUI()
        {
            if (collected || !LocalPlayerNear())
                return;

            GUILayout.BeginArea(new Rect(Screen.width / 2f - 110, Screen.height * 0.56f, 220, 30), GUI.skin.box);
            GUILayout.Label($"[E] Examine: {title}");
            GUILayout.EndArea();
        }
    }
}
