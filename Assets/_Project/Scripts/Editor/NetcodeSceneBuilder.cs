using System.IO;
using HollerHorror.Debugging;
using HollerHorror.Net;
using HollerHorror.Player;
using HollerHorror.Voice;
using Netcode.Transports.Facepunch;
using Unity.Netcode;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HollerHorror.Editor
{
    /// <summary>
    /// Builds the M1 netcode spike scene: greybox environment + NetworkManager
    /// (Facepunch transport) + SteamSessionManager + networked player prefab.
    /// Reproducible from "Holler Horror > Build Netcode Test Scene".
    /// </summary>
    public static class NetcodeSceneBuilder
    {
        private const string ScenePath = "Assets/_Project/Scenes/Netcode_Test.unity";
        private const string PrefabPath = "Assets/_Project/Prefabs/NetworkPlayer.prefab";
        private const string InputActionsPath = "Assets/InputSystem_Actions.inputactions";

        [MenuItem("Holler Horror/Build Netcode Test Scene")]
        public static void Build()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GreyboxSceneBuilder.BuildLighting();
            GreyboxSceneBuilder.BuildEnvironment();

            GameObject playerPrefab = BuildPlayerPrefab();

            // Lobby camera: what you look through before a session starts.
            var lobbyCamGo = new GameObject("LobbyCamera");
            lobbyCamGo.tag = "MainCamera";
            lobbyCamGo.transform.position = new Vector3(0, 12, -35);
            lobbyCamGo.transform.rotation = Quaternion.Euler(20, 0, 0);
            lobbyCamGo.AddComponent<Camera>();
            lobbyCamGo.AddComponent<AudioListener>();

            // NetworkManager + transport.
            var nmGo = new GameObject("NetworkManager");
            var nm = nmGo.AddComponent<NetworkManager>();
            var transport = nmGo.AddComponent<FacepunchTransport>();
            nm.NetworkConfig = new NetworkConfig
            {
                NetworkTransport = transport,
                PlayerPrefab = playerPrefab,
            };

            var sensesDebug = new GameObject("SensesDebug");
            sensesDebug.AddComponent<NoiseDebugRenderer>();
            sensesDebug.AddComponent<PlaceholderFootstepAudio>();

            // Steam session glue + Fetch replay debugger.
            var sessionGo = new GameObject("SteamSession");
            var session = sessionGo.AddComponent<SteamSessionManager>();
            sessionGo.AddComponent<VoiceReplayDebugger>();
            var sessionSo = new SerializedObject(session);
            sessionSo.FindProperty("lobbyCamera").objectReferenceValue = lobbyCamGo;
            sessionSo.ApplyModifiedPropertiesWithoutUndo();

            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
            EditorSceneManager.SaveScene(scene, ScenePath);
            AddSceneToBuildSettings();
            Debug.Log($"[NetcodeSceneBuilder] Built and saved {ScenePath}");
        }

        private static GameObject BuildPlayerPrefab()
        {
            var inputActions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputActionsPath);
            if (inputActions == null)
                throw new FileNotFoundException($"Input actions asset not found at {InputActionsPath}");

            var player = new GameObject("NetworkPlayer");

            var cc = player.AddComponent<CharacterController>();
            cc.height = 1.8f;
            cc.radius = 0.35f;
            cc.center = new Vector3(0, 0.9f, 0);
            cc.slopeLimit = 45f;
            cc.stepOffset = 0.3f;

            // Visible body so remote players can be seen (capsule stand-in).
            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            Object.DestroyImmediate(body.GetComponent<Collider>());
            body.transform.SetParent(player.transform);
            body.transform.localPosition = new Vector3(0, 0.9f, 0);
            body.transform.localScale = new Vector3(0.7f, 0.9f, 0.7f);

            var pivot = new GameObject("CameraPivot");
            pivot.transform.SetParent(player.transform);
            pivot.transform.localPosition = new Vector3(0, 1.66f, 0);

            var camRig = new GameObject("CameraRig");
            camRig.transform.SetParent(pivot.transform);
            camRig.transform.localPosition = Vector3.zero;
            var cam = camRig.AddComponent<Camera>();
            cam.nearClipPlane = 0.05f;
            cam.fieldOfView = 70f;
            camRig.AddComponent<AudioListener>();

            player.AddComponent<NetworkObject>();
            var netTransform = player.AddComponent<OwnerNetworkTransform>();
            netTransform.InLocalSpace = false;

            var stamina = player.AddComponent<PlayerStamina>();
            var controller = player.AddComponent<FirstPersonController>();
            var controllerSo = new SerializedObject(controller);
            controllerSo.FindProperty("inputActions").objectReferenceValue = inputActions;
            controllerSo.FindProperty("cameraPivot").objectReferenceValue = pivot.transform;
            controllerSo.ApplyModifiedPropertiesWithoutUndo();

            var emitter = player.AddComponent<HollerHorror.Senses.FootstepNoiseEmitter>();

            var hud = player.AddComponent<PlayerDebugHud>();
            var hudSo = new SerializedObject(hud);
            hudSo.FindProperty("controller").objectReferenceValue = controller;
            hudSo.FindProperty("stamina").objectReferenceValue = stamina;
            hudSo.FindProperty("noiseEmitter").objectReferenceValue = emitter;
            hudSo.ApplyModifiedPropertiesWithoutUndo();

            var voice = player.AddComponent<PlayerVoice>();
            var voiceSo = new SerializedObject(voice);
            voiceSo.FindProperty("voiceAnchor").objectReferenceValue = pivot.transform;
            voiceSo.ApplyModifiedPropertiesWithoutUndo();

            var netPlayer = player.AddComponent<NetworkPlayer>();
            var netPlayerSo = new SerializedObject(netPlayer);
            netPlayerSo.FindProperty("controller").objectReferenceValue = controller;
            netPlayerSo.FindProperty("cameraRig").objectReferenceValue = camRig;
            var ownerOnly = netPlayerSo.FindProperty("ownerOnlyBehaviours");
            ownerOnly.arraySize = 1;
            ownerOnly.GetArrayElementAtIndex(0).objectReferenceValue = hud;
            netPlayerSo.ApplyModifiedPropertiesWithoutUndo();

            Directory.CreateDirectory(Path.GetDirectoryName(PrefabPath));
            var prefab = PrefabUtility.SaveAsPrefabAsset(player, PrefabPath);
            Object.DestroyImmediate(player);
            return prefab;
        }

        private static void AddSceneToBuildSettings()
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
