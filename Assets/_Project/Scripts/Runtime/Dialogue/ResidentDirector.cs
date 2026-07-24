using System.Collections.Generic;
using UnityEngine;
using EntityId = HollerHorror.Clues.EntityId;

namespace HollerHorror.Dialogue
{
    /// <summary>
    /// Deals the resident cast their dispositions and knowledge for the run
    /// (GDD §6 randomized knowledge assignment). Most residents are honest but
    /// only partial; a couple are mistaken (sincere false leads), one withholds,
    /// and — rarely — one lies. The true clues about the active entity are spread
    /// across the honest/withholding residents, so no single interview solves it.
    /// Runs in Start, after CaseGenerator has rolled the entity in Awake.
    /// </summary>
    public sealed class ResidentDirector : MonoBehaviour
    {
        [SerializeField] private CaseDirector director;
        [SerializeField, Tooltip("How many of the cast sincerely misattribute the signs.")]
        private int mistakenCount = 1;
        [SerializeField, Tooltip("How many withhold until you've done some legwork.")]
        private int withholdingCount = 1;
        [SerializeField, Tooltip("How many actively lie (0-1 keeps it rare).")]
        private int lyingCount = 1;

        private void Start()
        {
            var entity = director != null ? director.ActiveEntity : EntityId.Wendigo;

            var residents = new List<Resident>(FindObjectsByType<Resident>(FindObjectsInactive.Exclude));
            Shuffle(residents);
            if (residents.Count == 0)
                return;

            var trueFacts = TestimonyPool.For(entity);
            var falseFacts = TestimonyPool.NotImplicating(entity);
            Shuffle(trueFacts);
            Shuffle(falseFacts);

            int mistaken = mistakenCount, withholding = withholdingCount, lying = lyingCount;
            int trueIndex = 0, falseIndex = 0;

            foreach (var resident in residents)
            {
                Reliability disposition;
                if (lying-- > 0) disposition = Reliability.Lying;
                else if (mistaken-- > 0) disposition = Reliability.Mistaken;
                else if (withholding-- > 0) disposition = Reliability.Withholding;
                else disposition = Reliability.Honest;

                // Liars and the mistaken point at an entity that isn't here;
                // everyone else carries a true fragment about the one that is.
                bool wrong = disposition == Reliability.Lying || disposition == Reliability.Mistaken;
                var pool = wrong ? falseFacts : trueFacts;
                if (pool.Count == 0)
                    continue; // fell short of facts — silent rather than repeating

                int cursor = wrong ? falseIndex++ : trueIndex++;
                resident.Assign(disposition, pool[cursor % pool.Count]);
            }

            Debug.Log($"[ResidentDirector] Dealt {residents.Count} residents for a {entity} case.");
        }

        private static void Shuffle<T>(IList<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
