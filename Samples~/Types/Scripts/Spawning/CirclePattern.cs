using UnityEngine;
using Aspid.FastTools.Types;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.Types
{
    // TypeSelectorDisplay customizes the picker label, group, tooltip and icon.
    /// <summary>
    /// <see cref="ISpawnPattern"/> that distributes enemies around a circle.
    /// </summary>
    [TypeSelectorDisplay(
        Name = "Circle",
        Group = "Spawn Patterns",
        Tooltip = "Evenly spaced around the arena edge",
        Icon = "d_SphereCollider Icon")]
    public sealed class CirclePattern : ISpawnPattern
    {
        /// <inheritdoc/>
        public Vector3 GetPosition(int index, int count, float radius)
        {
            var angle = index * Mathf.PI * 2f / Mathf.Max(count, 1);
            return new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
        }
    }
}
