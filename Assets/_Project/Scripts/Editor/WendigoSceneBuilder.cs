using System.IO;
using HollerHorror.Debugging;
using HollerHorror.Entities;
using HollerHorror.Player;
using HollerHorror.Senses;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace HollerHorror.Editor
{
    /// <summary>
    /// Builds the M3 Wendigo hunt scene: greybox environment + laurel-thicket
    /// clusters (line-of-sight breakers, the Wendigo counterplay), the Wendigo
    /// itself, runtime-baked NavMesh, and the senses debug layer.
    /// </summary>
    public static class WendigoSceneBuilder
    {
        private const string ScenePath = "Assets/_Project/Scenes/Wendigo_Test.unity";

        [MenuItem("Holler Horror/Build Wendigo Test Scene")]
        public static void Build()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GreyboxSceneBuilder.BuildLighting();
            GreyboxSceneBuilder.BuildEnvironment();
            BuildThicketClusters();

            var player = GreyboxSceneBuilder.BuildPlayer(new Vector3(0, 0.05f, -30));
            player.AddComponent<PlayerHealth>();

            BuildWendigo(new Vector3(-15, 0, 25));

            // NavMesh over everything, baked at runtime by NavMeshBootstrap.
            var navGo = new GameObject("NavMesh");
            var surface = navGo.AddComponent<NavMeshSurface>();
            surface.collectObjects = CollectObjects.All;
            navGo.AddComponent<NavMeshBootstrap>();

            var debugGo = new GameObject("SensesDebug");
            debugGo.AddComponent<NoiseDebugRenderer>();
            debugGo.AddComponent<PlaceholderFootstepAudio>();

            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log($"[WendigoSceneBuilder] Built and saved {ScenePath}");
        }

        /// <summary>
        /// Dense clusters of tall thin boxes — greybox stand-ins for laurel thickets.
        /// Players can weave through; the Wendigo's vision breaks against them.
        /// </summary>
        private static void BuildThicketClusters()
        {
            var root = new GameObject("Thickets").transform;
            const string matPath = "Assets/_Project/Materials/Entities/Greybox_Thicket.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null)
            {
                mat = new Material(Shader.Find("Universal Render Pipeline/Lit"))
                {
                    color = new Color(0.2f, 0.3f, 0.18f),
                };
                Directory.CreateDirectory("Assets/_Project/Materials/Entities");
                AssetDatabase.CreateAsset(mat, matPath);
            }

            var rng = new System.Random(43);
            Vector3[] clusterCenters =
            {
                new(-8, 0, 6), new(14, 0, -4), new(-20, 0, -14), new(4, 0, 22), new(24, 0, 14),
            };

            foreach (var center in clusterCenters)
            {
                int stalks = 14 + rng.Next(8);
                for (int i = 0; i < stalks; i++)
                {
                    float angle = (float)(rng.NextDouble() * Mathf.PI * 2);
                    float dist = (float)(rng.NextDouble() * 4.5);
                    var pos = center + new Vector3(Mathf.Cos(angle) * dist, 1.4f, Mathf.Sin(angle) * dist);

                    var stalk = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    stalk.name = "ThicketStalk";
                    stalk.transform.SetParent(root);
                    stalk.transform.position = pos;
                    stalk.transform.localScale = new Vector3(
                        0.25f + (float)rng.NextDouble() * 0.3f, 2.8f, 0.25f + (float)rng.NextDouble() * 0.3f);
                    stalk.transform.rotation = Quaternion.Euler(0, (float)rng.NextDouble() * 360f, 0);
                    stalk.GetComponent<Renderer>().sharedMaterial = mat;
                }
            }
        }

        private static void BuildWendigo(Vector3 position)
        {
            var wendigo = new GameObject("Wendigo");
            wendigo.transform.position = position;

            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(wendigo.transform);
            body.transform.localPosition = new Vector3(0, 1.3f, 0);
            body.transform.localScale = new Vector3(0.9f, 1.3f, 0.9f); // tall, gaunt silhouette

            var head = GameObject.CreatePrimitive(PrimitiveType.Cube);
            head.name = "Head";
            Object.DestroyImmediate(head.GetComponent<Collider>());
            head.transform.SetParent(wendigo.transform);
            head.transform.localPosition = new Vector3(0, 2.5f, 0.25f);
            head.transform.localScale = new Vector3(0.35f, 0.45f, 0.55f);

            var agent = wendigo.AddComponent<UnityEngine.AI.NavMeshAgent>();
            agent.height = 2.6f;
            agent.radius = 0.45f;
            agent.acceleration = 14f;
            agent.angularSpeed = 240f;

            var perception = wendigo.AddComponent<EntityPerception>();
            var perceptionSo = new SerializedObject(perception);
            perceptionSo.FindProperty("eyeHeight").floatValue = 2.5f;
            perceptionSo.FindProperty("visionRange").floatValue = 30f;
            perceptionSo.FindProperty("visionFovDegrees").floatValue = 140f;
            perceptionSo.FindProperty("hearingSensitivity").floatValue = 1.25f; // GDD: drawn strongly to sound
            perceptionSo.ApplyModifiedPropertiesWithoutUndo();

            wendigo.AddComponent<WendigoController>();
            wendigo.AddComponent<WendigoAmbientCalls>();

            var display = wendigo.AddComponent<PerceptionDebugDisplay>();
            var displaySo = new SerializedObject(display);
            displaySo.FindProperty("perception").objectReferenceValue = perception;
            displaySo.FindProperty("body").objectReferenceValue = body.GetComponent<Renderer>();
            displaySo.FindProperty("controlRotation").boolValue = false; // NavMeshAgent steers
            displaySo.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
