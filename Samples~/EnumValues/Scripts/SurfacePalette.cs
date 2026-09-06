using UnityEngine;
using Aspid.FastTools.Enums;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.EnumValues
{
    // Shared look-up data. EnumValues<TEnum, TValue> fixes the enum at compile time: the type row in the
    // Inspector is read-only and GetValue takes a SurfaceType, not a boxed Enum.
    [CreateAssetMenu(menuName = "Aspid/FastTools/Samples/Surface Palette", fileName = "SurfacePalette")]
    public sealed class SurfacePalette : ScriptableObject
    {
        [SerializeField] private EnumValues<SurfaceType, Color> _tileColors;
        [SerializeField] private EnumValues<SurfaceType, Color> _footprintColors;

        public Color GetTileColor(SurfaceType surface) => _tileColors.GetValue(surface);

        public Color GetFootprintColor(SurfaceType surface) => _footprintColors.GetValue(surface);
    }
}
