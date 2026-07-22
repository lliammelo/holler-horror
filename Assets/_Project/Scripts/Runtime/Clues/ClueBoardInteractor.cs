using HollerHorror.Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HollerHorror.Clues
{
    /// <summary>
    /// Player-side board interaction. Walk up, press E: the view swaps to a fixed
    /// board camera with a free cursor.
    ///   LMB drag  — move a card (committed on release, so one sync per drop)
    ///   RMB drag  — connect two cards with yarn (drag between pins); repeat to cut
    ///   T (hover) — cycle theory tag        C (hover) — flag/unflag contradiction
    ///   N         — pin a placeholder clue (slice testing)
    ///   E / Esc   — step back
    /// </summary>
    public sealed class ClueBoardInteractor : MonoBehaviour
    {
        [SerializeField] private float interactDistance = 3f;

        private FirstPersonController controller;
        private Camera playerCamera;
        private ClueBoardView boardView;
        private Camera boardCamera;

        private bool atBoard;
        private ClueCardView draggingCard;
        private ClueCardView stringFromCard;
        private ClueCardView hoveredCard;
        private LineRenderer stringPreview;

        private void Awake()
        {
            controller = GetComponent<FirstPersonController>();
        }

        private void Start()
        {
            boardView = FindAnyObjectByType<ClueBoardView>();
            if (boardView == null)
            {
                enabled = false;
                return;
            }

            // Fixed viewing camera 1.6m in front of the board (viewer side is -Z).
            var camGo = new GameObject("BoardCamera");
            camGo.transform.SetParent(boardView.transform, false);
            camGo.transform.localPosition = new Vector3(0f, 0f, -1.6f);
            camGo.transform.localRotation = Quaternion.identity;
            boardCamera = camGo.AddComponent<Camera>();
            boardCamera.fieldOfView = 55f;
            boardCamera.nearClipPlane = 0.05f;
            boardCamera.enabled = false;
        }

        private bool NearBoard =>
            Vector3.Distance(transform.position, boardView.transform.position) <= interactDistance;

        private void Update()
        {
            if (boardView == null || Keyboard.current == null)
                return;

            if (!atBoard)
            {
                if (controller.enabled && NearBoard && Keyboard.current.eKey.wasPressedThisFrame)
                    EnterBoard();
                return;
            }

            if (Keyboard.current.eKey.wasPressedThisFrame || Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                ExitBoard();
                return;
            }

            TickBoardInput();
        }

        private void EnterBoard()
        {
            atBoard = true;
            controller.enabled = false;
            playerCamera = Camera.main;
            if (playerCamera != null)
                playerCamera.enabled = false;
            boardCamera.enabled = true;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void ExitBoard()
        {
            CancelString();
            draggingCard = null;
            atBoard = false;
            boardCamera.enabled = false;
            if (playerCamera != null)
                playerCamera.enabled = true;
            controller.enabled = true;
        }

        private void TickBoardInput()
        {
            var mouse = Mouse.current;
            if (mouse == null)
                return;

            Ray ray = boardCamera.ScreenPointToRay(mouse.position.ReadValue());
            ClueCardView hovered = boardView.RaycastCard(ray);
            hoveredCard = hovered;

            // --- Move (LMB) ---
            if (mouse.leftButton.wasPressedThisFrame && hovered != null)
                draggingCard = hovered;

            if (draggingCard != null)
            {
                if (TryRayToBoardPlane(ray, out Vector3 world))
                    draggingCard.transform.position = world + boardView.transform.forward * -0.001f;

                if (mouse.leftButton.wasReleasedThisFrame)
                {
                    ClueBoard.Instance.MoveCard(draggingCard.CardId,
                        boardView.WorldToNormalized(draggingCard.transform.position));
                    draggingCard = null;
                }
            }

            // --- Connect (RMB drag) ---
            if (mouse.rightButton.wasPressedThisFrame && hovered != null)
            {
                stringFromCard = hovered;
                stringPreview = MakePreviewLine();
            }

            if (stringFromCard != null && stringPreview != null && TryRayToBoardPlane(ray, out Vector3 cursorWorld))
            {
                stringPreview.SetPosition(0, stringFromCard.PinWorldPosition);
                stringPreview.SetPosition(1, cursorWorld);
            }

            if (mouse.rightButton.wasReleasedThisFrame && stringFromCard != null)
            {
                if (hovered != null && hovered != stringFromCard)
                    ClueBoard.Instance.ToggleConnection(stringFromCard.CardId, hovered.CardId);
                CancelString();
            }

            // --- Tag / flag ---
            if (hovered != null && Keyboard.current.tKey.wasPressedThisFrame)
                ClueBoard.Instance.CycleTheory(hovered.CardId);
            if (hovered != null && Keyboard.current.cKey.wasPressedThisFrame)
                ClueBoard.Instance.ToggleContradiction(hovered.CardId);

            // --- Placeholder clue (testing) ---
            if (Keyboard.current.nKey.wasPressedThisFrame && TryRayToBoardPlane(ray, out Vector3 spawnWorld))
                PlaceholderClues.PinRandom(boardView.WorldToNormalized(spawnWorld));
        }

        private bool TryRayToBoardPlane(Ray ray, out Vector3 world)
        {
            // Board surface plane, slightly toward the viewer.
            var plane = new Plane(-boardView.transform.forward,
                boardView.transform.position - boardView.transform.forward * 0.02f);
            if (plane.Raycast(ray, out float distance))
            {
                world = ray.GetPoint(distance);
                return true;
            }
            world = default;
            return false;
        }

        private LineRenderer MakePreviewLine()
        {
            var go = new GameObject("YarnPreview");
            var line = go.AddComponent<LineRenderer>();
            line.material = new Material(Shader.Find("Sprites/Default"));
            line.startColor = line.endColor = new Color(ClueTheme.Yarn.r, ClueTheme.Yarn.g, ClueTheme.Yarn.b, 0.5f);
            line.widthMultiplier = 0.006f;
            line.positionCount = 2;
            return line;
        }

        private void CancelString()
        {
            stringFromCard = null;
            if (stringPreview != null)
                Destroy(stringPreview.gameObject);
        }

        private void OnGUI()
        {
            if (boardView == null)
                return;

            if (atBoard)
            {
                GUILayout.BeginArea(new Rect(10, Screen.height - 64, 720, 54), GUI.skin.box);
                GUILayout.Label("LMB drag: move   RMB drag: string   T: tag theory   C: contradiction   N: new placeholder clue   E/Esc: step back");
                GUILayout.EndArea();

                // Inspect panel: full text of the hovered card.
                if (hoveredCard != null && ClueBoard.Instance.State.TryGetCard(hoveredCard.CardId, out var data))
                {
                    GUILayout.BeginArea(new Rect(10, 10, 340, 150), GUI.skin.box);
                    GUILayout.Label($"{data.Title}" + (data.Contradiction ? "   [!]" : ""));
                    GUILayout.Label(data.Body);
                    GUILayout.Label(data.Kind == ClueKind.Testimony ? $"— {data.Source}" : $"found: {data.Source}");
                    GUILayout.Label($"theory: {ClueTheme.TheoryLabel(data.Theory)}");
                    GUILayout.EndArea();
                }

                DrawDeclarationPanel();
            }
            else if (controller.enabled && NearBoard)
            {
                GUILayout.BeginArea(new Rect(Screen.width / 2f - 90, Screen.height * 0.62f, 180, 30), GUI.skin.box);
                GUILayout.Label("[E] Examine the board");
                GUILayout.EndArea();
            }
        }

        /// <summary>
        /// Ritual preparation (M5): choose which entity's rite to gather for.
        /// Changeable here — the real commitment is speaking the words at the site.
        /// </summary>
        private void DrawDeclarationPanel()
        {
            var director = Dialogue.CaseDirector.Instance;
            if (director == null || director.CaseWon)
                return;

            GUILayout.BeginArea(new Rect(Screen.width - 290, 10, 280, 240), GUI.skin.box);

            if (director.PreparedRitual == EntityId.Unknown)
            {
                GUILayout.Label("PREPARE A RITUAL — what preys on this holler?");
                if (GUILayout.Button("The Wendigo — Burning of the Name")) director.PrepareRitual(EntityId.Wendigo);
                if (GUILayout.Button("The Fetch — Mirror Binding")) director.PrepareRitual(EntityId.Fetch);
                if (GUILayout.Button("The Hollow — Consecration")) director.PrepareRitual(EntityId.Hollow);
            }
            else
            {
                var ritual = director.PreparedRitual;
                GUILayout.Label($"Preparing: {Rituals.RitualDefinition.DisplayName(ritual)}");
                GUILayout.Label($"Perform at {Rituals.RitualDefinition.SiteDescription(ritual)}.");
                GUILayout.Label("Components:");
                foreach (var item in Rituals.RitualDefinition.ComponentsOf(ritual))
                {
                    bool has = Rituals.RitualInventory.Has(item);
                    GUILayout.Label($"  {(has ? "✔" : "✘")} {Rituals.RitualDefinition.ItemDisplayName(item)}");
                }
                GUILayout.Space(6);
                if (GUILayout.Button("Reconsider (prepare a different rite)"))
                    director.PrepareRitual(EntityId.Unknown);
            }

            GUILayout.EndArea();
        }
    }
}
