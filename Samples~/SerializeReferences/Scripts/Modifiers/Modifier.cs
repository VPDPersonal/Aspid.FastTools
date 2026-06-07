using System;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.SerializeReferences
{
    // Non-abstract generic base for the [SerializeReferenceSelector] generic test.
    //
    // Because it is a concrete open generic, [SerializeReferenceSelector] lists it as "Modifier<T>".
    //   - On a non-generic IModifier field, picking it opens a second window to choose the argument T
    //     (e.g. string in one case, float in another), then instantiates Modifier<string> / Modifier<float>.
    //   - On a closed-generic field such as Modifier<float>, the argument is inferred from the field, so it
    //     is created directly as Modifier<float> without the extra window.
    //
    // The typed _value field verifies that Unity's generic serialization handles a bare type-parameter
    // field (for float/int/string) and renders it inline under the dropdown.
    [Serializable]
    public class Modifier<T> : IModifier
    {
        [SerializeField] private T _value;

        protected T Value => _value;

        public virtual string Describe() => $"Modifier<{typeof(T).Name}> = {_value}";
    }
}
