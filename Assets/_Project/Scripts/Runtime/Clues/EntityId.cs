namespace HollerHorror.Clues
{
    /// <summary>The entity theories a clue can support (GDD §4). Unknown = untagged.</summary>
    public enum EntityId : byte
    {
        Unknown = 0,
        Wendigo = 1,
        Fetch = 2,
        Hollow = 3,
    }
}
