using System.Collections.Generic;
using UnityEngine;

namespace HollerHorror.Entities
{
    /// <summary>
    /// A stretch of running water (the creek). The Fetch cannot cross it
    /// (GDD §4.2) — players can use it as a barrier. Represented as an axis-aligned
    /// box; a segment that passes through any water box is considered blocked.
    /// </summary>
    public sealed class RunningWater : MonoBehaviour
    {
        private static readonly List<RunningWater> volumes = new();

        private BoxCollider box;

        private void Awake() => box = GetComponent<BoxCollider>();
        private void OnEnable() => volumes.Add(this);
        private void OnDisable() => volumes.Remove(this);

        /// <summary>True if the straight segment a→b passes through any running-water volume.</summary>
        public static bool SegmentCrossesWater(Vector3 a, Vector3 b)
        {
            foreach (var water in volumes)
                if (water != null && water.box != null && water.SegmentIntersects(a, b))
                    return true;
            return false;
        }

        private bool SegmentIntersects(Vector3 a, Vector3 b)
        {
            var bounds = box.bounds;
            // Sample the segment; cheap and adequate for greybox creek widths.
            const int steps = 16;
            for (int i = 0; i <= steps; i++)
            {
                Vector3 p = Vector3.Lerp(a, b, i / (float)steps);
                p.y = bounds.center.y; // ignore vertical — the creek is a ground band
                if (bounds.Contains(p))
                    return true;
            }
            return false;
        }
    }
}
