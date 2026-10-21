using UnityEngine;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.EnumValues.Presentation
{
    public sealed class SurfaceTrail : MonoBehaviour
    {
        private const float HalfWidth = 0.225f;

        private static readonly int[] _forward = { 0, 2, 1, 1, 2, 3 };
        private static readonly int[] _backward = { 0, 1, 2, 1, 3, 2 };

        private readonly List<Vector3> _points = new();
        private readonly List<float> _times = new();
        private readonly List<Vector3> _vertices = new();
        private readonly List<Vector3> _normals = new();
        private readonly List<int> _triangles = new();

        private Mesh _mesh;
        private float _lifetime;

        private void LateUpdate()
        {
            var cutoff = Time.time - _lifetime;
            while (_times.Count > 1 && _times[1] <= cutoff)
            {
                _times.RemoveAt(index: 0);
                _points.RemoveAt(index: 0);
            }

            _mesh.Clear();
            if (_points.Count < 2)
                return;

            _vertices.Clear();
            _normals.Clear();
            _triangles.Clear();
            var halfWidth = Vector3.forward * HalfWidth;

            for (var i = 0; i < _points.Count; i++)
            {
                var point = _points[i];

                if (i == 0 && _times[0] < cutoff)
                    point = Vector3.Lerp(a: point, b: _points[1], t: Mathf.InverseLerp(a: _times[0], b: _times[1], value: cutoff));

                _vertices.Add(point + halfWidth);
                _vertices.Add(point - halfWidth);
                _normals.Add(Vector3.up);
                _normals.Add(Vector3.up);
                if (i == 0)
                    continue;

                var start = (i - 1) * 2;
                foreach (var offset in _points[i].x >= _points[i - 1].x ? _forward : _backward)
                    _triangles.Add(start + offset);
            }

            _mesh.SetVertices(_vertices);
            _mesh.SetNormals(_normals);
            _mesh.SetTriangles(triangles: _triangles, submesh: 0);
            _mesh.RecalculateBounds();
        }

        private void OnDestroy() =>
            Destroy(_mesh);

        public void Initialize(Material material, Color color, float lifetime)
        {
            _lifetime = lifetime;
            _mesh = new Mesh();
            gameObject.AddComponent<MeshFilter>().sharedMesh = _mesh;

            var trailRenderer = gameObject.AddComponent<MeshRenderer>();
            trailRenderer.sharedMaterial = material;
            var block = new MaterialPropertyBlock();
            block.SetColor(name: "_Color", value: color);
            trailRenderer.SetPropertyBlock(block);
        }

        public void AddPoint(Vector3 position, bool force = false)
        {
            if (!force && _points.Count > 0 && Vector3.Distance(a: _points[^1], b: position) < 0.02f)
                return;

            _points.Add(position);
            _times.Add(Time.time);
        }
    }
}
