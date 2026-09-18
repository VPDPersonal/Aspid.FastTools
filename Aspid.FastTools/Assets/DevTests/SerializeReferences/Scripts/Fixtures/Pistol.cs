using System;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Game.Gear
{
    [Serializable]
    public sealed class Pistol : IWeapon
    {
        [SerializeField] [Min(0)] private int _damage = 10;

        public void Fire() => Debug.Log($"Pistol: {_damage} dmg");
    }
}
