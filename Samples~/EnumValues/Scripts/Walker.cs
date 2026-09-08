using UnityEngine;
using Aspid.FastTools.Enums;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.EnumValues
{
    // Paces over the tiles, drawing a continuous trail. Color sampling comes from the surface, speed from
    // terrain flags and trail color from the palette; every lookup has a default fallback.
    public sealed class Walker : MonoBehaviour
    {
        [SerializeField] private SurfacePalette _palette;
        [SerializeField] [Min(0.1f)] private float _speed = 3f;
        [SerializeField] [Min(1f)] private float _range = 10f;

        // Enum fixed in code. No row for a surface means the Default Value.
        [SerializeField] [InspectorName("Color Sample Interval")]
        private EnumValues<SurfaceType, float> _stepInterval;

        // Enum picked in the Inspector (TerrainFlags here). [Flags] lookup: an exact key wins first, then the
        // first entry whose flags are all contained in the value, then the default.
        [SerializeField] private EnumValues<float> _speedByTerrain;

        [SerializeField] [Min(0.1f)] [InspectorName("Trail Lifetime")]
        private float _footprintLifetime = 2f;

        private int _direction = 1;
        private float _nextStep;
        private SurfaceTile _tile;
        private SurfaceTrail _trail;
        private Material _trailMaterial;
        private Color _trailColor;
        private Vector3 _lastTrailPosition;

        private void Awake() => _trailMaterial = GetComponent<Renderer>().sharedMaterial;

        private void Update()
        {
            var speed = _speed * (_tile is null ? 1f : _speedByTerrain.GetValue(_tile.Flags));
            var position = transform.position + Vector3.right * (_direction * speed * Time.deltaTime);
            if (Mathf.Abs(position.x) > _range) _direction = -_direction;
            transform.position = position;

            var previousTile = _tile;
            _tile = FindTileBelow();
            if (_tile is null)
            {
                FinishTrail();
                return;
            }

            var trailPosition = transform.position;
            trailPosition.y = _tile.GetComponent<Collider>().bounds.max.y + 0.01f;
            if (_trail == null || previousTile != _tile || Time.time >= _nextStep)
            {
                _nextStep = Time.time + Mathf.Max(0.02f, _stepInterval.GetValue(_tile.Surface));
                var color = _palette.GetFootprintColor(_tile.Surface);
                if (_trail == null || color != _trailColor)
                {
                    var join = trailPosition;
                    if (_trail != null && previousTile != null && previousTile != _tile)
                    {
                        // These tiles are axis-aligned: split at the collider edge, not the previous frame.
                        var bounds = previousTile.GetComponent<Collider>().bounds;
                        var edge = trailPosition.x > _lastTrailPosition.x ? bounds.max.x : bounds.min.x;
                        var fraction = Mathf.InverseLerp(_lastTrailPosition.x, trailPosition.x, edge);
                        join = Vector3.Lerp(_lastTrailPosition, trailPosition, fraction);
                        join.x = edge;
                    }
                    if (_trail != null) _trail.AddPoint(join, true);
                    BeginTrail(join, color);
                }
            }

            // Geometry follows every frame; table sampling never creates gaps between points.
            _trail.AddPoint(trailPosition);
            _lastTrailPosition = trailPosition;
        }

        private SurfaceTile FindTileBelow() =>
            Physics.Raycast(transform.position, Vector3.down, out var hit, 5f)
                ? hit.collider.GetComponent<SurfaceTile>()
                : null;

        private void BeginTrail(Vector3 position, Color color)
        {
            FinishTrail();
            var trailObject = new GameObject("Surface trail");
            _trail = trailObject.AddComponent<SurfaceTrail>();
            _trail.Initialize(_trailMaterial, color, _footprintLifetime);
            _trailColor = color;
            _trail.AddPoint(position, true);
        }

        private void FinishTrail()
        {
            if (_trail == null) return;
            Destroy(_trail.gameObject, _footprintLifetime);
            _trail = null;
        }

        private void OnDisable() => FinishTrail();

        [ContextMenu("Log Tables")]
        private void LogTables()
        {
            // foreach yields the configured rows only, in list order; the default value is not part of it.
            foreach (var (surface, interval) in _stepInterval)
                Debug.Log($"Color sample interval {surface}: {interval:0.00}s", this);

            foreach (var (flags, multiplier) in _speedByTerrain)
                Debug.Log($"Speed x{multiplier:0.00} on [{flags}]", this);
        }
    }
}
