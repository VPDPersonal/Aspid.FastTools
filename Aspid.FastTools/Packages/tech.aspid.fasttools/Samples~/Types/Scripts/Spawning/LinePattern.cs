using UnityEngine;
using UnityEngine.Scripting;
using Aspid.FastTools.Types;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.Types.Spawning
{
    [Preserve]
    [TypeSelectorDisplay(Name = "Line", Group = "Spawn Patterns", Icon = "d_BoxCollider Icon")]
    public sealed class LinePattern : ISpawnPattern
    {
        public Vector3 GetPosition(int index, int count, float radius)
        {
            var t = count <= 1
                ? 0.5f
                : (float)index / (count - 1);

            return new Vector3(x: Mathf.Lerp(a: -radius, b: radius, t: t), y: 0f, z: radius);
        }
    }
}
