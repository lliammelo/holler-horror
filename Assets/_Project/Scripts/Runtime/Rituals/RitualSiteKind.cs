namespace HollerHorror.Rituals
{
    /// <summary>Where each entity's banishing ritual must be performed (GDD §4).</summary>
    public enum RitualSiteKind : byte
    {
        FirstKillSite = 0, // Wendigo: where it first killed — varies with the carcass
        Crossroads = 1,    // Fetch: mirror and salt at the crossroads
        ChapelStone = 2,   // Hollow: the consecration
    }
}
