using System;
using UnityEngine;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors.Internal
{
    // A button-like VisualElement with a horizontal gradient background and an animated accent overlay that fades in
    // on hover. The gradient and accent colors are sourced from USS custom properties or set via UXML attributes /
    // fluent extensions.
    [UxmlElement(libraryPath = "Aspid/FastTools")]
    internal sealed partial class AspidGradientButton : VisualElement
    {
        private const string StyleSheetPath = "UI/Components/Aspid-FastTools-AspidGradientButton";
        private const string BlockClass = "aspid-fasttools-gradient-button";
        private const string LabelClass = "aspid-fasttools-gradient-button__label";
        private const string TrailingLabelClass = "aspid-fasttools-gradient-button__trailing-label";

        private readonly Label _label;
        private readonly Label _trailingLabel;
        private readonly AspidHoverGradientOverlay _overlay;
        private readonly AspidGradientButtonColorsStyle _colors;

        private Texture2D _gradientTexture;
        private bool _highlighted;
        private bool _hovered;

        internal bool Highlighted
        {
            get => _highlighted;
            set
            {
                _highlighted = value;
                ApplyHoverVisual(_highlighted || _hovered);
            }
        }

        [UxmlAttribute]
        public string Text
        {
            get => _label.text;
            set => _label.text = value;
        }

        [UxmlAttribute]
        public string TrailingText
        {
            get => _trailingLabel.text;
            set
            {
                _trailingLabel.text = value ?? string.Empty;
                _trailingLabel.style.display = string.IsNullOrEmpty(value)
                    ? DisplayStyle.None
                    : DisplayStyle.Flex;
            }
        }

        [UxmlAttribute]
        public Color Gradient
        {
            get => _colors.Gradient;
            set => _colors.SetGradient(value);
        }

        [UxmlAttribute]
        public Color Accent
        {
            get => _colors.Accent;
            set => _colors.SetAccent(value);
        }

        public AspidGradientButton()
            : this(AspidGradientButtonPreset.Default) { }

        public AspidGradientButton(string text, Action<EventBase> onClick = null)
            : this(AspidGradientButtonPreset.Default.SetText(text).SetOnClick(onClick)) { }

        public AspidGradientButton(string text, string trailingText, Action<EventBase> onClick = null)
            : this(AspidGradientButtonPreset.Default
                .SetText(text)
                .SetTrailingText(trailingText)
                .SetOnClick(onClick)) { }

        public AspidGradientButton(AspidGradientButtonPreset preset)
        {
            this.AddClass(BlockClass)
                .AddStyleSheetsFromResource(StyleSheetPath);
            focusable = true;

            // Drawn on top of the static gradient pill but BEFORE the label, so the text
            // always reads cleanly above the hover effect.
            _overlay = new AspidHoverGradientOverlay();
            Add(_overlay);

            _label = new Label(preset.Text)
                .SetFlexGrow(1f)
                .SetPickingMode(PickingMode.Ignore);
            _label.AddClass(LabelClass);
            Add(_label);

            _trailingLabel = new Label(preset.TrailingText ?? string.Empty)
                .SetPickingMode(PickingMode.Ignore);
            _trailingLabel.AddClass(LabelClass);
            _trailingLabel.AddClass(TrailingLabelClass);
            if (string.IsNullOrEmpty(preset.TrailingText))
                _trailingLabel.style.display = DisplayStyle.None;
            Add(_trailingLabel);

            if (preset.OnClick != null)
                this.AddManipulator(new Clickable(preset.OnClick));

            _colors = new AspidGradientButtonColorsStyle(
                this,
                preset.Gradient,
                preset.Accent,
                RebuildGradient,
                accent => _overlay.Color = accent);

            RegisterCallback<MouseEnterEvent>(OnMouseEnter);
            RegisterCallback<MouseLeaveEvent>(OnMouseLeave);
            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
        }

        public T AddLeadingContent<T>(T content) where T : VisualElement
        {
            Insert(IndexOf(_label), content);
            return content;
        }

        public T AddTrailingContent<T>(T content) where T : VisualElement
        {
            Insert(IndexOf(_label) + 1, content);
            return content;
        }

        public void FillWithLeadingContent() => _label.style.flexGrow = 0f;

        public void FillWithTrailingContent() => _label.style.flexGrow = 0f;

        private void OnMouseEnter(MouseEnterEvent _)
        {
            _hovered = true;
            ApplyHoverVisual(true);
        }

        private void OnMouseLeave(MouseLeaveEvent _)
        {
            _hovered = false;
            ApplyHoverVisual(_highlighted);
        }

        private void ApplyHoverVisual(bool on)
        {
            _overlay.SetTarget(on ? 1f : 0f);
            var labelColor = on ? new StyleColor(_colors.Accent) : new StyleColor(StyleKeyword.Null);
            _label.style.color = labelColor;
            _trailingLabel.style.color = labelColor;
        }

        private void OnAttachToPanel(AttachToPanelEvent _)
        {
            if (_gradientTexture == null)
                RebuildGradient(_colors.Gradient);
        }

        private void OnDetachFromPanel(DetachFromPanelEvent _) => DisposeTexture();

        private void RebuildGradient(Color color)
        {
            // Textures live only while the element is attached: OnAttachToPanel builds the first
            // one and OnDetachFromPanel disposes it, so a detached button never holds a texture.
            if (panel == null) return;

            DisposeTexture();
            _gradientTexture = CreateHorizontalFadeTexture(color);
            style.backgroundImage = new StyleBackground(_gradientTexture);
        }

        private void DisposeTexture()
        {
            if (_gradientTexture == null) return;

            UnityEngine.Object.DestroyImmediate(_gradientTexture);
            _gradientTexture = null;
        }

        private static Texture2D CreateHorizontalFadeTexture(Color baseColor)
        {
            const int width = 256;
            var texture = new Texture2D(width, 1, TextureFormat.RGBA32, mipChain: false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
                hideFlags = HideFlags.HideAndDontSave,
            };

            var pixels = new Color[width];
            for (var i = 0; i < width; i++)
            {
                var t = (float)i / (width - 1);
                pixels[i] = new Color(baseColor.r, baseColor.g, baseColor.b, baseColor.a * (1f - t));
            }

            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }
    }
}
