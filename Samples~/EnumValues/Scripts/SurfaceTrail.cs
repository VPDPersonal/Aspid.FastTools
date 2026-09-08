using UnityEngine;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.EnumValues
{
    // Each color section retains its original color when the palette changes.
    /// <summary>
    /// <see cref="MonoBehaviour"/> that draws a ribbon with a smoothly expiring tail.
    /// </summary>
    public sealed class SurfaceTrail : MonoBehaviour
    {
        private readonly List<Vector3> _points = new();
        private readonly List<float> _times = new();
        private readonly List<Vector3> _vertices = new();
        private readonly List<Vector3> _normals = new();
        private readonly List<int> _triangles = new();
        private Mesh _mesh;
        private float _lifetime;

        /// <summary>
        /// Creates the ribbon mesh and renderer.
        /// </summary>
        /// <param name="material">Shared material used to render the ribbon.</param>
        /// <param name="color">Color applied to the entire ribbon.</param>
        /// <param name="lifetime">Lifetime of each point in seconds.</param>
        /// <remarks>Call once before adding points or enabling frame updates.</remarks>
        public void Initialize(Material material, Color color, float lifetime)
        {
            _lifetime = lifetime;
            _mesh = new Mesh { name = "Surface trail ribbon" };
            _mesh.MarkDynamic();
            gameObject.AddComponent<MeshFilter>().sharedMesh = _mesh;
            var renderer = gameObject.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            var block = new MaterialPropertyBlock();
            block.SetColor("_Color", color);
            block.SetColor("_BaseColor", color);
            renderer.SetPropertyBlock(block);
        }

        /// <summary>
        /// Appends a timestamped point to the ribbon.
        /// </summary>
        /// <param name="position">Point in world space.</param>
        /// <param name="force">When <see langword="true"/>, adds the point even when it is close to the previous one.</param>
        public void AddPoint(Vector3 position, bool force = false)
        {
            if (!force && _points.Count > 0 && Vector3.Distance(_points[_points.Count - 1], position) < 0.02f)
                return;

            _points.Add(position);
            _times.Add(Time.time);
        }

        private void LateUpdate()
        {
            var cutoff = Time.time - _lifetime;
            while (_times.Count > 1 && _times[1] <= cutoff)
            {
                _times.RemoveAt(0);
                _points.RemoveAt(0);
            }

            _mesh.Clear();
            if (_points.Count < 2)
                return;

            _vertices.Clear();
            _normals.Clear();
            _triangles.Clear();
            var halfWidth = Vector3.forward * 0.225f;
            for (var i = 0; i < _points.Count; i++)
            {
                var point = _points[i];
                if (i == 0 && _times[0] < cutoff)
                    point = Vector3.Lerp(point, _points[1], Mathf.InverseLerp(_times[0], _times[1], cutoff));
                _vertices.Add(point + halfWidth);
                _vertices.Add(point - halfWidth);
                _normals.Add(Vector3.up);
                _normals.Add(Vector3.up);
                if (i == 0)
                    continue;

                var a = (i - 1) * 2;
                if (_points[i].x >= _points[i - 1].x)
                {
                    _triangles.Add(a);
                    _triangles.Add(a + 2);
                    _triangles.Add(a + 1);
                    _triangles.Add(a + 1);
                    _triangles.Add(a + 2);
                    _triangles.Add(a + 3);
                }
                else
                {
                    _triangles.Add(a);
                    _triangles.Add(a + 1);
                    _triangles.Add(a + 2);
                    _triangles.Add(a + 1);
                    _triangles.Add(a + 3);
                    _triangles.Add(a + 2);
                }
            }
            _mesh.SetVertices(_vertices);
            _mesh.SetNormals(_normals);
            _mesh.SetTriangles(_triangles, 0);
            _mesh.RecalculateBounds();
        }

        private void OnDestroy()
        {
            if (_mesh != null)
                Destroy(_mesh);
        }
    }
}
