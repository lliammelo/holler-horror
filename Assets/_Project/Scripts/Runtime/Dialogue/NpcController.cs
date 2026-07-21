using HollerHorror.Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HollerHorror.Dialogue
{
    /// <summary>
    /// A resident of the holler: stands at their homestead, talks when approached.
    /// Visuals are greybox capsules for now; knowledge lives in Yarn scripts and
    /// per-run variable assignments, not here.
    /// </summary>
    public sealed class NpcController : MonoBehaviour
    {
        [SerializeField] private string npcName = "Resident";
        [SerializeField] private string startNode = "Start";
        [SerializeField] private float talkDistance = 2.8f;

        public string NpcName { get => npcName; set => npcName = value; }
        public string StartNode { get => startNode; set => startNode = value; }

        private bool LocalPlayerNear()
        {
            foreach (var player in PlayerRegistry.All)
                if (Vector3.Distance(player.transform.position, transform.position) <= talkDistance)
                    return true;
            return false;
        }

        private void Update()
        {
            if (DialogueService.Instance == null || DialogueService.Instance.IsBusy)
                return;

            if (LocalPlayerNear() && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
                DialogueService.Instance.StartConversation(npcName, startNode);
        }

        private void OnGUI()
        {
            if (DialogueService.Instance == null || DialogueService.Instance.IsBusy || !LocalPlayerNear())
                return;

            GUILayout.BeginArea(new Rect(Screen.width / 2f - 100, Screen.height * 0.62f, 200, 30), GUI.skin.box);
            GUILayout.Label($"[E] Talk to {npcName}");
            GUILayout.EndArea();
        }
    }
}
