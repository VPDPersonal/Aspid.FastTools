using UnityEngine;
using Aspid.FastTools.Types;

namespace Aspid.FastTools.Samples.Types
{
    public abstract class Enemy : MonoBehaviour
    {
        [SerializeField] private ComponentTypeSelector _enemyType;
        [SerializeField] private float _health = 100f;

        public float Health => _health;
    }
}
