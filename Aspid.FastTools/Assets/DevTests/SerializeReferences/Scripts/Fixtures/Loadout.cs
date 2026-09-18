using UnityEngine;
using System.Collections.Generic;
using Aspid.FastTools.Types;

// Fixture host for the prefabs next door: WeaponPreset (two ordinary references), SharedWeaponPreset
// (both fields pointing at one instance) and BrokenWeaponPreset (a deliberately misspelled stored type).
// They are the manual counterpart to the Project References and Asset References windows, which need
// assets on disk rather than an inspector. The namespace stays neutral because picker breadcrumbs show it.

// ReSharper disable once CheckNamespace
namespace Game.Gear
{
    public sealed class Loadout : MonoBehaviour
    {
        [TypeSelector]
        [SerializeReference] private IWeapon _primary;

        [TypeSelector]
        [SerializeReference] private List<IWeapon> _sidearms;
    }
}
