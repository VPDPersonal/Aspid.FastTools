using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.EnumValues
{
    // One floor tile. Colors itself from the palette so a palette edit is visible without Play Mode.
    [ExecuteAlways]
    public sealed class SurfaceTile : MonoBehaviour
    {
        [SerializeField] private SurfaceType _surface;
        [SerializeField] private TerrainFlags _flags;
        [SerializeField] private SurfacePalette _palette;

        private static readonly int _baseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int _colorId = Shader.PropertyToID("_Color");

        public SurfaceType Surface => _surface;

        public TerrainFlags Flags => _flags;

        private void OnEnable() => Refresh();

        private void OnValidate() => Refresh();

        private void Refresh()
        {
            if (_palette is null || !TryGetComponent<Renderer>(out var renderer)) return;

            var block = new MaterialPropertyBlock();
            var color = _palette.GetTileColor(_surface);
            block.SetColor(_baseColorId, color);
            block.SetColor(_colorId, color);
            renderer.SetPropertyBlock(block);
        }
    }
}
