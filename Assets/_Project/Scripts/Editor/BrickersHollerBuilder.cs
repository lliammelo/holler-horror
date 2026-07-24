using System.IO;
using HollerHorror.Clues;
using HollerHorror.Dialogue;
using HollerHorror.Entities;
using HollerHorror.Rituals;
using HollerHorror.World;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Yarn.Unity;

namespace HollerHorror.Editor
{
    /// <summary>
    /// Bricker's Holler — the one handcrafted valley (GDD §8). Fixed geography,
    /// randomized truth: the entity, its cast and the evidence are rolled at run
    /// start by CaseGenerator, so players can never read the answer off the map.
    /// Replaces the three per-entity test scenes as the real playable map.
    /// </summary>
    public static class BrickersHollerBuilder
    {
        private const string ScenePath = "Assets/_Project/Scenes/BrickersHoller.unity";
        private const string YarnProjectPath = "Assets/_Project/Dialogue/HollerHorror.yarnproject";

        private static Material ground, timber, stone, foliage, water;

        [MenuItem("Holler Horror/Build Bricker's Holler (Main Map)")]
        public static void Build()
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            MakeMaterials();
            GreyboxSceneBuilder.BuildLighting();
            BuildValleyFloor();
            BuildCreek();

            // --- Landmarks. Fixed forever; contents rolled per run. ---
            var camp = MakePoi(PoiKind.BaseCamp, new Vector3(0, 0, -70));
            var crossroads = MakePoi(PoiKind.Crossroads, new Vector3(0, 0, -22));
            var farmstead = MakePoi(PoiKind.Farmstead, new Vector3(-48, 0, -26));
            var chapel = MakePoi(PoiKind.Chapel, new Vector3(-42, 0, 46));
            var mine = MakePoi(PoiKind.MineMouth, new Vector3(46, 0, 52));
            var cabins = MakePoi(PoiKind.RidgeCabins, new Vector3(52, 0, -12));

            BuildBaseCamp(camp);
            BuildCrossroads(crossroads);
            BuildFarmstead(farmstead);
            BuildChapel(chapel);
            BuildMineMouth(mine);
            BuildRidgeCabins(cabins);
            BuildThickets();

            BuildRitualSites(farmstead, crossroads, chapel);

            // --- Always-on systems ---
            var yarnProject = AssetDatabase.LoadAssetAtPath<YarnProject>(YarnProjectPath);
            if (yarnProject == null)
                throw new FileNotFoundException($"Yarn project not imported at {YarnProjectPath}.");
            var dialogueGo = DialogueSceneBuilder.BuildDialogueSystem(yarnProject);
            var director = dialogueGo.GetComponent<CaseDirector>();

            var navGo = new GameObject("NavMesh");
            var surface = navGo.AddComponent<NavMeshSurface>();
            surface.collectObjects = CollectObjects.All;
            surface.useGeometry = UnityEngine.AI.NavMeshCollectGeometry.PhysicsColliders;
            navGo.AddComponent<NavMeshBootstrap>();

            var clockGo = new GameObject("NightClock");
            var clock = clockGo.AddComponent<NightClock>();
            var clockSo = new SerializedObject(clock);
            clockSo.FindProperty("sun").objectReferenceValue = FindSun();
            clockSo.ApplyModifiedPropertiesWithoutUndo();

            // --- The three possible truths, pre-built and disabled ---
            var wendigoSet = BuildWendigoSet(clock, farmstead, cabins);
            var fetchSet = BuildFetchSet(farmstead, cabins);
            var hollowSet = BuildHollowSet(farmstead, cabins, chapel);
            wendigoSet.SetActive(false);
            fetchSet.SetActive(false);
            hollowSet.SetActive(false);

            var generatorGo = new GameObject("CaseGenerator");
            var generator = generatorGo.AddComponent<CaseGenerator>();
            var genSo = new SerializedObject(generator);
            genSo.FindProperty("director").objectReferenceValue = director;
            genSo.FindProperty("wendigoSet").objectReferenceValue = wendigoSet;
            genSo.FindProperty("fetchSet").objectReferenceValue = fetchSet;
            genSo.FindProperty("hollowSet").objectReferenceValue = hollowSet;
            genSo.ApplyModifiedPropertiesWithoutUndo();

            // The broader systemic cast: always present, knowledge dealt per run.
            BuildResidents(director);

            // --- Player + presentation ---
            var player = GreyboxSceneBuilder.BuildPlayer(new Vector3(0, 0.05f, -76f));
            player.AddComponent<ClueBoardInteractor>();
            player.AddComponent<FieldJournal>();
            player.AddComponent<EntityJournal>();
            player.AddComponent<HollerHorror.Player.PlayerHealth>();
            player.AddComponent<HollerHorror.Player.PlayerSanity>();

            var presentation = new GameObject("Presentation");
            presentation.AddComponent<HollerHorror.Presentation.AtmosphereController>();
            presentation.AddComponent<HollerHorror.Presentation.AmbientAudioDirector>();
            presentation.AddComponent<HollerHorror.UI.PauseController>();
            presentation.AddComponent<HollerHorror.Debugging.NoiseDebugRenderer>();
            presentation.AddComponent<HollerHorror.Debugging.PlaceholderFootstepAudio>();

            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
            EditorSceneManager.SaveScene(scene, ScenePath);
            RegisterScene();
            Debug.Log($"[BrickersHoller] Built {ScenePath}. Entity rolls at run start.");
        }

        // ---------- terrain ----------

        private static void MakeMaterials()
        {
            ground = Mat("Holler_Ground", new Color(0.26f, 0.28f, 0.24f));
            timber = Mat("Holler_Timber", new Color(0.30f, 0.22f, 0.15f));
            stone = Mat("Holler_Stone", new Color(0.32f, 0.32f, 0.34f));
            foliage = Mat("Holler_Foliage", new Color(0.16f, 0.24f, 0.15f));
            water = Mat("Holler_Water", new Color(0.18f, 0.30f, 0.42f));
        }

        private static Material Mat(string name, Color color)
        {
            const string folder = "Assets/_Project/Materials/Holler";
            Directory.CreateDirectory(folder);
            string path = $"{folder}/{name}.mat";
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null) { existing.color = color; return existing; }
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit")) { color = color };
            AssetDatabase.CreateAsset(mat, path);
            return mat;
        }

        private static void BuildValleyFloor()
        {
            var root = new GameObject("Valley").transform;
            Box(root, "Floor", new Vector3(0, -0.5f, 0), new Vector3(180, 1, 180), ground);

            // Valley walls — steep ridges boxing the holler in.
            Box(root, "Ridge_W", new Vector3(-92, 8, 0), new Vector3(8, 18, 184), stone);
            Box(root, "Ridge_E", new Vector3(92, 8, 0), new Vector3(8, 18, 184), stone);
            Box(root, "Ridge_N", new Vector3(0, 8, 92), new Vector3(184, 18, 8), stone);
            Box(root, "Ridge_S", new Vector3(0, 8, -92), new Vector3(184, 18, 8), stone);
        }

        private static void BuildCreek()
        {
            var creek = new GameObject("Creek");
            creek.transform.position = new Vector3(0f, 0.05f, 10f);
            var box = creek.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = new Vector3(180f, 2f, 7f);
            creek.AddComponent<RunningWater>();

            var visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.name = "Water";
            Object.DestroyImmediate(visual.GetComponent<Collider>());
            visual.transform.SetParent(creek.transform, false);
            visual.transform.localScale = new Vector3(180f, 0.06f, 6.5f);
            visual.GetComponent<Renderer>().sharedMaterial = water;

            // The creek is a POI in its own right (Fetch evidence lives here).
            var poi = creek.AddComponent<PointOfInterest>();
            poi.Kind = PoiKind.Creek;
            AddAnchors(poi, creek.transform, new[]
            {
                new Vector3(-24, 0, -5), new Vector3(8, 0, -5), new Vector3(30, 0, 5),
            });
        }

        // ---------- POIs ----------

        private static PointOfInterest MakePoi(PoiKind kind, Vector3 position)
        {
            var go = new GameObject($"POI_{kind}");
            go.transform.position = position;
            var poi = go.AddComponent<PointOfInterest>();
            poi.Kind = kind;
            return poi;
        }

        private static void AddAnchors(PointOfInterest poi, Transform parent, Vector3[] localOffsets, bool npc = false)
        {
            for (int i = 0; i < localOffsets.Length; i++)
            {
                var anchor = new GameObject($"{(npc ? "NpcAnchor" : "EvidenceAnchor")}_{i}").transform;
                anchor.SetParent(parent, false);
                anchor.localPosition = localOffsets[i];
                if (npc) poi.AddNpcAnchor(anchor);
                else poi.AddEvidenceAnchor(anchor);
            }
        }

        private static void BuildBaseCamp(PointOfInterest poi)
        {
            var root = poi.transform;
            Box(root, "CampWall", new Vector3(0, 1.6f, 4f), new Vector3(8, 3.2f, 0.3f), timber);
            Box(root, "CampTable", new Vector3(3f, 0.45f, 2.4f), new Vector3(1.8f, 0.9f, 0.8f), timber);
            Lantern(root, new Vector3(0, 2.6f, 2f), 2.4f, 12f);

            var board = ClueBoardSceneBuilder.BuildBoard(poi.transform.position + new Vector3(0, 1.55f, 3.4f));
            board.transform.rotation = Quaternion.Euler(0, 180f, 0);
            board.transform.SetParent(root);
        }

        private static void BuildCrossroads(PointOfInterest poi)
        {
            var root = poi.transform;
            Box(root, "RoadNS", new Vector3(0, 0.03f, 0), new Vector3(6, 0.06f, 60), ground);
            Box(root, "RoadEW", new Vector3(0, 0.03f, 0), new Vector3(60, 0.06f, 6), ground);
            Box(root, "Signpost", new Vector3(2.5f, 1.4f, 2.5f), new Vector3(0.2f, 2.8f, 0.2f), timber);
            Box(root, "SignArm", new Vector3(3.2f, 2.4f, 2.5f), new Vector3(1.6f, 0.3f, 0.1f), timber);
            AddAnchors(poi, root, new[] { new Vector3(-4, 0, 4), new Vector3(5, 0, -4) });
        }

        private static void BuildFarmstead(PointOfInterest poi)
        {
            var root = poi.transform;
            Box(root, "Floor", new Vector3(0, 0.06f, 0), new Vector3(10, 0.12f, 8), timber);
            Box(root, "BackWall", new Vector3(0, 1.8f, 4f), new Vector3(10, 3.6f, 0.3f), timber);
            Box(root, "SideWall", new Vector3(-5f, 1.8f, 0), new Vector3(0.3f, 3.6f, 8), timber);
            Box(root, "Roof", new Vector3(0, 3.7f, 0), new Vector3(11, 0.2f, 9), stone);
            Box(root, "Smokehouse", new Vector3(9f, 1.2f, -3f), new Vector3(4, 2.4f, 4), timber);
            Box(root, "Barn", new Vector3(-11f, 2f, -5f), new Vector3(8, 4f, 7), timber);
            Lantern(root, new Vector3(0, 2.8f, 0), 1.8f, 10f);

            AddAnchors(poi, root, new[]
            {
                new Vector3(9f, 0, -6f), new Vector3(-11f, 0, -9f),
                new Vector3(3f, 0, -5f), new Vector3(-6f, 0, 2f),
            });
            AddAnchors(poi, root, new[] { new Vector3(1.5f, 0.12f, 1f), new Vector3(-2.5f, 0.12f, 1f) }, npc: true);
        }

        private static void BuildChapel(PointOfInterest poi)
        {
            var root = poi.transform;
            Box(root, "Floor", new Vector3(0, 0.06f, 0), new Vector3(9, 0.12f, 12), stone);
            Box(root, "BackWall", new Vector3(0, 2.2f, 6f), new Vector3(9, 4.4f, 0.4f), stone);
            Box(root, "WallL", new Vector3(-4.5f, 2.2f, 0), new Vector3(0.4f, 4.4f, 12), stone);
            Box(root, "WallR", new Vector3(4.5f, 2.2f, 0), new Vector3(0.4f, 4.4f, 12), stone);
            Box(root, "Steeple", new Vector3(0, 6f, 5f), new Vector3(2, 4f, 2), stone);
            Box(root, "Altar", new Vector3(0, 0.6f, 4.5f), new Vector3(2, 1.2f, 0.9f), stone);
            Lantern(root, new Vector3(0, 2.4f, 3.5f), 1.4f, 9f);

            AddAnchors(poi, root, new[] { new Vector3(0, 0.12f, 2f), new Vector3(-3f, 0.12f, -3f) });
            AddAnchors(poi, root, new[] { new Vector3(2.5f, 0.12f, 0f) }, npc: true);
        }

        private static void BuildMineMouth(PointOfInterest poi)
        {
            var root = poi.transform;
            Box(root, "Face", new Vector3(0, 5f, 6f), new Vector3(24, 10f, 6), stone);
            Box(root, "Adit", new Vector3(0, 1.5f, 2.6f), new Vector3(4, 3f, 4), foliage); // dark opening
            Box(root, "BeamL", new Vector3(-2.2f, 1.6f, 0.6f), new Vector3(0.4f, 3.2f, 0.4f), timber);
            Box(root, "BeamR", new Vector3(2.2f, 1.6f, 0.6f), new Vector3(0.4f, 3.2f, 0.4f), timber);
            Box(root, "Lintel", new Vector3(0, 3.3f, 0.6f), new Vector3(5, 0.4f, 0.4f), timber);
            Box(root, "Cart", new Vector3(-5f, 0.5f, -3f), new Vector3(1.6f, 1f, 2.6f), stone);

            AddAnchors(poi, root, new[] { new Vector3(-5f, 0, -5f), new Vector3(4f, 0, -3f), new Vector3(0, 0, -8f) });
        }

        private static void BuildRidgeCabins(PointOfInterest poi)
        {
            var root = poi.transform;
            for (int i = 0; i < 3; i++)
            {
                float x = (i - 1) * 12f;
                float z = i % 2 == 0 ? 0f : 7f;
                Box(root, $"Cabin{i}_Floor", new Vector3(x, 0.06f, z), new Vector3(7, 0.12f, 6), timber);
                Box(root, $"Cabin{i}_Back", new Vector3(x, 1.6f, z + 3f), new Vector3(7, 3.2f, 0.3f), timber);
                Box(root, $"Cabin{i}_Side", new Vector3(x - 3.5f, 1.6f, z), new Vector3(0.3f, 3.2f, 6), timber);
                Box(root, $"Cabin{i}_Roof", new Vector3(x, 3.3f, z), new Vector3(8, 0.2f, 7), stone);
            }
            Lantern(root, new Vector3(0, 2.6f, 2f), 1.6f, 9f);

            AddAnchors(poi, root, new[]
            {
                new Vector3(-12f, 0, -4f), new Vector3(0, 0, -4f), new Vector3(12f, 0, -4f), new Vector3(6f, 0, 9f),
            });
            AddAnchors(poi, root, new[] { new Vector3(-11f, 0.12f, 1f), new Vector3(1f, 0.12f, 8f) }, npc: true);
        }

        private static void BuildThickets()
        {
            var rng = new System.Random(9);
            Vector3[] centres =
            {
                new(-20, 0, -50), new(25, 0, -45), new(-60, 0, 10), new(20, 0, 30),
                new(60, 0, 25), new(-15, 0, 60), new(70, 0, -55),
            };

            for (int c = 0; c < centres.Length; c++)
            {
                var poi = MakePoi(PoiKind.Thicket, centres[c]);
                var root = poi.transform;
                int stalks = 18 + rng.Next(10);
                for (int i = 0; i < stalks; i++)
                {
                    float angle = (float)(rng.NextDouble() * Mathf.PI * 2);
                    float dist = (float)(rng.NextDouble() * 6.0);
                    var stalk = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    stalk.name = "Laurel";
                    stalk.transform.SetParent(root);
                    stalk.transform.localPosition = new Vector3(Mathf.Cos(angle) * dist, 1.4f, Mathf.Sin(angle) * dist);
                    stalk.transform.localScale = new Vector3(
                        0.3f + (float)rng.NextDouble() * 0.3f, 2.8f, 0.3f + (float)rng.NextDouble() * 0.3f);
                    stalk.transform.localRotation = Quaternion.Euler(0, (float)rng.NextDouble() * 360f, 0);
                    stalk.GetComponent<Renderer>().sharedMaterial = foliage;
                }
                AddAnchors(poi, root, new[] { new Vector3(0, 0, -7.5f) });
            }
        }

        // ---------- rituals ----------

        private static void BuildRitualSites(PointOfInterest farmstead, PointOfInterest crossroads, PointOfInterest chapel)
        {
            var root = new GameObject("RitualSites").transform;
            Site(root, farmstead.transform.position + new Vector3(12f, 0, -8f),
                new Color(0.6f, 0.2f, 0.15f), RitualSiteKind.FirstKillSite);
            Site(root, crossroads.transform.position, new Color(0.12f, 0.45f, 0.42f), RitualSiteKind.Crossroads);
            Site(root, chapel.transform.position + new Vector3(0, 0, 3f),
                new Color(0.35f, 0.22f, 0.5f), RitualSiteKind.ChapelStone);

            // Components scattered across the valley.
            Item(root, crossroads.transform.position + new Vector3(-8f, 0, 8f), new Color(0.7f, 0.65f, 0.5f), "ash_billet");
            Item(root, new Vector3(-30f, 0, -6f), new Color(0.7f, 0.75f, 0.8f), "mirror");
            Item(root, chapel.transform.position + new Vector3(6f, 0, -6f), new Color(0.55f, 0.5f, 0.3f), "lantern_oil");
        }

        private static void Site(Transform parent, Vector3 position, Color color, RitualSiteKind kind)
        {
            var marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            marker.name = $"RitualSite_{kind}";
            marker.transform.SetParent(parent);
            marker.transform.position = position + Vector3.up * 0.1f;
            marker.transform.localScale = new Vector3(2.4f, 0.1f, 2.4f);
            Object.DestroyImmediate(marker.GetComponent<Collider>());

            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit")) { color = color };
            mat.EnableKeyword("_EMISSION");
            mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            mat.SetColor("_EmissionColor", color * 1.6f);
            marker.GetComponent<Renderer>().sharedMaterial = mat;

            var beacon = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            beacon.name = "Beacon";
            Object.DestroyImmediate(beacon.GetComponent<Collider>());
            beacon.transform.SetParent(marker.transform, false);
            beacon.transform.localScale = new Vector3(0.06f, 40f, 0.06f);
            beacon.transform.localPosition = new Vector3(0, 40f, 0);
            beacon.GetComponent<Renderer>().sharedMaterial = mat;

            marker.AddComponent<RitualSite>().Kind = kind;
        }

        private static void Item(Transform parent, Vector3 position, Color color, string itemId)
        {
            var prop = GameObject.CreatePrimitive(PrimitiveType.Cube);
            prop.name = $"Item_{itemId}";
            prop.transform.SetParent(parent);
            prop.transform.position = position + Vector3.up * 0.3f;
            prop.transform.localScale = Vector3.one * 0.45f;
            prop.GetComponent<Renderer>().sharedMaterial =
                new Material(Shader.Find("Universal Render Pipeline/Lit")) { color = color };
            prop.AddComponent<ItemPickup>().Configure(itemId);
        }

        // ---------- the three truths ----------

        private static GameObject BuildWendigoSet(NightClock clock, PointOfInterest farmstead, PointOfInterest cabins)
        {
            var set = new GameObject("Set_Wendigo");

            var wendigo = CaseSceneBuilder.BuildWendigo(new Vector3(40f, 0f, 70f));
            wendigo.transform.SetParent(set.transform);

            var esc = set.AddComponent<EscalationController>();
            var escSo = new SerializedObject(esc);
            escSo.FindProperty("wendigo").objectReferenceValue = wendigo;
            escSo.FindProperty("clock").objectReferenceValue = clock;
            escSo.ApplyModifiedPropertiesWithoutUndo();

            Npc(set, "Ada Bricker", "Ada_Start", farmstead, 0, new Color(0.55f, 0.35f, 0.30f));
            Npc(set, "Old Man Tetch", "Tetch_Start", cabins, 0, new Color(0.35f, 0.40f, 0.50f));
            Npc(set, "Ruth Combs", "Ruth_Start", cabins, 1, new Color(0.45f, 0.45f, 0.30f));

            Resolver(set, wendigo);
            return set;
        }

        private static GameObject BuildFetchSet(PointOfInterest farmstead, PointOfInterest cabins)
        {
            var set = new GameObject("Set_Fetch");

            var fetch = CaseSceneBuilder.BuildFetchEntity(new Vector3(10f, 0f, 45f));
            fetch.transform.SetParent(set.transform);

            Npc(set, "Ruth Combs", "RuthReal_Start", farmstead, 0, new Color(0.45f, 0.45f, 0.30f));
            Npc(set, "Ada Bricker", "AdaFetch_Start", cabins, 0, new Color(0.55f, 0.35f, 0.30f));

            // The double, stranded north of the creek where it cannot cross back.
            var doubleNpc = DialogueSceneBuilder.BuildNpc("Ruth Combs", "RuthFetch_Start",
                new Vector3(-14f, 0.12f, 30f), new Color(0.45f, 0.45f, 0.30f));
            doubleNpc.transform.SetParent(set.transform);

            Resolver(set, fetch);
            return set;
        }

        private static GameObject BuildHollowSet(PointOfInterest farmstead, PointOfInterest cabins, PointOfInterest chapel)
        {
            var set = new GameObject("Set_Hollow");

            var hollowGo = new GameObject("Hollow");
            hollowGo.transform.SetParent(set.transform);
            var hollow = hollowGo.AddComponent<HollowController>();

            Npc(set, "Granny Slone", "Granny_Start", chapel, 0, new Color(0.5f, 0.45f, 0.4f));
            Npc(set, "Harlan Boggs", "HollowHarlan_Start", farmstead, 1, new Color(0.4f, 0.42f, 0.3f));

            Resolver(set, hollow);
            return set;
        }

        private static readonly string[] ResidentNames =
        {
            "Widow Combs", "Harlan Boggs", "Cy Tetch", "Nan Slone",
            "Elder Pike", "Ruth Ann", "Josiah Cole", "Merle Bricker",
        };

        /// <summary>
        /// Systemic residents at spare POI anchors, plus the ResidentDirector that
        /// deals their reliability + knowledge each run. Additive to the authored
        /// Yarn keystones in the entity sets.
        /// </summary>
        // Their own spots near POIs, clear of the authored NPCs' anchors.
        private static readonly Vector3[] ResidentSpots =
        {
            new(-44, 0, -30), new(48, 0, -6), new(56, 0, 6),
            new(-38, 0, 44), new(42, 0, 48), new(4, 0, -18),
        };

        private static void BuildResidents(CaseDirector director)
        {
            var root = new GameObject("Residents").transform;
            var rng = new System.Random(5);

            for (int i = 0; i < ResidentSpots.Length && i < ResidentNames.Length; i++)
            {
                string name = ResidentNames[i];

                var go = new GameObject($"Resident_{name.Replace(' ', '_')}");
                go.transform.SetParent(root);
                go.transform.position = ResidentSpots[i] + new Vector3(0, 0.05f, 0);

                var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                body.name = "Body";
                body.transform.SetParent(go.transform);
                body.transform.localPosition = new Vector3(0, 1f, 0);
                body.GetComponent<Renderer>().sharedMaterial =
                    Mat($"Resident_{name.Replace(' ', '_')}",
                        new Color(0.3f + (float)rng.NextDouble() * 0.25f, 0.3f + (float)rng.NextDouble() * 0.2f, 0.3f));

                var tag = new GameObject("NameTag");
                tag.transform.SetParent(go.transform);
                tag.transform.localPosition = new Vector3(0, 2.35f, 0);
                tag.AddComponent<HollerHorror.Presentation.Billboard>();
                var text = tag.AddComponent<TextMesh>();
                text.text = name;
                text.characterSize = 0.2f;
                text.fontSize = 32;
                text.anchor = TextAnchor.MiddleCenter;
                text.color = new Color(0.9f, 0.88f, 0.8f);

                go.AddComponent<Resident>().ResidentName = name;
            }

            var dirGo = new GameObject("ResidentDirector");
            var resDir = dirGo.AddComponent<ResidentDirector>();
            var so = new SerializedObject(resDir);
            so.FindProperty("director").objectReferenceValue = director;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void Npc(GameObject set, string name, string node, PointOfInterest poi, int anchorIndex, Color tint)
        {
            Vector3 pos = anchorIndex < poi.NpcAnchors.Count
                ? poi.NpcAnchors[anchorIndex].position
                : poi.transform.position + Vector3.right * (anchorIndex * 2f);
            var npc = DialogueSceneBuilder.BuildNpc(name, node, pos, tint);
            npc.transform.SetParent(set.transform);
        }

        private static void Resolver(GameObject set, MonoBehaviour entity)
        {
            var resolver = set.AddComponent<CaseResolver>();
            var so = new SerializedObject(resolver);
            so.FindProperty("entityBehaviour").objectReferenceValue = entity;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // ---------- helpers ----------

        private static Light FindSun()
        {
            foreach (var light in Object.FindObjectsByType<Light>(FindObjectsInactive.Include))
                if (light.type == LightType.Directional)
                    return light;
            return null;
        }

        private static void Lantern(Transform parent, Vector3 localPos, float intensity, float range)
        {
            var go = new GameObject("Lantern");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            var light = go.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.72f, 0.42f);
            light.intensity = intensity;
            light.range = range;
        }

        private static GameObject Box(Transform parent, string name, Vector3 localPos, Vector3 scale, Material mat)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent);
            go.transform.localPosition = localPos;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = mat;
            return go;
        }

        private static void RegisterScene()
        {
            foreach (var s in EditorBuildSettings.scenes)
                if (s.path == ScenePath)
                    return;
            var list = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes)
            {
                new EditorBuildSettingsScene(ScenePath, true)
            };
            EditorBuildSettings.scenes = list.ToArray();
        }
    }
}
