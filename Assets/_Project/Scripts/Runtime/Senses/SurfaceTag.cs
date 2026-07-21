using UnityEngine;

namespace HollerHorror.Senses
{
    /// <summary>
    /// Tag any walkable collider (or its parent) with this to define its surface.
    /// NOTE: must stay in its own SurfaceTag.cs — Unity can't serialize a
    /// MonoBehaviour whose file name doesn't match the class name.
    /// </summary>
    public sealed class SurfaceTag : MonoBehaviour
    {
        [SerializeField] private SurfaceType type = SurfaceType.Grass;
        public SurfaceType Type { get => type; set => type = value; }
    }
}
