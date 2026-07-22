using System.IO;
using HollerHorror.Clues;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Yarn.Unity;

namespace HollerHorror.Editor
{
    /// <summary>
    /// Builds the M4 full-case scene: a mini-holler with base camp + clue board,
    /// three homesteads (Ada honest, Tetch withholding, Ruth mistaken), and a
    /// physical evidence set with supporting, refutable, and negative clues —
    /// a Wendigo case soluble end-to-end, committed at the board.
    /// </summary>
    public static class CaseSceneBuilder
    {
        private const string ScenePath = "Assets/_Project/Scenes/Case_Test.unity";
        private const string YarnProjectPath = "Assets/_Project/Dialogue/HollerHorror.yarnproject";

        [MenuItem("Holler Horror/Build Case Test Scene")]
        public static void Build()
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GreyboxSceneBuilder.BuildLighting();
            GreyboxSceneBuilder.BuildEnvironment();
            BuildBaseCamp();

            var yarnProject = AssetDatabase.LoadAssetAtPath<YarnProject>(YarnProjectPath);
            if (yarnProject == null)
                throw new FileNotFoundException($"Yarn project not imported at {YarnProjectPath}.");

            DialogueSceneBuilder.BuildDialogueSystem(yarnProject);

            BuildHomestead("Ada Bricker", "Ada_Start", new Vector3(-26f, 0f, 26f), new Color(0.55f, 0.35f, 0.30f));
            BuildHomestead("Old Man Tetch", "Tetch_Start", new Vector3(26f, 0f, 22f), new Color(0.35f, 0.40f, 0.50f));
            BuildHomestead("Ruth Combs", "Ruth_Start", new Vector3(22f, 0f, -20f), new Color(0.45f, 0.45f, 0.30f));

            BuildEvidenceSet();

            var player = GreyboxSceneBuilder.BuildPlayer(new Vector3(0, 0.05f, -28f));
            player.AddComponent<ClueBoardInteractor>();
            player.AddComponent<FieldJournal>();

            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log($"[CaseSceneBuilder] Built and saved {ScenePath}");
        }

        private static void BuildBaseCamp()
        {
            var root = new GameObject("BaseCamp").transform;
            var wood = new Material(Shader.Find("Universal Render Pipeline/Lit"))
            { color = new Color(0.32f, 0.24f, 0.16f) };

            Box(root, "CampWall", new Vector3(0, 1.6f, -33.8f), new Vector3(6, 3.2f, 0.3f), wood);
            Box(root, "CampTable", new Vector3(2.4f, 0.45f, -32.6f), new Vector3(1.6f, 0.9f, 0.7f), wood);

            var board = ClueBoardSceneBuilder.BuildBoard(new Vector3(0, 1.55f, -33.4f));
            board.transform.rotation = Quaternion.Euler(0, 180f, 0); // viewer side faces into camp

            var lanternGo = new GameObject("CampLantern");
            lanternGo.transform.position = new Vector3(0, 2.6f, -31.5f);
            var lantern = lanternGo.AddComponent<Light>();
            lantern.type = LightType.Point;
            lantern.color = new Color(1f, 0.75f, 0.45f);
            lantern.intensity = 2.2f;
            lantern.range = 10f;
        }

        private static void BuildHomestead(string npcName, string startNode, Vector3 position, Color tint)
        {
            var root = new GameObject($"Homestead_{npcName.Replace(' ', '_')}").transform;
            var wood = new Material(Shader.Find("Universal Render Pipeline/Lit"))
            { color = new Color(0.30f, 0.22f, 0.15f) };

            Box(root, "Floor", position + new Vector3(0, 0.06f, 0), new Vector3(6, 0.12f, 6), wood);
            Box(root, "BackWall", position + new Vector3(0, 1.5f, 2.9f), new Vector3(6, 3f, 0.25f), wood);
            Box(root, "SideWall", position + new Vector3(-2.9f, 1.5f, 0), new Vector3(0.25f, 3f, 6), wood);

            var lanternGo = new GameObject("Lantern");
            lanternGo.transform.SetParent(root);
            lanternGo.transform.position = position + new Vector3(0, 2.4f, 0);
            var lantern = lanternGo.AddComponent<Light>();
            lantern.type = LightType.Point;
            lantern.color = new Color(1f, 0.72f, 0.42f);
            lantern.intensity = 1.6f;
            lantern.range = 8f;

            DialogueSceneBuilder.BuildNpc(npcName, startNode, position + new Vector3(0.8f, 0.12f, 0.5f), tint);
        }

        private static void BuildEvidenceSet()
        {
            var root = new GameObject("Evidence").transform;

            // Wendigo-supporting evidence — carcass location varies per run.
            var carcassA = EvidenceProp(root, new Vector3(-24f, 0, 18f), new Color(0.5f, 0.25f, 0.2f),
                "Stripped carcass", "A hog taken apart to clean bone behind the smokehouse. Nothing wasted, nothing dragged off.", "behind Ada's smokehouse");
            var carcassB = EvidenceProp(root, new Vector3(8f, 0, 32f), new Color(0.5f, 0.25f, 0.2f),
                "Stripped carcass", "A deer stripped to clean bone beside the ridge trail. No scavenger sign - nothing else will touch it.", "the ridge trail");
            var layout = root.gameObject.AddComponent<AlternativeEvidenceSpots>();
            layout.Alternatives = new[] { carcassA, carcassB };

            EvidenceProp(root, new Vector3(0f, 0, 10f), new Color(0.35f, 0.28f, 0.18f),
                "Claw-scored bark", "Ash trees scored to bare wood, head height and higher. Whatever reaches that high walks upright.", "the ash line");

            EvidenceProp(root, new Vector3(-27.5f, 0, 21f), new Color(0.4f, 0.3f, 0.2f),
                "Torn smokehouse door", "Ada's smokehouse door, torn off from the TOP hinge downward. The cured meat is gone; the mules untouched.", "Ada's smokehouse");

            // Refutes Ruth's cold-spot testimony.
            EvidenceProp(root, new Vector3(20f, 0, -14f), new Color(0.3f, 0.42f, 0.5f),
                "The spring channel", "The spring runs in a stone channel under Ruth's springhouse floor. Cold pools here every summer. Just water and stone.", "under Ruth's springhouse");

            // Negative evidence: actively rules out the Fetch.
            EvidenceProp(root, new Vector3(-15f, 0, -24f), new Color(0.25f, 0.35f, 0.45f),
                "The clear pool", "Still water by the creek, clear as glass, and the hand mirror left on the stone shows true. Nothing here fears its own face.", "the creek pool");
        }

        private static GameObject EvidenceProp(Transform root, Vector3 position, Color color,
            string title, string body, string foundAt)
        {
            var prop = GameObject.CreatePrimitive(PrimitiveType.Cube);
            prop.name = $"Evidence_{title.Replace(' ', '_')}";
            prop.transform.SetParent(root);
            prop.transform.position = position + Vector3.up * 0.4f;
            prop.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
            prop.GetComponent<Renderer>().sharedMaterial =
                new Material(Shader.Find("Universal Render Pipeline/Lit")) { color = color };

            var pickup = prop.AddComponent<EvidencePickup>();
            pickup.Configure(title, body, foundAt);
            return prop;
        }

        private static void Box(Transform parent, string name, Vector3 pos, Vector3 scale, Material mat)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent);
            go.transform.position = pos;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = mat;
        }
    }
}
