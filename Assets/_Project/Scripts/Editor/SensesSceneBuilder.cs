using System.IO;
using HollerHorror.Debugging;
using HollerHorror.Senses;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace HollerHorror.Editor
{
    /// <summary>
    /// Builds the M2 senses test scene: greybox environment plus a row of surface
    /// strips (each with its own acoustic profile), a perception "sensor dummy"
    /// that hears and sees, and the noise ring visualizer (F3 to toggle).
    /// </summary>
    public static class SensesSceneBuilder
    {
        private const string ScenePath = "Assets/_Project/Scenes/Senses_Test.unity";

        [MenuItem("Holler Horror/Build Senses Test Scene")]
        public static void Build()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GreyboxSceneBuilder.BuildLighting();
            GreyboxSceneBuilder.BuildEnvironment();
            BuildSurfaceStrips();

            var player = GreyboxSceneBuilder.BuildPlayer(new Vector3(0, 0.05f, -30));
            player.AddComponent<MicrophoneNoiseProbe>(); // local mic → voice noise, no Steam needed

            // Right at the end of the strip row: walking each strip toward it shows
            // the audibility differences clearly; creeping on grass still hides you.
            BuildSensorDummy(new Vector3(0, 0, 0));

            var debugGo = new GameObject("SensesDebug");
            debugGo.AddComponent<NoiseDebugRenderer>();
            debugGo.AddComponent<PlaceholderFootstepAudio>();

            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log($"[SensesSceneBuilder] Built and saved {ScenePath}");
        }

        private static void BuildSurfaceStrips()
        {
            var root = new GameObject("SurfaceStrips").transform;

            (SurfaceType type, Color color)[] strips =
            {
                (SurfaceType.Grass, new Color(0.35f, 0.48f, 0.30f)),
                (SurfaceType.LeafLitter, new Color(0.48f, 0.36f, 0.22f)),
                (SurfaceType.Gravel, new Color(0.52f, 0.50f, 0.48f)),
                (SurfaceType.Floorboards, new Color(0.55f, 0.42f, 0.28f)),
                (SurfaceType.Creek, new Color(0.25f, 0.40f, 0.55f)),
            };

            float stripWidth = 6f;
            float startX = -(strips.Length - 1) * stripWidth * 0.5f;

            for (int i = 0; i < strips.Length; i++)
            {
                var (type, color) = strips[i];
                float x = startX + i * stripWidth;

                var strip = GameObject.CreatePrimitive(PrimitiveType.Cube);
                strip.name = $"Strip_{type}";
                strip.transform.SetParent(root);
                strip.transform.position = new Vector3(x, 0.03f, -10f);
                strip.transform.localScale = new Vector3(stripWidth - 0.3f, 0.06f, 16f);
                strip.GetComponent<Renderer>().sharedMaterial = CreateStripMaterial(type.ToString(), color);
                strip.AddComponent<SurfaceTag>().Type = type;

                var labelGo = new GameObject($"Label_{type}");
                labelGo.transform.SetParent(root);
                labelGo.transform.position = new Vector3(x, 1.6f, -19.5f);
                labelGo.transform.rotation = Quaternion.Euler(0, 180, 0); // face the spawn
                var label = labelGo.AddComponent<TextMesh>();
                label.text = $"{type}\nx{SurfaceAcoustics.NoiseMultiplier(type):F2}";
                label.characterSize = 0.35f;
                label.fontSize = 32;
                label.anchor = TextAnchor.MiddleCenter;
                label.color = Color.white;
            }
        }

        private static void BuildSensorDummy(Vector3 position)
        {
            var dummy = new GameObject("SensorDummy");
            dummy.transform.position = position;
            dummy.transform.rotation = Quaternion.Euler(0, 180, 0); // faces the spawn area

            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(dummy.transform);
            body.transform.localPosition = new Vector3(0, 1f, 0);
            body.transform.localScale = new Vector3(1f, 1f, 1f);

            // Simple "nose" so facing direction is readable.
            var nose = GameObject.CreatePrimitive(PrimitiveType.Cube);
            nose.name = "Nose";
            Object.DestroyImmediate(nose.GetComponent<Collider>());
            nose.transform.SetParent(dummy.transform);
            nose.transform.localPosition = new Vector3(0, 1.6f, 0.55f);
            nose.transform.localScale = new Vector3(0.15f, 0.15f, 0.5f);

            var perception = dummy.AddComponent<EntityPerception>();

            var display = dummy.AddComponent<PerceptionDebugDisplay>();
            var so = new SerializedObject(display);
            so.FindProperty("perception").objectReferenceValue = perception;
            so.FindProperty("body").objectReferenceValue = body.GetComponent<Renderer>();
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Material CreateStripMaterial(string name, Color color)
        {
            const string folder = "Assets/_Project/Materials/Surfaces";
            Directory.CreateDirectory(folder);
            string path = $"{folder}/Surface_{name}.mat";
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null)
                return existing;

            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit")) { color = color };
            AssetDatabase.CreateAsset(mat, path);
            return mat;
        }
    }
}
