using System;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.EnumValues
{
    // Combinations such as Wet | Slippery can be keys of their own in an EnumValues list.
    [Flags]
    public enum TerrainFlags
    {
        None = 0,
        Wet = 1,
        Slippery = 2,
        Hot = 4,
    }
}
