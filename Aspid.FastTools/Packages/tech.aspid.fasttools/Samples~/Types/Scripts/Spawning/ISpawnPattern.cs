using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.Types.Spawning
{
    public interface ISpawnPattern
    {
        public Vector3 GetPosition(int index, int count, float radius);
    }
}
