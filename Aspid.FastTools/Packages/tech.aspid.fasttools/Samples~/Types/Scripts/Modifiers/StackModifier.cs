using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.Types
{
    // A concrete OPEN generic modifier. In the type picker it appears as StackModifier<T>; picking it
    // opens a second page to choose T before the closed type (e.g. StackModifier<float>) is built and stored.
    public sealed class StackModifier<T> : AbilityModifier
    {
        public override void Apply() =>
            Debug.Log($"StackModifier<{typeof(T).Name}> applied.");
    }
}
