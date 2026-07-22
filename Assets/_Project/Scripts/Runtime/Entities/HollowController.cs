using System.Collections.Generic;
using HollerHorror.Dialogue;
using UnityEngine;

namespace HollerHorror.Entities
{
    /// <summary>
    /// The Hollow (GDD §4.3): the valley awake. Not a body — it manages the
    /// drifting zones, spreads them over the night, and swallows homesteads,
    /// cutting residents (and their clues) off inside the dark. The Consecration
    /// unmakes it. Implements IBanishable so CaseResolver ends it on a win.
    /// </summary>
    public sealed class HollowController : MonoBehaviour, IBanishable
    {
        [SerializeField] private int zoneCount = 2;
        [SerializeField] private float mapRadius = 34f;
        [SerializeField] private Vector2 signIntervalRange = new(14f, 26f);

        private float nextSignTime;
        private readonly Dictionary<Light, float> gutteredLights = new();
        private float lightScanTimer;
        private Light[] sceneLights = System.Array.Empty<Light>();

        private void Start()
        {
            for (int i = 0; i < zoneCount; i++)
                SpawnZone(Random.insideUnitCircle * mapRadius);
            ScheduleSign();
        }

        private void SpawnZone(Vector2 flat)
        {
            var go = new GameObject("HollowZone");
            go.transform.position = new Vector3(flat.x, 0.5f, flat.y);
            go.AddComponent<HollowZone>();
        }

        private void Update()
        {
            EnvelopeCheck();
            GutterLights();

            if (Time.time >= nextSignTime)
            {
                ScheduleSign();
                Debug.Log("[Hollow] The insects stop. A cold seam opens in the air.");
                // (Placeholder sign — dead birdsong / cold spot. Real audio in M9.)
            }
        }

        /// <summary>Lights inside any zone gutter to a guttering flame; restored when the dark passes.</summary>
        private void GutterLights()
        {
            lightScanTimer -= Time.deltaTime;
            if (lightScanTimer <= 0f)
            {
                lightScanTimer = 1f;
                sceneLights = FindObjectsByType<Light>(FindObjectsInactive.Exclude);
            }

            foreach (var light in sceneLights)
            {
                if (light == null || light.type == LightType.Directional)
                    continue;

                bool inside = HollowZone.IsInside(light.transform.position);
                bool tracked = gutteredLights.ContainsKey(light);

                if (inside && !tracked)
                {
                    gutteredLights[light] = light.intensity;
                    light.intensity *= 0.15f;
                }
                else if (!inside && tracked)
                {
                    light.intensity = gutteredLights[light];
                    gutteredLights.Remove(light);
                }
            }
        }

        /// <summary>Residents caught inside a zone are cut off — can't be reached until the dark passes.</summary>
        private void EnvelopeCheck()
        {
            foreach (var npc in NpcRegistry.All)
            {
                if (npc == null || !npc.IsAlive)
                    continue;
                npc.SetEnveloped(HollowZone.IsInside(npc.transform.position));
            }
        }

        private void ScheduleSign() =>
            nextSignTime = Time.time + Random.Range(signIntervalRange.x, signIntervalRange.y);

        public void Banish()
        {
            Debug.Log("[Hollow] Consecrated. The valley exhales and is only a valley again.");
            foreach (var zone in new List<HollowZone>(HollowZone.All))
                if (zone != null)
                    Destroy(zone.gameObject);

            // Restore any lights the dark had guttered.
            foreach (var kvp in gutteredLights)
                if (kvp.Key != null)
                    kvp.Key.intensity = kvp.Value;
            gutteredLights.Clear();

            gameObject.SetActive(false);
        }
    }
}
