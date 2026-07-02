using System;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.SerializeReferences
{
    // Renamed from "CrossbowLauncher" — the declared [MovedFrom] is what turns Presets/RenamedWeaponPreset.asset
    // (whose YAML still stores the old class name) into a pending migration rather than a breakage: Unity migrates
    // the loaded object in memory, the Inspector shows a healthy Crossbow, and Project References offers an
    // authoritative one-click "Migrate all" that bakes the rename into the file. Once no file stores the old name,
    // this attribute can be deleted.
    [Serializable]
    [MovedFrom(false, null, null, "CrossbowLauncher")]
    public sealed class Crossbow : IRanged
    {
        [SerializeField] [Min(0)] private int _damage = 14;
        [SerializeField] [Min(0)] private int _boltCount = 8;

        public string Describe() => $"Crossbow — {_damage} dmg, {_boltCount} bolts";
    }
}
