using System;
using UnityEngine;
using Aspid.FastTools.Types;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.Types
{
    // Step-by-step tour of SerializableType<T> / [TypeSelector] (incl. generic picking + Required) /
    // ComponentTypeSelector: each [Header("STEP N …")] is one lesson. Open Scenes/TypesTutorial.unity
    // and follow TUTORIAL.md / TUTORIAL_RU.md.
    public sealed class TypesTutorial : MonoBehaviour
    {
        [Header("STEP 1 — Pick a type with SerializableType<T>")]
        [SerializeField]
        [Tooltip("A SerializableType<Ability> field: the picker is constrained to Ability subtypes by the generic argument. It stores the assembly-qualified name and resolves lazily to a Type on first access.")]
        private SerializableType<Ability> _step1Ability;

        [Header("STEP 2 — [TypeSelector] on a raw string[]")]
        [TypeSelector(typeof(AbilityModifier))]
        [SerializeField]
        [Tooltip("Each element is its own picker constrained to AbilityModifier. Allow defaults to TypeAllow.None — abstract bases and interfaces are hidden until you opt in.")]
        private string[] _step2ModifierTypes;

        [Header("STEP 3 — Generic types in the picker")]
        [TypeSelector(typeof(AbilityModifier))]
        [SerializeField]
        [Tooltip("The picker offers the concrete modifiers AND the open generic StackModifier<T>. Picking StackModifier<T> opens a second page to choose T (try float, then string); the stored value is the CLOSED type, e.g. StackModifier<float>.")]
        private string _step3GenericModifier;

        [Header("STEP 4 — Require a type with [TypeSelector(Required = true)]")]
        [TypeSelector(typeof(Ability), Required = true)]
        [SerializeField]
        [Tooltip("Left empty on purpose: a [TypeSelector(Required = true)] string shows an inline \"Required type is not set\" notice and, as a top-level field, counts as a violation for the build/CI gate. Pick any Ability to clear it.")]
        private string _step4RequiredAbility;

        [Header("STEP 5 — Swap a component with ComponentTypeSelector")]
        [SerializeField]
        [Tooltip("The Enemy GameObject in this scene. Select it and use the type dropdown at the top of its Inspector to swap FastEnemy <-> TankEnemy in place; the Health field persists across the swap.")]
        private EnemyBase _step5Enemy;

        [ContextMenu("Log Tutorial Lookups")]
        private void LogTutorialLookups()
        {
            // STEP 1 — SerializableType<T>.Type performs the lazy GetType() lookup and caches it.
            var abilityType = _step1Ability.Type;
            Debug.Log(abilityType is null
                ? "STEP 1 no ability type picked (or the stored name no longer resolves)"
                : $"STEP 1 picked ability type: {abilityType.Name}");

            // STEP 2 — the raw-string form resolves manually via Type.GetType.
            if (_step2ModifierTypes is { Length: > 0 })
            {
                foreach (var qualifiedName in _step2ModifierTypes)
                {
                    var modifierType = Type.GetType(qualifiedName);
                    Debug.Log(modifierType is null
                        ? $"STEP 2 unresolved modifier name: \"{qualifiedName}\""
                        : $"STEP 2 modifier type: {modifierType.Name}");
                }
            }
            else Debug.Log("STEP 2 no modifier types picked");

            // STEP 3 — a generic pick stores the CLOSED constructed type (e.g. StackModifier<float>).
            var genericType = string.IsNullOrEmpty(_step3GenericModifier) ? null : Type.GetType(_step3GenericModifier);
            Debug.Log(genericType is null
                ? "STEP 3 no generic modifier picked (pick StackModifier<T> and choose T)"
                : $"STEP 3 generic modifier: {genericType} (constructed generic: {genericType.IsConstructedGenericType})");

            // STEP 4 — the required string resolves like STEP 2, or is empty → the required notice / CI-gate violation.
            Debug.Log(string.IsNullOrEmpty(_step4RequiredAbility)
                ? "STEP 4 required ability type is not set (inline notice + CI-gate violation)"
                : $"STEP 4 required ability type: {Type.GetType(_step4RequiredAbility)?.Name ?? "<unresolved>"}");

            // STEP 5 — the concrete subtype the Enemy component was swapped to.
            Debug.Log(_step5Enemy is null
                ? "STEP 5 no Enemy assigned"
                : $"STEP 5 Enemy concrete type: {_step5Enemy.GetType().Name}");
        }
    }
}
