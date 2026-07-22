using HollerHorror.Clues;
using UnityEngine;
using Yarn.Unity;

namespace HollerHorror.Dialogue
{
    /// <summary>
    /// Glue between Yarn Spinner and the rest of the game:
    /// - starts conversations for NPCs,
    /// - syncs board-derived facts into Yarn variables before each conversation
    ///   (scripts read plain $vars — no custom compiler functions needed),
    /// - handles the &lt;&lt;testimony&gt;&gt; command, pinning a source-tagged
    ///   clue card for the currently-speaking NPC (GDD §5/§6).
    /// </summary>
    [RequireComponent(typeof(DialogueRunner))]
    public sealed class DialogueService : MonoBehaviour
    {
        public static DialogueService Instance { get; private set; }

        [SerializeField, Tooltip("Fallback wiring: applied at runtime if the runner lost its project reference.")]
        private YarnProject yarnProject;

        private DialogueRunner runner;

        /// <summary>Who the player is talking to right now — testimony cards credit this name.</summary>
        public string CurrentNpcName { get; private set; } = "";

        private void Awake()
        {
            Instance = this;
            runner = GetComponent<DialogueRunner>();
            if (runner.YarnProject == null && yarnProject != null)
                runner.SetProject(yarnProject);
            runner.AddCommandHandler<string, string>("testimony", PinTestimony);
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public bool IsBusy => runner.IsDialogueRunning;

        public void StartConversation(string npcName, string startNode)
        {
            if (IsBusy)
                return;

            CurrentNpcName = npcName;
            SyncBoardFacts();
            runner.StartDialogue(startNode);
        }

        /// <summary>Project board contents into Yarn variables scripts can gate on.</summary>
        private void SyncBoardFacts()
        {
            runner.VariableStorage.SetValue("$has_claw_clue", BoardMentions("claw"));
            runner.VariableStorage.SetValue("$has_springhouse_clue", BoardMentions("spring"));
        }

        private static bool BoardMentions(string term)
        {
            if (ClueBoard.Instance == null)
                return false;

            foreach (var card in ClueBoard.Instance.State.Cards.Values)
            {
                if (card.Title.ToLowerInvariant().Contains(term) ||
                    card.Body.ToLowerInvariant().Contains(term))
                    return true;
            }
            return false;
        }

        private void PinTestimony(string title, string body)
        {
            if (ClueBoard.Instance == null)
            {
                Debug.LogWarning("[Dialogue] Testimony given but no clue board in scene.");
                return;
            }

            int n = ClueBoard.Instance.State.Cards.Count;
            var position = new Vector2(0.12f + (n % 5) * 0.19f, 0.84f - (n / 5) * 0.30f);
            ClueBoard.Instance.AddCard(title, body, ClueKind.Testimony, CurrentNpcName, position);
            Debug.Log($"[Dialogue] Testimony pinned from {CurrentNpcName}: {title}");
        }
    }
}
