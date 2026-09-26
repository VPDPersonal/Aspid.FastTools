#nullable enable
using System;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
#endif

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types
{
    /// <summary>
    /// Unity-serializable wrapper around a <see cref="System.Type"/> referencing it through its <c>MonoScript</c>
    /// asset, so renaming or moving the class does not break the field.
    /// </summary>
    /// <remarks>
    /// <para>
    /// In the editor the script asset is the source of truth: on every serialization the stored assembly-qualified
    /// name is re-read from the script's class. The script reference is editor-only, so a player build carries just
    /// the name and resolves it exactly as <see cref="SerializableType"/> does. When that name no longer resolves in
    /// the editor, the type comes from the script, so an asset not saved since a class rename still works in Play Mode.
    /// </para>
    /// <para>
    /// Only types Unity maps to a script asset can be referenced this way — a top-level, non-generic class declared
    /// in a file of the same name. Use <see cref="SerializableType"/> for nested and generic types.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// public class Spawner : MonoBehaviour
    /// {
    ///     [SerializeField] private SerializableMonoScript _componentType;
    ///
    ///     private void Start()
    ///     {
    ///         Type type = _componentType;  // implicit conversion
    ///         if (type != null)
    ///             gameObject.AddComponent(type);
    ///     }
    /// }
    /// </code>
    /// </example>
    [Serializable]
    public class SerializableMonoScript : SerializableTypeBase
    {
#if UNITY_EDITOR
        [Tooltip("The script asset declaring the selected type.")]
        [SerializeField] private MonoScript? _script;
#endif

        internal SerializableMonoScript() { }

        /// <inheritdoc />
        public override Type BaseType => typeof(object);

        private protected sealed override void OnBeforeSerialize()
        {
#if UNITY_EDITOR
            // A live script asset re-syncs the stored name after a class rename. A script whose class Unity can no
            // longer find keeps the last known name, which the Inspector shows as missing instead of clearing it.
            if (!_script) return;

            var declared = _script.GetClass();
            if (declared is not null && declared.AssemblyQualifiedName != StoredAssemblyQualifiedName)
                SetAssemblyQualifiedName(declared.AssemblyQualifiedName);
#endif
        }

#if UNITY_EDITOR
        private protected sealed override Type? ResolveType(string? assemblyQualifiedName)
        {
            // Only OnBeforeSerialize re-syncs the name, and an object loaded in Play Mode may never run it — e.g. an
            // additional scene or Resources.Load after a class rename whose owners were not saved again. GetClass is
            // main-thread only, so the fallback is too.
            var type = base.ResolveType(assemblyQualifiedName);
            if (type is not null || !_script) return type;

            return _script!.GetClass();
        }
#endif

        /// <summary>
        /// Converts the wrapper to the type it holds.
        /// </summary>
        /// <param name="type">The wrapper to convert.</param>
        /// <returns>
        /// The wrapped type, or <see langword="null"/> when the wrapper is <see langword="null"/> or holds no
        /// resolvable type.
        /// </returns>
        public static implicit operator Type?(SerializableMonoScript? type) => type?.Type;
    }

    /// <summary>
    /// <see cref="SerializableMonoScript"/> constrained to types assignable to <typeparamref name="T"/>.
    /// </summary>
    /// <remarks>
    /// Unity serializes a field by its declared type, so a <see cref="SerializableMonoScript{T}"/> assigned from code
    /// to a field declared as <see cref="SerializableMonoScript"/> is reloaded unconstrained: the type survives, the
    /// constraint does not. Only the Inspector checks <typeparamref name="T"/> when a type is picked or dropped: a
    /// loaded name is not re-checked, so after <typeparamref name="T"/> or the stored class's base changes, the type
    /// may no longer be assignable to it.
    /// </remarks>
    /// <typeparam name="T">Base constraint type; the picker offers only types assignable to it.</typeparam>
    /// <example>
    /// <code><![CDATA[
    /// public class EnemySpawner : MonoBehaviour
    /// {
    ///     [SerializeField] private SerializableMonoScript<Enemy> _enemyType;
    ///
    ///     private void Spawn()
    ///     {
    ///         if (_enemyType.Type is { } type)
    ///             gameObject.AddComponent(type);
    ///     }
    /// }
    /// ]]></code>
    /// </example>
    [Serializable]
    public sealed class SerializableMonoScript<T> : SerializableMonoScript
    {
        internal SerializableMonoScript() { }

        /// <inheritdoc />
        public override Type BaseType => typeof(T);
    }
}
