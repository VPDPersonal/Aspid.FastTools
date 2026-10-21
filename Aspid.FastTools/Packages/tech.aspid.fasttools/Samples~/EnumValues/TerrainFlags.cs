using System;

namespace Aspid.FastTools.Samples.EnumValues
{
    [Flags]
    public enum TerrainFlags
    {
        None = 0,
        Wet = 1,
        Slippery = 2,
        Hot = 4,
    }
}
