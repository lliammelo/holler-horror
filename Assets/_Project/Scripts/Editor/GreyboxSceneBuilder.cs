using System.IO;
using HollerHorror.Debugging;
using HollerHorror.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace HollerHorror.Editor
{
    /// <summary>
    /// Generates the M0 greybox test scene from scratch so the scene is
    /// reproducible and never needs hand-editing to exist. Run from the
    /// "Holler Horror" menu, or in batch mode via
    /// -executeMethod HollerHorror.Editor.GreyboxSceneBuilder.BuildFromBatchMode.
    /// </summary>
    public static class GreyboxSceneBuilder
    {
        private const string ScenePath = "Assets/_Project/Scenes/Greybox_Test.unity";
        private const string InputActionsPath = "Assets/InputSystem_Actions.inputactions";
        private const string MaterialsFolder = "Assets/_Project/Materials/Greybox";

        [MenuItem("Holler Horror/Build Greybox Test Scene")]
        public static void Build()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            Material ground = CreateMaterial("Greybox_Ground", new Color(0.30f, 0.32f, 0.30f));
            Material structure = CreateMaterial("Greybox_Structure", new Color(0.55f, 0.55f, 0.58f));
            Material accent = CreateMaterial("Greybox_Accent", new Color(0.65f, 0.45f, 0.30f));

            BuildLighting();
            BuildTerrain(ground, structure, accent);
            BuildPlayer(new Vector3(0, 0.05f, -20));

            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
            EditorSceneManager.SaveScene(scene, ScenePath);
            AddSceneToBuildSettings();
            Debug.Log($"[GreyboxSceneBuilder] Built and saved {ScenePath}");
        }

        /// <summary>Batch-mode entry point: builds the scene then exits with a status code.</summary>
        public static void BuildFromBatchMode()
        {
            try
            {
                Build();
                EditorApplication.Exit(0);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[GreyboxSceneBuilder] Failed: {e}");
                EditorApplication.Exit(1);
            }
        }

        internal static void BuildLighting()
        {
            var lightGo = new GameObject("Directional Light (Dusk)");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1.0f, 0.72f, 0.50f); // low warm dusk sun
            light.intensity = 0.9f;
            lightGo.transform.rotation = Quaternion.Euler(18f, -140f, 0f);

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.35f, 0.32f, 0.40f);
            RenderSettings.ambientEquatorColor = new Color(0.22f, 0.22f, 0.26f);
            RenderSettings.ambientGroundColor = new Color(0.10f, 0.10f, 0.12f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.45f, 0.42f, 0.48f);
            RenderSettings.fogDensity = 0.015f;
        }

        internal static void BuildEnvironment()
        {
            BuildTerrain(
                CreateMaterial("Greybox_Ground", new Color(0.30f, 0.32f, 0.30f)),
                CreateMaterial("Greybox_Structure", new Color(0.55f, 0.55f, 0.58f)),
                CreateMaterial("Greybox_Accent", new Color(0.65f, 0.45f, 0.30f)));
        }

        private static void BuildTerrain(Material ground, Material structure, Material accent)
        {
            var root = new GameObject("Greybox").transform;

            // Ground slab — default surface is grass (quiet)
            var groundGo = Box(root, "Ground", new Vector3(0, -0.5f, 0), new Vector3(80, 1, 80), ground);
            groundGo.AddComponent<HollerHorror.Senses.SurfaceTag>().Type = HollerHorror.Senses.SurfaceType.Grass;

            // Perimeter walls
            Box(root, "Wall_N", new Vector3(0, 2, 40.5f), new Vector3(82, 4, 1), structure);
            Box(root, "Wall_S", new Vector3(0, 2, -40.5f), new Vector3(82, 4, 1), structure);
            Box(root, "Wall_E", new Vector3(40.5f, 2, 0), new Vector3(1, 4, 82), structure);
            Box(root, "Wall_W", new Vector3(-40.5f, 2, 0), new Vector3(1, 4, 82), structure);

            // Ramp up to a platform (slope traversal + sprint testing)
            var ramp = Box(root, "Ramp", new Vector3(12, 1.0f, 8), new Vector3(4, 0.3f, 12), structure);
            ramp.transform.rotation = Quaternion.Euler(-18f, 0, 0);
            Box(root, "Platform", new Vector3(12, 1.85f, 17.5f), new Vector3(8, 0.3f, 8), structure);

            // Stairs (step height test)
            for (int i = 0; i < 8; i++)
                Box(root, $"Stair_{i}", new Vector3(-10, 0.125f + i * 0.25f, 10 + i * 0.45f),
                    new Vector3(4, 0.25f, 0.45f), structure);
            Box(root, "Stair_Top", new Vector3(-10, 1.9f, 16.5f), new Vector3(4, 0.3f, 5), structure);

            // Tight "thicket" corridor — walk vs crouch width tests
            Box(root, "Thicket_A", new Vector3(-16, 1.25f, -10), new Vector3(0.5f, 2.5f, 14), accent);
            Box(root, "Thicket_B", new Vector3(-14.6f, 1.25f, -10), new Vector3(0.5f, 2.5f, 14), accent);

            // Low crawl — forces crouch, tests blocked stand-up
            Box(root, "Crawl_Roof", new Vector3(6, 1.45f, -14), new Vector3(6, 0.3f, 8), accent);
            Box(root, "Crawl_Wall_L", new Vector3(3.2f, 0.65f, -14), new Vector3(0.4f, 1.3f, 8), accent);
            Box(root, "Crawl_Wall_R", new Vector3(8.8f, 0.65f, -14), new Vector3(0.4f, 1.3f, 8), accent);

            // Scatter of cover blocks
            Box(root, "Cover_A", new Vector3(-4, 0.75f, 4), new Vector3(2, 1.5f, 2), structure);
            Box(root, "Cover_B", new Vector3(2, 0.5f, -4), new Vector3(3, 1.0f, 1.5f), structure);
            Box(root, "Cover_C", new Vector3(20, 1.0f, -8), new Vector3(2, 2.0f, 4), structure);
            Box(root, "Cover_D", new Vector3(-24, 0.6f, 18), new Vector3(4, 1.2f, 2), structure);
        }

        private static GameObject Box(Transform parent, string name, Vector3 position, Vector3 scale, Material material)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent);
            go.transform.position = position;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = material;
            return go;
        }

        internal static GameObject BuildPlayer(Vector3 position)
        {
            var inputActions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputActionsPath);
            if (inputActions == null)
                throw new FileNotFoundException($"Input actions asset not found at {InputActionsPath}");

            var player = new GameObject("Player");
            player.transform.position = position;

            var cc = player.AddComponent<CharacterController>();
            cc.height = 1.8f;
            cc.radius = 0.35f;
            cc.center = new Vector3(0, 0.9f, 0);
            cc.slopeLimit = 45f;
            cc.stepOffset = 0.3f;

            var pivot = new GameObject("CameraPivot");
            pivot.transform.SetParent(player.transform);
            pivot.transform.localPosition = new Vector3(0, 1.66f, 0);

            var camGo = new GameObject("MainCamera");
            camGo.tag = "MainCamera";
            camGo.transform.SetParent(pivot.transform);
            camGo.transform.localPosition = Vector3.zero;
            var cam = camGo.AddComponent<Camera>();
            cam.nearClipPlane = 0.05f;
            cam.fieldOfView = 70f;
            camGo.AddComponent<AudioListener>();

            var stamina = player.AddComponent<PlayerStamina>();
            var controller = player.AddComponent<FirstPersonController>();

            var so = new SerializedObject(controller);
            so.FindProperty("inputActions").objectReferenceValue = inputActions;
            so.FindProperty("cameraPivot").objectReferenceValue = pivot.transform;
            so.ApplyModifiedPropertiesWithoutUndo();

            var emitter = player.AddComponent<HollerHorror.Senses.FootstepNoiseEmitter>();

            // Carried light — the holler is too big and too dark without one.
            var torch = player.AddComponent<HollerHorror.Player.Flashlight>();
            var torchSo = new SerializedObject(torch);
            torchSo.FindProperty("mountPoint").objectReferenceValue = camGo.transform;
            torchSo.ApplyModifiedPropertiesWithoutUndo();

            var hud = player.AddComponent<PlayerDebugHud>();
            var hudSo = new SerializedObject(hud);
            hudSo.FindProperty("controller").objectReferenceValue = controller;
            hudSo.FindProperty("stamina").objectReferenceValue = stamina;
            hudSo.FindProperty("noiseEmitter").objectReferenceValue = emitter;
            hudSo.ApplyModifiedPropertiesWithoutUndo();
            return player;
        }

        private static Material CreateMaterial(string name, Color color)
        {
            Directory.CreateDirectory(MaterialsFolder);
            string path = $"{MaterialsFolder}/{name}.mat";
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null)
                return existing;

            var shader = Shader.Find("Universal Render Pipeline/Lit");
            var mat = new Material(shader) { color = color };
            AssetDatabase.CreateAsset(mat, path);
            return mat;
        }

        private static void AddSceneToBuildSettings()
        {
            var scenes = EditorBuildSettings.scenes;
            foreach (var s in scenes)
                if (s.path == ScenePath)
                    return;

            var list = new System.Collections.Generic.List<EditorBuildSettingsScene>(scenes)
            {
                new EditorBuildSettingsScene(ScenePath, true)
            };
            EditorBuildSettings.scenes = list.ToArray();
        }
    }
}
