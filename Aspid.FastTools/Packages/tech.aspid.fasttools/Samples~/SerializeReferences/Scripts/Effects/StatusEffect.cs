using System;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.SerializeReferences
{
    // Abstract base for a second polymorphic hierarchy.
    //
    // When a field is declared as StatusEffect, [SerializeReferenceSelector] offers only the
    // concrete subclasses (BurnEffect, FreezeEffect) — the abstract base itself is never listed,
    // because it cannot be instantiated.
    [Serializable]
    public abstract class StatusEffect
    {
        [SerializeField] [Min(0f)] private float _duration = 3f;

        protected float Duration => _duration;

        public abstract string Describe();
    }
}
