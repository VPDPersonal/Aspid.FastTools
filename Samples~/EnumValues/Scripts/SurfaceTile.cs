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

        private MaterialPropertyBlock _block;
        private Color _appliedColor;
        private Renderer _renderer;

        public SurfaceType Surface => _surface;

        public TerrainFlags Flags => _flags;

        private void OnEnable()
        {
            _renderer = null;
            Refresh();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.update -= Refresh;
            UnityEditor.EditorApplication.update += Refresh;
#endif
        }

        private void OnDisable()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.update -= Refresh;
#endif
        }

        private void Refresh()
        {
            if (this == null)
            {
                OnDisable();
                return;
            }

            if (_palette == null || !TryGetComponent<Renderer>(out var renderer)) return;

            var color = _palette.GetTileColor(_surface);
            if (_renderer == renderer && _appliedColor == color) return;

            _block ??= new MaterialPropertyBlock();
            _block.SetColor(_baseColorId, color);
            _block.SetColor(_colorId, color);
            renderer.SetPropertyBlock(_block);
            _renderer = renderer;
            _appliedColor = color;
#if UNITY_EDITOR
            UnityEditor.SceneView.RepaintAll();
#endif
        }
    }
}
