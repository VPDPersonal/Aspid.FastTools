using System;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.EnumValues
{
    [Flags]
    public enum TerrainFlags
    {
        None = 0,
        Wet = 1,
        Slippery = 2,
        Hot = 4,
        Soft = 8,
    }
}
