using System;
using UnityEngine;
using Aspid.FastTools.Types;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.SerializeReferences
{
    // The container is serialized inline; its nested weapon still gets a type picker.
    /// <summary>
    /// Represents a labeled slot with a ranged weapon reference.
    /// </summary>
    [Serializable]
    public sealed class WeaponSlot
    {
        [Tooltip("Slot label shown in the loadout summary.")]
        [SerializeField] private string _label = "Holster";

        [TypeSelector(typeof(IRanged))]
        [Tooltip("Weapon stored in this slot or preset.")]
        [SerializeReference] private IWeapon _weapon;

        /// <summary>
        /// Gets the label shown in the loadout summary.
        /// </summary>
        public string Label => _label;

        /// <summary>
        /// Gets the ranged weapon, or <see langword="null"/> when the slot is empty.
        /// </summary>
        public IWeapon Weapon => _weapon;
    }
}
