using System.Collections.Generic;
using HollerHorror.Clues;
using EntityId = HollerHorror.Clues.EntityId;

namespace HollerHorror.Dialogue
{
    /// <summary>
    /// Spoken clues residents can hold, tagged by the entity they implicate.
    /// Dealt per run by ResidentDirector: an honest resident speaks a true one
    /// about the active entity, a mistaken/lying one speaks a true-sounding line
    /// about an entity that isn't here — a genuine false lead you catch by
    /// cross-checking against physical evidence and other testimony.
    /// </summary>
    public static class TestimonyPool
    {
        public readonly struct Fact
        {
            public readonly string Title;
            public readonly string Body;
            public readonly EntityId Implicates;
            public Fact(string title, string body, EntityId implicates)
            {
                Title = title; Body = body; Implicates = implicates;
            }
        }

        private static readonly Fact[] Facts =
        {
            // Wendigo
            new("Screams down the valley", "\"Three nights running now. Comes down after midnight, wall to wall. Weren't no panther — panthers don't sound hungry.\"", EntityId.Wendigo),
            new("It took the meat first", "\"Boggs lost his whole smokehouse before it ever looked at a person. Went for the cured meat. The mules it never touched.\"", EntityId.Wendigo),
            new("A neighbour who came back changed", "\"Ask what happened the winter the food ran out. Ask who walked down off that ridge fat and smiling when the rest didn't walk down at all.\"", EntityId.Wendigo),

            // Fetch
            new("A conversation I never had", "\"Someone come to my door wearing your walk, talked me kind, and I told them things. Then you don't remember a word. One of you isn't you.\"", EntityId.Fetch),
            new("I saw myself by the creek", "\"Down in the shallows at dusk, looking back up at me. My granny always said running water's the one thing it won't cross.\"", EntityId.Fetch),
            new("The glass won't clear", "\"Every mirror in the house holds a grey breath that isn't mine. The still water in the trough shows nothing at all.\"", EntityId.Fetch),

            // Hollow
            new("The birds stopped all at once", "\"Three mornings back, like a hand closed over them. Then the cold come — not weather-cold, the kind that sits in a room and watches.\"", EntityId.Hollow),
            new("The paths moved", "\"The trail's run to the mine mouth sixty years. Last night it didn't. Ended in laurel that was never there.\"", EntityId.Hollow),
            new("Flames bend to no wind", "\"Every candle leans west toward the tree line, and there's not a breath of draft to blame. Toward wherever the quiet's thickest.\"", EntityId.Hollow),
        };

        public static List<Fact> For(EntityId entity)
        {
            var list = new List<Fact>();
            foreach (var f in Facts)
                if (f.Implicates == entity)
                    list.Add(f);
            return list;
        }

        /// <summary>A true-sounding fact about some entity that ISN'T active — the false lead.</summary>
        public static List<Fact> NotImplicating(EntityId active)
        {
            var list = new List<Fact>();
            foreach (var f in Facts)
                if (f.Implicates != active && f.Implicates != EntityId.Unknown)
                    list.Add(f);
            return list;
        }
    }
}
