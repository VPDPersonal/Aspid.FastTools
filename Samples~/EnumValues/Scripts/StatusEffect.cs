using System;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.EnumValues
{
    [Flags]
    public enum StatusEffect
    {
        None = 0,
        Burning = 1,
        Frozen = 2,
        Slowed = 4,
        Stunned = 8,

        // Combinations such as Burning | Slowed can be registered as their own EnumValues entry;
        // an exact-key entry always beats single-flag entries, regardless of list order.
    }
}
