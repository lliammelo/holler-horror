using Unity.AI.Navigation;
using UnityEngine;

namespace HollerHorror.Entities
{
    /// <summary>
    /// Runtime-bakes the NavMesh on scene load. Greybox scenes are generated from
    /// code, so baking at startup always matches the geometry — no stale baked
    /// assets. Real maps (M4+) will move to build-time baking.
    /// </summary>
    [RequireComponent(typeof(NavMeshSurface))]
    public sealed class NavMeshBootstrap : MonoBehaviour
    {
        private void Awake()
        {
            var surface = GetComponent<NavMeshSurface>();
            float start = Time.realtimeSinceStartup;
            surface.BuildNavMesh();
            Debug.Log($"[NavMeshBootstrap] Baked NavMesh in {(Time.realtimeSinceStartup - start) * 1000f:F0}ms");
        }
    }
}
