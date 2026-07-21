namespace HollerHorror.Clues
{
    /// <summary>
    /// Where a clue came from. Testimony carries a "who said it" source tag —
    /// load-bearing for Fetch cases where a source may not be who they appear.
    /// </summary>
    public enum ClueKind : byte
    {
        Evidence = 0,
        Testimony = 1,
    }
}
