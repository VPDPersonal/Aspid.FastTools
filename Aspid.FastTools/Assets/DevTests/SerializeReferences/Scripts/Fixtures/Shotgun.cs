using System;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Game.Gear
{
    // _damage is declared with the same name and type as on Pistol so that switching between the two
    // carries the value over, which is the behavior Documentation/03-serialize-reference-selector.md describes.
    [Serializable]
    public sealed class Shotgun : IWeapon
    {
        [SerializeField] [Min(0)] private int _damage = 20;
        [SerializeField] [Min(1)] private int _pellets = 6;

        public void Fire() => Debug.Log($"Shotgun: {_damage} dmg, {_pellets} pellets");
    }
}
