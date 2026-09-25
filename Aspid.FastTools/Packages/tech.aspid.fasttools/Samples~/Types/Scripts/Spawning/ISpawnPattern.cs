using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.Types
{
    // SerializableType<ISpawnPattern> stores the type name; the spawner creates it at runtime.
    /// <summary>
    /// Defines positions for enemies in a wave.
    /// </summary>
    public interface ISpawnPattern
    {
        /// <summary>
        /// Returns the position of an enemy within a wave.
        /// </summary>
        /// <param name="index">Zero-based enemy index within the wave.</param>
        /// <param name="count">Positive number of enemies in the wave.</param>
        /// <param name="radius">World-space scale of the pattern.</param>
        /// <returns>Spawn position on the horizontal plane.</returns>
        Vector3 GetPosition(int index, int count, float radius);
    }
}
