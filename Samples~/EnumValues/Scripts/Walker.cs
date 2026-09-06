using UnityEngine;
using Aspid.FastTools.Enums;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.EnumValues
{
    // Paces back and forth over the tiles. Step cadence comes from the surface, speed from the terrain flags,
    // footprint color from the palette; every value is an EnumValues lookup with a default fallback.
    public sealed class Walker : MonoBehaviour
    {
        [SerializeField] private SurfacePalette _palette;
        [SerializeField] [Min(0.1f)] private float _speed = 3f;
        [SerializeField] [Min(1f)] private float _range = 10f;

        // Enum fixed in code. No row for a surface means the Default Value.
        [SerializeField] private EnumValues<SurfaceType, float> _stepInterval;

        // Enum picked in the Inspector (TerrainFlags here). [Flags] lookup: an exact key wins first, then the
        // first entry whose flags are all contained in the value, then the default.
        [SerializeField] private EnumValues<float> _speedByTerrain;

        [SerializeField] [Min(0.1f)] private float _footprintLifetime = 2f;

        private static readonly int _baseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int _colorId = Shader.PropertyToID("_Color");

        private int _direction = 1;
        private float _nextStep;
        private SurfaceTile _tile;

        private void Update()
        {
            var speed = _speed * (_tile is null ? 1f : _speedByTerrain.GetValue(_tile.Flags));
            var position = transform.position + Vector3.right * (_direction * speed * Time.deltaTime);
            if (Mathf.Abs(position.x) > _range) _direction = -_direction;
            transform.position = position;

            _tile = FindTileBelow();
            if (_tile is null || Time.time < _nextStep) return;

            _nextStep = Time.time + _stepInterval.GetValue(_tile.Surface);
            LeaveFootprint(_tile.Surface);
        }

        private SurfaceTile FindTileBelow() =>
            Physics.Raycast(transform.position, Vector3.down, out var hit, 5f)
                ? hit.collider.GetComponent<SurfaceTile>()
                : null;

        private void LeaveFootprint(SurfaceType surface)
        {
            var print = GameObject.CreatePrimitive(PrimitiveType.Quad);
            Destroy(print.GetComponent<Collider>());
            print.name = $"Footprint ({surface})";
            print.transform.SetPositionAndRotation(transform.position + Vector3.down * 0.49f, Quaternion.Euler(90f, 0f, 0f));
            print.transform.localScale = Vector3.one * 0.5f;

            var block = new MaterialPropertyBlock();
            var color = _palette.GetFootprintColor(surface);
            block.SetColor(_baseColorId, color);
            block.SetColor(_colorId, color);
            print.GetComponent<Renderer>().SetPropertyBlock(block);

            Destroy(print, _footprintLifetime);
        }

        [ContextMenu("Log Tables")]
        private void LogTables()
        {
            // foreach yields the configured rows only, in list order; the default value is not part of it.
            foreach (var (surface, interval) in _stepInterval)
                Debug.Log($"Step interval {surface}: {interval:0.00}s", this);

            foreach (var (flags, multiplier) in _speedByTerrain)
                Debug.Log($"Speed x{multiplier:0.00} on [{flags}]", this);
        }
    }
}
