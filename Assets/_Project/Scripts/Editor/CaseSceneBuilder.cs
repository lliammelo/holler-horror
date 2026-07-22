using System.IO;
using HollerHorror.Clues;
using HollerHorror.Entities;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Yarn.Unity;

namespace HollerHorror.Editor
{
    /// <summary>
    /// Builds the full-case scene: a mini-holler with base camp + clue board,
    /// three homesteads (Ada honest, Tetch withholding, Ruth mistaken), and a
    /// physical evidence set — a Wendigo case soluble end-to-end. The "Night
    /// Hunt" variant (M6) adds the Wendigo, NavMesh, and the escalation clock so
    /// the holler is dangerous while you deduce.
    /// </summary>
    public static class CaseSceneBuilder
    {
        private const string ScenePath = "Assets/_Project/Scenes/Case_Test.unity";
        private const string HuntScenePath = "Assets/_Project/Scenes/CaseHunt_Test.unity";
        private const string YarnProjectPath = "Assets/_Project/Dialogue/HollerHorror.yarnproject";

        [MenuItem("Holler Horror/Build Case Test Scene")]
        public static void BuildCalm() => BuildInternal(withHunt: false, ScenePath);

        [MenuItem("Holler Horror/Build Case (Night Hunt) Test Scene")]
        public static void BuildHunt() => BuildInternal(withHunt: true, HuntScenePath);

        private static void BuildInternal(bool withHunt, string scenePath)
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
            BuildRitualLayer();

            var player = GreyboxSceneBuilder.BuildPlayer(new Vector3(0, 0.05f, -28f));
            player.AddComponent<ClueBoardInteractor>();
            player.AddComponent<FieldJournal>();
            player.AddComponent<HollerHorror.Player.PlayerHealth>(); // BuildPlayer already adds the noise emitter

            if (withHunt)
                BuildHuntLayer();

            Directory.CreateDirectory(Path.GetDirectoryName(scenePath));
            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log($"[CaseSceneBuilder] Built and saved {scenePath} (hunt={withHunt})");
        }

        private static void BuildHuntLayer()
        {
            // NavMesh over the whole holler, baked at runtime.
            var navGo = new GameObject("NavMesh");
            var surface = navGo.AddComponent<NavMeshSurface>();
            surface.collectObjects = CollectObjects.All;
            navGo.AddComponent<NavMeshBootstrap>();

            // The night clock.
            var clockGo = new GameObject("NightClock");
            var clock = clockGo.AddComponent<NightClock>();
            var clockSo = new SerializedObject(clock);
            clockSo.FindProperty("sun").objectReferenceValue = FindDirectionalLight();
            // Test pacing: reach the dangerous phases quickly. Real runs lengthen these (GDD: 30-50 min).
            var durations = clockSo.FindProperty("phaseDurations");
            durations.arraySize = 3;
            durations.GetArrayElementAtIndex(0).floatValue = 40f; // Dusk
            durations.GetArrayElementAtIndex(1).floatValue = 45f; // Early
            durations.GetArrayElementAtIndex(2).floatValue = 45f; // Mid
            clockSo.ApplyModifiedPropertiesWithoutUndo();

            // The Wendigo.
            var wendigo = BuildWendigo(new Vector3(20f, 0f, 30f));

            // Escalation glue.
            var escGo = new GameObject("Escalation");
            var esc = escGo.AddComponent<EscalationController>();
            var escSo = new SerializedObject(esc);
            escSo.FindProperty("wendigo").objectReferenceValue = wendigo;
            escSo.FindProperty("clock").objectReferenceValue = clock;
            escSo.ApplyModifiedPropertiesWithoutUndo();

            // Senses debug so noise rings/audio are visible while hunting.
            var debugGo = new GameObject("SensesDebug");
            debugGo.AddComponent<HollerHorror.Debugging.NoiseDebugRenderer>();
            debugGo.AddComponent<HollerHorror.Debugging.PlaceholderFootstepAudio>();
        }

        private static Light FindDirectionalLight()
        {
            foreach (var light in Object.FindObjectsByType<Light>(FindObjectsInactive.Include))
                if (light.type == LightType.Directional)
                    return light;
            return null;
        }

        private static WendigoController BuildWendigo(Vector3 position)
        {
            var wendigo = new GameObject("Wendigo");
            wendigo.transform.position = position;

            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(wendigo.transform);
            body.transform.localPosition = new Vector3(0, 1.3f, 0);
            body.transform.localScale = new Vector3(0.9f, 1.3f, 0.9f);

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

            var perception = wendigo.AddComponent<HollerHorror.Senses.EntityPerception>();
            var perceptionSo = new SerializedObject(perception);
            perceptionSo.FindProperty("eyeHeight").floatValue = 2.5f;
            perceptionSo.FindProperty("visionRange").floatValue = 30f;
            perceptionSo.FindProperty("visionFovDegrees").floatValue = 140f;
            perceptionSo.FindProperty("hearingSensitivity").floatValue = 1.25f;
            perceptionSo.ApplyModifiedPropertiesWithoutUndo();

            var controller = wendigo.AddComponent<WendigoController>();
            wendigo.AddComponent<WendigoAmbientCalls>();

            var display = wendigo.AddComponent<HollerHorror.Debugging.PerceptionDebugDisplay>();
            var displaySo = new SerializedObject(display);
            displaySo.FindProperty("perception").objectReferenceValue = perception;
            displaySo.FindProperty("body").objectReferenceValue = body.GetComponent<Renderer>();
            displaySo.FindProperty("controlRotation").boolValue = false;
            displaySo.ApplyModifiedPropertiesWithoutUndo();

            return controller;
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

            // Wendigo-supporting evidence — carcass location varies per run. Each
            // spot bundles the carcass AND its first-kill ritual site as siblings,
            // so collecting the carcass (which destroys the prop) leaves the site
            // standing, and the per-run toggle hides the unused pair entirely.
            var spotA = BuildFirstKillSpot(root, new Vector3(-24f, 0, 18f),
                "Stripped carcass", "A hog taken apart to clean bone behind the smokehouse. Nothing wasted, nothing dragged off.", "behind Ada's smokehouse");
            var spotB = BuildFirstKillSpot(root, new Vector3(8f, 0, 32f),
                "Stripped carcass", "A deer stripped to clean bone beside the ridge trail. No scavenger sign - nothing else will touch it.", "the ridge trail");
            var layout = root.gameObject.AddComponent<AlternativeEvidenceSpots>();
            layout.Alternatives = new[] { spotA, spotB };

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

        /// <summary>One first-kill spot: carcass evidence + its ritual site as siblings in a toggleable container.</summary>
        private static GameObject BuildFirstKillSpot(Transform root, Vector3 position,
            string title, string body, string foundAt)
        {
            var container = new GameObject("FirstKillSpot");
            container.transform.SetParent(root);
            container.transform.position = position;

            EvidenceProp(container.transform, position, new Color(0.5f, 0.25f, 0.2f), title, body, foundAt);

            var site = SiteMarker(container.transform, position + new Vector3(1.6f, 0, 1.6f),
                new Color(0.6f, 0.2f, 0.15f));
            site.Kind = HollerHorror.Rituals.RitualSiteKind.FirstKillSite;

            return container;
        }

        /// <summary>Standalone ritual sites + component pickups (M5).</summary>
        private static void BuildRitualLayer()
        {
            var root = new GameObject("Rituals").transform;

            // Crossroads (Fetch) and chapel stone (Hollow).
            SiteMarker(root, new Vector3(0f, 0f, 0f), new Color(0.12f, 0.45f, 0.42f))
                .Kind = HollerHorror.Rituals.RitualSiteKind.Crossroads;
            SiteMarker(root, new Vector3(-8f, 0f, 34f), new Color(0.35f, 0.22f, 0.5f))
                .Kind = HollerHorror.Rituals.RitualSiteKind.ChapelStone;

            // Component pickups.
            ItemProp(root, new Vector3(2f, 0, 8f), new Color(0.7f, 0.65f, 0.5f), "ash_billet");
            ItemProp(root, new Vector3(-11.5f, 0, -20.5f), new Color(0.7f, 0.75f, 0.8f), "mirror");
            ItemProp(root, new Vector3(24.5f, 0, 21f), new Color(0.55f, 0.5f, 0.3f), "lantern_oil");
            // salt comes from Ruth; the name comes from whoever holds the story.
        }

        private static HollerHorror.Rituals.RitualSite SiteMarker(Transform parent, Vector3 position, Color color)
        {
            var marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            marker.name = "RitualSite";
            marker.transform.SetParent(parent);
            marker.transform.position = position + Vector3.up * 0.1f;
            marker.transform.localScale = new Vector3(2.2f, 0.1f, 2.2f);
            Object.DestroyImmediate(marker.GetComponent<Collider>()); // walk-over disc, no collision

            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit")) { color = color };
            EnableEmission(mat, color); // glows so it reads at distance in the fog
            marker.GetComponent<Renderer>().sharedMaterial = mat;

            // Vertical beacon so the site is findable across the greybox valley.
            var beacon = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            beacon.name = "Beacon";
            Object.DestroyImmediate(beacon.GetComponent<Collider>());
            beacon.transform.SetParent(marker.transform, false);
            beacon.transform.localScale = new Vector3(0.06f, 40f, 0.06f); // tall thin shaft
            beacon.transform.localPosition = new Vector3(0f, 40f, 0f);
            beacon.GetComponent<Renderer>().sharedMaterial = mat;

            return marker.AddComponent<HollerHorror.Rituals.RitualSite>();
        }

        private static void EnableEmission(Material mat, Color color)
        {
            mat.EnableKeyword("_EMISSION");
            mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            mat.SetColor("_EmissionColor", color * 1.6f);
        }

        private static void ItemProp(Transform root, Vector3 position, Color color, string itemId)
        {
            var prop = GameObject.CreatePrimitive(PrimitiveType.Cube);
            prop.name = $"Item_{itemId}";
            prop.transform.SetParent(root);
            prop.transform.position = position + Vector3.up * 0.3f;
            prop.transform.localScale = Vector3.one * 0.45f;
            prop.GetComponent<Renderer>().sharedMaterial =
                new Material(Shader.Find("Universal Render Pipeline/Lit")) { color = color };
            prop.AddComponent<HollerHorror.Rituals.ItemPickup>().Configure(itemId);
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
