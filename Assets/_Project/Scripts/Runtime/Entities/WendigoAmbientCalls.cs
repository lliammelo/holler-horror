using UnityEngine;

namespace HollerHorror.Entities
{
    /// <summary>
    /// Faint scream-calls echoing down the valley while the Wendigo patrols —
    /// the environmental "sign" players use to deduce which entity is active.
    /// </summary>
    [RequireComponent(typeof(WendigoController))]
    public sealed class WendigoAmbientCalls : MonoBehaviour
    {
        [SerializeField] private Vector2 callIntervalRange = new(22f, 45f);

        private WendigoController wendigo;
        private float nextCallTime;

        private void Awake()
        {
            wendigo = GetComponent<WendigoController>();
            ScheduleNext();
        }

        private void Update()
        {
            if (Time.time < nextCallTime)
                return;

            ScheduleNext();
            if (wendigo.State == WendigoState.Patrol || wendigo.State == WendigoState.Search)
                WendigoAudio.PlayScreamAt(transform.position, loud: false);
        }

        private void ScheduleNext() =>
            nextCallTime = Time.time + Random.Range(callIntervalRange.x, callIntervalRange.y);
    }
}
