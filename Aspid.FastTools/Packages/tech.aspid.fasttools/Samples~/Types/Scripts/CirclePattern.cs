using UnityEngine;
using Aspid.FastTools.Types;

namespace Aspid.FastTools.Samples.Types
{
    [TypeSelectorDisplay(Name = "Circle", Group = "Spawn Patterns", Icon = "d_SphereCollider Icon")]
    public sealed class CirclePattern : ISpawnPattern
    {
        public Vector3 GetPosition(int index, int count, float radius)
        {
            var angle = index * Mathf.PI * 2f / Mathf.Max(a: count, b: 1);
            return new Vector3(x: Mathf.Cos(angle), y: 0f, z: Mathf.Sin(angle)) * radius;
        }
    }
}
