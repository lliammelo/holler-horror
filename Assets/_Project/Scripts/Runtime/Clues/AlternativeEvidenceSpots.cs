using UnityEngine;

namespace HollerHorror.Clues
{
    /// <summary>
    /// Keeps exactly one of several alternative evidence placements per run, so
    /// "the carcass is always behind the smokehouse" never becomes rote (GDD §8
    /// randomized evidence placement, in miniature).
    /// </summary>
    public sealed class AlternativeEvidenceSpots : MonoBehaviour
    {
        [SerializeField] private GameObject[] alternatives;

        public GameObject[] Alternatives { get => alternatives; set => alternatives = value; }

        private void Start()
        {
            if (alternatives == null || alternatives.Length == 0)
                return;

            int keep = Random.Range(0, alternatives.Length);
            for (int i = 0; i < alternatives.Length; i++)
                if (alternatives[i] != null)
                    alternatives[i].SetActive(i == keep);
        }
    }
}
