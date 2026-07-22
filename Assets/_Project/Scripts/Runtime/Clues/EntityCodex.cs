namespace HollerHorror.Clues
{
    /// <summary>
    /// The Journal's entity knowledge (GDD §9): each entity's evidence profile and
    /// banishing ritual. This is the deduction key — players match collected clues
    /// against these known signs. Profiles overlap by design (single clues suggest,
    /// combinations confirm); the codex never says which entity IS active.
    /// </summary>
    public static class EntityCodex
    {
        public readonly struct Profile
        {
            public readonly EntityId Id;
            public readonly string Name;
            public readonly string Fantasy;
            public readonly string[] Signs;
            public readonly string Ritual;

            public Profile(EntityId id, string name, string fantasy, string[] signs, string ritual)
            {
                Id = id; Name = name; Fantasy = fantasy; Signs = signs; Ritual = ritual;
            }
        }

        public static readonly Profile[] All =
        {
            new(EntityId.Wendigo, "The Wendigo", "The relentless hunter. Starvation given legs.",
                new[]
                {
                    "Carcasses stripped to clean bone — nothing wasted, nothing dragged off.",
                    "Claw marks scored at head height and above. It walks upright.",
                    "Scream-calls echoing down the valley after midnight.",
                    "Raids smokehouses and livestock before it ever touches people.",
                    "Bound to a person who vanished 'the winter the food ran out.'",
                },
                "The Burning of the Name — learn its human name from a resident, carve it in ash wood, and burn it at the site of its first kill."),

            new(EntityId.Fetch, "The Fetch", "The doppelganger. It wears the faces of the living.",
                new[]
                {
                    "Duplicate footprints — one set walks into the water and stops.",
                    "Mirrors fog and won't clear; still water gives no reflection.",
                    "Residents recall conversations the other party never had.",
                    "Your own voice from the treeline; a figure gone the instant you look straight at it.",
                    "It cannot cross running water.",
                },
                "The Mirror Binding — ring an unfogged mirror with salt at the crossroads and force it to hold one face."),

            new(EntityId.Hollow, "The Hollow", "The valley itself, awake. An absence that spreads.",
                new[]
                {
                    "Birdsong and insects stop dead, all at once.",
                    "Cold spots with no source.",
                    "Candle flames bending toward no wind.",
                    "Compasses spin, lights gutter, paths no longer lead where they did.",
                },
                "The Consecration — relight the chapel and walk a salt-and-fire boundary around the affected land."),
        };
    }
}
