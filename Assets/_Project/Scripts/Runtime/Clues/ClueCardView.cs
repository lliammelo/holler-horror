using UnityEngine;

namespace HollerHorror.Clues
{
    /// <summary>
    /// World-space visual for one clue card: paper quad, title/source text, a pin
    /// colored by theory tag, and a contradiction mark. Built entirely from
    /// primitives — art pass replaces this wholesale in M9.
    /// </summary>
    public sealed class ClueCardView : MonoBehaviour
    {
        public const float Width = 0.30f;
        public const float Height = 0.20f;

        private Renderer paper;
        private Renderer pin;
        private TextMesh titleText;
        private TextMesh sourceText;
        private TextMesh contradictionMark;
        private MaterialPropertyBlock block;
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        public int CardId { get; private set; }
        /// <summary>Where yarn strings attach, in world space.</summary>
        public Vector3 PinWorldPosition => pin != null ? pin.transform.position : transform.position;

        public static ClueCardView Create(Transform boardRoot, ClueCardData data)
        {
            var go = new GameObject($"Card_{data.Id}");
            go.transform.SetParent(boardRoot, false);
            var view = go.AddComponent<ClueCardView>();
            view.BuildVisual();
            view.CardId = data.Id;
            view.Refresh(data);
            return view;
        }

        private void BuildVisual()
        {
            block = new MaterialPropertyBlock();

            var paperGo = GameObject.CreatePrimitive(PrimitiveType.Quad);
            paperGo.name = "Paper";
            Destroy(paperGo.GetComponent<Collider>());
            paperGo.transform.SetParent(transform, false);
            paperGo.transform.localScale = new Vector3(Width, Height, 1f);
            paper = paperGo.GetComponent<Renderer>();
            paper.sharedMaterial = ClueBoardView.SharedUnlitMaterial;

            // One collider on the card root for interaction raycasts.
            var collider = gameObject.AddComponent<BoxCollider>();
            collider.size = new Vector3(Width, Height, 0.02f);

            titleText = MakeText("Title", new Vector3(0f, Height * 0.18f, -0.003f), 0.021f, FontStyle.Bold);
            sourceText = MakeText("Source", new Vector3(0f, -Height * 0.32f, -0.003f), 0.016f, FontStyle.Italic);

            var pinGo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            pinGo.name = "Pin";
            Destroy(pinGo.GetComponent<Collider>());
            pinGo.transform.SetParent(transform, false);
            pinGo.transform.localPosition = new Vector3(0f, Height * 0.46f, -0.012f);
            pinGo.transform.localScale = Vector3.one * 0.024f;
            pin = pinGo.GetComponent<Renderer>();
            pin.sharedMaterial = ClueBoardView.SharedUnlitMaterial;

            contradictionMark = MakeText("Contradiction", new Vector3(Width * 0.42f, Height * 0.34f, -0.004f), 0.035f, FontStyle.Bold);
            contradictionMark.text = "!";
            contradictionMark.color = ClueTheme.ContradictionRed;
        }

        private TextMesh MakeText(string name, Vector3 localPos, float characterSize, FontStyle style)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            go.transform.localPosition = localPos;
            var text = go.AddComponent<TextMesh>();
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.characterSize = characterSize;
            text.fontSize = 40;
            text.fontStyle = style;
            text.color = ClueTheme.Ink;
            return text;
        }

        public void Refresh(ClueCardData data)
        {
            transform.localPosition = ClueBoardView.NormalizedToLocal(data.BoardPosition);

            titleText.text = Wrap(data.Title, 18);
            sourceText.text = data.Kind == ClueKind.Testimony ? $"— {data.Source}" : $"[{data.Source}]";

            block.SetColor(BaseColorId, data.Kind == ClueKind.Testimony ? ClueTheme.TestimonyPaper : ClueTheme.CardPaper);
            paper.SetPropertyBlock(block);

            var pinBlock = new MaterialPropertyBlock();
            pinBlock.SetColor(BaseColorId, ClueTheme.PinColor(data.Theory));
            pin.SetPropertyBlock(pinBlock);

            contradictionMark.gameObject.SetActive(data.Contradiction);
        }

        private static string Wrap(string text, int maxLineChars)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxLineChars)
                return text;

            int split = text.LastIndexOf(' ', Mathf.Min(maxLineChars, text.Length - 1));
            return split <= 0 ? text : text[..split] + "\n" + text[(split + 1)..];
        }
    }
}
