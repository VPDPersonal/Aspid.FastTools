using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.EnumValues
{
    /// <summary>
    /// <see cref="MonoBehaviour"/> that applies palette colors in Edit Mode and Play Mode.
    /// </summary>
    [ExecuteAlways]
    public sealed class SurfaceTile : MonoBehaviour
    {
        [Tooltip("Surface used for color and sampling lookups.")]
        [SerializeField] private SurfaceType _surface;

        [Tooltip("Terrain properties used for movement speed.")]
        [SerializeField] private TerrainFlags _flags;

        [Tooltip("Surface colors for tiles and trails.")]
        [SerializeField] private SurfacePalette _palette;

        private static readonly int _baseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int _colorId = Shader.PropertyToID("_Color");

        private MaterialPropertyBlock _block;
        private Color _appliedColor;
        private Renderer _renderer;

        /// <summary>
        /// Gets the surface used for palette and sampling lookups.
        /// </summary>
        public SurfaceType Surface => _surface;

        /// <summary>
        /// Gets the terrain properties used for speed lookups.
        /// </summary>
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

            if (_palette == null || !TryGetComponent<Renderer>(out var renderer))
                return;

            var color = _palette.GetTileColor(_surface);
            if (_renderer == renderer && _appliedColor == color)
                return;

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
