using System.IO;
using HollerHorror.Clues;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace HollerHorror.Editor
{
    /// <summary>
    /// Builds the M2 (v2) clue board slice scene: a base-camp corner at dusk with
    /// the physical board, a player, and the field journal. Local-only board —
    /// the synced version lives in the Netcode test scene.
    /// </summary>
    public static class ClueBoardSceneBuilder
    {
        private const string ScenePath = "Assets/_Project/Scenes/ClueBoard_Test.unity";

        [MenuItem("Holler Horror/Build Clue Board Test Scene")]
        public static void Build()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GreyboxSceneBuilder.BuildLighting();
            BuildBaseCampCorner();
            BuildBoard(new Vector3(0, 1.55f, 6f));

            var player = GreyboxSceneBuilder.BuildPlayer(new Vector3(0, 0.05f, 2f));
            player.AddComponent<ClueBoardInteractor>();
            player.AddComponent<FieldJournal>();

            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log($"[ClueBoardSceneBuilder] Built and saved {ScenePath}");
        }

        internal static void BuildBaseCampCorner()
        {
            var root = new GameObject("BaseCamp").transform;
            var wood = new Material(Shader.Find("Universal Render Pipeline/Lit"))
            { color = new Color(0.32f, 0.24f, 0.16f) };

            Box(root, "Ground", new Vector3(0, -0.5f, 0), new Vector3(24, 1, 24), wood, new Color(0.28f, 0.30f, 0.26f));
            Box(root, "BackWall", new Vector3(0, 1.6f, 6.2f), new Vector3(6, 3.2f, 0.3f), wood, null);
            Box(root, "SideWall", new Vector3(-3.1f, 1.6f, 4.7f), new Vector3(0.3f, 3.2f, 3.2f), wood, null);
            Box(root, "Table", new Vector3(1.8f, 0.45f, 5.3f), new Vector3(1.6f, 0.9f, 0.7f), wood, null);

            // Warm lantern light over the board.
            var lanternGo = new GameObject("Lantern");
            lanternGo.transform.position = new Vector3(0, 2.6f, 4.6f);
            var lantern = lanternGo.AddComponent<Light>();
            lantern.type = LightType.Point;
            lantern.color = new Color(1f, 0.75f, 0.45f);
            lantern.intensity = 2.2f;
            lantern.range = 9f;
        }

        private static void Box(Transform parent, string name, Vector3 pos, Vector3 scale, Material mat, Color? colorOverride)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent);
            go.transform.position = pos;
            go.transform.localScale = scale;
            if (colorOverride.HasValue)
            {
                var instanced = new Material(mat) { color = colorOverride.Value };
                go.GetComponent<Renderer>().sharedMaterial = instanced;
            }
            else
            {
                go.GetComponent<Renderer>().sharedMaterial = mat;
            }
        }

        internal static GameObject BuildBoard(Vector3 position, bool networked = false)
        {
            var boardGo = new GameObject("ClueBoard");
            boardGo.transform.position = position;
            // Viewer side is the board's -Z; identity rotation faces world -Z (player approach side).
            boardGo.AddComponent<ClueBoard>();
            boardGo.AddComponent<ClueBoardView>();
            if (networked)
            {
                boardGo.AddComponent<Unity.Netcode.NetworkObject>();
                boardGo.AddComponent<ClueBoardNetSync>();
            }
            return boardGo;
        }
    }
}
