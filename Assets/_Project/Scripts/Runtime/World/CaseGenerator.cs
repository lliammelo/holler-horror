using System.Collections.Generic;
using HollerHorror.Clues;
using HollerHorror.Dialogue;
using UnityEngine;
using EntityId = HollerHorror.Clues.EntityId;

namespace HollerHorror.World
{
    /// <summary>
    /// Rolls the run on a fixed map (GDD §8): which entity is really out there,
    /// which evidence exists and where, and which cast is present. The geography
    /// never changes, so players can't read the answer off the map — only off
    /// what they find and who they talk to.
    ///
    /// Entity and cast objects live in the scene pre-built but disabled; the
    /// generator enables the rolled set. Evidence props are built from the
    /// catalog at run start.
    /// </summary>
    public sealed class CaseGenerator : MonoBehaviour
    {
        [SerializeField] private CaseDirector director;

        [Header("Pre-built, disabled in scene — one set is enabled per run")]
        [SerializeField] private GameObject wendigoSet;
        [SerializeField] private GameObject fetchSet;
        [SerializeField] private GameObject hollowSet;

        [Header("Evidence mix")]
        [SerializeField, Tooltip("Ambiguous clues that read as more than one entity.")]
        private int ambiguousCount = 2;
        [SerializeField, Tooltip("Clues pointing at an entity that ISN'T active — genuine false leads.")]
        private int misleadingCount = 2;

        [Header("Testing")]
        [SerializeField, Tooltip("Unknown = roll randomly. Set an entity to force it.")]
        private EntityId forceEntity = EntityId.Unknown;

        private readonly HashSet<Transform> usedAnchors = new();

        public EntityId Rolled { get; private set; }

        private void Awake()
        {
            Rolled = forceEntity != EntityId.Unknown
                ? forceEntity
                : (EntityId)Random.Range(1, 4); // Wendigo=1, Fetch=2, Hollow=3

            if (director != null)
                director.SetActiveEntity(Rolled);
            else
                Debug.LogError("[CaseGenerator] No CaseDirector wired — the case has no truth to check against.");

            ActivateSet();
            Debug.Log($"[CaseGenerator] This run: {Rolled}.");
        }

        // Evidence waits for Start: POIs register themselves in OnEnable, and
        // Awake ordering across objects isn't guaranteed.
        private void Start() => PlaceEvidence();

        private void ActivateSet()
        {
            if (wendigoSet != null) wendigoSet.SetActive(Rolled == EntityId.Wendigo);
            if (fetchSet != null) fetchSet.SetActive(Rolled == EntityId.Fetch);
            if (hollowSet != null) hollowSet.SetActive(Rolled == EntityId.Hollow);
        }

        private void PlaceEvidence()
        {
            var chosen = new List<EvidenceCatalog.Entry>();
            chosen.AddRange(EvidenceCatalog.Supporting(Rolled));            // the truth
            chosen.AddRange(PickSome(EvidenceCatalog.Ambiguous(), ambiguousCount));
            chosen.AddRange(PickSome(EvidenceCatalog.Misleading(Rolled), misleadingCount));

            var root = new GameObject("GeneratedEvidence").transform;
            foreach (var entry in chosen)
            {
                var anchor = FindFreeAnchor(entry.Places);
                if (anchor == null)
                {
                    Debug.LogWarning($"[CaseGenerator] No free anchor for '{entry.Title}' — skipped.");
                    continue;
                }
                Spawn(entry, anchor, root);
            }
        }

        private static List<T> PickSome<T>(List<T> pool, int count)
        {
            var result = new List<T>();
            for (int i = 0; i < count && pool.Count > 0; i++)
            {
                int index = Random.Range(0, pool.Count);
                result.Add(pool[index]);
                pool.RemoveAt(index);
            }
            return result;
        }

        private Transform FindFreeAnchor(PoiKind[] allowed)
        {
            var candidates = new List<Transform>();
            foreach (var kind in allowed)
                foreach (var poi in PointOfInterest.OfKind(kind))
                    foreach (var anchor in poi.EvidenceAnchors)
                        if (anchor != null && !usedAnchors.Contains(anchor))
                            candidates.Add(anchor);

            if (candidates.Count == 0)
                return null;

            var pick = candidates[Random.Range(0, candidates.Count)];
            usedAnchors.Add(pick);
            return pick;
        }

        private static void Spawn(EvidenceCatalog.Entry entry, Transform anchor, Transform root)
        {
            var prop = GameObject.CreatePrimitive(PrimitiveType.Cube);
            prop.name = $"Evidence_{entry.Title.Replace(' ', '_')}";
            prop.transform.SetParent(root);
            prop.transform.position = anchor.position + Vector3.up * 0.4f;
            prop.transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
            prop.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            prop.GetComponent<Renderer>().sharedMaterial =
                new Material(Shader.Find("Universal Render Pipeline/Lit")) { color = entry.Tint };

            prop.AddComponent<EvidencePickup>().Configure(entry.Title, entry.Body, entry.FoundAt);
        }
    }
}
