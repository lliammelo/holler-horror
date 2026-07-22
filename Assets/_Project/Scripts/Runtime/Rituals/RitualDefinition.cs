using System.Collections.Generic;
using HollerHorror.Clues;

namespace HollerHorror.Rituals
{
    /// <summary>
    /// The three banishing rituals (GDD §4): what each needs and where it happens.
    /// Static data for the M5 slice; grows into ScriptableObjects when cases
    /// multiply.
    /// </summary>
    public static class RitualDefinition
    {
        public static string DisplayName(EntityId entity) => entity switch
        {
            EntityId.Wendigo => "The Burning of the Name",
            EntityId.Fetch => "The Mirror Binding",
            EntityId.Hollow => "The Consecration",
            _ => "?",
        };

        public static RitualSiteKind SiteOf(EntityId entity) => entity switch
        {
            EntityId.Wendigo => RitualSiteKind.FirstKillSite,
            EntityId.Fetch => RitualSiteKind.Crossroads,
            _ => RitualSiteKind.ChapelStone,
        };

        public static string SiteDescription(EntityId entity) => entity switch
        {
            EntityId.Wendigo => "the site of its first kill",
            EntityId.Fetch => "the crossroads",
            _ => "the chapel stone",
        };

        public static IReadOnlyList<string> ComponentsOf(EntityId entity) => entity switch
        {
            EntityId.Wendigo => new[] { "the_name", "ash_billet" },
            EntityId.Fetch => new[] { "mirror", "salt" },
            EntityId.Hollow => new[] { "salt", "lantern_oil" },
            _ => System.Array.Empty<string>(),
        };

        public static string ItemDisplayName(string itemId) => itemId switch
        {
            "the_name" => "the name, written down",
            "ash_billet" => "an ash-wood billet",
            "mirror" => "the hand mirror",
            "salt" => "a sack of salt",
            "lantern_oil" => "lantern oil",
            _ => itemId,
        };

        public static readonly string[] StepPrompts =
        {
            "Place the components",
            "Hold the circle",
            "Speak the words",
        };

        public static readonly float[] StepDurations = { 3f, 6f, 4f };
    }
}
