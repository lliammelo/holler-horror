using HollerHorror.Player;
using UnityEngine;
using UnityEngine.AI;

namespace HollerHorror.Entities
{
    public enum FetchState { Wander, Manifest, Retreat }

    /// <summary>
    /// The Fetch (GDD §4.2): weak in confrontation, devastating through deception.
    /// It never kills. It wanders (never crossing running water), and periodically
    /// MANIFESTS at a distance — showing a borrowed face and name, throwing your
    /// own voice from the treeline — then vanishes the moment you close on it.
    /// Its real danger is the false testimony it plants elsewhere (FetchDouble).
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public sealed class FetchController : MonoBehaviour, IBanishable
    {
        [Header("Movement")]
        [SerializeField] private float wanderSpeed = 2.6f;
        [SerializeField] private float wanderRadius = 30f;

        [Header("Manifestation")]
        [SerializeField] private float manifestRange = 24f;
        [SerializeField] private Vector2 manifestIntervalRange = new(10f, 20f);
        [SerializeField] private float manifestDuration = 4f;
        [SerializeField, Tooltip("Get this close and it vanishes.")]
        private float vanishDistance = 7f;

        [Header("Disguise")]
        [SerializeField] private Renderer bodyRenderer;
        [SerializeField] private GameObject disguiseVisual;
        [SerializeField] private TextMesh nameTag;
        [SerializeField, Tooltip("Names it borrows when it shows itself.")]
        private string[] borrowedNames = { "Ada Bricker", "Old Man Tetch", "Ruth Combs" };

        private NavMeshAgent agent;
        private Vector3 spawnPoint;
        private float nextManifestTime;
        private float manifestUntil;

        public FetchState State { get; private set; } = FetchState.Wander;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            spawnPoint = transform.position;
        }

        private void Start()
        {
            agent.Warp(transform.position);
            agent.speed = wanderSpeed;
            SetVisible(false);
            ScheduleNextManifest();
            PickWanderPoint();
        }

        private void Update()
        {
            switch (State)
            {
                case FetchState.Wander: TickWander(); break;
                case FetchState.Manifest: TickManifest(); break;
                case FetchState.Retreat: TickRetreat(); break;
            }
        }

        private void TickWander()
        {
            if (!agent.pathPending && agent.remainingDistance < 1f)
                PickWanderPoint();

            var player = NearestVisiblePlayerInRange();
            if (player != null && Time.time >= nextManifestTime)
                EnterManifest(player);
        }

        private void EnterManifest(FirstPersonController player)
        {
            State = FetchState.Manifest;
            agent.ResetPath();
            manifestUntil = Time.time + manifestDuration;

            // Borrow a face and a name, and turn to face the witness.
            if (nameTag != null)
                nameTag.text = borrowedNames.Length > 0 ? borrowedNames[Random.Range(0, borrowedNames.Length)] : "";
            FaceInstant(player.transform.position);
            SetVisible(true);
            FetchAudio.PlayVoiceFromTheTreeline(transform.position);
        }

        private void TickManifest()
        {
            var player = NearestPlayer(out float distance);

            if (player != null)
            {
                FaceToward(player.transform.position);
                if (distance <= vanishDistance) // you got too close — it isn't there anymore
                {
                    EnterRetreat();
                    return;
                }
            }

            if (Time.time >= manifestUntil)
                EnterRetreat();
        }

        private void EnterRetreat()
        {
            State = FetchState.Retreat;
            SetVisible(false);

            // Slip away to a distant spot it can reach without crossing water.
            for (int i = 0; i < 16; i++)
            {
                if (TryWanderPoint(out Vector3 point) && Vector3.Distance(point, transform.position) > 18f)
                {
                    agent.Warp(point);
                    break;
                }
            }
            ScheduleNextManifest();
        }

        private void TickRetreat()
        {
            PickWanderPoint();
            State = FetchState.Wander;
        }

        private void PickWanderPoint()
        {
            if (TryWanderPoint(out Vector3 point))
                agent.SetDestination(point);
        }

        /// <summary>A reachable point that doesn't require crossing running water.</summary>
        private bool TryWanderPoint(out Vector3 result)
        {
            var path = new NavMeshPath();
            for (int i = 0; i < 16; i++)
            {
                Vector2 flat = Random.insideUnitCircle * wanderRadius;
                Vector3 candidate = spawnPoint + new Vector3(flat.x, 0f, flat.y);
                if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 3f, NavMesh.AllAreas)
                    && !RunningWater.SegmentCrossesWater(transform.position, hit.position)
                    && agent.CalculatePath(hit.position, path)
                    && path.status == NavMeshPathStatus.PathComplete)
                {
                    result = hit.position;
                    return true;
                }
            }
            result = transform.position;
            return false;
        }

        private FirstPersonController NearestVisiblePlayerInRange()
        {
            var player = NearestPlayer(out float distance);
            if (player == null || distance > manifestRange)
                return null;

            // It won't manifest across running water, and needs a clear line.
            if (RunningWater.SegmentCrossesWater(transform.position, player.transform.position))
                return null;
            Vector3 eye = transform.position + Vector3.up * 1.6f;
            Vector3 head = player.Head != null ? player.Head.position : player.transform.position + Vector3.up * 1.6f;
            if (Physics.Linecast(eye, head, out RaycastHit hit) && !hit.transform.IsChildOf(player.transform))
                return null;

            return player;
        }

        private FirstPersonController NearestPlayer(out float distance)
        {
            FirstPersonController nearest = null;
            distance = float.MaxValue;
            foreach (var p in PlayerRegistry.All)
            {
                float d = Vector3.Distance(p.transform.position, transform.position);
                if (d < distance) { distance = d; nearest = p; }
            }
            return nearest;
        }

        private void FaceInstant(Vector3 target)
        {
            Vector3 to = target - transform.position; to.y = 0f;
            if (to.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.LookRotation(to);
        }

        private void FaceToward(Vector3 target)
        {
            Vector3 to = target - transform.position; to.y = 0f;
            if (to.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation, Quaternion.LookRotation(to), 180f * Time.deltaTime);
        }

        private void SetVisible(bool visible)
        {
            if (disguiseVisual != null)
                disguiseVisual.SetActive(visible);
            else if (bodyRenderer != null)
                bodyRenderer.enabled = visible;
            if (nameTag != null)
                nameTag.gameObject.SetActive(visible);
        }

        private void ScheduleNextManifest() =>
            nextManifestTime = Time.time + Random.Range(manifestIntervalRange.x, manifestIntervalRange.y);

        /// <summary>The Mirror Binding unmakes it (called on correct-ritual win).</summary>
        public void Banish()
        {
            if (agent != null && agent.isActiveAndEnabled)
                agent.isStopped = true;
            Debug.Log("[Fetch] Bound to the glass. It holds one face at last.");
            gameObject.SetActive(false);
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(Screen.width - 270, 90, 260, 44), GUI.skin.box);
            GUILayout.Label($"Fetch: {State}");
            GUILayout.EndArea();
        }
    }
}
