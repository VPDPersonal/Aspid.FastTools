using UnityEngine;
using Aspid.FastTools.Enums;

// ReSharper disable CheckNamespace
namespace Aspid.FastTools.Samples.EnumValues
{
    public sealed class SurfacePalette : ScriptableObject
    {
        [SerializeField] private EnumValues<SurfaceType, Color> _tileColors;
        [SerializeField] private EnumValues<SurfaceType, Color> _footprintColors;

        private void OnValidate()
        {
            foreach (var tile in SurfaceTile.Enabled)
                tile.Refresh();
        }

        public Color GetTileColor(SurfaceType surface) =>
            _tileColors.GetValue(surface);

        public Color GetFootprintColor(SurfaceType surface) =>
            _footprintColors.GetValue(surface);
    }
}
