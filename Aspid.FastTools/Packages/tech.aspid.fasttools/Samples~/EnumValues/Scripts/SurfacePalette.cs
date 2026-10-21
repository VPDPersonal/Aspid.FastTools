using UnityEngine;
using Aspid.FastTools.Enums;

namespace Aspid.FastTools.Samples.EnumValues
{
    public sealed class SurfacePalette : ScriptableObject
    {
        [SerializeField] private EnumValues<SurfaceType, Color> _tileColors;
        [SerializeField] private EnumValues<SurfaceType, Color> _footprintColors;

        public Color GetTileColor(SurfaceType surface) => _tileColors.GetValue(surface);

        public Color GetFootprintColor(SurfaceType surface) => _footprintColors.GetValue(surface);

        private void OnValidate()
        {
            foreach (var tile in FindObjectsByType<SurfaceTile>(FindObjectsSortMode.None))
                tile.Refresh();
        }
    }
}
