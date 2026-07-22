using System.Collections.Generic;
using UnityEngine;

namespace HollerHorror.Player
{
    public enum SanityTier { Calm, Uneasy, Fraying }

    /// <summary>
    /// Per-player sanity (GDD §7). Drained by darkness and by the Hollow's zones,
    /// restored near fire and light. Low sanity is where client-side hallucinations
    /// begin — the HallucinationDirector reads Tier. Fire is the refuge: stay in
    /// the light to keep your head.
    /// </summary>
    public sealed class PlayerSanity : MonoBehaviour
    {
        public static PlayerSanity Local { get; private set; }

        [SerializeField] private float max = 100f;
        [SerializeField, Tooltip("Sanity lost per second in darkness.")]
        private float drainInDark = 2.5f;
        [SerializeField, Tooltip("Extra sanity lost per second inside a Hollow zone.")]
        private float drainInHollow = 9f;
        [SerializeField, Tooltip("Sanity regained per second in adequate light.")]
        private float regenInLight = 6f;
        [SerializeField, Tooltip("A light counts as a refuge within this fraction of its range.")]
        private float lightRefugeFraction = 0.75f;
        [SerializeField, Tooltip("Minimum light intensity that still wards off the dark.")]
        private float minWardIntensity = 0.4f;

        private readonly List<Light> lights = new();
        private float lightScanTimer;

        public float Current { get; private set; }
        public float Normalized => Current / max;
        public bool InLight { get; private set; }
        public bool InHollow { get; private set; }

        public SanityTier Tier =>
            Normalized > 0.6f ? SanityTier.Calm :
            Normalized > 0.3f ? SanityTier.Uneasy : SanityTier.Fraying;

        private void Awake()
        {
            Local = this;
            Current = max;
        }

        private void OnDestroy()
        {
            if (Local == this)
                Local = null;
        }

        private void Update()
        {
            InLight = NearWardingLight();
            InHollow = Entities.HollowZone.IsInside(transform.position);

            if (InHollow)
                Current -= drainInHollow * Time.deltaTime;

            if (InLight && !InHollow)
                Current += regenInLight * Time.deltaTime;
            else if (!InLight)
                Current -= drainInDark * Time.deltaTime;

            Current = Mathf.Clamp(Current, 0f, max);
        }

        private bool NearWardingLight()
        {
            // Refresh the light set periodically (braziers/lanterns are scene objects;
            // ritual/hallucination lights come and go).
            lightScanTimer -= Time.deltaTime;
            if (lightScanTimer <= 0f)
            {
                lightScanTimer = 1f;
                lights.Clear();
                foreach (var light in FindObjectsByType<Light>(FindObjectsInactive.Exclude))
                    if (light.type != LightType.Directional)
                        lights.Add(light);
            }

            foreach (var light in lights)
            {
                if (light == null || !light.isActiveAndEnabled || light.intensity < minWardIntensity)
                    continue;
                if (Vector3.Distance(transform.position, light.transform.position) <= light.range * lightRefugeFraction)
                    return true;
            }
            return false;
        }

        private void OnGUI()
        {
            var style = new GUIStyle(GUI.skin.box) { fontSize = 12, alignment = TextAnchor.MiddleCenter };
            string state = Tier switch
            {
                SanityTier.Calm => "steady",
                SanityTier.Uneasy => "uneasy",
                _ => "fraying — trust nothing",
            };
            GUI.Box(new Rect(10, 148, 250, 24),
                $"Sanity {Mathf.RoundToInt(Current)} — {state}" + (InHollow ? "  [THE HOLLOW]" : InLight ? "  [lit]" : "  [dark]"),
                style);
        }
    }
}
