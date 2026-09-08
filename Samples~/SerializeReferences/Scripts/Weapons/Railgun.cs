using System;
using UnityEngine;
using Aspid.FastTools.Types;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.SerializeReferences
{
    // The nested charge effect has its own picker under the weapon foldout.
    /// <summary>
    /// <see cref="IRanged"/> with a nested charge effect.
    /// </summary>
    [Serializable]
    [TypeSelectorDisplay(Group = "Weapons/Ranged", Icon = "d_Transform Icon", Tooltip = "Fixed damage with a nested charge effect")]
    public sealed class Railgun : IRanged
    {
        [Tooltip("Damage dealt by one attack.")]
        [SerializeField, Min(0)] private int _damage = 45;

        [TypeSelector]
        [Tooltip("Additional effect applied after a railgun attack.")]
        [SerializeReference] private StatusEffect _chargeEffect;

        /// <summary>
        /// Gets the display name.
        /// </summary>
        public string Name => _chargeEffect is null ? "Railgun" : $"Railgun + {_chargeEffect.Name}";

        /// <summary>
        /// Gets the additional hit effect, or <see langword="null"/> when none is assigned.
        /// </summary>
        public StatusEffect ChargeEffect => _chargeEffect;

        /// <inheritdoc/>
        public int Fire() => _damage;
    }
}
