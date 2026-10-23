using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Tests
{
    // A component the TypeField tests save into a prefab. Unity attaches only a runtime script whose file carries its
    // name, so it cannot live beside the editor-only tests.
    [AddComponentMenu("")]
    public sealed class TypeFieldPrefabProbe : MonoBehaviour
    {
        public string typeName;
    }
}
