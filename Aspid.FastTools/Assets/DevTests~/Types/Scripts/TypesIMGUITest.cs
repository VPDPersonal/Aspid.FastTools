using UnityEngine;
using Aspid.FastTools.Types;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.DevTests.Types
{
    // Dev-only harness for the IMGUI rendering path of the Type drawers — NOT part of the package or
    // its samples. The companion editor (Editor/TypesIMGUITestEditor.cs) forces the whole inspector
    // through IMGUI, routing the fields below through TypeIMGUIPropertyDrawer.OnGUI and
    // MonoScriptIMGUIPropertyDrawer.OnGUI instead of the UIToolkit CreatePropertyGUI path.
    //
    // The field list mirrors what the Types sample demonstrates in UI Toolkit, so anything the sample
    // covers has an IMGUI counterpart here.
    //
    // To test: open Prefabs/TypesDevTest.prefab — this component sits next to TypesUIToolkitTest
    // (same fields, default UIToolkit inspector) for side-by-side comparison, and a FastEnemy child
    // covers the forced-IMGUI ComponentTypeSelector swap (see EnemyBase / EnemyBaseEditor).
    public sealed class TypesIMGUITest : MonoBehaviour
    {
        // SerializableType<T>: strongly typed wrapper — the generic argument constrains the picker.
        [SerializeField] private SerializableType<MonoBehaviour> _serializableType;

        // Non-generic wrapper: the attribute carries the only constraint, and BaseType stays typeof(object).
        [TypeSelector(typeof(Collider))]
        [SerializeField] private SerializableType _untypedWrapper;

        // Wrappers inside a collection: each element gets its own picker row.
        [SerializeField] private SerializableType<Collider>[] _wrapperArray;

        // MonoScript-backed wrapper: only types a MonoScript resolves are offered, and a .cs file can
        // be dragged onto the field from Project.
        [SerializeField] private SerializableMonoScript<MonoBehaviour> _monoScript;

        // [TypeSelector] on a raw string: the same picker window on an un-wrapped assembly-qualified name.
        [TypeSelector(typeof(Collider))]
        [SerializeField] private string _typeSelectorString;

        // [TypeSelector] on a string[]: each element is its own picker constrained to the base type.
        [TypeSelector(typeof(ScriptableObject))]
        [SerializeField] private string[] _typeSelectorArray;

        // The three Allow fields share one constraint and differ only in what the picker adds to the
        // concrete FastEnemy and TankEnemy: nothing, the abstract EnemyBase, or the IEnemy interface.
        [TypeSelector(typeof(IEnemy), Allow = TypeAllow.None)]
        [SerializeField] private string _concreteOnly;

        [TypeSelector(typeof(IEnemy), Allow = TypeAllow.Abstract)]
        [SerializeField] private string _withAbstract;

        [TypeSelector(typeof(IEnemy), Allow = TypeAllow.Interface)]
        [SerializeField] private string _withInterface;

        // Left empty on the prefab on purpose: the inline "required" notice is drawn by
        // InspectorNoticeGUI here and by the InspectorNotice element in the UIToolkit twin.
        [TypeSelector(typeof(Collider), Required = true)]
        [SerializeField] private string _requiredType;

        // Member reference: _dependentType follows whatever _category currently holds, and falls back
        // to a notice while the constraint cannot be resolved.
        [TypeSelector(typeof(IEnemy))]
        [SerializeField] private SerializableType<MonoBehaviour> _category;

        [TypeSelector(nameof(_category), Allow = TypeAllow.None)]
        [SerializeField] private string _dependentType;
    }
}
