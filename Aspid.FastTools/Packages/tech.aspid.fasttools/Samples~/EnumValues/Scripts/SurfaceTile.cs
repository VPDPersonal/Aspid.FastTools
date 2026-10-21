using UnityEngine;
using System.Collections.Generic;

namespace Aspid.FastTools.Samples.EnumValues
{
    [ExecuteAlways]
    public sealed class SurfaceTile : MonoBehaviour
    {
        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private static readonly List<SurfaceTile> EnabledTiles = new();

        [SerializeField] private SurfaceType _surface;
        [SerializeField] private TerrainFlags _flags;
        [SerializeField] private SurfacePalette _palette;

        public SurfaceType Surface => _surface;

        public TerrainFlags Flags => _flags;

        public Bounds Bounds => GetComponent<Renderer>().bounds;

        public static IReadOnlyList<SurfaceTile> Enabled => EnabledTiles;

        private void OnEnable()
        {
            EnabledTiles.Add(this);
            Refresh();
        }

        private void OnDisable() => EnabledTiles.Remove(this);

        private void OnValidate() => Refresh();

        public void Refresh()
        {
            if (_palette == null || !TryGetComponent<Renderer>(out var tileRenderer))
                return;

            var block = new MaterialPropertyBlock();
            block.SetColor(nameID: ColorId, value: _palette.GetTileColor(_surface));
            tileRenderer.SetPropertyBlock(block);
#if UNITY_EDITOR
            UnityEditor.SceneView.RepaintAll();
#endif
        }
    }
}
