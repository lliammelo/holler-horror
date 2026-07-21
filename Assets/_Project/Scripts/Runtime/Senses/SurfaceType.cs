namespace HollerHorror.Senses
{
    /// <summary>Ground surface categories that drive footstep noise (GDD §5 noise model).</summary>
    public enum SurfaceType
    {
        Grass,
        Gravel,
        LeafLitter,
        Floorboards,
        Creek,
        Stone,
    }

    /// <summary>How loud each surface is relative to baseline. Tuning table for the whole game.</summary>
    public static class SurfaceAcoustics
    {
        public static float NoiseMultiplier(SurfaceType type) => type switch
        {
            SurfaceType.Grass => 0.55f,
            SurfaceType.Gravel => 1.0f,
            SurfaceType.LeafLitter => 0.85f,
            SurfaceType.Floorboards => 0.9f,
            SurfaceType.Creek => 1.25f, // splashing carries down the holler
            SurfaceType.Stone => 0.7f,
            _ => 0.7f,
        };
    }

}
