using UnityEngine;

namespace Aspid.FastTools.Samples.Types
{
    public interface ISpawnPattern
    {
        Vector3 GetPosition(int index, int count, float radius);
    }
}
