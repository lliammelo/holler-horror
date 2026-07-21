using UnityEngine;
using UnityEngine.InputSystem;

namespace HollerHorror.Clues
{
    /// <summary>
    /// The portable field journal (GDD §5): a read-only mirror of the board,
    /// available anywhere via J. Read and review — no rearranging; the physical
    /// board at base camp is where deduction work happens.
    /// </summary>
    public sealed class FieldJournal : MonoBehaviour
    {
        private bool open;
        private Vector2 scroll;

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.jKey.wasPressedThisFrame)
                open = !open;
        }

        private void OnGUI()
        {
            if (!open || ClueBoard.Instance == null)
                return;

            var state = ClueBoard.Instance.State;
            float width = 420f;
            GUILayout.BeginArea(new Rect(Screen.width - width - 10, 10, width, Screen.height * 0.75f), GUI.skin.box);
            GUILayout.Label($"FIELD JOURNAL — {state.Cards.Count} clue(s)   [J] close");
            scroll = GUILayout.BeginScrollView(scroll);

            foreach (var card in state.Cards.Values)
            {
                GUILayout.BeginVertical(GUI.skin.box);
                string flags = (card.Contradiction ? "  [!]" : "") +
                               (card.Theory != EntityId.Unknown ? $"  <{ClueTheme.TheoryLabel(card.Theory)}>" : "");
                GUILayout.Label($"{card.Title}{flags}");
                GUILayout.Label(card.Body);
                GUILayout.Label(card.Kind == ClueKind.Testimony ? $"— {card.Source}" : $"found: {card.Source}");
                GUILayout.EndVertical();
            }

            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }
    }
}
