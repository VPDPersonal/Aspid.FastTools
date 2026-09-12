using System;
using UnityEngine;
using Aspid.FastTools.Types;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.SerializeReferences
{
    // Switching between Pistol and Shotgun preserves serialized fields with matching names.
    /// <summary>
    /// <see cref="IRanged"/> that automatically reloads an empty magazine.
    /// </summary>
    [Serializable]
    [TypeSelectorDisplay(Group = "Weapons/Ranged", Icon = "d_Transform Icon")]
    public sealed class Pistol : IRanged
    {
        [Tooltip("Damage dealt by one attack.")]
        [SerializeField, Min(0)] private int _damage = 10;

        [Tooltip("Shots available before automatically reloading.")]
        [SerializeField, Min(1)] private int _magazineSize = 12;

        private int _rounds;

        /// <inheritdoc/>
        public string Name => "Pistol";

        /// <inheritdoc/>
        public int Fire()
        {
            if (_rounds <= 0)
                _rounds = _magazineSize;
            _rounds--;
            return _damage;
        }
    }
}
