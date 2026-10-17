#nullable enable
using System;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEditorInternal;
#endif

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types
{
    /// <summary>
    /// Unity-serializable wrapper around a <see cref="System.Type"/> referencing it through its <c>MonoScript</c>
    /// asset, so renaming or moving the class does not break the field.
    /// </summary>
    /// <remarks>
    /// A script asset exists only for a top-level, non-generic class in a file of the same name, or for a
    /// <see cref="UnityEngine.MonoBehaviour"/> or <see cref="UnityEngine.ScriptableObject"/> in a DLL, matched by
    /// namespace and name; use <see cref="SerializableType"/> for other types. A player keeps only the name and
    /// resolves it as <see cref="SerializableType"/> does.
    /// </remarks>
    /// <example>
    /// <code>
    /// [SerializeField] private SerializableMonoScript _componentType;
    ///
    /// Type type = _componentType; // implicit conversion
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
            // GetClass throws off the main thread: a worker keeps the stored name for the next main-thread save.
            if (!InternalEditorUtility.CurrentThreadIsMainThread()) return;

            // A live script asset re-syncs the stored name after a class rename. A script whose class Unity can no
            // longer find keeps the last known name, which the Inspector shows as missing instead of clearing it.
            if (!_script) return;

            var declared = _script.GetClass();
            if (declared is not null && declared.AssemblyQualifiedName != StoredAssemblyQualifiedName)
                SetAssemblyQualifiedName(declared.AssemblyQualifiedName);
#endif
        }

#if UNITY_EDITOR
        private protected sealed override Type? ResolveType(string? assemblyQualifiedName, out bool cacheable)
        {
            // Only OnBeforeSerialize re-syncs the name, and an object loaded in Play Mode may never run it — e.g. an
            // additional scene or Resources.Load after a class rename whose owners were not saved again.
            var type = base.ResolveType(assemblyQualifiedName, out cacheable);
            if (type is not null) return type;

            // GetClass throws off the main thread: a worker gets null and leaves the fallback to a main-thread read.
            if (!InternalEditorUtility.CurrentThreadIsMainThread())
            {
                cacheable = false;
                return null;
            }

            return _script ? _script!.GetClass() : null;
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
    /// Assigned to a field declared as <see cref="SerializableMonoScript"/>, it is reloaded without the constraint.
    /// </remarks>
    /// <typeparam name="T">The base type that the Inspector requires when a type is picked or dropped.</typeparam>
    /// <example>
    /// <code><![CDATA[
    /// [SerializeField] private SerializableMonoScript<Enemy> _enemyType;
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
