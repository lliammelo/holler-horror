using UnityEngine;

namespace HollerHorror.Presentation
{
    /// <summary>
    /// Keeps world-space text (NPC nametags, surface labels) facing the viewer.
    /// Without this, TextMesh renders mirrored from behind — fine on a fixed test
    /// scene, wrong on a map you approach from any direction.
    /// </summary>
    public sealed class Billboard : MonoBehaviour
    {
        private Transform viewer;

        private void LateUpdate()
        {
            if (viewer == null)
            {
                var cam = Camera.main;
                if (cam == null)
                    return;
                viewer = cam.transform;
            }

            // Face the viewer, staying upright (no roll/pitch on the text).
            Vector3 forward = transform.position - viewer.position;
            forward.y = 0f;
            if (forward.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.LookRotation(forward);
        }
    }
}
