using UnityEngine;
using UnityEngine.InputSystem;

namespace HollerHorror.Player
{
    /// <summary>
    /// The player's carried light (GDD §7). Toggled with F, points where you look,
    /// and runs on a battery — so light is a resource, not a given. A live
    /// flashlight also wards off the dark for sanity purposes, which makes the
    /// battery the real clock on how long you can stay out in the holler.
    /// </summary>
    public sealed class Flashlight : MonoBehaviour
    {
        [SerializeField] private Transform mountPoint;
        [SerializeField] private float maxBattery = 100f;
        [SerializeField, Tooltip("Battery drained per second while lit. ~6.5 min of continuous light.")]
        private float drainPerSecond = 0.25f;
        [SerializeField] private float range = 26f;
        [SerializeField] private float intensity = 2.6f;
        [SerializeField, Range(10f, 120f)] private float spotAngle = 55f;

        private Light spot;

        public float Battery { get; private set; }
        public bool IsOn => spot != null && spot.enabled;

        private void Awake()
        {
            Battery = maxBattery;

            var mount = mountPoint != null ? mountPoint : FindCameraMount();
            var go = new GameObject("FlashlightBeam");
            go.transform.SetParent(mount, false);
            go.transform.localPosition = new Vector3(0.15f, -0.1f, 0.2f);

            spot = go.AddComponent<Light>();
            spot.type = LightType.Spot;
            spot.color = new Color(1f, 0.95f, 0.85f);
            spot.intensity = intensity;
            spot.range = range;
            spot.spotAngle = spotAngle;
            spot.innerSpotAngle = spotAngle * 0.45f;
            spot.shadows = LightShadows.Soft;
            spot.enabled = false;
        }

        private Transform FindCameraMount()
        {
            var cam = GetComponentInChildren<Camera>(includeInactive: true);
            return cam != null ? cam.transform : transform;
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame && Battery > 0f)
                spot.enabled = !spot.enabled;

            if (!spot.enabled)
                return;

            Battery -= drainPerSecond * Time.deltaTime;
            if (Battery <= 0f)
            {
                Battery = 0f;
                spot.enabled = false;
            }

            // Guttering as the cell dies — a warning you can see.
            float low = Mathf.InverseLerp(0f, 20f, Battery);
            spot.intensity = intensity * Mathf.Lerp(0.35f, 1f, low)
                             * (Battery < 20f ? 0.85f + 0.15f * Mathf.Sin(Time.time * 18f) : 1f);
        }

        /// <summary>Spare cells / recharging at camp will call this later.</summary>
        public void Recharge(float amount) => Battery = Mathf.Min(maxBattery, Battery + amount);

        private void OnGUI()
        {
            GUI.Box(new Rect(10, 176, 250, 24),
                $"Torch [F]: {(IsOn ? "on" : "off")}   battery {Mathf.RoundToInt(Battery)}%");
        }
    }
}
