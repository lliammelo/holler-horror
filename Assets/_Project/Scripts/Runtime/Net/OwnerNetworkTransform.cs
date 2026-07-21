using Unity.Netcode.Components;

namespace HollerHorror.Net
{
    /// <summary>
    /// Owner-authoritative NetworkTransform for the M1 spike: each client simulates
    /// its own movement locally (zero input latency) and replicates outward.
    /// Fine for co-op vs AI; revisit if cheating ever matters (GDD: no PvP in EA).
    /// </summary>
    public sealed class OwnerNetworkTransform : NetworkTransform
    {
        protected override bool OnIsServerAuthoritative() => false;
    }
}
