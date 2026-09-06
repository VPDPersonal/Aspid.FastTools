using UnityEngine;
using Aspid.FastTools.Types;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.Types
{
    [TypeSelectorDisplay(
        Name = "Line",
        Group = "Spawn Patterns",
        Tooltip = "A single row along the far edge",
        Icon = "d_BoxCollider Icon")]
    public sealed class LinePattern : ISpawnPattern
    {
        public Vector3 GetPosition(int index, int count, float radius)
        {
            var t = count <= 1 ? 0.5f : (float)index / (count - 1);
            return new Vector3(Mathf.Lerp(-radius, radius, t), 0f, radius);
        }
    }
}
