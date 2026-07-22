using System.Collections.Generic;

namespace HollerHorror.Dialogue
{
    /// <summary>All residents in the run, for entity NPC-hunting and escalation queries.</summary>
    public static class NpcRegistry
    {
        private static readonly List<NpcController> npcs = new();

        public static IReadOnlyList<NpcController> All => npcs;

        internal static void Register(NpcController npc)
        {
            if (!npcs.Contains(npc))
                npcs.Add(npc);
        }

        internal static void Unregister(NpcController npc) => npcs.Remove(npc);
    }
}
