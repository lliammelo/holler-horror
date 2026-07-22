using HollerHorror.Dialogue;
using UnityEngine;

namespace HollerHorror.Entities
{
    /// <summary>
    /// Bridges the night clock to the hunter and the case: maps each phase to a
    /// Wendigo aggression tier, drives ambient signs in the calm phases, and ties
    /// the case verdict to the clock (banish → dawn, backfire → straight to late
    /// night). Also the place where a dead resident's cost is applied.
    /// </summary>
    public sealed class EscalationController : MonoBehaviour
    {
        [SerializeField] private WendigoController wendigo;
        [SerializeField] private NightClock clock;

        private CaseDirector director;
        private bool backfireHandled;
        private bool winHandled;

        private void Start()
        {
            director = CaseDirector.Instance;

            if (clock != null)
                clock.PhaseChanged += OnPhaseChanged;

            foreach (var npc in NpcRegistry.All)
                npc.Died += OnNpcDied;

            ApplyTier(clock != null ? clock.Current : NightClock.Phase.MidNight);
        }

        private void OnDestroy()
        {
            if (clock != null)
                clock.PhaseChanged -= OnPhaseChanged;
            foreach (var npc in NpcRegistry.All)
                if (npc != null)
                    npc.Died -= OnNpcDied;
        }

        private void OnPhaseChanged(NightClock.Phase phase) => ApplyTier(phase);

        private void ApplyTier(NightClock.Phase phase)
        {
            if (wendigo == null)
                return;

            wendigo.Aggression = phase switch
            {
                NightClock.Phase.Dusk => AggressionTier.Passive,
                NightClock.Phase.EarlyNight => AggressionTier.Passive,
                NightClock.Phase.MidNight => AggressionTier.HuntPlayers,
                NightClock.Phase.LateNight => AggressionTier.HuntAll,
                _ => AggressionTier.HuntPlayers,
            };
        }

        private void OnNpcDied(NpcController npc)
        {
            // A dead resident takes their clues with them. If they held the name
            // (the Wendigo ritual component) and no one learned it yet, that path
            // is now closed — the team must have banked it earlier or is in trouble.
            Debug.Log($"[Escalation] {npc.NpcName} is gone. Any clue only they held is now unreachable.");
        }

        private void Update()
        {
            if (director == null || clock == null)
                return;

            if (!winHandled && director.CaseWon)
            {
                winHandled = true;
                clock.ResolveToDawn();
                if (wendigo != null)
                    wendigo.Banish();
            }

            if (!backfireHandled && director.Backfired)
            {
                backfireHandled = true;
                clock.ForcePhase(NightClock.Phase.LateNight);
            }
        }

        private void OnGUI()
        {
            if (clock == null)
                return;

            string label = clock.Current == NightClock.Phase.Dawn
                ? "DAWN"
                : $"{Readable(clock.Current)}  {Mathf.RoundToInt(clock.PhaseProgress() * 100)}%";

            GUILayout.BeginArea(new Rect(Screen.width / 2f - 90, 8, 180, 26), GUI.skin.box);
            GUILayout.Label($"The night: {label}");
            GUILayout.EndArea();
        }

        private static string Readable(NightClock.Phase phase) => phase switch
        {
            NightClock.Phase.Dusk => "Dusk",
            NightClock.Phase.EarlyNight => "Early night",
            NightClock.Phase.MidNight => "Deep night",
            NightClock.Phase.LateNight => "The dead hours",
            _ => phase.ToString(),
        };
    }
}
