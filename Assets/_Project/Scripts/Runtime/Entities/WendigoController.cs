using HollerHorror.Player;
using HollerHorror.Senses;
using UnityEngine;
using UnityEngine.AI;

namespace HollerHorror.Entities
{
    public enum WendigoState { Patrol, Investigate, Chase, Search, Feeding, StalkNpc }

    /// <summary>How bold the Wendigo is, driven by the night clock (GDD §3 escalation).</summary>
    public enum AggressionTier
    {
        Passive = 0,   // dusk/early: signs and stalking only — investigates but won't chase
        HuntPlayers = 1, // mid: full pursuit of players
        HuntAll = 2,   // late: also drags off unsheltered residents
    }

    /// <summary>
    /// M3: the relentless pursuer (GDD §4.1). State machine driven entirely by
    /// EntityPerception — this class decides movement/attacks, never does its own
    /// sensing. Faster than a sprinting player in the open; the counterplay is
    /// silence and breaking line of sight, which drops it into Search.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(EntityPerception))]
    public sealed class WendigoController : MonoBehaviour, IBanishable
    {
        [Header("Speeds (player sprints at 6.2)")]
        [SerializeField] private float patrolSpeed = 2.2f;
        [SerializeField] private float investigateSpeed = 3.8f;
        [SerializeField] private float chaseSpeed = 7.0f;
        [SerializeField] private float searchSpeed = 3.2f;

        [Header("Patrol")]
        [SerializeField, Tooltip("Wander radius around spawn point.")]
        private float patrolRadius = 28f;
        [SerializeField] private Vector2 patrolPauseRange = new(1.5f, 4f);

        [Header("Investigate / Search")]
        [SerializeField, Tooltip("Seconds scanning at an investigated position before losing interest.")]
        private float investigateScanSeconds = 3.5f;
        [SerializeField] private float searchPointRadius = 7f;
        [SerializeField] private int searchPoints = 3;

        [Header("Chase / Attack")]
        [SerializeField, Tooltip("Seconds without sight before a chase degrades to Search.")]
        private float lostSightGraceSeconds = 2.5f;
        [SerializeField, Tooltip("Scream windup on first lock-on: it stands and shrieks before sprinting — the player's head start.")]
        private float screamWindupSeconds = 1.6f;
        [SerializeField, Tooltip("Noises quieter than this (crouch steps) don't re-feed an ongoing chase.")]
        private float chaseRefeedMinLoudness = 0.45f;
        [SerializeField] private float attackRange = 1.8f;
        [SerializeField, Tooltip("Seconds spent over a downed player before wandering off.")]
        private float feedingSeconds = 3f;

        private NavMeshAgent agent;
        private EntityPerception perception;
        private Vector3 spawnPoint;

        private FirstPersonController target;
        private Vector3 lastKnownTargetPosition;
        private float lastSeenTime;
        private float stateTimer;
        private int searchPointsLeft;
        private float windupUntil;
        private Dialogue.NpcController npcPrey;

        public WendigoState State { get; private set; } = WendigoState.Patrol;

        /// <summary>Set each frame by the escalation controller from the night phase.</summary>
        public AggressionTier Aggression { get; set; } = AggressionTier.HuntPlayers;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            perception = GetComponent<EntityPerception>();
            spawnPoint = transform.position;
        }

        private void OnEnable() => perception.Heard += OnHeardNoise;
        private void OnDisable() => perception.Heard -= OnHeardNoise;

        private void OnHeardNoise(NoiseEvent noiseEvent)
        {
            // Mid-chase, sound keeps the pursuit locked on even without sight —
            // running loud never shakes it; going quiet (crouch) does.
            if (State == WendigoState.Chase && perception.VisibleTarget == null
                && noiseEvent.Loudness >= chaseRefeedMinLoudness)
            {
                lastKnownTargetPosition = noiseEvent.Position;
                lastSeenTime = Time.time - lostSightGraceSeconds * 0.5f; // extends the chase, decaying
            }
        }

        private void Start()
        {
            agent.Warp(transform.position); // NavMesh is runtime-baked in Awake by NavMeshBootstrap
            EnterPatrol();
        }

        /// <summary>The correct ritual unmakes it — stop hunting and leave the holler.</summary>
        public void Banish()
        {
            if (agent != null && agent.isActiveAndEnabled)
                agent.isStopped = true;
            Debug.Log("[Wendigo] The name burned. It is unmade.");
            gameObject.SetActive(false);
        }

        private void Update()
        {
            switch (State)
            {
                case WendigoState.Patrol: TickPatrol(); break;
                case WendigoState.Investigate: TickInvestigate(); break;
                case WendigoState.Chase: TickChase(); break;
                case WendigoState.Search: TickSearch(); break;
                case WendigoState.Feeding: TickFeeding(); break;
                case WendigoState.StalkNpc: TickStalkNpc(); break;
            }
        }

        // ---- Patrol ----

        private void EnterPatrol()
        {
            State = WendigoState.Patrol;
            agent.speed = patrolSpeed;
            stateTimer = 0f;
            PickPatrolPoint();
        }

        private void TickPatrol()
        {
            if (TryEscalate())
                return;

            // Late night with no player threat: go hunt a resident.
            if (Aggression == AggressionTier.HuntAll && TryStalkNpc())
                return;

            if (!agent.pathPending && agent.remainingDistance < 0.6f)
            {
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0f)
                    PickPatrolPoint();
            }
        }

        private void PickPatrolPoint()
        {
            stateTimer = Random.Range(patrolPauseRange.x, patrolPauseRange.y);
            if (TryRandomNavPoint(spawnPoint, patrolRadius, out Vector3 point))
                agent.SetDestination(point);
        }

        // ---- Investigate ----

        private void EnterInvestigate(Vector3 position)
        {
            State = WendigoState.Investigate;
            agent.speed = investigateSpeed;
            stateTimer = investigateScanSeconds;
            agent.SetDestination(position);
        }

        private void TickInvestigate()
        {
            if (TryEscalate())
                return;

            // Fresher noise re-aims the investigation.
            if (perception.LastHeardPosition.HasValue &&
                Vector3.Distance(agent.destination, perception.LastHeardPosition.Value) > 1.5f)
                agent.SetDestination(perception.LastHeardPosition.Value);

            if (!agent.pathPending && agent.remainingDistance < 1.2f)
            {
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0f || perception.State == AwarenessState.Unaware)
                    EnterPatrol();
            }
        }

        // ---- Chase ----

        private void EnterChase(FirstPersonController victim)
        {
            bool firstLock = State != WendigoState.Chase;
            State = WendigoState.Chase;
            agent.speed = chaseSpeed;
            target = victim;
            lastSeenTime = Time.time;

            if (firstLock)
            {
                // The scream is the player's warning AND their head start.
                WendigoAudio.PlayScreamAt(transform.position, loud: true);
                windupUntil = Time.time + screamWindupSeconds;
                lastKnownTargetPosition = victim.transform.position;
                agent.ResetPath();
            }
        }

        private void TickChase()
        {
            if (Time.time < windupUntil)
            {
                // Rooted mid-scream; track the target with our eyes only.
                if (perception.VisibleTarget != null)
                {
                    lastKnownTargetPosition = perception.VisibleTarget.transform.position;
                    lastSeenTime = Time.time;
                    Vector3 to = lastKnownTargetPosition - transform.position;
                    to.y = 0f;
                    if (to.sqrMagnitude > 0.01f)
                        transform.rotation = Quaternion.RotateTowards(
                            transform.rotation, Quaternion.LookRotation(to), 360f * Time.deltaTime);
                }
                return;
            }

            if (perception.VisibleTarget != null)
            {
                target = perception.VisibleTarget;
                lastSeenTime = Time.time;
                lastKnownTargetPosition = target.transform.position;
            }

            agent.SetDestination(lastKnownTargetPosition);

            if (target != null && Vector3.Distance(transform.position, target.transform.position) <= attackRange)
            {
                var health = target.GetComponent<PlayerHealth>();
                if (health != null && !health.IsDowned)
                {
                    health.Down(transform);
                    EnterFeeding();
                    return;
                }
            }

            bool lostSight = Time.time - lastSeenTime > lostSightGraceSeconds;
            if (lostSight && !agent.pathPending && agent.remainingDistance < 1.2f)
                EnterSearch();
        }

        // ---- Search ----

        private void EnterSearch()
        {
            State = WendigoState.Search;
            agent.speed = searchSpeed;
            searchPointsLeft = searchPoints;
            NextSearchPoint();
        }

        private void TickSearch()
        {
            if (TryEscalate())
                return;

            if (!agent.pathPending && agent.remainingDistance < 0.8f)
            {
                stateTimer -= Time.deltaTime;
                if (stateTimer > 0f)
                    return;

                searchPointsLeft--;
                if (searchPointsLeft <= 0)
                    EnterPatrol();
                else
                    NextSearchPoint();
            }
        }

        private void NextSearchPoint()
        {
            stateTimer = 1.5f;
            if (TryRandomNavPoint(lastKnownTargetPosition, searchPointRadius, out Vector3 point))
                agent.SetDestination(point);
        }

        // ---- Feeding (over a downed player) ----

        private void EnterFeeding()
        {
            State = WendigoState.Feeding;
            agent.ResetPath();
            stateTimer = feedingSeconds;
        }

        private void TickFeeding()
        {
            stateTimer -= Time.deltaTime;
            if (stateTimer <= 0f)
            {
                target = null;
                perception.ResetSuspicion(); // sated — starts cold again
                EnterPatrol();

                // Leave the kill site: head somewhere clearly away from the body.
                for (int i = 0; i < 6; i++)
                {
                    if (TryRandomNavPoint(spawnPoint, patrolRadius, out Vector3 retreat)
                        && Vector3.Distance(retreat, transform.position) > 18f)
                    {
                        agent.SetDestination(retreat);
                        break;
                    }
                }
            }
        }

        // ---- Shared ----

        /// <summary>Escalates to Chase (sight) or Investigate (sound). True if state changed.</summary>
        private bool TryEscalate()
        {
            // Passive (dusk/early): it stalks and investigates but does not commit to a kill.
            if (Aggression != AggressionTier.Passive &&
                perception.State == AwarenessState.Alert && perception.VisibleTarget != null)
            {
                EnterChase(perception.VisibleTarget);
                return true;
            }

            if (State != WendigoState.Investigate &&
                perception.State >= AwarenessState.Suspicious && perception.LastHeardPosition.HasValue)
            {
                EnterInvestigate(perception.LastHeardPosition.Value);
                return true;
            }

            return false;
        }

        // ---- NPC stalking (late night) ----

        private bool TryStalkNpc()
        {
            var prey = NearestUnshelteredNpc();
            if (prey == null)
                return false;

            npcPrey = prey;
            State = WendigoState.StalkNpc;
            agent.speed = investigateSpeed;
            agent.SetDestination(prey.transform.position);
            return true;
        }

        private void TickStalkNpc()
        {
            // A player threat always takes priority over hunting a resident.
            if (TryEscalate())
                return;

            // Prey died, was sheltered, or we dropped to a calmer tier: abandon the hunt.
            if (npcPrey == null || !npcPrey.IsAlive || npcPrey.IsSheltered() || Aggression != AggressionTier.HuntAll)
            {
                npcPrey = null;
                EnterPatrol();
                return;
            }

            agent.SetDestination(npcPrey.transform.position);
            if (Vector3.Distance(transform.position, npcPrey.transform.position) <= attackRange)
            {
                WendigoAudio.PlayScreamAt(transform.position, loud: true);
                npcPrey.Kill();
                npcPrey = null;
                EnterFeeding();
            }
        }

        private Dialogue.NpcController NearestUnshelteredNpc()
        {
            Dialogue.NpcController nearest = null;
            float best = float.MaxValue;
            foreach (var npc in Dialogue.NpcRegistry.All)
            {
                if (npc == null || !npc.IsAlive || npc.IsSheltered())
                    continue;
                float d = Vector3.Distance(transform.position, npc.transform.position);
                if (d < best) { best = d; nearest = npc; }
            }
            return nearest;
        }

        /// <summary>Random reachable point near center — rejects unreachable spots (wall tops, sealed platforms).</summary>
        private bool TryRandomNavPoint(Vector3 center, float radius, out Vector3 result)
        {
            var path = new NavMeshPath();
            for (int i = 0; i < 12; i++)
            {
                Vector2 flat = Random.insideUnitCircle * radius;
                Vector3 candidate = center + new Vector3(flat.x, 0f, flat.y);
                if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 3f, NavMesh.AllAreas)
                    && agent.CalculatePath(hit.position, path)
                    && path.status == NavMeshPathStatus.PathComplete)
                {
                    result = hit.position;
                    return true;
                }
            }
            result = center;
            return false;
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(Screen.width - 270, 90, 260, 44), GUI.skin.box);
            GUILayout.Label($"Wendigo: {State} [{Aggression}]  (spd {agent.velocity.magnitude:F1})");
            GUILayout.EndArea();
        }
    }
}
