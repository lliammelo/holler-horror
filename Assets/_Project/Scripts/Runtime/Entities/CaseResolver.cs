using HollerHorror.Dialogue;
using UnityEngine;

namespace HollerHorror.Entities
{
    /// <summary>
    /// Banishes the run's entity when the correct ritual resolves the case. Used
    /// in scenes without the full Wendigo EscalationController (e.g. the Fetch
    /// case, whose entity doesn't hunt on the night clock). Reference any
    /// component that implements IBanishable.
    /// </summary>
    public sealed class CaseResolver : MonoBehaviour
    {
        [SerializeField, Tooltip("A component implementing IBanishable (WendigoController / FetchController).")]
        private MonoBehaviour entityBehaviour;

        private IBanishable entity;
        private bool handled;

        private void Awake() => entity = entityBehaviour as IBanishable;

        private void Update()
        {
            if (handled || CaseDirector.Instance == null || !CaseDirector.Instance.CaseWon)
                return;

            handled = true;
            entity?.Banish();
        }
    }
}
