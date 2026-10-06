using System;
using UnityEditor;
using UnityEngine;
using UnityEditorInternal;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors.Internal
{
    [UxmlElement(libraryPath = "Aspid/FastTools")]
    internal sealed partial class AspidAnimatedDotsBackground : VisualElement
    {
        private const int BlobCount = 3;
        private const int DotSegments = 10;
        private const float MinVisibleAlpha = 1f / 255f;

        internal const int VerticesPerDot = DotSegments * 2 + 1;
        internal const int IndicesPerDot = DotSegments * 9;
        internal const int MaxDotsPerMesh = ushort.MaxValue / VerticesPerDot;
        private const string StyleSheetPath = "UI/Components/Aspid-FastTools-AspidAnimatedDotsBackground";

        private readonly Vector2[] _blobRadii = new Vector2[BlobCount];
        private readonly Vector2[] _blobCenters = new Vector2[BlobCount];

        private readonly StatusStyle _status;
        private readonly AspidAnimatedDotsBackgroundColorsStyle _colors;
        private readonly AspidAnimatedDotsBackgroundSizeStyle _size;

        private Vertex[] _vertices = Array.Empty<Vertex>();
        private ushort[] _indices = Array.Empty<ushort>();

        private IVisualElementScheduledItem _animation;

        [UxmlAttribute]
        public StatusStyle.Type Status
        {
            get => _status.Value;
            set => _status.SetValue(value);
        }

        [UxmlAttribute]
        public Color Color1
        {
            get => _colors.Color1;
            set => _colors.SetColor1(value);
        }

        [UxmlAttribute]
        public Color Color2
        {
            get => _colors.Color2;
            set => _colors.SetColor2(value);
        }

        [UxmlAttribute]
        public Color Color3
        {
            get => _colors.Color3;
            set => _colors.SetColor3(value);
        }

        [UxmlAttribute]
        public float DotRadius
        {
            get => _size.DotRadius;
            set => _size.SetDotRadius(value);
        }

        [UxmlAttribute]
        public float DotSpacing
        {
            get => _size.DotSpacing;
            set => _size.SetDotSpacing(value);
        }

        [UxmlAttribute]
        public float ScaleReferenceSize
        {
            get => _size.ScaleReference;
            set => _size.SetScaleReference(value);
        }

        public AspidAnimatedDotsBackground()
            : this(AspidAnimatedDotsBackgroundPreset.Default) { }

        public AspidAnimatedDotsBackground(AspidAnimatedDotsBackgroundPreset preset)
        {
            this.AddStyleSheetFromResources(StyleSheetPath);
            generateVisualContent += OnGenerateVisualContent;

            _status = new StatusStyle(this, preset.Status);

            _colors = new AspidAnimatedDotsBackgroundColorsStyle(
                this, preset.Color1, preset.Color2, preset.Color3, MarkDirtyRepaint);

            _size = new AspidAnimatedDotsBackgroundSizeStyle(
                this, preset.DotRadius, preset.DotSpacing, preset.ScaleReferenceSize, MarkDirtyRepaint);

            _animation = schedule.Execute(Tick).Every(33);

            RegisterCallback<AttachToPanelEvent>(_ => _animation.Resume());
            RegisterCallback<DetachFromPanelEvent>(_ => _animation.Pause());
        }

        private void Tick()
        {
            // The field only drifts with time, so there is nothing new to show while Unity is in the background.
            if (InternalEditorUtility.isApplicationActive)
                MarkDirtyRepaint();
        }

        private void OnGenerateVisualContent(MeshGenerationContext context)
        {
            var rect = contentRect;
            if (rect.width <= 0f || rect.height <= 0f) return;
            if (!(_size.ScaleReference > 0f)) return;

            var time = (float)EditorApplication.timeSinceStartup;

            var scale = Mathf.Sqrt(Mathf.Min(rect.width, rect.height) / _size.ScaleReference);
            var spacing = _size.DotSpacing * scale;
            var radius = _size.DotRadius * scale;
            if (!(spacing > 0f) || !(radius > 0f)) return;

            var fringe = 1f / Mathf.Max(EditorGUIUtility.pixelsPerPoint, 1f);

            for (var i = 0; i < BlobCount; i++)
            {
                _blobCenters[i] = GetBlobCenter(i, rect.size, time);
                _blobRadii[i] = GetBlobRadius(i, rect.size);
            }

            // Every dot goes into one hand-built mesh: a Painter2D path and Fill per dot re-tessellated
            // a thousand-plus paths on every frame.
            var dotCount = 0;

            for (var y = spacing * 0.5f; y < rect.height; y += spacing)
            {
                for (var x = spacing * 0.5f; x < rect.width; x += spacing)
                {
                    var (strength, blendedHighlight) = SampleBlobField(new Vector2(x, y), time);
                    strength = Mathf.SmoothStep(0f, 1f, strength);

                    var color = blendedHighlight;
                    color.a = blendedHighlight.a * strength;
                    if (color.a < MinVisibleAlpha) continue;

                    EnsureCapacity(dotCount + 1);
                    WriteDot(_vertices, _indices, dotCount, new Vector2(x, y), radius, fringe, color);

                    if (++dotCount is MaxDotsPerMesh)
                    {
                        Flush(context, dotCount);
                        dotCount = 0;
                    }
                }
            }

            if (dotCount > 0)
                Flush(context, dotCount);
        }

        private void EnsureCapacity(int dotCount)
        {
            var vertexCount = dotCount * VerticesPerDot;
            if (_vertices.Length >= vertexCount) return;

            var capacity = Mathf.Min(Mathf.Max(dotCount, _vertices.Length / VerticesPerDot * 2, 64), MaxDotsPerMesh);
            Array.Resize(ref _vertices, capacity * VerticesPerDot);
            Array.Resize(ref _indices, capacity * IndicesPerDot);
        }

        private void Flush(MeshGenerationContext context, int dotCount)
        {
            var vertexCount = dotCount * VerticesPerDot;
            var indexCount = dotCount * IndicesPerDot;
            var mesh = context.Allocate(vertexCount, indexCount);

            for (var i = 0; i < vertexCount; i++)
                mesh.SetNextVertex(_vertices[i]);

            for (var i = 0; i < indexCount; i++)
                mesh.SetNextIndex(_indices[i]);
        }

        // A solid fan out to half a pixel inside the radius and a ring fading to transparent half a pixel outside it,
        // which stands in for the anti-aliased edge Painter2D gave each dot.
        internal static void WriteDot(
            Vertex[] vertices, ushort[] indices, int slot, Vector2 center, float radius, float fringe, Color color)
        {
            var firstVertex = slot * VerticesPerDot;
            var firstIndex = slot * IndicesPerDot;

            var innerRadius = Mathf.Max(radius - fringe * 0.5f, 0f);
            var outerRadius = radius + fringe * 0.5f;
            var transparent = new Color(color.r, color.g, color.b, 0f);

            vertices[firstVertex] = CreateVertex(center, color);

            for (var i = 0; i < DotSegments; i++)
            {
                var angle = Mathf.PI * 2f * i / DotSegments;
                var direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

                vertices[firstVertex + 1 + i] = CreateVertex(center + direction * innerRadius, color);
                vertices[firstVertex + 1 + DotSegments + i] = CreateVertex(center + direction * outerRadius, transparent);
            }

            var centerIndex = (ushort)firstVertex;
            var index = firstIndex;

            for (var i = 0; i < DotSegments; i++)
            {
                var next = (i + 1) % DotSegments;
                var inner = (ushort)(centerIndex + 1 + i);
                var innerNext = (ushort)(centerIndex + 1 + next);
                var outer = (ushort)(centerIndex + 1 + DotSegments + i);
                var outerNext = (ushort)(centerIndex + 1 + DotSegments + next);

                indices[index++] = centerIndex;
                indices[index++] = inner;
                indices[index++] = innerNext;

                indices[index++] = inner;
                indices[index++] = outer;
                indices[index++] = outerNext;

                indices[index++] = inner;
                indices[index++] = outerNext;
                indices[index++] = innerNext;
            }
        }

        private static Vertex CreateVertex(Vector2 position, Color color) => new()
        {
            position = new Vector3(position.x, position.y, Vertex.nearZ),
            tint = color,
        };

        private (float strength, Color blendedHighlight) SampleBlobField(Vector2 point, float time)
        {
            var maxField = 0f;
            var totalContribution = 0f;
            var blendedHighlight = new Color(0f, 0f, 0f, 0f);

            for (var i = 0; i < BlobCount; i++)
            {
                var center = _blobCenters[i];
                var radius = _blobRadii[i];
                var distortedPoint = ApplyDistortion(point, i, time);

                var dx = (distortedPoint.x - center.x) / radius.x;
                var dy = (distortedPoint.y - center.y) / radius.y;

                var distance = dx * dx + dy * dy;
                var contribution = Mathf.Exp(-distance * 1.2f);

                if (contribution > maxField)
                    maxField = contribution;

                totalContribution += contribution;
                blendedHighlight += _colors[i] * contribution;
            }

            if (totalContribution > 0f) blendedHighlight /= totalContribution;
            else blendedHighlight = _colors[0];

            var strength = Mathf.InverseLerp(0.05f, 0.92f, maxField);
            return (strength, blendedHighlight);
        }

        private static Vector2 GetBlobCenter(int index, Vector2 size, float time) => index switch
        {
            0 => new Vector2(
                size.x * (0.24f + Mathf.Sin(time * 0.38f) * 0.11f),
                size.y * (0.30f + Mathf.Cos(time * 0.44f) * 0.09f)),

            1 => new Vector2(
                size.x * (0.63f + Mathf.Sin(time * 0.32f + 1.4f) * 0.14f),
                size.y * (0.42f + Mathf.Cos(time * 0.35f + 0.6f) * 0.11f)),

            _ => new Vector2(
                size.x * (0.46f + Mathf.Sin(time * 0.27f + 2.3f) * 0.16f),
                size.y * (0.73f + Mathf.Cos(time * 0.32f + 2.8f) * 0.1f)),
        };

        private static Vector2 GetBlobRadius(int index, Vector2 size) => index switch
        {
            0 => new Vector2(size.x * 0.22f, size.y * 0.16f),
            1 => new Vector2(size.x * 0.26f, size.y * 0.20f),
            _ => new Vector2(size.x * 0.21f, size.y * 0.15f),
        };

        private static Vector2 ApplyDistortion(Vector2 point, int index, float time)
        {
            var offsetX = Mathf.Sin(point.y * 0.026f + time * 1.18f + index * 1.7f) * 14f
                + Mathf.Cos(point.x * 0.018f - time * 0.82f + index) * 7f;

            var offsetY = Mathf.Cos(point.x * 0.022f - time * 0.96f + index * 0.9f) * 15f
                + Mathf.Sin(point.y * 0.019f + time * 0.67f + index * 1.3f) * 6f;

            return new Vector2(point.x + offsetX, point.y + offsetY);
        }
    }
}
