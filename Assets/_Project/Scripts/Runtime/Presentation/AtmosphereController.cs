using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace HollerHorror.Presentation
{
    /// <summary>
    /// The M9 "art pass" for a code-driven greybox: a global post-processing
    /// grade that pushes the flat greybox toward the concept art's cinematic
    /// folk-horror mood — cool desaturated grade, edge vignette, and film grain.
    /// Built at runtime and applied to every camera, so it needs no per-scene
    /// setup and survives camera swaps (player ↔ board camera).
    /// </summary>
    public sealed class AtmosphereController : MonoBehaviour
    {
        [SerializeField, Range(-1f, 1f)] private float exposure = -0.25f;
        [SerializeField, Range(-100f, 100f)] private float contrast = 14f;
        [SerializeField, Range(-100f, 100f)] private float saturation = -18f;
        [SerializeField] private Color colorFilter = new(0.86f, 0.91f, 1f); // cool moonlight cast
        [SerializeField, Range(0f, 1f)] private float vignette = 0.42f;
        [SerializeField, Range(0f, 1f)] private float grain = 0.32f;

        private void Start()
        {
            EnablePostOnAllCameras();
            BuildVolume();
        }

        private static void EnablePostOnAllCameras()
        {
            foreach (var cam in FindObjectsByType<Camera>(FindObjectsInactive.Include))
            {
                if (!cam.TryGetComponent(out UniversalAdditionalCameraData data))
                    data = cam.gameObject.AddComponent<UniversalAdditionalCameraData>();
                data.renderPostProcessing = true;
            }
        }

        private void BuildVolume()
        {
            var volumeGo = new GameObject("AtmosphereVolume");
            volumeGo.transform.SetParent(transform);
            var volume = volumeGo.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 10f;

            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            volume.sharedProfile = profile;

            var color = profile.Add<ColorAdjustments>();
            color.postExposure.Override(exposure);
            color.contrast.Override(contrast);
            color.saturation.Override(saturation);
            color.colorFilter.Override(colorFilter);

            var vig = profile.Add<Vignette>();
            vig.intensity.Override(vignette);
            vig.smoothness.Override(0.5f);

            var film = profile.Add<FilmGrain>();
            film.type.Override(FilmGrainLookup.Medium1);
            film.intensity.Override(grain);
            film.response.Override(0.8f);
        }
    }
}
