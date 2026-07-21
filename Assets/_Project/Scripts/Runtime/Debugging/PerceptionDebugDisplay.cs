using HollerHorror.Senses;
using UnityEngine;

namespace HollerHorror.Debugging
{
    /// <summary>
    /// Makes an EntityPerception readable at a glance: tints the body by state
    /// (grey → amber → red), turns to face what it heard/sees, and shows a
    /// suspicion readout. Stand-in for real entity AI until M3.
    /// </summary>
    public sealed class PerceptionDebugDisplay : MonoBehaviour
    {
        [SerializeField] private EntityPerception perception;
        [SerializeField] private Renderer body;
        [SerializeField] private float turnSpeed = 120f;
        [SerializeField, Tooltip("Disable when something else (e.g. a NavMeshAgent) drives rotation.")]
        private bool controlRotation = true;

        private MaterialPropertyBlock block;
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        private void Awake() => block = new MaterialPropertyBlock();

        private void Update()
        {
            if (perception == null)
                return;

            TintByState();
            if (controlRotation)
                FacePointOfInterest();
        }

        private void TintByState()
        {
            if (body == null)
                return;

            Color color = perception.State switch
            {
                AwarenessState.Alert => new Color(0.85f, 0.15f, 0.1f),
                AwarenessState.Suspicious => new Color(0.95f, 0.7f, 0.15f),
                _ => new Color(0.45f, 0.45f, 0.5f),
            };
            block.SetColor(BaseColorId, color);
            body.SetPropertyBlock(block);
        }

        private void FacePointOfInterest()
        {
            Vector3? focus = perception.VisibleTarget != null
                ? perception.VisibleTarget.transform.position
                : perception.LastHeardPosition;

            if (focus == null || perception.State == AwarenessState.Unaware)
                return;

            Vector3 to = focus.Value - transform.position;
            to.y = 0f;
            if (to.sqrMagnitude < 0.01f)
                return;

            var targetRotation = Quaternion.LookRotation(to);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }

        private void OnGUI()
        {
            if (perception == null)
                return;

            GUILayout.BeginArea(new Rect(Screen.width - 270, 10, 260, 70), GUI.skin.box);
            GUILayout.Label($"Entity: {perception.State}");
            GUILayout.Label($"Suspicion: {perception.Suspicion:F2}" +
                            (perception.VisibleTarget != null ? "  [SEES YOU]" : ""));
            GUILayout.EndArea();
        }
    }
}
