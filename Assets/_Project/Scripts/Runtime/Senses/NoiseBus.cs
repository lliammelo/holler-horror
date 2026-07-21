using System;
using UnityEngine;

namespace HollerHorror.Senses
{
    public enum NoiseKind { Footstep, Voice, Interaction }

    /// <summary>
    /// A single audible event. Radius is the maximum distance (metres) at which an
    /// entity with default hearing can perceive it; Loudness (0..1) weights how much
    /// suspicion it generates for listeners well inside that radius.
    /// </summary>
    public readonly struct NoiseEvent
    {
        public readonly Vector3 Position;
        public readonly float Radius;
        public readonly float Loudness;
        public readonly NoiseKind Kind;
        public readonly Transform Source;

        public NoiseEvent(Vector3 position, float radius, float loudness, NoiseKind kind, Transform source)
        {
            Position = position;
            Radius = radius;
            Loudness = loudness;
            Kind = kind;
            Source = source;
        }
    }

    /// <summary>
    /// Local event bus between noise producers (players, doors, items) and consumers
    /// (entity perception, debug view). Multiplayer note: this bus is per-client;
    /// forwarding remote players' noise to the host's entities is M3 wiring.
    /// </summary>
    public static class NoiseBus
    {
        public static event Action<NoiseEvent> OnNoise;

        public static void Emit(in NoiseEvent noiseEvent) => OnNoise?.Invoke(noiseEvent);
    }
}
