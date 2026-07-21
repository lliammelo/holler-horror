using UnityEngine;

namespace HollerHorror.Clues
{
    /// <summary>
    /// The run's shared clue board — public API every gameplay system talks to.
    /// If a ClueBoardNetSync is present and live, mutations route through the
    /// server so all clients stay identical; otherwise they apply locally
    /// (solo/test scenes). Reads always come straight from local State.
    /// </summary>
    public sealed class ClueBoard : MonoBehaviour
    {
        public static ClueBoard Instance { get; private set; }

        private ClueBoardNetSync netSync;

        public ClueBoardState State { get; } = new();

        private void Awake()
        {
            Instance = this;
            netSync = GetComponent<ClueBoardNetSync>();
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        private bool UseNet => netSync != null && netSync.IsSpawned;

        public void AddCard(string title, string body, ClueKind kind, string source, Vector2 boardPosition)
        {
            if (UseNet)
            {
                netSync.RequestAddCard(title, body, kind, source, boardPosition);
                return;
            }

            State.AddCard(new ClueCardData
            {
                Id = State.ClaimNextId(),
                Title = title,
                Body = body,
                Kind = kind,
                Source = source,
                BoardPosition = boardPosition,
            });
        }

        public void MoveCard(int id, Vector2 boardPosition)
        {
            if (UseNet) netSync.RequestMoveCard(id, boardPosition);
            else State.MoveCard(id, boardPosition);
        }

        public void CycleTheory(int id)
        {
            if (!State.TryGetCard(id, out var card))
                return;
            var next = (EntityId)(((byte)card.Theory + 1) % 4);
            if (UseNet) netSync.RequestSetTheory(id, next);
            else State.SetTheory(id, next);
        }

        public void ToggleContradiction(int id)
        {
            if (!State.TryGetCard(id, out var card))
                return;
            if (UseNet) netSync.RequestSetContradiction(id, !card.Contradiction);
            else State.SetContradiction(id, !card.Contradiction);
        }

        public void ToggleConnection(int a, int b)
        {
            if (UseNet) netSync.RequestToggleConnection(a, b);
            else State.ToggleConnection(a, b);
        }
    }
}
