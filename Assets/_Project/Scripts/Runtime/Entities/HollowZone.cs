using System.Collections.Generic;
using UnityEngine;

namespace HollerHorror.Entities
{
    /// <summary>
    /// A drifting zone of the Hollow (GDD §4.3): unnatural dark and silence that
    /// spreads across the valley. It herds — drifting toward players — but is
    /// repelled by fire. Inside, lights gutter and sanity drains. Not chased;
    /// managed. This component is the volume; PlayerSanity reads it, the
    /// HollowController spawns and escalates them.
    /// </summary>
    public sealed class HollowZone : MonoBehaviour
    {
        private static readonly List<HollowZone> zones = new();

        [SerializeField] private float radius = 7f;
        [SerializeField] private float driftSpeed = 1.6f;
        [SerializeField, Tooltip("How strongly it drifts toward the nearest player.")]
        private float herdBias = 0.5f;
        [SerializeField, Tooltip("Bright lights within this distance push the zone away.")]
        private float fireAvoidRange = 6f;
        [SerializeField] private float maxRadius = 12f;
        [SerializeField, Tooltip("Radius growth per second while active.")]
        private float growthRate = 0.15f;

        private Renderer visual;

        private float noiseSeed;

        public float Radius => radius;

        public static bool IsInside(Vector3 position)
        {
            foreach (var zone in zones)
                if (zone != null && (position - zone.transform.position).sqrMagnitude <= zone.radius * zone.radius)
                    return true;
            return false;
        }

        public static IReadOnlyList<HollowZone> All => zones;

        private void OnEnable() => zones.Add(this);

        private void OnDisable() => zones.Remove(this);

        private void Start()
        {
            noiseSeed = Random.Range(0f, 1000f); // per-zone wander offset

            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.name = "ZoneVisual";
            Destroy(sphere.GetComponent<Collider>());
            sphere.transform.SetParent(transform, false);
            sphere.transform.localScale = Vector3.one * (radius * 2f);
            visual = sphere.GetComponent<Renderer>();
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"))
            { color = new Color(0.02f, 0.02f, 0.04f) };
            visual.sharedMaterial = mat;
        }

        private void Update()
        {
            Drift();
            Grow();
            if (visual != null)
                visual.transform.localScale = Vector3.one * (radius * 2f);
        }

        private void Drift()
        {
            Vector3 dir = GetWanderDir();

            var player = Player.PlayerSanity.Local;
            if (player != null)
            {
                Vector3 toPlayer = player.transform.position - transform.position;
                toPlayer.y = 0f;
                if (toPlayer.sqrMagnitude > 0.1f)
                    dir += toPlayer.normalized * herdBias;
            }

            // Pushed away from fire.
            foreach (var light in FindObjectsByType<Light>(FindObjectsInactive.Exclude))
            {
                if (light.type == LightType.Directional || light.intensity < 0.4f)
                    continue;
                Vector3 away = transform.position - light.transform.position;
                away.y = 0f;
                float d = away.magnitude;
                if (d < fireAvoidRange && d > 0.01f)
                    dir += away.normalized * (1f - d / fireAvoidRange) * 2f;
            }

            dir.y = 0f;
            if (dir.sqrMagnitude > 0.01f)
                transform.position += dir.normalized * driftSpeed * Time.deltaTime;
        }

        private Vector3 GetWanderDir()
        {
            // Smooth low-frequency wander from perlin noise.
            float t = Time.time * 0.15f + noiseSeed;
            return new Vector3(Mathf.PerlinNoise(t, 0f) - 0.5f, 0f, Mathf.PerlinNoise(0f, t) - 0.5f);
        }

        private void Grow()
        {
            if (radius < maxRadius)
                radius = Mathf.Min(maxRadius, radius + growthRate * Time.deltaTime);
        }
    }
}
