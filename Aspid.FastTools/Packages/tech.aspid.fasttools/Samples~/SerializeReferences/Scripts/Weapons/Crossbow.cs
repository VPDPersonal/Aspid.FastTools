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
    /// <see cref="IRanged"/> with a migrated class name and fixed attack damage.
    /// </summary>
    [Serializable]
    [MovedFrom(false, null, null, "CrossbowLauncher")]
    [TypeSelectorDisplay(Group = "Weapons/Ranged", Icon = "d_Transform Icon")]
    public sealed class Crossbow : IRanged
    {
        [Tooltip("Damage dealt by one attack.")]
        [SerializeField, Min(0)] private int _damage = 14;

        [Tooltip("Bolt capacity shown as sample data.")]
        [SerializeField, Min(1)] private int _boltCount = 8;

        /// <inheritdoc/>
        public string Name => "Crossbow";

        /// <inheritdoc/>
        public int Fire() => _damage;
    }
}
