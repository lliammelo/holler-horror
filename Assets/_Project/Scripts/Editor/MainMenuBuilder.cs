using System.IO;
using HollerHorror.Presentation;
using HollerHorror.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace HollerHorror.Editor
{
    /// <summary>
    /// Builds the M9 main menu: the watching-eye concept art as backdrop, a title,
    /// and buttons into the case scenes. Copies the art out of Docs/ArtBible into
    /// the project and imports it as a sprite. Sets the menu as build scene 0.
    /// </summary>
    public static class MainMenuBuilder
    {
        private const string ScenePath = "Assets/_Project/Scenes/MainMenu.unity";
        private const string ArtDestDir = "Assets/_Project/Art/Menu";
        private const string ArtDest = ArtDestDir + "/MainMenu.png";
        private const string ArtSource = "Docs/ArtBible/MainMenu.png";
        private const string MusicAsset = "Assets/_Project/Audio/Music/Shepherd_Instrumental.mp3";

        [MenuItem("Holler Horror/Build Main Menu Scene")]
        public static void Build()
        {
            Sprite bg = ImportBackground();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camGo = new GameObject("MenuCamera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
            camGo.AddComponent<AudioListener>();

            var font = (Font)Resources.GetBuiltinResource(typeof(Font), "LegacyRuntime.ttf");

            var canvasGo = new GameObject("MenuCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            // Full-bleed background.
            var bgGo = new GameObject("Background", typeof(Image));
            bgGo.transform.SetParent(canvasGo.transform, false);
            var bgImage = bgGo.GetComponent<Image>();
            bgImage.sprite = bg;
            bgImage.preserveAspect = false;
            Stretch(bgImage.rectTransform);

            // Title, upper-left (the art reserves that corner).
            var title = MakeText(canvasGo.transform, "Title", "HOLLER\nHORROR", font, 84, FontStyle.Bold,
                TextAnchor.UpperLeft, new Vector2(0, 1), new Vector2(80, -70), new Vector2(760, 240));
            title.color = new Color(0.92f, 0.90f, 0.86f);

            // Button column, lower-left over the dark forest.
            float y = -300f;
            var newBtn = MakeButton(canvasGo.transform, font, "New Investigation", new Vector2(80, y)); y -= 70;
            var wendigoBtn = MakeButton(canvasGo.transform, font, "Case: The Wendigo", new Vector2(80, y)); y -= 60;
            var fetchBtn = MakeButton(canvasGo.transform, font, "Case: The Fetch", new Vector2(80, y)); y -= 60;
            var hollowBtn = MakeButton(canvasGo.transform, font, "Case: The Hollow", new Vector2(80, y)); y -= 80;
            var quitBtn = MakeButton(canvasGo.transform, font, "Quit", new Vector2(80, y));

            // Event system (new Input System module).
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));

            // Controller wiring.
            var controllerGo = new GameObject("MainMenu");
            var controller = controllerGo.AddComponent<MainMenuController>();
            var so = new SerializedObject(controller);
            so.FindProperty("newInvestigationButton").objectReferenceValue = newBtn;
            so.FindProperty("wendigoButton").objectReferenceValue = wendigoBtn;
            so.FindProperty("fetchButton").objectReferenceValue = fetchBtn;
            so.FindProperty("hollowButton").objectReferenceValue = hollowBtn;
            so.FindProperty("quitButton").objectReferenceValue = quitBtn;
            so.ApplyModifiedPropertiesWithoutUndo();

            // Licensed menu music ("Shepherd" — in-game use only; trailers separate).
            var music = AssetDatabase.LoadAssetAtPath<AudioClip>(MusicAsset);
            if (music != null)
            {
                var musicGo = new GameObject("Music");
                var player = musicGo.AddComponent<MusicPlayer>();
                var musicSo = new SerializedObject(player);
                musicSo.FindProperty("track").objectReferenceValue = music;
                musicSo.ApplyModifiedPropertiesWithoutUndo();
            }
            else
            {
                Debug.LogWarning($"[MainMenuBuilder] No music clip at {MusicAsset} — menu will be silent but for wind.");
            }

            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
            EditorSceneManager.SaveScene(scene, ScenePath);
            RegisterBuildScenes();
            Debug.Log($"[MainMenuBuilder] Built {ScenePath} and set it as build scene 0.");
        }

        private static Sprite ImportBackground()
        {
            string projectRoot = Directory.GetParent(Application.dataPath)!.FullName;
            string srcFull = Path.Combine(projectRoot, ArtSource.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(srcFull))
                throw new FileNotFoundException($"Menu art not found at {ArtSource} — put MainMenu.png in Docs/ArtBible.");

            Directory.CreateDirectory(ArtDestDir);
            File.Copy(srcFull, Path.Combine(projectRoot, ArtDest.Replace('/', Path.DirectorySeparatorChar)), true);
            AssetDatabase.ImportAsset(ArtDest, ImportAssetOptions.ForceUpdate);

            var importer = (TextureImporter)AssetImporter.GetAtPath(ArtDest);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single; // Multiple has no loadable sprite at the path
            importer.mipmapEnabled = false;
            importer.maxTextureSize = 2048;
            importer.SaveAndReimport();

            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(ArtDest);
            if (sprite == null)
                Debug.LogError($"[MainMenuBuilder] Sprite still null after import at {ArtDest}.");
            return sprite;
        }

        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static Text MakeText(Transform parent, string name, string content, Font font, int size,
            FontStyle style, TextAnchor anchor, Vector2 pivotAnchor, Vector2 anchoredPos, Vector2 sizeDelta)
        {
            var go = new GameObject(name, typeof(Text));
            go.transform.SetParent(parent, false);
            var text = go.GetComponent<Text>();
            text.font = font;
            text.text = content;
            text.fontSize = size;
            text.fontStyle = style;
            text.alignment = anchor;
            text.color = Color.white;
            var rt = text.rectTransform;
            rt.anchorMin = rt.anchorMax = pivotAnchor;
            rt.pivot = pivotAnchor;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;
            return text;
        }

        private static Button MakeButton(Transform parent, Font font, string label, Vector2 anchoredPos)
        {
            var go = new GameObject($"Button_{label}", typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            image.color = new Color(0f, 0f, 0f, 0.35f);
            var rt = image.rectTransform;
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = new Vector2(420, 56);

            var button = go.GetComponent<Button>();
            var colors = button.colors;
            colors.highlightedColor = new Color(0.8f, 0.2f, 0.15f, 0.7f); // yarn-red hover
            colors.pressedColor = new Color(0.6f, 0.15f, 0.1f, 0.8f);
            button.colors = colors;

            var text = MakeText(go.transform, "Label", label, font, 28, FontStyle.Normal,
                TextAnchor.MiddleLeft, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(400, 56));
            text.rectTransform.anchoredPosition = new Vector2(20, 0);
            text.color = new Color(0.9f, 0.88f, 0.84f);
            return button;
        }

        private static void RegisterBuildScenes()
        {
            var wanted = new[]
            {
                ScenePath,
                "Assets/_Project/Scenes/BrickersHoller.unity",
                "Assets/_Project/Scenes/CaseHunt_Test.unity",
                "Assets/_Project/Scenes/FetchCase_Test.unity",
                "Assets/_Project/Scenes/HollowCase_Test.unity",
            };

            var list = new System.Collections.Generic.List<EditorBuildSettingsScene>();
            foreach (var path in wanted)
                if (File.Exists(path))
                    list.Add(new EditorBuildSettingsScene(path, true));

            // Keep any other already-registered scenes after the menu set.
            foreach (var existing in EditorBuildSettings.scenes)
                if (System.Array.IndexOf(wanted, existing.path) < 0)
                    list.Add(existing);

            EditorBuildSettings.scenes = list.ToArray();
        }
    }
}
