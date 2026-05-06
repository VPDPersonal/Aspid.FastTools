using UnityEngine;
using Aspid.FastTools.Types;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.Types
{
    // Demonstrates the IMGUI rendering path for Aspid.FastTools Type drawers.
    //
    // The same SerializableType<T> and [TypeSelector] fields used by the UIToolkit-based
    // AbilitySelector sample are reused here, but the companion editor
    // (IMGUIAbilityHolderEditor) overrides OnInspectorGUI without CreateInspectorGUI,
    // forcing Unity to render the inspector — and every nested Type picker — through
    // the IMGUI code path (TypeIMGUIPropertyDrawer).
    //
    // Useful when migrating projects that still rely on IMGUI editors, or when verifying
    // that both rendering paths stay visually and behaviourally aligned.
    public sealed class IMGUIAbilitySelector : MonoBehaviour
    {
        // Picker is constrained to Ability subtypes by the generic argument.
        [SerializeField] private SerializableType<Ability> _primaryAbility;

        // Array field + attribute: each element is its own picker constrained to AbilityModifier.
        // Allow defaults to TypeAllow.None — abstract bases and interfaces are hidden;
        // set Allow = TypeAllow.Abstract / Interface / All to opt in.
        [TypeSelector(typeof(AbilityModifier))]
        [SerializeField] private string[] _modifierTypes;
    }
}
