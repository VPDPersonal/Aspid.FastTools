using UnityEngine;
using Aspid.FastTools.Types;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.Types
{
    // TypeSelectorDisplay controls how the type is presented in the picker: a friendlier name, an explicit
    // group instead of the namespace path, a tooltip and a built-in editor icon.
    [TypeSelectorDisplay(
        Name = "Circle",
        Group = "Spawn Patterns",
        Tooltip = "Evenly spaced around the arena edge",
        Icon = "d_SphereCollider Icon")]
    public sealed class CirclePattern : ISpawnPattern
    {
        public Vector3 GetPosition(int index, int count, float radius)
        {
            var angle = index * Mathf.PI * 2f / Mathf.Max(count, 1);
            return new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
        }
    }
}
