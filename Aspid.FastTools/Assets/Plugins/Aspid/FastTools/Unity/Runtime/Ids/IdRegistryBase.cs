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

        public abstract int Count { get; }

        public abstract bool Contains(int id);

        public void InvalidateCache() => _cacheDirty = true;

        protected void EnsureCache()
        {
            if (!_cacheDirty) return;
            RebuildCache();
            _cacheDirty = false;
        }

        protected abstract void RebuildCache();

#if UNITY_EDITOR
        protected virtual void OnValidate() => _cacheDirty = true;
#endif
    }
}
