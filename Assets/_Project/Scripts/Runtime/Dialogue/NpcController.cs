using System;
using HollerHorror.Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HollerHorror.Dialogue
{
    /// <summary>
    /// A resident of the holler: stands at their homestead, talks when approached.
    /// In late-night escalation they become targets — a dead resident can no longer
    /// be interviewed, taking their remaining clues (and any ritual component they
    /// held) with them (GDD §3 escalation, §6 NPCs-as-stakes). A resident with a
    /// living player nearby is sheltered and won't be taken.
    /// </summary>
    public sealed class NpcController : MonoBehaviour
    {
        [SerializeField] private string npcName = "Resident";
        [SerializeField] private string startNode = "Start";
        [SerializeField] private float talkDistance = 2.8f;
        [SerializeField, Tooltip("A player within this range shelters the resident from the entity.")]
        private float shelterRadius = 6f;

        [SerializeField] private Renderer bodyRenderer;

        public string NpcName { get => npcName; set => npcName = value; }
        public string StartNode { get => startNode; set => startNode = value; }
        public bool IsAlive { get; private set; } = true;
        /// <summary>True while a Hollow zone has swallowed this resident — unreachable until it passes.</summary>
        public bool IsEnveloped { get; private set; }

        /// <summary>Raised when this resident is killed — clue/component holders can react.</summary>
        public event Action<NpcController> Died;

        public void SetEnveloped(bool enveloped) => IsEnveloped = enveloped;

        private void OnEnable() => NpcRegistry.Register(this);
        private void OnDisable() => NpcRegistry.Unregister(this);

        public bool IsSheltered()
        {
            foreach (var player in PlayerRegistry.All)
                if (Vector3.Distance(player.transform.position, transform.position) <= shelterRadius)
                    return true;
            return false;
        }

        public void Kill()
        {
            if (!IsAlive)
                return;

            IsAlive = false;
            // Topple + grey out so a dead resident reads at a glance.
            transform.rotation = Quaternion.Euler(90f, transform.eulerAngles.y, 0f);
            transform.position += Vector3.up * -0.4f;
            if (bodyRenderer != null)
            {
                var block = new MaterialPropertyBlock();
                block.SetColor(Shader.PropertyToID("_BaseColor"), new Color(0.2f, 0.2f, 0.22f));
                bodyRenderer.SetPropertyBlock(block);
            }

            Debug.Log($"[NPC] {npcName} was taken. Their clues are lost.");
            Died?.Invoke(this);
        }

        private bool LocalPlayerNear()
        {
            foreach (var player in PlayerRegistry.All)
                if (Vector3.Distance(player.transform.position, transform.position) <= talkDistance)
                    return true;
            return false;
        }

        private void Update()
        {
            if (!IsAlive || IsEnveloped || DialogueService.Instance == null || DialogueService.Instance.IsBusy)
                return;

            if (LocalPlayerNear() && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
                DialogueService.Instance.StartConversation(npcName, startNode);
        }

        private void OnGUI()
        {
            if (!IsAlive || DialogueService.Instance == null || DialogueService.Instance.IsBusy || !LocalPlayerNear())
                return;

            GUILayout.BeginArea(new Rect(Screen.width / 2f - 130, Screen.height * 0.62f, 260, 30), GUI.skin.box);
            GUILayout.Label(IsEnveloped
                ? $"{npcName} is somewhere in the dark — you can't reach them"
                : $"[E] Talk to {npcName}");
            GUILayout.EndArea();
        }
    }
}
