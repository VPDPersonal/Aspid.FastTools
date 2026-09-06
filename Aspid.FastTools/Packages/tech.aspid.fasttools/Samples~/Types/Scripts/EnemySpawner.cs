using System;
using UnityEngine;
using System.Collections;
using Aspid.FastTools.Types;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.Types
{
    // Spawns a wave of enemies every interval. Three ways to store a System.Type in one component:
    // a MonoBehaviour subtype referenced through its script asset, a plain C# strategy by name, and a
    // raw string whose picker is constrained by another field.
    public sealed class EnemySpawner : MonoBehaviour
    {
        // SerializableMonoScript<T> keeps a MonoScript reference in the editor, so renaming or moving the
        // class does not break the scene. Required = true flags an empty field in the Inspector and in the
        // build/CI gate.
        [Header("Enemy")]
        [TypeSelector(Required = true)]
        [SerializeField] private SerializableMonoScript<Enemy> _enemyType;

        // Member reference: the picker offers only types assignable to whatever _enemyType currently holds,
        // so the elite variant is always a subtype of the regular one (ArmoredGrunt for Grunt, Sniper for Archer).
        [TypeSelector(nameof(_enemyType))]
        [SerializeField] private string _eliteType;

        [SerializeField] [Min(0)] private int _eliteEvery = 4;

        // SerializableType<T> for a plain class. Allow = TypeAllow.None hides the interface itself; the
        // remaining candidates carry [TypeSelectorDisplay] names, one group and icons.
        [Header("Wave")]
        [TypeSelector(Allow = TypeAllow.None)]
        [SerializeField] private SerializableType<ISpawnPattern> _pattern = new(typeof(CirclePattern));

        [SerializeField] [Range(1, 32)] private int _count = 8;
        [SerializeField] [Min(1f)] private float _radius = 8f;
        [SerializeField] [Min(0.5f)] private float _interval = 6f;

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

        [ContextMenu("Spawn Wave")]
        private void SpawnWave()
        {
            // .Type resolves lazily and returns null when nothing is picked or the stored name no longer resolves.
            var enemyType = _enemyType.Type;
            if (enemyType is null)
            {
                Debug.LogWarning("Pick an enemy type first.", this);
                return;
            }

            var pattern = GetPattern();
            var eliteType = string.IsNullOrEmpty(_eliteType) ? null : Type.GetType(_eliteType);

            for (var i = 0; i < _count; i++)
            {
                var isElite = eliteType is not null && _eliteEvery > 0 && ++_spawned % _eliteEvery == 0;
                var type = isElite ? eliteType : enemyType;

                var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                go.name = type.Name;
                go.transform.SetParent(transform);
                go.transform.position = pattern.GetPosition(i, _count, _radius) + Vector3.up;

                // AddComponent(Type) is why the field is constrained to Enemy: the picker never offers anything else.
                var enemy = (Enemy)go.AddComponent(type);
                Debug.Log($"Spawned {enemy}", enemy);
            }
        }

        private ISpawnPattern GetPattern()
        {
            var type = _pattern.Type ?? typeof(CirclePattern);
            if (_patternInstance is null || _patternInstance.GetType() != type)
                _patternInstance = (ISpawnPattern)Activator.CreateInstance(type);

            return _patternInstance;
        }
    }
}
