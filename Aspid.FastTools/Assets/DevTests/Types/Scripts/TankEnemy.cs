using UnityEngine;

// Serialized fields are read by the Inspector only.
#pragma warning disable CS0414

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.DevTests.Types
{
    public sealed class TankEnemy : EnemyBase
    {
        [SerializeField] [Min(0)] private float _armor = 50f;
    }
}
#pragma warning restore CS0414
