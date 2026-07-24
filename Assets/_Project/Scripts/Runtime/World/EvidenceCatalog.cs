using System.Collections.Generic;
using HollerHorror.Clues;
using UnityEngine;
using EntityId = HollerHorror.Clues.EntityId; // Unity 6 ships a UnityEngine.EntityId

namespace HollerHorror.World
{
    /// <summary>
    /// Every piece of physical evidence in the game as data, so the same map can
    /// host a different truth each run. Entries carry which entity they implicate
    /// (Unknown = ambiguous, readable as more than one thing — the GDD's
    /// "single clues suggest, combinations confirm") and where they can plausibly
    /// be found.
    /// </summary>
    public static class EvidenceCatalog
    {
        public sealed class Entry
        {
            public string Title;
            public string Body;
            public string FoundAt;
            /// <summary>Entity this supports. Unknown = ambiguous / red herring.</summary>
            public EntityId Implicates;
            public PoiKind[] Places;
            public Color Tint;
        }

        public static readonly Entry[] All =
        {
            // ---- Wendigo: meat first, walks upright, screams ----
            new() { Implicates = EntityId.Wendigo, Tint = new Color(0.50f, 0.25f, 0.20f),
                Title = "Stripped carcass", FoundAt = "the kill site",
                Body = "A hog taken apart to clean bone. Nothing wasted, nothing dragged off. No scavenger will touch what's left.",
                Places = new[] { PoiKind.Farmstead, PoiKind.RidgeCabins, PoiKind.Thicket } },
            new() { Implicates = EntityId.Wendigo, Tint = new Color(0.35f, 0.28f, 0.18f),
                Title = "Claw-scored bark", FoundAt = "the ash line",
                Body = "Ash trees scored to bare wood, head height and higher. Whatever reaches that high walks upright.",
                Places = new[] { PoiKind.Thicket, PoiKind.RidgeCabins, PoiKind.MineMouth } },
            new() { Implicates = EntityId.Wendigo, Tint = new Color(0.40f, 0.30f, 0.20f),
                Title = "Torn smokehouse door", FoundAt = "the smokehouse",
                Body = "The door torn off from the TOP hinge downward. Cured meat gone; the mules never touched.",
                Places = new[] { PoiKind.Farmstead, PoiKind.RidgeCabins } },

            // ---- Fetch: duplication, reflection, running water ----
            new() { Implicates = EntityId.Fetch, Tint = new Color(0.70f, 0.72f, 0.75f),
                Title = "Fogged mirror", FoundAt = "the house",
                Body = "The hall mirror will not wipe clear. The glass holds a grey breath that isn't yours.",
                Places = new[] { PoiKind.Farmstead, PoiKind.RidgeCabins, PoiKind.Chapel } },
            new() { Implicates = EntityId.Fetch, Tint = new Color(0.40f, 0.35f, 0.30f),
                Title = "Duplicate footprints", FoundAt = "the creek bank",
                Body = "Two identical sets of prints leave the mud - same boot, same stride. One set walks INTO the water and stops.",
                Places = new[] { PoiKind.Creek, PoiKind.Crossroads } },
            new() { Implicates = EntityId.Fetch, Tint = new Color(0.30f, 0.40f, 0.48f),
                Title = "Still water, no reflection", FoundAt = "the shallows",
                Body = "The shallows lie dead flat. You lean over and nothing leans back.",
                Places = new[] { PoiKind.Creek } },

            // ---- Hollow: absence, cold, wrong light ----
            new() { Implicates = EntityId.Hollow, Tint = new Color(0.45f, 0.45f, 0.50f),
                Title = "Candle flames bent to no wind", FoundAt = "the chapel",
                Body = "Every candle leans the same way, west, toward the tree line. There is no draft to blame.",
                Places = new[] { PoiKind.Chapel, PoiKind.Farmstead } },
            new() { Implicates = EntityId.Hollow, Tint = new Color(0.35f, 0.40f, 0.45f),
                Title = "A cold seam in the air", FoundAt = "the hollow ground",
                Body = "A band of air cold as a cellar, with no cellar under it. Your breath fogs crossing it and clears on the far side.",
                Places = new[] { PoiKind.MineMouth, PoiKind.Thicket, PoiKind.Crossroads } },
            new() { Implicates = EntityId.Hollow, Tint = new Color(0.28f, 0.32f, 0.36f),
                Title = "A path that no longer leads", FoundAt = "the old trail",
                Body = "The trail runs sixty years to the mine mouth. It doesn't anymore. It ends in laurel that was never there.",
                Places = new[] { PoiKind.MineMouth, PoiKind.RidgeCabins } },

            // ---- Ambiguous: reads as more than one thing. The heart of deduction. ----
            new() { Implicates = EntityId.Unknown, Tint = new Color(0.38f, 0.38f, 0.34f),
                Title = "Dead birdsong", FoundAt = "the tree line",
                Body = "The insects and birds stop dead at the tree line, all at once. Something passing would do it. So would something arriving.",
                Places = new[] { PoiKind.Thicket, PoiKind.Creek, PoiKind.RidgeCabins } },
            new() { Implicates = EntityId.Unknown, Tint = new Color(0.30f, 0.42f, 0.50f),
                Title = "The spring channel", FoundAt = "under the springhouse",
                Body = "The spring runs in a stone channel under the floor. Cold pools here every summer. Just water and stone - whatever anyone tells you.",
                Places = new[] { PoiKind.Farmstead } },
            new() { Implicates = EntityId.Unknown, Tint = new Color(0.42f, 0.36f, 0.28f),
                Title = "Livestock will not settle", FoundAt = "the barn",
                Body = "The mules won't leave the barn nor eat. Animals know something's wrong long before they know what.",
                Places = new[] { PoiKind.Farmstead, PoiKind.RidgeCabins } },
        };

        public static List<Entry> Supporting(EntityId entity)
        {
            var list = new List<Entry>();
            foreach (var e in All)
                if (e.Implicates == entity)
                    list.Add(e);
            return list;
        }

        public static List<Entry> Ambiguous()
        {
            var list = new List<Entry>();
            foreach (var e in All)
                if (e.Implicates == EntityId.Unknown)
                    list.Add(e);
            return list;
        }

        /// <summary>Evidence that points at an entity which is NOT active — a genuine false lead.</summary>
        public static List<Entry> Misleading(EntityId active)
        {
            var list = new List<Entry>();
            foreach (var e in All)
                if (e.Implicates != EntityId.Unknown && e.Implicates != active)
                    list.Add(e);
            return list;
        }
    }
}
