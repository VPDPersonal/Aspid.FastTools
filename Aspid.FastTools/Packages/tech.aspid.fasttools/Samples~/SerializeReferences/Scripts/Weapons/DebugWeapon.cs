using System;
using Aspid.FastTools.Types;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.SerializeReferences
{
    // Hidden excludes assignment candidates; existing values still render and remain repairable.
    /// <summary>
    /// <see cref="IWeapon"/> with maximum damage, hidden from the assignment picker.
    /// </summary>
    [Serializable]
    [TypeSelectorDisplay(Hidden = true)]
    public sealed class DebugWeapon : IWeapon
    {
        /// <inheritdoc/>
        public string Name => "Debug (one-shot)";

        /// <inheritdoc/>
        public int Fire() => int.MaxValue;
    }
}
