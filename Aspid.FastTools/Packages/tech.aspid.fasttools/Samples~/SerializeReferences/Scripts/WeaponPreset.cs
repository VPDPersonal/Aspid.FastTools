using UnityEngine;
using Aspid.FastTools.Types;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.SerializeReferences
{
    // A ScriptableObject host for the repair demos in Presets/: the shipped assets store type names that no
    // longer resolve (GhostWeapon), a stale namespace (MovedWeaponPreset) or a [MovedFrom] old name
    // (RenamedWeaponPreset). Its inspector is IMGUI, see Editor/WeaponPresetEditor.cs.
    [CreateAssetMenu(menuName = "Aspid/FastTools/Samples/Weapon Preset", fileName = "WeaponPreset")]
    public sealed class WeaponPreset : ScriptableObject
    {
        [TypeSelector]
        [SerializeReference] private IWeapon _weapon;

        [TypeSelector]
        [SerializeReference] private List<IWeapon> _alternates = new();
    }
}
