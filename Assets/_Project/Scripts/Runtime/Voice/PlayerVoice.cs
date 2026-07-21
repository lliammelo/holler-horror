using System.IO;
using Steamworks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HollerHorror.Voice
{
    /// <summary>
    /// M1 proximity VOIP over Steam Voice: the owner records compressed voice
    /// (push-to-talk V, open mic toggleable in inspector), ships it to everyone
    /// via unreliable RPC; remotes decompress and play it through a spatialized
    /// AudioSource on this player's head. Every received packet is also archived
    /// in the VoiceEchoLibrary for later Fetch mimicry.
    /// </summary>
    public sealed class PlayerVoice : NetworkBehaviour
    {
        [SerializeField, Tooltip("Head-level transform the remote voice source sits on.")]
        private Transform voiceAnchor;
        [SerializeField] private bool pushToTalk = true;
        [SerializeField] private float maxAudibleDistance = 22f;

        private readonly MemoryStream readStream = new();
        private readonly MemoryStream decompressStream = new();
        private VoiceOutput output;

        private bool talkHeld;
        private int packetsSent;
        private int lastPacketBytes;

        [SerializeField, Tooltip("Audible radius (m) of normal talking for entity hearing.")]
        private float voiceNoiseRadius = 15f;
        private float lastVoiceNoiseTime;

        public override void OnNetworkSpawn()
        {
            if (!IsOwner)
            {
                output = VoiceOutput.CreateOn(voiceAnchor != null ? voiceAnchor : transform, maxAudibleDistance);
            }
        }

        public override void OnNetworkDespawn()
        {
            if (IsOwner && SteamClient.IsValid)
                SteamUser.VoiceRecord = false;
        }

        private void Update()
        {
            if (!IsOwner || !IsSpawned || !SteamClient.IsValid)
                return;

            talkHeld = !pushToTalk || (Keyboard.current != null && Keyboard.current.vKey.isPressed);
            SteamUser.VoiceRecord = talkHeld;

            while (SteamUser.HasVoiceData)
            {
                readStream.SetLength(0);
                int written = SteamUser.ReadVoiceData(readStream);
                if (written <= 0)
                    break;

                var packet = new byte[written];
                System.Array.Copy(readStream.GetBuffer(), packet, written);

                packetsSent++;
                lastPacketBytes = written;

                // Talking is audible to entities (GDD: voice feeds Wendigo aggro). Throttled.
                if (Time.time - lastVoiceNoiseTime > 0.33f)
                {
                    lastVoiceNoiseTime = Time.time;
                    Senses.NoiseBus.Emit(new Senses.NoiseEvent(
                        transform.position, voiceNoiseRadius, 0.6f, Senses.NoiseKind.Voice, transform));
                }

                // Capture our own voice too, so the Fetch replay (F5) is testable solo.
                VoiceEchoLibrary.Record(OwnerClientId, packet);
                SendVoiceRpc(packet);
            }
        }

        private void OnGUI()
        {
            if (!IsOwner || !IsSpawned)
                return;

            GUILayout.BeginArea(new Rect(10, 340, 360, 70), GUI.skin.box);
            GUILayout.Label($"Mic [V]: {(talkHeld ? "RECORDING" : "off")}  (Steam: {(SteamClient.IsValid ? "ok" : "NOT RUNNING")})");
            GUILayout.Label($"Voice packets sent: {packetsSent}  last: {lastPacketBytes} B");
            GUILayout.EndArea();
        }

        [Rpc(SendTo.NotMe, Delivery = RpcDelivery.Unreliable)]
        private void SendVoiceRpc(byte[] compressed)
        {
            VoiceEchoLibrary.Record(OwnerClientId, compressed);

            if (output == null)
                return;

            decompressStream.SetLength(0);
            int pcmBytes = SteamUser.DecompressVoice(compressed, decompressStream);
            if (pcmBytes > 0)
                output.EnqueuePcm16(decompressStream.GetBuffer(), pcmBytes);
        }
    }
}
