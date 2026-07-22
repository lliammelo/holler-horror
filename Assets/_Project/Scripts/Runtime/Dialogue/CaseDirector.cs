using HollerHorror.Clues;
using UnityEngine;
using Yarn.Unity;
using EntityId = HollerHorror.Clues.EntityId; // Unity 6 ships a UnityEngine.EntityId

namespace HollerHorror.Dialogue
{
    /// <summary>
    /// Owns the run's case: which entity is actually active, per-run knowledge
    /// randomization (GDD §6), and the player's declaration. In M5 the
    /// declaration becomes the ritual choice with a real backfire; for M4 it
    /// reports correct/incorrect so the deduction loop is testable end-to-end.
    /// </summary>
    [RequireComponent(typeof(DialogueRunner))]
    public sealed class CaseDirector : MonoBehaviour
    {
        public static CaseDirector Instance { get; private set; }

        [SerializeField, Tooltip("The entity actually preying on the holler this run.")]
        private EntityId activeEntity = EntityId.Wendigo;

        public EntityId ActiveEntity => activeEntity;
        public bool HasDeclared { get; private set; }
        public bool DeclaredCorrectly { get; private set; }
        public EntityId DeclaredEntity { get; private set; }

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

        /// <summary>Commit to a theory. One shot per run — deduction under commitment is the game.</summary>
        public bool Declare(EntityId guess)
        {
            if (HasDeclared)
                return DeclaredCorrectly;

            HasDeclared = true;
            DeclaredEntity = guess;
            DeclaredCorrectly = guess == activeEntity;
            Debug.Log($"[CaseDirector] Declared {guess} — actual {activeEntity}: {(DeclaredCorrectly ? "CORRECT" : "WRONG")}");
            return DeclaredCorrectly;
        }
    }
}
