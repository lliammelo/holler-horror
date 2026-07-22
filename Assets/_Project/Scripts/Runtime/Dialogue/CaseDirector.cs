using HollerHorror.Clues;
using HollerHorror.Entities;
using HollerHorror.Rituals;
using UnityEngine;
using Yarn.Unity;
using EntityId = HollerHorror.Clues.EntityId; // Unity 6 ships a UnityEngine.EntityId

namespace HollerHorror.Dialogue
{
    /// <summary>
    /// Owns the run's case: the active entity, per-run knowledge randomization,
    /// and the ritual verdict. Preparing a ritual is a checklist (changeable);
    /// PERFORMING one is the commitment — correct banishes, wrong backfires and
    /// turns the night hostile (GDD §3.5). After a backfire the team can
    /// re-deduce and attempt the true rite.
    /// </summary>
    [RequireComponent(typeof(DialogueRunner))]
    public sealed class CaseDirector : MonoBehaviour
    {
        public static CaseDirector Instance { get; private set; }

        [SerializeField, Tooltip("The entity actually preying on the holler this run.")]
        private EntityId activeEntity = EntityId.Wendigo;

        public EntityId ActiveEntity => activeEntity;
        public EntityId PreparedRitual { get; private set; } = EntityId.Unknown;
        public bool CaseWon { get; private set; }
        public bool Backfired { get; private set; }

        private void Awake() => Instance = this;

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        private void Start()
        {
            var storage = GetComponent<DialogueRunner>().VariableStorage;
            string nameHolder = Random.value < 0.5f ? "ada" : "tetch";
            storage.SetValue("$name_holder", nameHolder);
            Debug.Log($"[CaseDirector] Active entity: {activeEntity}. Name-holder: {nameHolder}");
        }

        /// <summary>Choose which ritual to gather for. Changeable — the performance is the bet.</summary>
        public void PrepareRitual(EntityId entity)
        {
            PreparedRitual = entity;
            Debug.Log($"[CaseDirector] Preparing {RitualDefinition.DisplayName(entity)}");
        }

        public void OnRitualPerformed(EntityId ritual, Vector3 sitePosition)
        {
            if (ritual == activeEntity)
            {
                CaseWon = true;
                Debug.Log("[CaseDirector] Ritual CORRECT — banished. Dawn breaks.");
                SetDawn();
            }
            else
            {
                Backfired = true;
                PreparedRitual = EntityId.Unknown;
                Debug.Log($"[CaseDirector] Ritual WRONG ({ritual} vs {activeEntity}) — BACKFIRE.");
                SetBackfireNight(sitePosition);
            }
        }

        private static void SetDawn()
        {
            Light sun = null;
            foreach (var light in FindObjectsByType<Light>(FindObjectsInactive.Include))
                if (light.type == LightType.Directional)
                    sun = light;

            if (sun != null)
            {
                sun.color = new Color(1f, 0.85f, 0.65f);
                sun.intensity = 1.5f;
                sun.transform.rotation = Quaternion.Euler(35f, -120f, 0f);
            }
            RenderSettings.fogDensity = 0.004f;
            RenderSettings.fogColor = new Color(0.75f, 0.72f, 0.65f);
        }

        private void SetBackfireNight(Vector3 sitePosition)
        {
            foreach (var light in FindObjectsByType<Light>(FindObjectsInactive.Include))
                if (light.type == LightType.Directional)
                {
                    light.color = new Color(0.35f, 0.25f, 0.45f);
                    light.intensity = 0.35f;
                }
            RenderSettings.fogDensity = 0.03f;
            RenderSettings.fogColor = new Color(0.12f, 0.10f, 0.16f);

            // Something answers from the dark — twice, from different directions.
            WendigoAudio.PlayScreamAt(sitePosition + new Vector3(25f, 0f, 10f), loud: true);
            WendigoAudio.PlayScreamAt(sitePosition + new Vector3(-30f, 0f, -20f), loud: false);
        }

        private void OnGUI()
        {
            if (CaseWon)
            {
                DrawBanner("DAWN BREAKS", "The words held. The holler is quiet for the first time in weeks.\nCase closed.");
                return;
            }

            if (Backfired)
            {
                DrawBanner("THE NIGHT TURNS", $"That rite was not for the thing out there.\nIt knows your voices now. Re-read the board — and be right this time.");
            }

            // Persistent objective hint once a rite is prepared, so the hand-off
            // from board to world is never a mystery.
            if (PreparedRitual != EntityId.Unknown)
            {
                var style = new GUIStyle(GUI.skin.box) { fontSize = 12, alignment = TextAnchor.MiddleCenter };
                GUI.Box(new Rect(Screen.width / 2f - 220, Screen.height - 30, 440, 24),
                    $"Rite prepared: {RitualDefinition.DisplayName(PreparedRitual)} — perform at {RitualDefinition.SiteDescription(PreparedRitual)} (follow the beam)",
                    style);
            }
        }

        private static void DrawBanner(string title, string body)
        {
            var style = new GUIStyle(GUI.skin.box)
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleCenter,
            };
            GUI.Box(new Rect(Screen.width / 2f - 240, 24, 480, 78), $"{title}\n{body}", style);
        }
    }
}
