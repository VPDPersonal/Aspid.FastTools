using UnityEngine;
using Aspid.FastTools.Types;

namespace Aspid.FastTools.Samples.Types
{
    [TypeSelectorDisplay(Name = "Circle", Group = "Spawn Patterns", Icon = "d_SphereCollider Icon")]
    public sealed class CirclePattern : ISpawnPattern
    {
        public Vector3 GetPosition(int index, int count)
        {
            var angle = 2f * Mathf.PI * index / count;
            return new Vector3(x: Mathf.Cos(angle), y: 0f, z: Mathf.Sin(angle)) * 4f;
        }
    }
}
