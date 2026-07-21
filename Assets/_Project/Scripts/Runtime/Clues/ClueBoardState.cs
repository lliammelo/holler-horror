using System;
using System.Collections.Generic;
using UnityEngine;

namespace HollerHorror.Clues
{
    /// <summary>
    /// The board's whole truth: cards + string connections. Pure C# with events;
    /// the view renders it, the net-sync layer replicates it, players mutate it
    /// only through ClueBoard's API. The board never auto-solves (GDD §5).
    /// </summary>
    public sealed class ClueBoardState
    {
        private readonly Dictionary<int, ClueCardData> cards = new();
        private readonly HashSet<(int a, int b)> connections = new();
        private int nextId = 1;

        public IReadOnlyDictionary<int, ClueCardData> Cards => cards;
        public IEnumerable<(int a, int b)> Connections => connections;

        public event Action<ClueCardData> CardAdded;
        public event Action<ClueCardData> CardChanged; // moved, retagged, or flagged
        public event Action<int, int> ConnectionAdded;
        public event Action<int, int> ConnectionRemoved;

        public int ClaimNextId() => nextId++;

        public void AddCard(ClueCardData card)
        {
            if (card.Id >= nextId)
                nextId = card.Id + 1;
            cards[card.Id] = card;
            CardAdded?.Invoke(card);
        }

        public void MoveCard(int id, Vector2 boardPosition)
        {
            if (!cards.TryGetValue(id, out var card))
                return;
            card.BoardPosition = new Vector2(Mathf.Clamp01(boardPosition.x), Mathf.Clamp01(boardPosition.y));
            CardChanged?.Invoke(card);
        }

        public void SetTheory(int id, EntityId theory)
        {
            if (!cards.TryGetValue(id, out var card))
                return;
            card.Theory = theory;
            CardChanged?.Invoke(card);
        }

        public void SetContradiction(int id, bool flagged)
        {
            if (!cards.TryGetValue(id, out var card))
                return;
            card.Contradiction = flagged;
            CardChanged?.Invoke(card);
        }

        /// <summary>Adds the connection if absent, removes it if present. Returns true if it now exists.</summary>
        public bool ToggleConnection(int a, int b)
        {
            if (a == b || !cards.ContainsKey(a) || !cards.ContainsKey(b))
                return false;

            var key = Normalize(a, b);
            if (connections.Remove(key))
            {
                ConnectionRemoved?.Invoke(key.a, key.b);
                return false;
            }

            connections.Add(key);
            ConnectionAdded?.Invoke(key.a, key.b);
            return true;
        }

        public bool TryGetCard(int id, out ClueCardData card) => cards.TryGetValue(id, out card);

        private static (int a, int b) Normalize(int a, int b) => a < b ? (a, b) : (b, a);
    }
}
