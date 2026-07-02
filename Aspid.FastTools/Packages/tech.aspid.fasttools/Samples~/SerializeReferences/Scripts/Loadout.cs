using System.Collections.Generic;
using UnityEngine;
using Aspid.FastTools.Types;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.SerializeReferences
{
    // The demo component behind the Prefabs/ scenarios: every flavour of [SerializeReference] +
    // [TypeSelector] field (single, list, abstract base, generics) on one MonoBehaviour, rendered
    // through the default UIToolkit Inspector. The guided walkthrough of each flavour lives in
    // TUTORIAL.md and Scripts/Tutorial/TypeSelectorTutorial.cs.
    public sealed class Loadout : MonoBehaviour
    {
        // Interface-typed field: lists every IWeapon implementation (Sword, Pistol, Shotgun, Railgun, Crossbow).
        [SerializeReference] [TypeSelector]
        private IWeapon _primaryWeapon;

        // Each list element is its own independent picker.
        [SerializeReference] [TypeSelector]
        private List<IWeapon> _sidearms = new();

        // Abstract-base field: the picker offers BurnEffect / FreezeEffect, never StatusEffect.
        [SerializeReference] [TypeSelector]
        private StatusEffect _onHitEffect;

        // Open-generic entry point: offers the closed subclasses AND Modifier<T> itself (see Modifiers/).
        [SerializeReference] [TypeSelector]
        private IModifier _modifier;

        // Closed-generic field type: candidates are constrained by assignability to Modifier<float>.
        [SerializeReference] [TypeSelector]
        private Modifier<float> _floatModifier;

        // Polymorphic list mixing different closed-generic subclasses.
        [SerializeReference] [TypeSelector]
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
