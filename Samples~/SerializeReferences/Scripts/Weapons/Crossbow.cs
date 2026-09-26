using System;
using UnityEngine;
using Aspid.FastTools.Types;
using UnityEngine.Scripting.APIUpdating;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.SerializeReferences
{
    // MovedFrom loads the old CrossbowLauncher name in RenamedWeaponPreset.asset.
    // Project References can write the current name back to the asset.
    /// <summary>
    /// <see cref="IRanged"/> with a migrated class name that automatically reloads an empty quiver.
    /// </summary>
    [Serializable]
    [MovedFrom(false, null, null, "CrossbowLauncher")]
    [TypeSelectorDisplay(Group = "Weapons/Ranged")]
    public sealed class Crossbow : IRanged
    {
        [Tooltip("Damage dealt by one attack.")]
        [SerializeField, Min(0)] private int _damage = 14;

        [Tooltip("Bolts available before automatically reloading.")]
        [SerializeField, Min(1)] private int _boltCount = 8;

        private int _bolts;

        /// <inheritdoc/>
        public string Name => "Crossbow";

        /// <inheritdoc/>
        public int Fire()
        {
            if (_bolts <= 0)
                _bolts = _boltCount;
            _bolts--;
            return _damage;
        }
    }
}
