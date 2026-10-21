using UnityEngine;
using Aspid.FastTools.Enums;

namespace Aspid.FastTools.Samples.EnumValues
{
    public sealed class Walker : MonoBehaviour
    {
        [SerializeField] private SurfacePalette _palette;
        [SerializeField, Min(0.1f)] private float _speed = 3f;
        [SerializeField, Min(1f)] private float _range = 10f;
        [SerializeField] private EnumValues<float> _speedByTerrain;
        [SerializeField, Min(0.1f)] private float _footprintLifetime = 2f;

        private int _direction = 1;
        private SurfaceTile[] _tiles;
        private SurfaceTile _tile;
        private SurfaceTrail _trail;
        private Material _trailMaterial;
        private Vector3 _lastPoint;

        private void Awake()
        {
            _trailMaterial = GetComponent<Renderer>().sharedMaterial;
            _tiles = FindObjectsByType<SurfaceTile>(FindObjectsSortMode.None);
        }

        private void Update()
        {
            var speed = _speed * (_tile == null ? 1f : _speedByTerrain.GetValue(_tile.Flags));
            var position = transform.position + Vector3.right * (_direction * speed * Time.deltaTime);
            if (Mathf.Abs(position.x) > _range)
                _direction = -_direction;
            transform.position = position;

            var previous = _tile;
            _tile = FindTile(position.x);
            if (_tile == null)
            {
                FinishTrail();
                return;
            }

            var point = new Vector3(x: position.x, y: _tile.Bounds.max.y + 0.01f, z: position.z);
            if (_trail == null || previous != _tile)
            {
                var join = point;
                if (_trail != null && previous != null)
                {
                    join.x = point.x > _lastPoint.x ? previous.Bounds.max.x : previous.Bounds.min.x;
                    _trail.AddPoint(join, force: true);
                }

                BeginTrail(join, _palette.GetFootprintColor(_tile.Surface));
            }

            _trail.AddPoint(point);
            _lastPoint = point;
        }

        private void OnDisable() => FinishTrail();

        private SurfaceTile FindTile(float x)
        {
            foreach (var tile in _tiles)
            {
                var bounds = tile.Bounds;
                if (x >= bounds.min.x && x <= bounds.max.x)
                    return tile;
            }

            return null;
        }

        private void BeginTrail(Vector3 position, Color color)
        {
            FinishTrail();
            _trail = new GameObject("Surface trail").AddComponent<SurfaceTrail>();
            _trail.Initialize(material: _trailMaterial, color: color, lifetime: _footprintLifetime);
            _trail.AddPoint(position, force: true);
        }

        private void FinishTrail()
        {
            if (_trail == null)
                return;

            Destroy(obj: _trail.gameObject, t: _footprintLifetime);
            _trail = null;
        }
    }
}
