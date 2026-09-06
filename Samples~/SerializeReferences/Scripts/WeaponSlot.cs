using System;
using UnityEngine;
using Aspid.FastTools.Types;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.SerializeReferences
{
    // A plain [Serializable] container, not a managed reference itself. The weapon inside still gets the
    // full picker, at any nesting depth.
    [Serializable]
    public sealed class WeaponSlot
    {
        [SerializeField] private string _label = "Holster";

        [TypeSelector(typeof(IRanged))]
        [SerializeReference] private IWeapon _weapon;

        public string Label => _label;

        public IWeapon Weapon => _weapon;
    }
}
