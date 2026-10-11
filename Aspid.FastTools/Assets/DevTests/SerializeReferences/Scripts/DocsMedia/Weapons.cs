using System;
using UnityEngine;

// Serialized fields are read by the Inspector only.
#pragma warning disable CS0414

// ReSharper disable once CheckNamespace
namespace Game.Weapons
{
    public interface IWeapon { }

    [Serializable]
    public sealed class Pistol : IWeapon
    {
        [SerializeField] private int _damage = 10;
        [SerializeField] private int _magazineSize = 12;
    }

    [Serializable]
    public sealed class Shotgun : IWeapon
    {
        [SerializeField] private int _damage = 6;
        [SerializeField] private int _pellets = 8;
    }

    [Serializable]
    public sealed class Crossbow : IWeapon
    {
        [SerializeField] private int _damage = 14;
    }

    [Serializable]
    public sealed class Railgun : IWeapon
    {
        [SerializeField] private int _damage = 25;
    }

    [Serializable]
    public sealed class Sword : IWeapon
    {
        [SerializeField] private int _damage = 12;
    }
}
#pragma warning restore CS0414
