using UnityEngine;
using Aspid.FastTools.Types;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.DevTests.Types
{
    // Dev-only UIToolkit counterpart to TypesIMGUITest: identical fields, default inspector — the
    // reference rendering the forced-IMGUI component is compared against on Prefabs/TypesDevTest.prefab.
    public sealed class TypesUIToolkitTest : MonoBehaviour
    {
        [SerializeField] private SerializableType<MonoBehaviour> _serializableType;

        [TypeSelector(typeof(Collider))]
        [SerializeField] private string _typeSelectorString;

        [TypeSelector(typeof(ScriptableObject))]
        [SerializeField] private string[] _typeSelectorArray;
    }
}
