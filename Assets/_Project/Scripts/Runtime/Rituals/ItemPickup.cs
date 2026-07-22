using HollerHorror.Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HollerHorror.Rituals
{
    /// <summary>A ritual component lying in the world. E to take; the prop disappears.</summary>
    public sealed class ItemPickup : MonoBehaviour
    {
        [SerializeField] private string itemId = "";
        [SerializeField] private float pickupDistance = 2.5f;

        public void Configure(string id) => itemId = id;

        private bool LocalPlayerNear()
        {
            foreach (var player in PlayerRegistry.All)
                if (Vector3.Distance(player.transform.position, transform.position) <= pickupDistance)
                    return true;
            return false;
        }

        private void Update()
        {
            if (!LocalPlayerNear() || Keyboard.current == null || !Keyboard.current.eKey.wasPressedThisFrame)
                return;

            RitualInventory.Grant(itemId);
            Debug.Log($"[Ritual] Took {RitualDefinition.ItemDisplayName(itemId)}");
            Destroy(gameObject);
        }

        private void OnGUI()
        {
            if (!LocalPlayerNear())
                return;

            GUILayout.BeginArea(new Rect(Screen.width / 2f - 120, Screen.height * 0.56f, 240, 30), GUI.skin.box);
            GUILayout.Label($"[E] Take {RitualDefinition.ItemDisplayName(itemId)}");
            GUILayout.EndArea();
        }
    }
}
