using UnityEngine;
using Aspid.FastTools.Types;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.DevTests.Types
{
    // Dev-only harness for the IMGUI rendering path of the Type drawers — NOT part of the
    // package or its samples. The companion editor (Editor/TypesIMGUITestEditor.cs) forces the
    // whole inspector through IMGUI, routing SerializableType<T> and [TypeSelector] fields through
    // TypeIMGUIPropertyDrawer.OnGUI instead of the UIToolkit CreatePropertyGUI path.
    //
    // To test: open Prefabs/TypesDevTest.prefab — this component sits next to TypesUIToolkitTest
    // (same fields, default UIToolkit inspector) for side-by-side comparison, and a FastEnemy child
    // covers the forced-IMGUI ComponentTypeSelector swap (see EnemyBase / EnemyBaseEditor).
    public sealed class TypesIMGUITest : MonoBehaviour
    {
        // SerializableType<T>: strongly typed wrapper — the generic argument constrains the picker.
        [SerializeField] private SerializableType<MonoBehaviour> _serializableType;

        // [TypeSelector] on a raw string: the same picker window on an un-wrapped assembly-qualified name.
        [TypeSelector(typeof(Collider))]
        [SerializeField] private string _typeSelectorString;

        // [TypeSelector] on a string[]: each element is its own picker constrained to the base type.
        [TypeSelector(typeof(ScriptableObject))]
        [SerializeField] private string[] _typeSelectorArray;
    }
}
