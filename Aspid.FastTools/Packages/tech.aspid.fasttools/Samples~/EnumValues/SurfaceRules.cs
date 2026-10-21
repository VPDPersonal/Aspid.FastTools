using UnityEngine;
using Aspid.FastTools.Enums;

namespace Aspid.FastTools.Samples.EnumValues
{
    public sealed class SurfaceRules : MonoBehaviour
    {
        [SerializeField] private EnumValues<SurfaceType, Color> _surfaceColors;
        [SerializeField] private EnumValues<float> _terrainSpeeds;

        public Color GetColor(SurfaceType surface) => _surfaceColors.GetValue(surface);

        public float GetSpeed(TerrainFlags terrain) => _terrainSpeeds.GetValue(terrain);
    }
}
