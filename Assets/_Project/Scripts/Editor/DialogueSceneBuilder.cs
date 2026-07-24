using System.IO;
using HollerHorror.Clues;
using HollerHorror.Dialogue;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Yarn.Unity;

namespace HollerHorror.Editor
{
    /// <summary>
    /// Builds the M3 dialogue slice scene: base camp + clue board, two residents
    /// (Ada, Tetch) with Yarn conversations, and a scored-bark evidence prop that
    /// unlocks their deeper branches. Which one holds the Elias Cole story is
    /// rolled per run by CaseDirector.
    /// </summary>
    public static class DialogueSceneBuilder
    {
        private const string ScenePath = "Assets/_Project/Scenes/Dialogue_Test.unity";
        private const string YarnProjectPath = "Assets/_Project/Dialogue/HollerHorror.yarnproject";

        [MenuItem("Holler Horror/Build Dialogue Test Scene")]
        public static void Build()
        {
            // Flush any pending imports BEFORE creating the scene — loading the Yarn
            // asset earlier hands us an instance that a mid-build reimport destroys,
            // which then serializes as null.
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GreyboxSceneBuilder.BuildLighting();
            ClueBoardSceneBuilder.BuildBaseCampCorner();
            ClueBoardSceneBuilder.BuildBoard(new Vector3(0, 1.55f, 6f));

            var yarnProject = AssetDatabase.LoadAssetAtPath<YarnProject>(YarnProjectPath);
            if (yarnProject == null)
                throw new FileNotFoundException(
                    $"Yarn project not imported at {YarnProjectPath} — let Unity finish importing, then rebuild.");
            string yarnProjectName = yarnProject.name;

            var player = GreyboxSceneBuilder.BuildPlayer(new Vector3(0, 0.05f, 2f));
            player.AddComponent<ClueBoardInteractor>();
            player.AddComponent<FieldJournal>();

            BuildDialogueSystem(yarnProject);
            BuildNpc("Ada Bricker", "Ada_Start", new Vector3(-5f, 0f, 3.5f), new Color(0.55f, 0.35f, 0.30f));
            BuildNpc("Old Man Tetch", "Tetch_Start", new Vector3(7f, 0f, -1f), new Color(0.35f, 0.40f, 0.50f));

            BuildEvidenceProp(new Vector3(2f, 0f, -8.5f));

            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log($"[DialogueSceneBuilder] Built and saved {ScenePath} — wired Yarn project '{yarnProjectName}'");
        }

        internal static GameObject BuildDialogueSystem(YarnProject yarnProject)
        {
            var go = new GameObject("DialogueSystem");
            var runner = go.AddComponent<DialogueRunner>();
            var storage = go.AddComponent<InMemoryVariableStorage>();
            var hud = go.AddComponent<DialogueHud>();
            var service = go.AddComponent<DialogueService>();
            go.AddComponent<CaseDirector>();

            var serviceSo = new SerializedObject(service);
            serviceSo.FindProperty("yarnProject").objectReferenceValue = yarnProject;
            serviceSo.ApplyModifiedPropertiesWithoutUndo();

            var so = new SerializedObject(runner);
            so.FindProperty("yarnProject").objectReferenceValue = yarnProject;
            so.FindProperty("variableStorage").objectReferenceValue = storage;
            var presenters = so.FindProperty("dialoguePresenters");
            presenters.arraySize = 1;
            presenters.GetArrayElementAtIndex(0).objectReferenceValue = hud;
            so.ApplyModifiedPropertiesWithoutUndo();
            return go;
        }

        internal static GameObject BuildNpc(string npcName, string startNode, Vector3 position, Color tint)
        {
            var npc = new GameObject($"NPC_{npcName.Replace(' ', '_')}");
            npc.transform.position = position;

            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(npc.transform);
            body.transform.localPosition = new Vector3(0, 1f, 0);
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit")) { color = tint };
            body.GetComponent<Renderer>().sharedMaterial = mat;

            var labelGo = new GameObject("NameTag");
            labelGo.transform.SetParent(npc.transform);
            labelGo.transform.localPosition = new Vector3(0, 2.35f, 0);
            labelGo.AddComponent<HollerHorror.Presentation.Billboard>(); // always faces the player
            var label = labelGo.AddComponent<TextMesh>();
            label.text = npcName;
            label.characterSize = 0.22f;
            label.fontSize = 32;
            label.anchor = TextAnchor.MiddleCenter;
            label.color = new Color(0.9f, 0.88f, 0.8f);

            var controller = npc.AddComponent<NpcController>();
            controller.NpcName = npcName;
            controller.StartNode = startNode;

            var controllerSo = new SerializedObject(controller);
            controllerSo.FindProperty("bodyRenderer").objectReferenceValue = body.GetComponent<Renderer>();
            controllerSo.ApplyModifiedPropertiesWithoutUndo();
            return npc;
        }

        private static void BuildEvidenceProp(Vector3 position)
        {
            var prop = GameObject.CreatePrimitive(PrimitiveType.Cube);
            prop.name = "Evidence_ScoredBark";
            prop.transform.position = position + Vector3.up * 0.8f;
            prop.transform.localScale = new Vector3(0.5f, 1.6f, 0.5f);
            prop.GetComponent<Renderer>().sharedMaterial =
                new Material(Shader.Find("Universal Render Pipeline/Lit")) { color = new Color(0.35f, 0.28f, 0.18f) };

            var pickup = prop.AddComponent<EvidencePickup>();
            pickup.Configure("Claw-scored bark",
                "Ash tree scored to bare wood, head height and higher. Whatever reaches that high walks upright.",
                "the ash line");
        }
    }
}
