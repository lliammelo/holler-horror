using HollerHorror.Player;
using Unity.Netcode;
using UnityEngine;

namespace HollerHorror.Net
{
    /// <summary>
    /// Enables local simulation (controller, camera, audio listener, HUD) only on the
    /// owning client; remote copies are just replicated transforms with a voice source.
    /// </summary>
    public sealed class NetworkPlayer : NetworkBehaviour
    {
        [SerializeField] private FirstPersonController controller;
        [SerializeField] private GameObject cameraRig;
        [SerializeField] private Behaviour[] ownerOnlyBehaviours;

        public override void OnNetworkSpawn()
        {
            bool isLocal = IsOwner;

            controller.enabled = isLocal;
            cameraRig.SetActive(isLocal);
            foreach (var b in ownerOnlyBehaviours)
                if (b != null)
                    b.enabled = isLocal;

            if (isLocal)
            {
                // Spread spawn positions so players don't stack inside each other.
                transform.position += new Vector3((OwnerClientId % 4) * 1.5f, 0f, 0f);
            }

            name = $"Player_{OwnerClientId}" + (isLocal ? " (You)" : "");
        }
    }
}
