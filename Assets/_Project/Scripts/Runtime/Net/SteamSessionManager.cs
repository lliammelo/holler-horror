using System.Threading.Tasks;
using Netcode.Transports.Facepunch;
using Steamworks;
using Steamworks.Data;
using Unity.Netcode;
using UnityEngine;

namespace HollerHorror.Net
{
    /// <summary>
    /// M1 spike: Steam lobby lifecycle + NGO host/join glue over the Facepunch transport.
    /// Flow: Host creates a friends-only lobby and starts an NGO host; friends join via
    /// the Steam overlay (Join Game) or by pasting the host's SteamID. Debug IMGUI only —
    /// real menus come much later.
    /// </summary>
    public sealed class SteamSessionManager : MonoBehaviour
    {
        public static SteamSessionManager Instance { get; private set; }

        [SerializeField, Tooltip("Spacewar test AppID until we have our own.")]
        private uint appId = 480;
        [SerializeField] private int maxPlayers = 4;

        [SerializeField, Tooltip("Scene camera used before a session starts; disabled once networking is live.")]
        private GameObject lobbyCamera;

        private Lobby? currentLobby;
        private string joinIdField = "";
        private string status = "Offline";

        private void Update()
        {
            // The transport only pumps callbacks while a session is running;
            // we pump here so lobby creation/join callbacks fire outside sessions too.
            if (SteamClient.IsValid)
                SteamClient.RunCallbacks();

            if (lobbyCamera != null && NetworkManager.Singleton != null)
                lobbyCamera.SetActive(!NetworkManager.Singleton.IsListening);
        }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            try
            {
                // Synchronous callback mode: we pump RunCallbacks() in Update, keeping
                // all Steam callbacks on the main thread (same model the transport uses).
                SteamClient.Init(appId, asyncCallbacks: false);
                status = $"Steam OK: {SteamClient.Name}";
            }
            catch (System.Exception e)
            {
                status = $"Steam init FAILED: {e.Message} (is Steam running?)";
                Debug.LogError(status);
            }

            SteamMatchmaking.OnLobbyCreated += OnLobbyCreated;
            SteamMatchmaking.OnLobbyEntered += OnLobbyEntered;
            SteamFriends.OnGameLobbyJoinRequested += OnGameLobbyJoinRequested;
        }

        private void OnDestroy()
        {
            if (Instance != this)
                return;
            SteamMatchmaking.OnLobbyCreated -= OnLobbyCreated;
            SteamMatchmaking.OnLobbyEntered -= OnLobbyEntered;
            SteamFriends.OnGameLobbyJoinRequested -= OnGameLobbyJoinRequested;
            currentLobby?.Leave();
            SteamClient.Shutdown();
        }

        public async void HostSession()
        {
            status = "Creating lobby...";
            await SteamMatchmaking.CreateLobbyAsync(maxPlayers);
        }

        private void OnLobbyCreated(Result result, Lobby lobby)
        {
            if (result != Result.OK)
            {
                status = $"Lobby create failed: {result}";
                return;
            }
            lobby.SetFriendsOnly();
            lobby.SetJoinable(true);

            if (lobbyCamera != null)
                lobbyCamera.SetActive(false); // before StartHost so the player's listener is the only one
            NetworkManager.Singleton.StartHost();
            status = $"Hosting. SteamID (share to join): {SteamClient.SteamId}";
        }

        private void OnLobbyEntered(Lobby lobby)
        {
            currentLobby = lobby;
            if (NetworkManager.Singleton.IsHost)
                return;

            // Client: connect the transport to the lobby owner.
            ConnectToHost(lobby.Owner.Id);
        }

        private async void OnGameLobbyJoinRequested(Lobby lobby, SteamId _)
        {
            await lobby.Join();
        }

        public void ConnectToHost(SteamId hostId)
        {
            var transport = NetworkManager.Singleton.GetComponent<FacepunchTransport>();
            transport.targetSteamId = hostId;
            if (lobbyCamera != null)
                lobbyCamera.SetActive(false);
            NetworkManager.Singleton.StartClient();
            status = $"Connecting to {hostId}...";
        }

        public void Disconnect()
        {
            try { currentLobby?.Leave(); }
            catch (System.Exception e) { Debug.LogWarning($"Lobby leave failed (ignorable in spike): {e.Message}"); }
            currentLobby = null;
            if (NetworkManager.Singleton != null)
                NetworkManager.Singleton.Shutdown();
            status = "Disconnected";
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 130, 360, 200), GUI.skin.box);
            GUILayout.Label(status);

            var nm = NetworkManager.Singleton;
            bool inSession = nm != null && (nm.IsHost || nm.IsClient && nm.IsConnectedClient);

            if (!inSession)
            {
                if (GUILayout.Button("Host (friends can join via Steam overlay)"))
                    HostSession();

                GUILayout.BeginHorizontal();
                joinIdField = GUILayout.TextField(joinIdField, GUILayout.Width(220));
                if (GUILayout.Button("Join SteamID") && ulong.TryParse(joinIdField, out ulong id))
                    ConnectToHost(id);
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.Label($"Connected. Players: {nm.ConnectedClientsList.Count}");
                if (GUILayout.Button("Leave"))
                    Disconnect();
            }
            GUILayout.EndArea();
        }
    }
}
