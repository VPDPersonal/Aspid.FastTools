using System;
using UnityEngine;
using System.Collections.Generic;
using Aspid.FastTools.Types;

// Docs-media harness: mirrors the SerializeReference Selector example in
// Documentation/EN|RU/SerializeReferences.md — IWeapon implementations picked into
// [SerializeReference] fields. Lives in DevTests, but uses a neutral game-like
// namespace because the picker breadcrumbs are visible in the recorded media.

// ReSharper disable once CheckNamespace
namespace Game.Gear
{
    public interface IWeapon
    {
        void Fire();
    }

    [Serializable]
    public sealed class Pistol : IWeapon
    {
        [SerializeField] [Min(0)] private int _damage = 10;

        public void Fire() => Debug.Log($"Pistol: {_damage} dmg");
    }

    [Serializable]
    public sealed class Shotgun : IWeapon
    {
        [SerializeField] [Min(1)] private int _pellets = 8;
        [SerializeField] [Range(0f, 45f)] private float _spread = 12f;

        public void Fire() => Debug.Log($"Shotgun: {_pellets} pellets, {_spread}° spread");
    }

    [Serializable]
    public sealed class PlasmaRifle : IWeapon
    {
        [SerializeField] [Min(0f)] private float _power = 40f;
        [SerializeField] [Min(0f)] private float _range = 60f;

        public void Fire() => Debug.Log($"Plasma rifle: {_power} power, {_range} m");
    }

    public sealed class Loadout : MonoBehaviour
    {
        [TypeSelector]
        [SerializeReference] private IWeapon _primary;

        [TypeSelector]
        [SerializeReference] private List<IWeapon> _sidearms;
    }
}
