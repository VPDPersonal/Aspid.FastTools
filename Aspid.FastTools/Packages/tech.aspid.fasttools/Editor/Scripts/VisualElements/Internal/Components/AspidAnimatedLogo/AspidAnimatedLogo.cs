using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors.Internal
{
    [UxmlElement(libraryPath = "Aspid/FastTools", visibility = LibraryVisibility.Hidden)]
    internal sealed partial class AspidAnimatedLogo : VisualElement
    {
        private const string StyleSheetPath = "UI/Components/Aspid-FastTools-AspidAnimatedLogo";
        private const int LayerCount = 3;
        private const long AnimationIntervalMs = 33;
        private const float RestAmplitude = 0.0005f;
        private const float PulseAmplitudeSmoothing = 0.07f;
        private const string LayerClass = "aspid-fasttools-animated-logo__layer";
        private const string LayerVisibleClass = "aspid-fasttools-animated-logo__layer--visible";
        private readonly VisualElement[] _layers = new VisualElement[LayerCount];

        private readonly AspidAnimatedLogoPulseSpeedStyle _pulseSpeed;
        private readonly AspidAnimatedLogoPulseHoverAmplitudeStyle _pulseHoverAmplitude;
        private readonly AspidAnimatedLogoLayerImageStyle _layer1Image;
        private readonly AspidAnimatedLogoLayerImageStyle _layer2Image;
        private readonly AspidAnimatedLogoLayerImageStyle _layer3Image;

        private int _currentLayer;
        private bool _hovered;
        private bool _animated = true;
        private float _pulseAmplitude;
        private IVisualElementScheduledItem _colorCycle;
        private IVisualElementScheduledItem _pulse;

        [UxmlAttribute]
        public long ColorCycleIntervalMs { get; set; }

        [UxmlAttribute]
        public float PulseSpeed
        {
            get => _pulseSpeed.Value;
            set => _pulseSpeed.SetValue(value);
        }

        [UxmlAttribute]
        public float PulseHoverAmplitude
        {
            get => _pulseHoverAmplitude.Value;
            set => _pulseHoverAmplitude.SetValue(value);
        }

        [UxmlAttribute]
        public Texture2D Image1
        {
            get => _layer1Image.Value;
            set => _layer1Image.SetValue(value);
        }

        [UxmlAttribute]
        public Texture2D Image2
        {
            get => _layer2Image.Value;
            set => _layer2Image.SetValue(value);
        }

        [UxmlAttribute]
        public Texture2D Image3
        {
            get => _layer3Image.Value;
            set => _layer3Image.SetValue(value);
        }

        public AspidAnimatedLogo()
            : this(AspidAnimatedLogoPreset.Default) { }

        public AspidAnimatedLogo(AspidAnimatedLogoPreset preset)
        {
            this.AddStyleSheetFromResources(StyleSheetPath);

            ColorCycleIntervalMs = preset.ColorCycleIntervalMs;

            for (var i = 0; i < LayerCount; i++)
            {
                var layer = new VisualElement()
                    .AddClass(LayerClass)
                    .AddClass($"{LayerClass}--{i}")
                    .SetPickingMode(PickingMode.Ignore);

                this.AddChild(layer);
                _layers[i] = layer;
            }

            _layers[0].AddClass(LayerVisibleClass);

            _pulseSpeed = new AspidAnimatedLogoPulseSpeedStyle(this, preset.PulseSpeed);
            _pulseHoverAmplitude = new AspidAnimatedLogoPulseHoverAmplitudeStyle(this, preset.PulseHoverAmplitude);

            _layer1Image = new AspidAnimatedLogoLayerImageStyle(
                target: _layers[0],
                eventSource: this,
                AspidAnimatedLogoLayerImageStyle.Layer1StyleProperty,
                preset.Image1);

            _layer2Image = new AspidAnimatedLogoLayerImageStyle(
                target: _layers[1],
                eventSource: this,
                AspidAnimatedLogoLayerImageStyle.Layer2StyleProperty,
                preset.Image2);

            _layer3Image = new AspidAnimatedLogoLayerImageStyle(
                target: _layers[2],
                eventSource: this,
                AspidAnimatedLogoLayerImageStyle.Layer3StyleProperty,
                preset.Image3);

            RegisterCallback<PointerEnterEvent>(OnPointerEnter);
            RegisterCallback<PointerLeaveEvent>(OnPointerLeave);

            _pulse = schedule.Execute(UpdatePulse).Every(AnimationIntervalMs);
            _pulse.Pause();

            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
        }

        internal bool IsPulsing => _pulse.isActive;

        internal bool IsCyclingColors => _colorCycle is { isActive: true };

        private void OnAttachToPanel(AttachToPanelEvent _)
        {
            AspidAnimatedDotsBackgroundSettings.Changed += ApplyAnimationSetting;
            ApplyAnimationSetting();
        }

        private void OnDetachFromPanel(DetachFromPanelEvent _)
        {
            AspidAnimatedDotsBackgroundSettings.Changed -= ApplyAnimationSetting;
            _pulse.Pause();
            _colorCycle?.Pause();
        }

        // With the "Animated background" setting off neither the pulse nor the hover color cycle runs,
        // and the logo returns to its first layer at its natural size.
        private void ApplyAnimationSetting()
        {
            _animated = AspidAnimatedDotsBackgroundSettings.Enabled;

            if (!_animated)
            {
                _pulse.Pause();
                StopColorCycle();

                _pulseAmplitude = 0f;
                style.scale = StyleKeyword.Null;
                return;
            }

            _pulse.Resume();
            if (!_hovered) return;

            // Resume the existing item because StartColorCycle skips an already-created schedule.
            if (_colorCycle is null) StartColorCycle();
            else _colorCycle.Resume();
        }

        private void OnPointerEnter(PointerEnterEvent evt)
        {
            _hovered = true;
            if (_animated) StartColorCycle();
        }

        private void OnPointerLeave(PointerLeaveEvent evt)
        {
            _hovered = false;
            StopColorCycle();
        }

        private void StartColorCycle()
        {
            if (_colorCycle != null) return;
            _colorCycle = schedule.Execute(AdvanceLayer).Every(ColorCycleIntervalMs);
        }

        private void StopColorCycle()
        {
            _colorCycle?.Pause();
            _colorCycle = null;

            if (_currentLayer == 0) return;
            _layers[_currentLayer].RemoveClass(LayerVisibleClass);
            _layers[0].AddClass(LayerVisibleClass);
            _currentLayer = 0;
        }

        private void AdvanceLayer()
        {
            _layers[_currentLayer].RemoveClass(LayerVisibleClass);
            _currentLayer = (_currentLayer + 1) % LayerCount;
            _layers[_currentLayer].AddClass(LayerVisibleClass);
        }

        // Away from the pointer the pulse fades out and the logo rests at its natural size: a scale written on every
        // tick would keep redrawing the window while nobody hovers the logo.
        internal void UpdatePulse()
        {
            if (!_hovered && _pulseAmplitude is 0f) return;

            var targetAmplitude = _hovered ? PulseHoverAmplitude : 0f;
            _pulseAmplitude = Mathf.Lerp(_pulseAmplitude, targetAmplitude, PulseAmplitudeSmoothing);

            if (!_hovered && _pulseAmplitude < RestAmplitude)
            {
                _pulseAmplitude = 0f;
                style.scale = StyleKeyword.Null;
                return;
            }

            var time = (float)EditorApplication.timeSinceStartup;
            var pulse = 1f + Mathf.Sin(time * PulseSpeed) * _pulseAmplitude;

            style.scale = new Scale(new Vector3(pulse, pulse, 1f));
        }
    }
}
