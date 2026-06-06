using System;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.SerializeReferences
{
    [Serializable]
    public sealed class Shotgun : IWeapon
    {
        [SerializeField] [Min(1)] private int _pellets = 8;
        [SerializeField] [Range(0f, 90f)] private float _spreadAngle = 25f;

        public string Describe() => $"Shotgun — {_pellets} pellets, {_spreadAngle}° spread";
    }
}
