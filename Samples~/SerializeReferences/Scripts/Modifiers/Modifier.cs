using System;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.SerializeReferences
{
    // A concrete open generic. On a Modifier<float> field T is fixed by the field, so the picker creates
    // Modifier<float> directly and excludes AmmoModifier (int) and NameModifier (string).
    [Serializable]
    public class Modifier<T> : IModifier
    {
        [SerializeField] private T _value;

        protected T Value => _value;

        public virtual string Describe() => $"{typeof(T).Name} = {_value}";

        public virtual int ModifyDamage(int damage) => damage;
    }
}
