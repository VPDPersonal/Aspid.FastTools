using System.Collections.Generic;
using UnityEngine;
using Aspid.FastTools.SerializeReferences;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.SerializeReferences
{
    // Demonstrates [SerializeReferenceSelector] through the default (UIToolkit) Inspector.
    //
    // Add [SerializeReferenceSelector] next to [SerializeReference] and the field renders as a
    // searchable, hierarchical type dropdown:
    //   - single field      → pick one IWeapon implementation
    //   - List<T> / array    → each element is its own polymorphic picker
    //   - abstract base type → only concrete subclasses are offered
    //   - generic hierarchy  → concrete closed-generic subclasses are offered; the open generic
    //                          Modifier<T> is also offered and opens a second window to pick T;
    //                          a closed-generic field type (Modifier<float>) constrains candidates
    //                          by assignability and infers T directly
    //
    // Picking a type instantiates it, <None> clears the reference, and the assigned instance's
    // serialized fields appear inline under the foldout. Nested [SerializeReference] fields
    // (e.g. Railgun's charge effect) get their own dropdown recursively.
    public sealed class Loadout : MonoBehaviour
    {
        // Interface-typed field: lists every IWeapon implementation (Pistol, Shotgun, Railgun).
        [SerializeReference] [SerializeReferenceSelector]
        private IWeapon _primaryWeapon;

        // Each list element is its own independent picker.
        [SerializeReference] [SerializeReferenceSelector]
        private List<IWeapon> _sidearms = new();

        // Abstract-base field: the picker offers BurnEffect / FreezeEffect, never StatusEffect.
        [SerializeReference] [SerializeReferenceSelector]
        private StatusEffect _onHitEffect;

        // Generic hierarchy. Non-generic IModifier field: the picker offers the concrete subclasses
        // (DamageModifier, AmmoModifier, NameModifier) AND the open generic Modifier<T> — choosing the
        // latter opens a second window to pick T (e.g. string vs float).
        [SerializeReference] [SerializeReferenceSelector]
        private IModifier _modifier;

        // Closed-generic field type: only types assignable to Modifier<float> are offered —
        // DamageModifier (Modifier<float>) and Modifier<T> (its T is inferred to float, no extra window).
        // AmmoModifier (int) and NameModifier (string) are excluded.
        [SerializeReference] [SerializeReferenceSelector]
        private Modifier<float> _floatModifier;

        // Polymorphic list mixing different closed-generic subclasses.
        [SerializeReference] [SerializeReferenceSelector]
        private List<IModifier> _modifiers = new();

        [ContextMenu("Log Loadout")]
        private void LogLoadout()
        {
            Debug.Log($"Primary: {_primaryWeapon?.Describe() ?? "none"}");

            for (var i = 0; i < _sidearms.Count; i++)
                Debug.Log($"Sidearm {i}: {_sidearms[i]?.Describe() ?? "none"}");

            Debug.Log($"On-hit effect: {_onHitEffect?.Describe() ?? "none"}");
            Debug.Log($"Modifier: {_modifier?.Describe() ?? "none"}");
            Debug.Log($"Float modifier: {_floatModifier?.Describe() ?? "none"}");

            for (var i = 0; i < _modifiers.Count; i++)
                Debug.Log($"Modifier {i}: {_modifiers[i]?.Describe() ?? "none"}");
        }
    }
}
