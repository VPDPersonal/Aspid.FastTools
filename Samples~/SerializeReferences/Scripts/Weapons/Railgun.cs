using System;
using UnityEngine;
using Aspid.FastTools.Types;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.SerializeReferences
{
    // Demonstrates a nested [SerializeReference] inside a managed-reference payload:
    // the charge effect is itself polymorphic and gets its own inline dropdown.
    [Serializable]
    public sealed class Railgun : IWeapon
    {
        [SerializeField] [Min(0f)] private float _chargeTime = 1.5f;

        [SerializeReference] [TypeSelector]
        private StatusEffect _chargeEffect;

        public string Describe()
        {
            var effect = _chargeEffect is null ? "none" : _chargeEffect.Describe();
            return $"Railgun — {_chargeTime}s charge, effect: {effect}";
        }
    }
}
