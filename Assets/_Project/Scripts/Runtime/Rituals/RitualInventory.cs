using System;
using System.Collections.Generic;

namespace HollerHorror.Rituals
{
    /// <summary>
    /// What the (local) team is carrying for ritual purposes. M5 slice: a flat
    /// shared set — the real slot-based per-player inventory (GDD §7, ritual
    /// logistics) comes with multiplayer wiring.
    /// </summary>
    public static class RitualInventory
    {
        private static readonly HashSet<string> items = new();

        public static IReadOnlyCollection<string> Items => items;
        public static event Action Changed;

        public static bool Has(string itemId) => items.Contains(itemId);

        public static void Grant(string itemId)
        {
            if (items.Add(itemId))
                Changed?.Invoke();
        }

        public static void Consume(IEnumerable<string> itemIds)
        {
            foreach (var id in itemIds)
                items.Remove(id);
            Changed?.Invoke();
        }

        /// <summary>Domain-reload safety for editor play sessions.</summary>
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset() => items.Clear();
    }
}
