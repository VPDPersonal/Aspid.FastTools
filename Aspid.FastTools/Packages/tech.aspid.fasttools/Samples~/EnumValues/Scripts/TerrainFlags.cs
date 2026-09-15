using System;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.EnumValues
{
    // Combinations such as Wet | Slippery can be keys of their own in an EnumValues list.
    /// <summary>
    /// Specifies combinable terrain properties used for speed lookups.
    /// </summary>
    [Flags]
    public enum TerrainFlags
    {
        /// <summary>
        /// No terrain properties.
        /// </summary>
        None = 0,
        /// <summary>
        /// Wet terrain.
        /// </summary>
        Wet = 1,
        /// <summary>
        /// Slippery terrain.
        /// </summary>
        Slippery = 2,
        /// <summary>
        /// Hot terrain.
        /// </summary>
        Hot = 4,
        /// <summary>
        /// Soft terrain.
        /// </summary>
        Soft = 8,
    }
}
