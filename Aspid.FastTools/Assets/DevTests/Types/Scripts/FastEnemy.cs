using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.DevTests.Types
{
    public sealed class FastEnemy : EnemyBase
    {
        [SerializeField] [Min(0)] private float _speed = 25f;
    }
}
