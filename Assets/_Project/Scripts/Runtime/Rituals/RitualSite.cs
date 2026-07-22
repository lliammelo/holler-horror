using HollerHorror.Clues;
using HollerHorror.Dialogue;
using HollerHorror.Player;
using UnityEngine;
using UnityEngine.InputSystem;
using EntityId = HollerHorror.Clues.EntityId;

namespace HollerHorror.Rituals
{
    /// <summary>
    /// A place where a ritual can be performed. The sequence is multi-step and
    /// interruptible (GDD §3.5): place components (hold E), hold the circle
    /// (stay close), speak the words (hold E). Stepping away resets the current
    /// step, not the whole rite. Completion hands the verdict to CaseDirector —
    /// banishment or backfire.
    /// </summary>
    public sealed class RitualSite : MonoBehaviour
    {
        [SerializeField] private RitualSiteKind kind = RitualSiteKind.FirstKillSite;
        [SerializeField] private float interactDistance = 3f;
        [SerializeField] private float circleRadius = 2.5f;

        public RitualSiteKind Kind { get => kind; set => kind = value; }

        private bool performing;
        private int step;
        private float stepProgress;
        private Renderer beacon;

        private void Awake()
        {
            var beaconTransform = transform.Find("Beacon");
            if (beaconTransform != null)
                beacon = beaconTransform.GetComponent<Renderer>();
        }

        /// <summary>Only the prepared rite's beam shows, so "follow the beam" is unambiguous.
        /// Before any rite is prepared, every site's beam is lit for discovery.</summary>
        private void UpdateBeacon(CaseDirector director)
        {
            if (beacon == null)
                return;
            bool show = director.PreparedRitual == EntityId.Unknown
                        || RitualDefinition.SiteOf(director.PreparedRitual) == kind;
            if (beacon.enabled != show)
                beacon.enabled = show;
        }

        private FirstPersonController NearestPlayer()
        {
            FirstPersonController nearest = null;
            float best = float.MaxValue;
            foreach (var player in PlayerRegistry.All)
            {
                float d = Vector3.Distance(player.transform.position, transform.position);
                if (d < best) { best = d; nearest = player; }
            }
            return nearest;
        }

        private void Update()
        {
            var director = CaseDirector.Instance;
            if (director == null || director.CaseWon)
                return;

            UpdateBeacon(director);

            var player = NearestPlayer();
            if (player == null)
                return;
            float distance = Vector3.Distance(player.transform.position, transform.position);

            if (!performing)
            {
                bool ready = director.PreparedRitual != EntityId.Unknown
                             && RitualDefinition.SiteOf(director.PreparedRitual) == kind
                             && HasAllComponents(director.PreparedRitual);
                if (ready && distance <= interactDistance
                    && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
                {
                    performing = true;
                    step = 0;
                    stepProgress = 0f;
                }
                return;
            }

            // --- performing ---
            if (distance > circleRadius)
            {
                stepProgress = 0f; // broke the circle: current step resets
                return;
            }

            bool holdStep = step != 1; // steps 0 and 2 need E held; step 1 needs presence
            bool advancing = !holdStep || (Keyboard.current != null && Keyboard.current.eKey.isPressed);

            if (advancing)
            {
                stepProgress += Time.deltaTime;
                if (stepProgress >= RitualDefinition.StepDurations[step])
                {
                    step++;
                    stepProgress = 0f;
                    if (step >= RitualDefinition.StepPrompts.Length)
                        Complete();
                }
            }

            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                performing = false;
                stepProgress = 0f;
            }
        }

        private static bool HasAllComponents(EntityId ritual)
        {
            foreach (var item in RitualDefinition.ComponentsOf(ritual))
                if (!RitualInventory.Has(item))
                    return false;
            return true;
        }

        private void Complete()
        {
            performing = false;
            var director = CaseDirector.Instance;
            var ritual = director.PreparedRitual;
            RitualInventory.Consume(RitualDefinition.ComponentsOf(ritual));
            director.OnRitualPerformed(ritual, transform.position);
        }

        private void OnGUI()
        {
            var director = CaseDirector.Instance;
            if (director == null || director.CaseWon)
                return;

            var player = NearestPlayer();
            if (player == null || Vector3.Distance(player.transform.position, transform.position) > interactDistance)
                return;

            GUILayout.BeginArea(new Rect(Screen.width / 2f - 190, Screen.height * 0.48f, 380, 110), GUI.skin.box);

            if (performing)
            {
                string prompt = RitualDefinition.StepPrompts[step];
                float duration = RitualDefinition.StepDurations[step];
                string action = step == 1 ? "(stay inside the circle)" : "(hold E)";
                GUILayout.Label($"{RitualDefinition.DisplayName(director.PreparedRitual)} — step {step + 1}/3");
                GUILayout.Label($"{prompt} {action}");
                GUILayout.Label(Bar(stepProgress / duration));
            }
            else if (director.PreparedRitual == EntityId.Unknown)
            {
                GUILayout.Label($"A ritual site: {RitualDefinition.SiteDescription(SiteEntity())}.");
                GUILayout.Label("Prepare a ritual at the clue board first.");
            }
            else if (RitualDefinition.SiteOf(director.PreparedRitual) != kind)
            {
                GUILayout.Label($"This is not the place for {RitualDefinition.DisplayName(director.PreparedRitual)}.");
                GUILayout.Label($"That rite belongs at {RitualDefinition.SiteDescription(director.PreparedRitual)}.");
            }
            else if (!HasAllComponents(director.PreparedRitual))
            {
                GUILayout.Label($"{RitualDefinition.DisplayName(director.PreparedRitual)} — components missing:");
                foreach (var item in RitualDefinition.ComponentsOf(director.PreparedRitual))
                    if (!RitualInventory.Has(item))
                        GUILayout.Label($"  • {RitualDefinition.ItemDisplayName(item)}");
            }
            else
            {
                GUILayout.Label($"[E] Begin {RitualDefinition.DisplayName(director.PreparedRitual)}");
                GUILayout.Label("There is no taking it back once the words are spoken.");
            }

            GUILayout.EndArea();
        }

        private EntityId SiteEntity() => kind switch
        {
            RitualSiteKind.FirstKillSite => EntityId.Wendigo,
            RitualSiteKind.Crossroads => EntityId.Fetch,
            _ => EntityId.Hollow,
        };

        private static string Bar(float t)
        {
            int filled = Mathf.RoundToInt(Mathf.Clamp01(t) * 24);
            return "[" + new string('#', filled) + new string('-', 24 - filled) + "]";
        }
    }
}
