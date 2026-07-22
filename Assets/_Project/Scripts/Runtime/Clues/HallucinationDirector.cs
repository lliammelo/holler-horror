using HollerHorror.Player;
using UnityEngine;

namespace HollerHorror.Clues
{
    /// <summary>
    /// Spawns client-side hallucinations while the local player's sanity is low
    /// (GDD §7). False clue-props appear near the player and vanish when examined
    /// or when the mind steadies — teaching players to distrust their own reports.
    /// Client-side by design: in multiplayer these are never networked.
    /// </summary>
    public sealed class HallucinationDirector : MonoBehaviour
    {
        [SerializeField] private Vector2 spawnIntervalRange = new(6f, 12f);
        [SerializeField] private float spawnDistance = 8f;
        [SerializeField] private int maxConcurrent = 3;

        private static readonly string[] Phantoms =
        {
            "A stripped carcass", "Claw marks on the bark", "Fresh footprints",
            "A figure, watching", "A cold handprint", "Salt, spilled in a circle",
        };

        private float nextSpawn;
        private int liveCount;

        private void Update()
        {
            var sanity = PlayerSanity.Local;
            if (sanity == null || sanity.Tier != SanityTier.Fraying)
                return;

            if (Time.time < nextSpawn || liveCount >= maxConcurrent)
                return;

            nextSpawn = Time.time + Random.Range(spawnIntervalRange.x, spawnIntervalRange.y);
            Spawn(sanity.transform);
        }

        private void Spawn(Transform player)
        {
            Vector2 flat = Random.insideUnitCircle.normalized * Random.Range(spawnDistance * 0.5f, spawnDistance);
            Vector3 pos = player.position + new Vector3(flat.x, 0f, flat.y);
            if (!Physics.Raycast(pos + Vector3.up * 3f, Vector3.down, out RaycastHit hit, 6f))
                return;

            var prop = GameObject.CreatePrimitive(PrimitiveType.Cube);
            prop.name = "Hallucination";
            prop.transform.position = hit.point + Vector3.up * 0.4f;
            prop.transform.localScale = new Vector3(0.7f, 0.8f, 0.7f);
            prop.GetComponent<Renderer>().sharedMaterial =
                new Material(Shader.Find("Universal Render Pipeline/Lit")) { color = new Color(0.3f, 0.28f, 0.3f) };

            var fake = prop.AddComponent<HallucinatedEvidence>();
            fake.Configure(Phantoms[Random.Range(0, Phantoms.Length)]);

            liveCount++;
            var tracker = prop.AddComponent<DestroyNotifier>();
            tracker.OnDestroyed = () => liveCount--;
        }

        /// <summary>Tiny helper so the director can keep an accurate live count.</summary>
        private sealed class DestroyNotifier : MonoBehaviour
        {
            public System.Action OnDestroyed;
            private void OnDestroy() => OnDestroyed?.Invoke();
        }
    }
}
