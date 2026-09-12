using UnityEngine;
using Aspid.FastTools.Types;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.SerializeReferences
{
    // Presets demonstrate missing types, stale namespaces and MovedFrom migration.
    // The custom inspector exercises the IMGUI repair workflow.
    /// <summary>
    /// <see cref="ScriptableObject"/> that stores weapons for managed-reference repair examples.
    /// </summary>
    [CreateAssetMenu(menuName = "Aspid/FastTools/Samples/Weapon Preset", fileName = "WeaponPreset")]
    public sealed class WeaponPreset : ScriptableObject
    {
        [TypeSelector]
        [Tooltip("Weapon stored in this slot or preset.")]
        [SerializeReference] private IWeapon _weapon;

        [TypeSelector]
        [Tooltip("Alternative weapons stored in the preset.")]
        [SerializeReference] private List<IWeapon> _alternates = new();
    }
}
