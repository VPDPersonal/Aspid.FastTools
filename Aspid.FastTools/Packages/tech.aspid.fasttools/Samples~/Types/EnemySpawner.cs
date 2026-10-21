using System;
using UnityEngine;
using Aspid.FastTools.Types;

namespace Aspid.FastTools.Samples.Types
{
    public sealed class EnemySpawner : MonoBehaviour
    {
        private const int Count = 8;

        [TypeSelector(Allow = TypeAllow.None)]
        [SerializeField] private SerializableMonoScript<Enemy> _enemyType;

        [TypeSelector(Allow = TypeAllow.None)]
        [SerializeField] private SerializableType<ISpawnPattern> _pattern = new(typeof(CirclePattern));

        [SerializeField] private GameObject _enemyPrefab;

        private ISpawnPattern _patternInstance;

        private void Start()
        {
            if (_enemyType?.Type is not { } enemyType)
                return;

            for (var i = 0; i < Count; i++)
            {
                var enemy = Instantiate(original: _enemyPrefab, parent: transform);
                enemy.AddComponent(enemyType);
            }
        }

        private void Update()
        {
            if (_pattern.Type is not { } patternType)
                return;

            if (_patternInstance?.GetType() != patternType)
                _patternInstance = (ISpawnPattern)Activator.CreateInstance(patternType);

            for (var i = 0; i < transform.childCount; i++)
            {
                var enemy = transform.GetChild(i);
                var target = _patternInstance.GetPosition(index: i, count: transform.childCount);
                enemy.localPosition = Vector3.Lerp(a: enemy.localPosition, b: target, t: 4f * Time.deltaTime);
            }
        }
    }
}
