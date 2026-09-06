using System;
using Aspid.FastTools.Types;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.SerializeReferences
{
    // Assignable to every IWeapon field but never offered by the picker: Hidden is for types that only code
    // should create. A value already stored in an asset keeps rendering, and the repair picker still lists it.
    [Serializable]
    [TypeSelectorDisplay(Hidden = true)]
    public sealed class DebugWeapon : IWeapon
    {
        public string Name => "Debug (one-shot)";

        public int Fire() => int.MaxValue;
    }
}
