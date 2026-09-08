using System;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.SerializeReferences
{
    /// <summary>
    /// <see cref="Modifier{T}"/> that describes a name override.
    /// </summary>
    [Serializable]
    public sealed class NameModifier : Modifier<string>
    {
        /// <summary>
        /// Returns a description of the name override.
        /// </summary>
        /// <returns>Modifier description.</returns>
        public override string Describe() => $"named \"{Value}\"";
    }
}
