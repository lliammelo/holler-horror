using Unity.Netcode;
using UnityEngine;

namespace HollerHorror.Clues
{
    /// <summary>
    /// Server-authoritative replication for the clue board (GDD §11: board state
    /// is a first-class netcode object). Clients request mutations; the server
    /// applies them via Everyone-RPCs so every client's ClueBoardState stays
    /// identical. Late joiners get a full-state replay on connect.
    /// </summary>
    [RequireComponent(typeof(ClueBoard))]
    public sealed class ClueBoardNetSync : NetworkBehaviour
    {
        private ClueBoard board;

        private void Awake() => board = GetComponent<ClueBoard>();

        public override void OnNetworkSpawn()
        {
            if (IsServer)
                NetworkManager.OnClientConnectedCallback += OnClientConnected;
        }

        public override void OnNetworkDespawn()
        {
            if (IsServer)
                NetworkManager.OnClientConnectedCallback -= OnClientConnected;
        }

        private void OnClientConnected(ulong clientId)
        {
            if (clientId == NetworkManager.LocalClientId)
                return;

            foreach (var card in board.State.Cards.Values)
                ApplyAddCardRpc(card.Id, card.Title, card.Body, (byte)card.Kind, card.Source,
                    card.BoardPosition, (byte)card.Theory, card.Contradiction,
                    RpcTarget.Single(clientId, RpcTargetUse.Temp));
            foreach (var (a, b) in board.State.Connections)
                ApplyToggleConnectionRpc(a, b, RpcTarget.Single(clientId, RpcTargetUse.Temp));
        }

        // ---- Requests (any client -> server) ----

        public void RequestAddCard(string title, string body, ClueKind kind, string source, Vector2 pos) =>
            AddCardServerRpc(title, body, (byte)kind, source, pos);

        public void RequestMoveCard(int id, Vector2 pos) => MoveCardServerRpc(id, pos);
        public void RequestSetTheory(int id, EntityId theory) => SetTheoryServerRpc(id, (byte)theory);
        public void RequestSetContradiction(int id, bool flagged) => SetContradictionServerRpc(id, flagged);
        public void RequestToggleConnection(int a, int b) => ToggleConnectionServerRpc(a, b);

        [Rpc(SendTo.Server)]
        private void AddCardServerRpc(string title, string body, byte kind, string source, Vector2 pos)
        {
            int id = board.State.ClaimNextId();
            ApplyAddCardRpc(id, title, body, kind, source, pos, (byte)EntityId.Unknown, false,
                RpcTarget.Everyone);
        }

        [Rpc(SendTo.Server)]
        private void MoveCardServerRpc(int id, Vector2 pos) =>
            ApplyMoveCardRpc(id, pos, RpcTarget.Everyone);

        [Rpc(SendTo.Server)]
        private void SetTheoryServerRpc(int id, byte theory) =>
            ApplySetTheoryRpc(id, theory, RpcTarget.Everyone);

        [Rpc(SendTo.Server)]
        private void SetContradictionServerRpc(int id, bool flagged) =>
            ApplySetContradictionRpc(id, flagged, RpcTarget.Everyone);

        [Rpc(SendTo.Server)]
        private void ToggleConnectionServerRpc(int a, int b) =>
            ApplyToggleConnectionRpc(a, b, RpcTarget.Everyone);

        // ---- Application (server -> everyone / late joiner) ----

        [Rpc(SendTo.SpecifiedInParams)]
        private void ApplyAddCardRpc(int id, string title, string body, byte kind, string source,
            Vector2 pos, byte theory, bool contradiction, RpcParams rpcParams = default)
        {
            board.State.AddCard(new ClueCardData
            {
                Id = id,
                Title = title,
                Body = body,
                Kind = (ClueKind)kind,
                Source = source,
                BoardPosition = pos,
                Theory = (EntityId)theory,
                Contradiction = contradiction,
            });
        }

        [Rpc(SendTo.SpecifiedInParams)]
        private void ApplyMoveCardRpc(int id, Vector2 pos, RpcParams rpcParams = default) =>
            board.State.MoveCard(id, pos);

        [Rpc(SendTo.SpecifiedInParams)]
        private void ApplySetTheoryRpc(int id, byte theory, RpcParams rpcParams = default) =>
            board.State.SetTheory(id, (EntityId)theory);

        [Rpc(SendTo.SpecifiedInParams)]
        private void ApplySetContradictionRpc(int id, bool flagged, RpcParams rpcParams = default) =>
            board.State.SetContradiction(id, flagged);

        [Rpc(SendTo.SpecifiedInParams)]
        private void ApplyToggleConnectionRpc(int a, int b, RpcParams rpcParams = default) =>
            board.State.ToggleConnection(a, b);
    }
}
