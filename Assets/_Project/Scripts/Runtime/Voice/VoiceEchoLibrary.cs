using System.Collections.Generic;
using UnityEngine;

namespace HollerHorror.Voice
{
    /// <summary>
    /// Rolling capture of every player's recent compressed voice packets.
    /// This is the raw material the Fetch will mimic from (GDD §4.2) — M1 only
    /// proves we can store and replay it; the entity logic comes in M5.
    /// </summary>
    public static class VoiceEchoLibrary
    {
        public readonly struct Packet
        {
            public readonly float Time;
            public readonly byte[] Compressed;
            public Packet(float time, byte[] compressed) { Time = time; Compressed = compressed; }
        }

        private const float WindowSeconds = 90f;
        private static readonly Dictionary<ulong, List<Packet>> store = new();

        public static void Record(ulong clientId, byte[] compressed)
        {
            if (!store.TryGetValue(clientId, out var list))
                store[clientId] = list = new List<Packet>();

            list.Add(new Packet(UnityEngine.Time.time, compressed));

            float cutoff = UnityEngine.Time.time - WindowSeconds;
            while (list.Count > 0 && list[0].Time < cutoff)
                list.RemoveAt(0);
        }

        /// <summary>
        /// Returns up to snippetSeconds of a random *speech burst* (contiguous packets)
        /// at least minSeconds long, from a random speaker — or null if nobody has
        /// said anything substantial yet. Burst-based so replays are coherent phrases,
        /// never sub-second scraps.
        /// </summary>
        public static (ulong clientId, List<Packet> packets)? GetRandomSnippet(float snippetSeconds, float minSeconds = 1f)
        {
            const float burstGap = 0.6f; // silence longer than this splits bursts

            var candidateBursts = new List<(ulong who, int start, int count)>();
            foreach (var kvp in store)
            {
                var list = kvp.Value;
                int burstStart = 0;
                for (int i = 1; i <= list.Count; i++)
                {
                    bool endOfBurst = i == list.Count || list[i].Time - list[i - 1].Time > burstGap;
                    if (!endOfBurst)
                        continue;

                    if (list[i - 1].Time - list[burstStart].Time >= minSeconds)
                        candidateBursts.Add((kvp.Key, burstStart, i - burstStart));
                    burstStart = i;
                }
            }

            if (candidateBursts.Count == 0)
                return null;

            var (who2, start, count) = candidateBursts[Random.Range(0, candidateBursts.Count)];
            var source = store[who2];

            // Random window of up to snippetSeconds within the burst.
            float burstLength = source[start + count - 1].Time - source[start].Time;
            float windowStart = burstLength > snippetSeconds
                ? Random.Range(0f, burstLength - snippetSeconds)
                : 0f;

            var snippet = new List<Packet>();
            float t0 = source[start].Time + windowStart;
            for (int i = start; i < start + count; i++)
            {
                if (source[i].Time < t0)
                    continue;
                if (source[i].Time - t0 > snippetSeconds)
                    break;
                snippet.Add(source[i]);
            }

            return (who2, snippet);
        }
    }
}
