using UnityEngine;
using Aspid.FastTools.Types;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.Types
{
    // Hidden excludes this pattern from the picker without invalidating stored references.
    /// <summary>
    /// <see cref="ISpawnPattern"/> that places every enemy at the origin.
    /// </summary>
    [TypeSelectorDisplay(Hidden = true)]
    public sealed class OriginPattern : ISpawnPattern
    {
        /// <inheritdoc/>
        public Vector3 GetPosition(int index, int count, float radius) => Vector3.zero;
    }
}
