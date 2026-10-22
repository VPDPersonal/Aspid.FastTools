using System;
using UnityEngine;
using System.Collections;
using Aspid.FastTools.Types;
using Aspid.FastTools.Samples.Types.Enemies;
using Aspid.FastTools.Samples.Types.Spawning;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.Types
{
    public sealed class EnemySpawner : MonoBehaviour
    {
        [TypeSelector(Allow = TypeAllow.None)]
        [SerializeField] private SerializableMonoScript<Enemy> _enemyType;

        [TypeSelector(nameof(_enemyType))]
        [SerializeField] private string _eliteType;

        [SerializeField] [Min(0)] private int _eliteEvery = 4;

        [TypeSelector(Allow = TypeAllow.None)]
        [SerializeField] private SerializableType<ISpawnPattern> _pattern = new(typeof(CirclePattern));

        [SerializeField] [Range(1, 32)] private int _count = 8;
        [SerializeField] [Min(1f)] private float _radius = 8f;
        [SerializeField] [Min(0.5f)] private float _interval = 6f;

        [SerializeField] private Mesh _body;
        [SerializeField] private Material _presentationMaterial;

        private int _spawned;
        private ISpawnPattern _patternInstance;

        private IEnumerator Start()
        {
            while (true)
            {
                SpawnWave();
                yield return new WaitForSeconds(_interval);
            }
        }

        private void SpawnWave()
        {
            if (_enemyType?.Type is not { } enemyType || _pattern.Type is not { } patternType)
                return;

            if (_patternInstance?.GetType() != patternType)
                _patternInstance = (ISpawnPattern)Activator.CreateInstance(patternType);

            var eliteType = string.IsNullOrEmpty(_eliteType)
                ? null
                : Type.GetType(_eliteType, throwOnError: false);

            for (var i = 0; i < _count; i++)
            {
                var isElite = eliteType is not null && _eliteEvery > 0 && ++_spawned % _eliteEvery == 0;
                var type = isElite
                    ? eliteType
                    : enemyType;

                var enemy = new GameObject(name: type.Name, components: new[] { typeof(MeshFilter), typeof(MeshRenderer), type });
                enemy.GetComponent<MeshFilter>().sharedMesh = _body;
                enemy.GetComponent<MeshRenderer>().sharedMaterial = _presentationMaterial;
                enemy.transform.SetParent(transform);
                enemy.transform.position = _patternInstance.GetPosition(index: i, count: _count, radius: _radius) + Vector3.up;
            }
        }
    }
}
