using System;
using UnityEngine;
using Aspid.FastTools.Types;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.SerializeReferences
{
    /// <summary>
    /// <see cref="IMelee"/> with a chance to deal double damage.
    /// </summary>
    [Serializable]
    [TypeSelectorDisplay(Group = "Weapons/Melee", Icon = "d_Transform Icon")]
    public sealed class Sword : IMelee
    {
        [Tooltip("Damage dealt by one attack.")]
        [SerializeField, Min(0)] private int _damage = 30;

        [Tooltip("Chance for an attack to deal double damage.")]
        [SerializeField, Range(0f, 1f)] private float _critChance = 0.25f;

        /// <inheritdoc/>
        public string Name => "Sword";

        /// <inheritdoc/>
        public int Fire() =>
            UnityEngine.Random.value < _critChance ? _damage * 2 : _damage;
    }
}
