using UnityEngine;

// Serialized fields are read by the Inspector only.
#pragma warning disable CS0414

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.DevTests.Types
{
    public sealed class FastEnemy : EnemyBase
    {
        [SerializeField] [Min(0)] private float _speed = 25f;
    }
}
#pragma warning restore CS0414
