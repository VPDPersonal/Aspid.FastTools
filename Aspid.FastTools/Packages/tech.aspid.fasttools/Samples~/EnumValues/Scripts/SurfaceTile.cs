using UnityEngine;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.EnumValues
{
    [ExecuteAlways]
    public sealed class SurfaceTile : MonoBehaviour
    {
        private static readonly List<SurfaceTile> _enabledTiles = new();
        private static readonly int _colorId = Shader.PropertyToID("_Color");

        [SerializeField] private SurfaceType _surface;
        [SerializeField] private TerrainFlags _flags;
        [SerializeField] private SurfacePalette _palette;

        public SurfaceType Surface => _surface;

        public TerrainFlags Flags => _flags;

        public Bounds Bounds => GetComponent<Renderer>().bounds;

        public static IReadOnlyList<SurfaceTile> Enabled => _enabledTiles;

        private void OnEnable()
        {
            _enabledTiles.Add(this);
            Refresh();
        }

        private void OnDisable() =>
            _enabledTiles.Remove(this);

        private void OnValidate() =>
            Refresh();

        public void Refresh()
        {
            if (_palette == null || !TryGetComponent<Renderer>(out var tileRenderer))
                return;

            var block = new MaterialPropertyBlock();
            block.SetColor(nameID: _colorId, value: _palette.GetTileColor(_surface));
            tileRenderer.SetPropertyBlock(block);

#if UNITY_EDITOR
            UnityEditor.SceneView.RepaintAll();
#endif
        }
    }
}
