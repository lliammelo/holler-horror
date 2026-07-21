using System.Collections.Generic;

namespace HollerHorror.Player
{
    /// <summary>
    /// All locally-simulated player controllers in the scene, for entity vision
    /// queries. Controllers self-register while enabled (remote network copies
    /// have their controller disabled, so they never appear here).
    /// </summary>
    public static class PlayerRegistry
    {
        private static readonly List<FirstPersonController> players = new();

        public static IReadOnlyList<FirstPersonController> All => players;

        internal static void Register(FirstPersonController player)
        {
            if (!players.Contains(player))
                players.Add(player);
        }

        internal static void Unregister(FirstPersonController player) => players.Remove(player);
    }
}
