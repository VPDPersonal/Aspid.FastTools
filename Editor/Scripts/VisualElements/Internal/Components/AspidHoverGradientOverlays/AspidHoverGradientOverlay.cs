using UnityEngine;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors.Internal
{
    [UxmlElement(libraryPath = "Aspid/FastTools")]
    internal sealed partial class AspidHoverGradientOverlay : VisualElement
    {
        private const long TickMs = 16;
        private const float DrawThreshold = 0.01f;
        private const float ProgressEpsilon = 0.001f;
        private const int DefaultSteps = 75;
        private const float DefaultLerpRate = 0.12f;
        private const float DefaultAlphaScale = 0.35f;

        // Each strip emits six indices; cap the mesh conservatively within the 16-bit range.
        private const int MaxSteps = ushort.MaxValue / 6;
        private const string StyleSheetPath = "UI/Components/Aspid-FastTools-AspidHoverGradientOverlay";

        private readonly AspidHoverGradientOverlayColorStyle _color;
        private readonly AspidHoverGradientOverlayMetricsStyle _metrics;

        private float _progress;
        private float _targetProgress;
        private IVisualElementScheduledItem _animation;

        [UxmlAttribute]
        public Color Color
        {
            get => _color.Value;
            set => _color.SetValue(value);
        }

        [UxmlAttribute]
        public int Steps
        {
            get => _metrics.Steps;
            set => _metrics.SetSteps(value);
        }

        [UxmlAttribute]
        public float LerpRate
        {
            get => _metrics.LerpRate;
            set => _metrics.SetLerpRate(value);
        }

        [UxmlAttribute]
        public float AlphaScale
        {
            get => _metrics.AlphaScale;
            set => _metrics.SetAlphaScale(value);
        }

        public AspidHoverGradientOverlay()
        {
            this.AddStyleSheetFromResources(StyleSheetPath);
            pickingMode = PickingMode.Ignore;

            _color = new AspidHoverGradientOverlayColorStyle(this, default, MarkDirtyRepaint);
            _metrics = new AspidHoverGradientOverlayMetricsStyle(this, DefaultSteps, DefaultLerpRate, DefaultAlphaScale, MarkDirtyRepaint);

            generateVisualContent += DrawOverlay;
            _animation = schedule.Execute(Tick).Every(TickMs);

            RegisterCallback<AttachToPanelEvent>(_ => _animation.Resume());
            RegisterCallback<DetachFromPanelEvent>(_ => _animation.Pause());
        }

        public void SetTarget(float target) => _targetProgress = Mathf.Clamp01(target);

        private void Tick()
        {
            var previous = _progress;
            _progress = Mathf.Lerp(_progress, _targetProgress, _metrics.LerpRate);

            if (Mathf.Abs(_progress - previous) > ProgressEpsilon)
                MarkDirtyRepaint();
        }

        private void DrawOverlay(MeshGenerationContext ctx)
        {
            if (_progress <= DrawThreshold) return;

            var rect = contentRect;
            if (rect.width <= 0f || rect.height <= 0f) return;

            var steps = Mathf.Clamp(_metrics.Steps, 1, MaxSteps);
            var alphaScale = _metrics.AlphaScale;
            var baseColor = _color.Value;

            // Shared boundary vertices prevent seams between separately anti-aliased strips.
            var columns = steps + 1;
            var mesh = ctx.Allocate(columns * 2, steps * 6);

            for (var j = 0; j < columns; j++)
            {
                var t = (float)j / steps;
                var alpha = (1f - t) * (1f - t) * _progress * alphaScale;
                var color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
                var x = t * rect.width;

                mesh.SetNextVertex(new Vertex { position = new Vector3(x, 0f, Vertex.nearZ), tint = color });
                mesh.SetNextVertex(new Vertex { position = new Vector3(x, rect.height, Vertex.nearZ), tint = color });
            }

            for (var i = 0; i < steps; i++)
            {
                var topLeft = (ushort)(i * 2);
                var bottomLeft = (ushort)(i * 2 + 1);
                var topRight = (ushort)(i * 2 + 2);
                var bottomRight = (ushort)(i * 2 + 3);

                mesh.SetNextIndex(bottomLeft);
                mesh.SetNextIndex(topLeft);
                mesh.SetNextIndex(topRight);

                mesh.SetNextIndex(topRight);
                mesh.SetNextIndex(bottomRight);
                mesh.SetNextIndex(bottomLeft);
            }
        }
    }
}
