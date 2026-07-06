using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.DevTests.Types
{
    public sealed class TankEnemy : EnemyBase
    {
        [SerializeField] [Min(0)] private float _armor = 50f;
    }
}
