using UnityEngine;
using UnityEngine.InputSystem;

namespace HollerHorror.Clues
{
    /// <summary>
    /// The Journal (GDD §9): the persistent codex of entity knowledge, toggled
    /// with B anywhere. Read it against the clue board / field notes to work out
    /// which entity the evidence points to. It lists what's known; it never marks
    /// the answer — deduction stays with the player.
    /// </summary>
    public sealed class EntityJournal : MonoBehaviour
    {
        private bool open;
        private Vector2 scroll;

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.bKey.wasPressedThisFrame)
                open = !open;
        }

        private void OnGUI()
        {
            if (!open)
                return;

            float width = 460f;
            GUILayout.BeginArea(new Rect(20, 20, width, Screen.height * 0.82f), GUI.skin.box);
            GUILayout.Label("THE JOURNAL — known entities   [B] close");
            GUILayout.Label("Match the signs below against what you've pinned. Combinations confirm; single signs only suggest.");
            GUILayout.Space(6);

            scroll = GUILayout.BeginScrollView(scroll);
            foreach (var profile in EntityCodex.All)
            {
                GUILayout.BeginVertical(GUI.skin.box);
                GUILayout.Label($"{profile.Name} — {profile.Fantasy}");
                foreach (var sign in profile.Signs)
                    GUILayout.Label($"  • {sign}");
                GUILayout.Label($"  Ritual: {profile.Ritual}");
                GUILayout.EndVertical();
                GUILayout.Space(4);
            }
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }
    }
}
