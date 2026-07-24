using System.Collections.Generic;
using UnityEngine;

namespace HollerHorror.World
{
    /// <summary>
    /// A landmark in the holler with anchor points the case generator can fill.
    /// The map is fixed; what appears at these anchors is rolled per run, which
    /// is what stops players reading the entity off the geography (GDD §8).
    /// </summary>
    public sealed class PointOfInterest : MonoBehaviour
    {
        private static readonly List<PointOfInterest> all = new();

        [SerializeField] private PoiKind kind = PoiKind.Farmstead;
        [SerializeField, Tooltip("Where evidence props may be placed.")]
        private List<Transform> evidenceAnchors = new();
        [SerializeField, Tooltip("Where a resident may stand.")]
        private List<Transform> npcAnchors = new();

        public PoiKind Kind { get => kind; set => kind = value; }
        public IReadOnlyList<Transform> EvidenceAnchors => evidenceAnchors;
        public IReadOnlyList<Transform> NpcAnchors => npcAnchors;

        public static IReadOnlyList<PointOfInterest> All => all;

        private void OnEnable() => all.Add(this);
        private void OnDisable() => all.Remove(this);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetRegistry() => all.Clear();

        public void AddEvidenceAnchor(Transform t) => evidenceAnchors.Add(t);
        public void AddNpcAnchor(Transform t) => npcAnchors.Add(t);

        /// <summary>All POIs of a given kind (there may be several thickets, cabins, etc).</summary>
        public static IEnumerable<PointOfInterest> OfKind(PoiKind kind)
        {
            foreach (var poi in all)
                if (poi != null && poi.kind == kind)
                    yield return poi;
        }
    }
}
