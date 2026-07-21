using System.Collections.Generic;
using UnityEngine;

namespace HollerHorror.Clues
{
    /// <summary>
    /// Renders ClueBoardState in world space: cork surface, card views, and red
    /// yarn strings between connected pins. Local XY is the board surface; cards
    /// sit slightly in front (-Z toward the viewer).
    /// </summary>
    [RequireComponent(typeof(ClueBoard))]
    public sealed class ClueBoardView : MonoBehaviour
    {
        public const float BoardWidth = 2.4f;
        public const float BoardHeight = 1.4f;

        private static Material sharedUnlit;
        public static Material SharedUnlitMaterial
        {
            get
            {
                if (sharedUnlit == null)
                    sharedUnlit = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                return sharedUnlit;
            }
        }

        private ClueBoard board;
        private readonly Dictionary<int, ClueCardView> cardViews = new();
        private readonly Dictionary<(int, int), LineRenderer> strings = new();

        public IReadOnlyDictionary<int, ClueCardView> CardViews => cardViews;

        public static Vector3 NormalizedToLocal(Vector2 normalized) => new(
            (normalized.x - 0.5f) * (BoardWidth - ClueCardView.Width),
            (normalized.y - 0.5f) * (BoardHeight - ClueCardView.Height),
            -0.02f);

        public Vector2 WorldToNormalized(Vector3 worldPoint)
        {
            Vector3 local = transform.InverseTransformPoint(worldPoint);
            return new Vector2(
                Mathf.Clamp01(local.x / (BoardWidth - ClueCardView.Width) + 0.5f),
                Mathf.Clamp01(local.y / (BoardHeight - ClueCardView.Height) + 0.5f));
        }

        private void Awake()
        {
            board = GetComponent<ClueBoard>();
            BuildSurface();

            board.State.CardAdded += OnCardAdded;
            board.State.CardChanged += OnCardChanged;
            board.State.ConnectionAdded += OnConnectionAdded;
            board.State.ConnectionRemoved += OnConnectionRemoved;
        }

        private void BuildSurface()
        {
            var cork = GameObject.CreatePrimitive(PrimitiveType.Quad);
            cork.name = "Cork";
            cork.transform.SetParent(transform, false);
            cork.transform.localPosition = Vector3.zero;
            // Unity Quads face -Z by default — same side the viewer stands on.
            cork.transform.localScale = new Vector3(BoardWidth, BoardHeight, 1f);
            var renderer = cork.GetComponent<Renderer>();
            renderer.sharedMaterial = SharedUnlitMaterial;
            var block = new MaterialPropertyBlock();
            block.SetColor(Shader.PropertyToID("_BaseColor"), new Color(0.45f, 0.33f, 0.22f));
            renderer.SetPropertyBlock(block);

            // Collider for interaction raycasts against the board surface itself.
            var surfaceCollider = gameObject.AddComponent<BoxCollider>();
            surfaceCollider.size = new Vector3(BoardWidth, BoardHeight, 0.02f);
        }

        private void OnCardAdded(ClueCardData card) =>
            cardViews[card.Id] = ClueCardView.Create(transform, card);

        private void OnCardChanged(ClueCardData card)
        {
            if (cardViews.TryGetValue(card.Id, out var view))
            {
                view.Refresh(card);
                RefreshStringsFor(card.Id);
            }
        }

        private void OnConnectionAdded(int a, int b)
        {
            var go = new GameObject($"Yarn_{a}_{b}");
            go.transform.SetParent(transform, false);
            var line = go.AddComponent<LineRenderer>();
            line.material = new Material(Shader.Find("Sprites/Default"));
            line.startColor = line.endColor = ClueTheme.Yarn;
            line.widthMultiplier = 0.008f;
            line.positionCount = 2;
            line.useWorldSpace = true;
            strings[(a, b)] = line;
            UpdateString(a, b, line);
        }

        private void OnConnectionRemoved(int a, int b)
        {
            if (strings.Remove((a, b), out var line) && line != null)
                Destroy(line.gameObject);
        }

        private void RefreshStringsFor(int cardId)
        {
            foreach (var ((a, b), line) in strings)
                if (a == cardId || b == cardId)
                    UpdateString(a, b, line);
        }

        private void UpdateString(int a, int b, LineRenderer line)
        {
            if (cardViews.TryGetValue(a, out var viewA) && cardViews.TryGetValue(b, out var viewB))
            {
                line.SetPosition(0, viewA.PinWorldPosition);
                line.SetPosition(1, viewB.PinWorldPosition);
            }
        }

        /// <summary>Card under a world-space ray, if any.</summary>
        public ClueCardView RaycastCard(Ray ray)
        {
            if (Physics.Raycast(ray, out RaycastHit hit, 5f) &&
                hit.collider.TryGetComponent(out ClueCardView card))
                return card;
            return null;
        }
    }
}
