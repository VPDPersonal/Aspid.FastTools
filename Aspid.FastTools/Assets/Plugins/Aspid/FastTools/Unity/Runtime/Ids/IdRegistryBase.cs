#nullable enable
using System;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Ids
{
    /// <summary>
    /// Common base for <c>IdRegistry</c> and <c>StringIdRegistry</c>: holds the
    /// dirty-flag/cache scaffolding so concrete registries only describe storage and rebuild logic.
    /// Override <see cref="OnValidate"/> in derived classes only when extra logic is needed,
    /// and call <c>base.OnValidate()</c> first.
    /// </summary>
    public abstract class IdRegistryBase : ScriptableObject
    {
        /// <summary>
        /// <c>true</c> when the in-memory lookup cache is out of sync with the serialized data
        /// and must be rebuilt before the next read. Reset to <c>false</c> by <see cref="EnsureCache"/>.
        /// </summary>
        [field: NonSerialized]
        public bool IsCacheDirty { get; private set; }

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

        /// <summary>
        /// Marks the cache dirty so the next <see cref="EnsureCache"/> call rebuilds it.
        /// Call after mutating the underlying serialized data outside of <see cref="OnValidate"/>.
        /// </summary>
        public void InvalidateCache() =>
            IsCacheDirty = true;

        /// <summary>
        /// Rebuilds the lookup cache via <see cref="RebuildCache"/> if it is currently dirty.
        /// Read paths must call this before consulting cached state.
        /// </summary>
        public void EnsureCache()
        {
            if (!IsCacheDirty) return;

            RebuildCache();
            IsCacheDirty = false;
        }

        /// <summary>
        /// Rebuilds the in-memory lookup structures from the serialized backing store.
        /// Invoked by <see cref="EnsureCache"/> when the cache is dirty.
        /// </summary>
        protected abstract void RebuildCache();

        /// <summary>
        /// Marks the cache dirty when the asset is edited in the inspector.
        /// Override only when extra logic is needed and call <c>base.OnValidate()</c> first.
        /// </summary>
        protected virtual void OnValidate() =>
            IsCacheDirty = true;
    }
}
