using UnityEngine;

namespace HollerHorror.Clues
{
    /// <summary>Shared colors for board visuals and journal text.</summary>
    public static class ClueTheme
    {
        public static Color PinColor(EntityId theory) => theory switch
        {
            EntityId.Wendigo => new Color(0.72f, 0.14f, 0.10f),
            EntityId.Fetch => new Color(0.12f, 0.55f, 0.52f),
            EntityId.Hollow => new Color(0.42f, 0.24f, 0.58f),
            _ => new Color(0.55f, 0.52f, 0.48f),
        };

        public static string TheoryLabel(EntityId theory) =>
            theory == EntityId.Unknown ? "untagged" : theory.ToString();

        public static readonly Color CardPaper = new(0.92f, 0.88f, 0.78f);
        public static readonly Color TestimonyPaper = new(0.88f, 0.90f, 0.82f);
        public static readonly Color Ink = new(0.14f, 0.12f, 0.10f);
        public static readonly Color Yarn = new(0.75f, 0.12f, 0.10f);
        public static readonly Color ContradictionRed = new(0.85f, 0.1f, 0.05f);
    }
}
