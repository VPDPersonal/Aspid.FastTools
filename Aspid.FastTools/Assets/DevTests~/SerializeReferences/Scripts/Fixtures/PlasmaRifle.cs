using System;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Game.Gear
{
    // A third implementation that shares no field with the other two: switching to it drops the
    // carried-over data instead of preserving it.
    [Serializable]
    public sealed class PlasmaRifle : IWeapon
    {
        [SerializeField] [Min(0f)] private float _power = 40f;
        [SerializeField] [Min(0f)] private float _range = 60f;

        public void Fire() => Debug.Log($"Plasma rifle: {_power} power, {_range} m");
    }
}
