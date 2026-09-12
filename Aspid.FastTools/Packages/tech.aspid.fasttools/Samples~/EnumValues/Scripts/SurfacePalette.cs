using UnityEngine;
using Aspid.FastTools.Enums;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.EnumValues
{
    // EnumValues<TEnum, TValue> fixes the enum type in code; the Inspector cannot change it.
    /// <summary>
    /// <see cref="ScriptableObject"/> that maps surfaces to tile and trail colors.
    /// </summary>
    [CreateAssetMenu(menuName = "Aspid/FastTools/Samples/Surface Palette", fileName = "SurfacePalette")]
    public sealed class SurfacePalette : ScriptableObject
    {
        [Tooltip("Tile colors for each surface.")]
        [SerializeField] private EnumValues<SurfaceType, Color> _tileColors;

        [Tooltip("Trail colors for each surface.")]
        [SerializeField] private EnumValues<SurfaceType, Color> _footprintColors;

        /// <summary>
        /// Returns the tile color for a surface.
        /// </summary>
        /// <param name="surface">Surface to look up.</param>
        /// <returns>Configured color, or the table default when no row matches.</returns>
        public Color GetTileColor(SurfaceType surface) => _tileColors.GetValue(surface);

        /// <summary>
        /// Returns the trail color for a surface.
        /// </summary>
        /// <param name="surface">Surface to look up.</param>
        /// <returns>Configured color, or the table default when no row matches.</returns>
        public Color GetFootprintColor(SurfaceType surface) => _footprintColors.GetValue(surface);
    }
}
