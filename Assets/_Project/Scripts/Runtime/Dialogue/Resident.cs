using HollerHorror.Clues;
using HollerHorror.Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HollerHorror.Dialogue
{
    public enum Reliability { Honest, Mistaken, Withholding, Lying }

    /// <summary>
    /// A systemic resident (GDD §6). Holds one dealt clue and a reliability that
    /// shapes how they give it: honest speak true, mistaken sincerely misattribute,
    /// withholding won't talk until you've done some legwork, lying point you
    /// wrong on purpose. Their statement pins to the board tagged with their name,
    /// so contradictions between residents — and against physical evidence — are
    /// how a case actually cracks. Reliability is never shown to the player; that's
    /// the puzzle.
    /// </summary>
    public sealed class Resident : MonoBehaviour
    {
        [SerializeField] private string residentName = "Resident";
        [SerializeField] private float talkDistance = 3f;
        [SerializeField, Tooltip("Board must hold at least this many clues before a withholding resident opens up.")]
        private int withholdingUnlockClues = 3;

        private Reliability reliability;
        private TestimonyPool.Fact fact;
        private bool hasFact;
        private bool given;
        private bool alive = true;

        // Transient conversation UI.
        private string[] lines;
        private int lineIndex = -1;

        public string ResidentName { get => residentName; set => residentName = value; }
        public Reliability Disposition => reliability;

        public void Assign(Reliability disposition, TestimonyPool.Fact dealtFact)
        {
            reliability = disposition;
            fact = dealtFact;
            hasFact = true;
        }

        public void Kill() => alive = false;

        private bool PlayerNear()
        {
            foreach (var p in PlayerRegistry.All)
                if (Vector3.Distance(p.transform.position, transform.position) <= talkDistance)
                    return true;
            return false;
        }

        private bool InConversation => lineIndex >= 0 && lines != null;

        private void Update()
        {
            if (!alive || Keyboard.current == null)
                return;

            if (InConversation)
            {
                if (Keyboard.current.eKey.wasPressedThisFrame)
                    Advance();
                return;
            }

            if (PlayerNear() && Keyboard.current.eKey.wasPressedThisFrame)
                BeginConversation();
        }

        private void BeginConversation()
        {
            if (!hasFact)
            {
                lines = new[] { $"{residentName}: \"I keep to myself, stranger. I've nothing for you.\"" };
            }
            else if (reliability == Reliability.Withholding && !Withheld())
            {
                lines = new[]
                {
                    $"{residentName}: \"I might know a thing. But I don't talk to folk who haven't looked for themselves yet.\"",
                    $"{residentName}: \"Go and find something first. Then we'll see.\"",
                };
            }
            else if (given)
            {
                lines = new[] { $"{residentName}: \"I've told you what I know. Go on now.\"" };
            }
            else
            {
                lines = new[] { Preamble(), fact.Body, PinAndClose() };
            }

            lineIndex = 0;
        }

        private bool Withheld() =>
            ClueBoard.Instance != null && ClueBoard.Instance.State.Cards.Count >= withholdingUnlockClues;

        private string Preamble() => reliability switch
        {
            Reliability.Mistaken => $"{residentName}: \"I'll tell you what it is, plain as I see it — and I've seen plenty.\"",
            Reliability.Lying => $"{residentName}: \"...Alright. I'll tell you. But you keep my name out of it.\"",
            _ => $"{residentName}: \"You want to know what I've seen? Sit close, then.\"",
        };

        private string PinAndClose()
        {
            if (ClueBoard.Instance != null)
            {
                int n = ClueBoard.Instance.State.Cards.Count;
                var pos = new Vector2(0.12f + (n % 5) * 0.19f, 0.84f - (n / 5) * 0.30f);
                ClueBoard.Instance.AddCard(fact.Title, fact.Body, ClueKind.Testimony, residentName, pos);
            }
            given = true;
            Debug.Log($"[Resident] {residentName} ({reliability}) gave testimony implicating {fact.Implicates}.");
            return "(pinned to your board)";
        }

        private void Advance()
        {
            lineIndex++;
            if (lineIndex >= lines.Length)
            {
                lineIndex = -1;
                lines = null;
            }
        }

        private void OnGUI()
        {
            if (!alive)
                return;

            if (InConversation)
            {
                float w = Mathf.Min(720f, Screen.width - 40f);
                GUILayout.BeginArea(new Rect((Screen.width - w) / 2f, Screen.height - 150, w, 130), GUI.skin.box);
                GUILayout.Label(lines[lineIndex]);
                GUILayout.FlexibleSpace();
                GUILayout.Label(lineIndex < lines.Length - 1 ? "[E] continue" : "[E] end");
                GUILayout.EndArea();
                return;
            }

            if (PlayerNear())
            {
                GUILayout.BeginArea(new Rect(Screen.width / 2f - 100, Screen.height * 0.6f, 200, 30), GUI.skin.box);
                GUILayout.Label($"[E] Talk to {residentName}");
                GUILayout.EndArea();
            }
        }
    }
}
