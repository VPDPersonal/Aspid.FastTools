using System;
using UnityEngine;
using Aspid.FastTools.Types;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.SerializeReferences
{
    // A managed reference nested inside another: the charge effect gets its own dropdown under the Railgun foldout.
    [Serializable]
    [TypeSelectorDisplay(Group = "Weapons/Ranged", Icon = "d_Transform Icon", Tooltip = "Slow, pierces armor, applies a charge effect")]
    public sealed class Railgun : IRanged
    {
        [SerializeField] [Min(0)] private int _damage = 45;

        [TypeSelector]
        [SerializeReference] private StatusEffect _chargeEffect;

        public string Name => _chargeEffect is null ? "Railgun" : $"Railgun + {_chargeEffect.Name}";

        public StatusEffect ChargeEffect => _chargeEffect;

        public int Fire() => _damage;
    }
}
