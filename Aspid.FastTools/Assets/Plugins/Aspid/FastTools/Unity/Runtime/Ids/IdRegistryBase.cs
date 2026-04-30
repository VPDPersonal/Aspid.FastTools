#nullable enable
using System;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools
{
    /// <summary>
    /// Common base for <c>IdRegistry</c> and <c>StringIdRegistry</c>: holds the
    /// dirty-flag/cache scaffolding so concrete registries only describe storage and rebuild logic.
    /// Override <see cref="OnValidate"/> in derived classes only when extra logic is needed,
    /// and call <c>base.OnValidate()</c> first.
    /// </summary>
    public abstract class IdRegistryBase : ScriptableObject
    {
        [NonSerialized] private bool _cacheDirty = true;

        /// <summary>
        /// Gets the number of registered entries.
        /// </summary>
        public abstract int Count { get; }

        /// <summary>
        /// Determines whether the registry contains the specified integer id.
        /// </summary>
        /// <param name="id">The integer id to look up.</param>
        /// <returns><c>true</c> if the id is registered; otherwise <c>false</c>.</returns>
        public abstract bool Contains(int id);

        public void InvalidateCache() => _cacheDirty = true;

        protected void EnsureCache()
        {
            if (!_cacheDirty) return;
            RebuildCache();
            _cacheDirty = false;
        }

        protected abstract void RebuildCache();
        
        protected virtual void OnValidate() => _cacheDirty = true;
    }
}
