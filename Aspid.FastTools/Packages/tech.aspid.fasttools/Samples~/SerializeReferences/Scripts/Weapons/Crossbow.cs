using System;
using UnityEngine;
using Aspid.FastTools.Types;
using UnityEngine.Scripting.APIUpdating;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.SerializeReferences
{
    // Renamed from CrossbowLauncher. [MovedFrom] lets Unity load assets that still store the old class name
    // (Presets/RenamedWeaponPreset.asset), and the Project References tab offers to bake the rename into
    // those files, after which the attribute can be deleted.
    [Serializable]
    [MovedFrom(false, null, null, "CrossbowLauncher")]
    [TypeSelectorDisplay(Group = "Weapons/Ranged", Icon = "d_Transform Icon")]
    public sealed class Crossbow : IRanged
    {
        [SerializeField] [Min(0)] private int _damage = 14;
        [SerializeField] [Min(1)] private int _boltCount = 8;

        public string Name => "Crossbow";

        public int Fire() => _damage;
    }
}
