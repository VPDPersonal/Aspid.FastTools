using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.Types
{
    public interface ISpawnPattern
    {
        public Vector3 GetPosition(int index, int count, float radius);
    }
}
