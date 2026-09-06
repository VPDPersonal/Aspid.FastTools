using UnityEngine;
using Aspid.FastTools.Types;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.Types
{
    [TypeSelectorDisplay(
        Name = "Grid",
        Group = "Spawn Patterns",
        Tooltip = "A square block behind the far edge",
        Icon = "d_Grid Icon")]
    public sealed class GridPattern : ISpawnPattern
    {
        public Vector3 GetPosition(int index, int count, float radius)
        {
            var columns = Mathf.CeilToInt(Mathf.Sqrt(count));
            var spacing = radius * 2f / Mathf.Max(columns - 1, 1);
            var x = -radius + index % columns * spacing;
            var z = radius + index / columns * spacing;
            return new Vector3(x, 0f, z);
        }
    }
}
