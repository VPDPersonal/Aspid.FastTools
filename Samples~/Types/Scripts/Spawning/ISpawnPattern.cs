using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.Types
{
    // Plain C# strategy stored through SerializableType<ISpawnPattern>: no MonoBehaviour, no ScriptableObject,
    // just a type name in the scene file and Activator.CreateInstance at runtime.
    public interface ISpawnPattern
    {
        Vector3 GetPosition(int index, int count, float radius);
    }
}
