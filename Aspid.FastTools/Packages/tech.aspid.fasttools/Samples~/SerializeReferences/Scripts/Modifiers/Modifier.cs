using System;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.SerializeReferences
{
    // A Modifier<float> field fixes T and excludes AmmoModifier and NameModifier.
    /// <summary>
    /// Concrete modifier that stores a typed value and leaves damage unchanged.
    /// </summary>
    /// <typeparam name="T">Value stored by the modifier.</typeparam>
    [Serializable]
    public class Modifier<T> : IModifier
    {
        [Tooltip("Value stored by the modifier.")]
        [SerializeField] private T _value;

        /// <summary>
        /// Gets the stored modifier value.
        /// </summary>
        protected T Value => _value;

        /// <summary>
        /// Called when describing the modifier. Override to format the stored value.
        /// </summary>
        /// <returns>Modifier description.</returns>
        public virtual string Describe() => $"{typeof(T).Name} = {_value}";

        /// <summary>
        /// Called when calculating attack damage. Override to transform the incoming damage.
        /// </summary>
        /// <param name="damage">Incoming attack damage.</param>
        /// <returns>Unchanged incoming damage.</returns>
        public virtual int ModifyDamage(int damage) => damage;
    }
}
