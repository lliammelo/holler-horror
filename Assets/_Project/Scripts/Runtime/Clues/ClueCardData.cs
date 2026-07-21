using UnityEngine;

namespace HollerHorror.Clues
{
    /// <summary>
    /// One card on the board. Pure data — no scene references, so it serializes
    /// trivially over the network and into saves later.
    /// </summary>
    public sealed class ClueCardData
    {
        public int Id;
        public string Title;
        public string Body;
        public ClueKind Kind;
        /// <summary>Testimony: who said it. Evidence: where it was found.</summary>
        public string Source;
        /// <summary>Normalized board position, (0,0) bottom-left .. (1,1) top-right.</summary>
        public Vector2 BoardPosition;
        public EntityId Theory;
        public bool Contradiction;
    }
}
