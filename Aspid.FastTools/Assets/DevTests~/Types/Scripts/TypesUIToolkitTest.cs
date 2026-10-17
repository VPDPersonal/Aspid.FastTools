using UnityEngine;
using Aspid.FastTools.Types;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.DevTests.Types
{
    // Dev-only UIToolkit counterpart to TypesIMGUITest: identical fields, default inspector — the
    // reference rendering the forced-IMGUI component is compared against on Prefabs/TypesDevTest.prefab.
    // Keep the two field lists in step; a field present in only one of them cannot be compared.
    public sealed class TypesUIToolkitTest : MonoBehaviour
    {
        [SerializeField] private SerializableType<MonoBehaviour> _serializableType;

        [TypeSelector(typeof(Collider))]
        [SerializeField] private SerializableType _untypedWrapper;

        [SerializeField] private SerializableType<Collider>[] _wrapperArray;

        [SerializeField] private SerializableMonoScript<MonoBehaviour> _monoScript;

        [TypeSelector(typeof(Collider))]
        [SerializeField] private string _typeSelectorString;

        [TypeSelector(typeof(ScriptableObject))]
        [SerializeField] private string[] _typeSelectorArray;

        [TypeSelector(typeof(IEnemy), Allow = TypeAllow.None)]
        [SerializeField] private string _concreteOnly;

        [TypeSelector(typeof(IEnemy), Allow = TypeAllow.Abstract)]
        [SerializeField] private string _withAbstract;

        [TypeSelector(typeof(IEnemy), Allow = TypeAllow.Interface)]
        [SerializeField] private string _withInterface;

        [TypeSelector(typeof(Collider), Required = true)]
        [SerializeField] private string _requiredType;

        [TypeSelector(typeof(IEnemy))]
        [SerializeField] private SerializableType<MonoBehaviour> _category;

        [TypeSelector(nameof(_category), Allow = TypeAllow.None)]
        [SerializeField] private string _dependentType;
    }
}
