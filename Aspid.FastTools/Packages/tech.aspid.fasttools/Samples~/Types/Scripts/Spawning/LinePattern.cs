using UnityEngine;
using Aspid.FastTools.Types;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.Types
{
    /// <summary>
    /// <see cref="ISpawnPattern"/> that distributes enemies along the far arena edge.
    /// </summary>
    [TypeSelectorDisplay(
        Name = "Line",
        Group = "Spawn Patterns",
        Tooltip = "A single row along the far edge",
        Icon = "d_BoxCollider Icon")]
    public sealed class LinePattern : ISpawnPattern
    {
        /// <inheritdoc/>
        public Vector3 GetPosition(int index, int count, float radius)
        {
            var t = count <= 1 ? 0.5f : (float)index / (count - 1);
            return new Vector3(Mathf.Lerp(-radius, radius, t), 0f, radius);
        }
    }
}
