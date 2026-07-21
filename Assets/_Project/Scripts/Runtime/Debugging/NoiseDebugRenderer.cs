using System.Collections.Generic;
using HollerHorror.Senses;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HollerHorror.Debugging
{
    /// <summary>
    /// Draws every NoiseEvent as an expanding, fading ring on the ground in the
    /// game view, so noise radii can be tuned by eye. Toggle with F3.
    /// Footsteps: white. Voice: cyan. Interactions: orange.
    /// </summary>
    public sealed class NoiseDebugRenderer : MonoBehaviour
    {
        private const int Segments = 48;
        private const float ExpandSeconds = 0.7f;

        [SerializeField] private bool visible = true;

        private readonly List<(LineRenderer line, NoiseEvent evt, float born)> rings = new();
        private Material ringMaterial;

        private void OnEnable() => NoiseBus.OnNoise += SpawnRing;
        private void OnDisable() => NoiseBus.OnNoise -= SpawnRing;

        private void Awake()
        {
            ringMaterial = new Material(Shader.Find("Sprites/Default"));
        }

        private void SpawnRing(NoiseEvent noiseEvent)
        {
            if (!visible)
                return;

            var go = new GameObject("NoiseRing");
            go.transform.position = noiseEvent.Position + Vector3.up * 0.05f;
            var line = go.AddComponent<LineRenderer>();
            line.material = ringMaterial;
            line.loop = true;
            line.positionCount = Segments;
            line.widthMultiplier = 0.08f;
            line.useWorldSpace = false;
            rings.Add((line, noiseEvent, Time.time));
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.f3Key.wasPressedThisFrame)
                visible = !visible;

            for (int i = rings.Count - 1; i >= 0; i--)
            {
                var (line, evt, born) = rings[i];
                float t = (Time.time - born) / ExpandSeconds;
                if (t >= 1f || line == null)
                {
                    if (line != null)
                        Destroy(line.gameObject);
                    rings.RemoveAt(i);
                    continue;
                }

                float radius = Mathf.Lerp(0.3f, evt.Radius, Mathf.Sin(t * Mathf.PI * 0.5f));
                for (int s = 0; s < Segments; s++)
                {
                    float angle = s / (float)Segments * Mathf.PI * 2f;
                    line.SetPosition(s, new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius));
                }

                Color c = evt.Kind switch
                {
                    NoiseKind.Voice => Color.cyan,
                    NoiseKind.Interaction => new Color(1f, 0.6f, 0.1f),
                    _ => Color.white,
                };
                c.a = (1f - t) * Mathf.Lerp(0.25f, 0.9f, evt.Loudness);
                line.startColor = line.endColor = c;
            }
        }
    }
}
