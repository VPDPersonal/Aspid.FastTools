using UnityEngine;
using NUnit.Framework;
using UnityEngine.UIElements;

namespace Aspid.FastTools.UIElements.Editors.Internal.Tests
{
    [TestFixture]
    internal sealed class AspidAnimatedDotsBackgroundMeshTests
    {
        private const int Slot = 2;
        private const float Radius = 2f;
        private const float Fringe = 1f;

        private static readonly Vector2 Center = new(40f, 25f);
        private static readonly Color Color = new(0.2f, 0.6f, 0.4f, 0.8f);

        private Vertex[] _vertices;
        private ushort[] _indices;

        [SetUp]
        public void SetUp()
        {
            _vertices = new Vertex[(Slot + 1) * AspidAnimatedDotsBackground.VerticesPerDot];
            _indices = new ushort[(Slot + 1) * AspidAnimatedDotsBackground.IndicesPerDot];

            AspidAnimatedDotsBackground.WriteDot(_vertices, _indices, Slot, Center, Radius, Fringe, Color);
        }

        [Test]
        public void MaxDotsPerMesh_FitsSixteenBitIndices()
        {
            Assert.LessOrEqual(
                AspidAnimatedDotsBackground.MaxDotsPerMesh * AspidAnimatedDotsBackground.VerticesPerDot,
                ushort.MaxValue + 1);
        }

        [Test]
        public void WriteDot_FillsASolidCoreAndFadesTheEdge()
        {
            var firstVertex = Slot * AspidAnimatedDotsBackground.VerticesPerDot;
            var segments = (AspidAnimatedDotsBackground.VerticesPerDot - 1) / 2;

            Assert.AreEqual(Center, (Vector2)_vertices[firstVertex].position);
            Assert.AreEqual((Color32)Color, _vertices[firstVertex].tint);

            for (var i = 0; i < segments; i++)
            {
                var inner = _vertices[firstVertex + 1 + i];
                var outer = _vertices[firstVertex + 1 + segments + i];

                Assert.AreEqual(Radius - Fringe * 0.5f, Vector2.Distance(Center, inner.position), 1e-4f);
                Assert.AreEqual(Radius + Fringe * 0.5f, Vector2.Distance(Center, outer.position), 1e-4f);
                Assert.AreEqual((Color32)Color, inner.tint, "The core must keep the dot's color.");
                Assert.AreEqual(0, outer.tint.a, "The edge must fade out to transparent.");
            }
        }

        [Test]
        public void WriteDot_IndexesOnlyItsOwnVertices()
        {
            var firstVertex = Slot * AspidAnimatedDotsBackground.VerticesPerDot;
            var firstIndex = Slot * AspidAnimatedDotsBackground.IndicesPerDot;

            for (var i = firstIndex; i < _indices.Length; i++)
            {
                Assert.GreaterOrEqual(_indices[i], firstVertex);
                Assert.Less(_indices[i], firstVertex + AspidAnimatedDotsBackground.VerticesPerDot);
            }
        }

        [Test]
        public void WriteDot_WindsEveryTriangleTheSameWay()
        {
            var firstIndex = Slot * AspidAnimatedDotsBackground.IndicesPerDot;

            for (var i = firstIndex; i < _indices.Length; i += 3)
            {
                var a = (Vector2)_vertices[_indices[i]].position;
                var b = (Vector2)_vertices[_indices[i + 1]].position;
                var c = (Vector2)_vertices[_indices[i + 2]].position;

                var cross = (b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x);
                Assert.Greater(cross, 0f, $"Triangle {(i - firstIndex) / 3} is degenerate or wound the other way.");
            }
        }
    }
}
