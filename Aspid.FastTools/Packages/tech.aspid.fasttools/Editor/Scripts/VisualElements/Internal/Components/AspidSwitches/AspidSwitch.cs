using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors.Internal
{
    [UxmlElement(libraryPath = "Aspid/FastTools", visibility = LibraryVisibility.Hidden)]
    internal sealed partial class AspidSwitch : BaseField<bool>
    {
        private const string StyleSheetPath = "UI/Components/Aspid-FastTools-AspidSwitch";
        private const string SwitchClass = "aspid-fasttools-switch";
        private const float TrackWidth = 44f;
        private const float TrackHeight = 24f;
        private const float HandleSize = 18f;
        private const float TrackBorderWidth = 1.5f;
        private const float LightOffFillAlpha = 0.06f;
        private const float AnimationDuration = 0.15f;

        // UI Toolkit absolute offsets start inside the border, so handle travel uses the inner box.
        private const float TrackInnerWidth = TrackWidth - 2f * TrackBorderWidth;
        private const float TrackInnerHeight = TrackHeight - 2f * TrackBorderWidth;
        private const float HandleInset = (TrackInnerHeight - HandleSize) / 2f;

        private static readonly CustomStyleProperty<Color> _accentProperty = new("--aspid-fasttools-colors-switch-accent");
        private static readonly CustomStyleProperty<Color> _trackOffBorderProperty = new("--aspid-fasttools-colors-switch-track_border");
        private static readonly CustomStyleProperty<Color> _handleProperty = new("--aspid-fasttools-colors-switch-handle");
        private static readonly CustomStyleProperty<Color> _handleShadowProperty = new("--aspid-fasttools-colors-switch-handle_shadow");

        // Fallbacks while the --aspid-colors-switch-* palette tokens are unset; the skin is read on every style
        // resolve, so a switch of the editor skin updates them. The accents are the documentation site's greens.
        // On the dark skin an on switch tints its track under a light handle; on the light skin the track fills with
        // the accent under a white handle, since a pale tint under a dark handle reads as disabled there.
        private static Color DefaultAccentColor => EditorGUIUtility.isProSkin
            ? new Color(0.361f, 0.706f, 0.42f, 1f)
            : new Color(0.133f, 0.627f, 0.294f, 1f);

        private static Color DefaultTrackOffBorderColor => EditorGUIUtility.isProSkin
            ? new Color(0.32f, 0.32f, 0.34f, 1f)
            : new Color(0.45f, 0.45f, 0.47f, 1f);

        private static Color DefaultHandleColor => EditorGUIUtility.isProSkin
            ? new Color(0.74f, 0.74f, 0.77f, 0.85f)
            : Color.white;

        private static Color DefaultHandleShadowColor => EditorGUIUtility.isProSkin
            ? new Color(0f, 0f, 0f, 0.15f)
            : new Color(0f, 0f, 0f, 0.25f);

        private static float OnFillAlpha => EditorGUIUtility.isProSkin ? 0.30f : 1f;

        // A solid light track would lose its outline in a border of the same colour, so the light skin darkens it.
        private static float OnBorderDarkening => EditorGUIUtility.isProSkin ? 0f : 0.3f;

        private readonly VisualElement _track;
        private readonly VisualElement _handle;

        private Color _accentColor = DefaultAccentColor;
        private Color _trackOffBorderColor = DefaultTrackOffBorderColor;

        private float _handlePosition;
        private IVisualElementScheduledItem _animation;

        public AspidSwitch()
            : this(null) { }

        public AspidSwitch(string label)
            : this(label, new VisualElement()) { }

        private AspidSwitch(string label, VisualElement input)
            : base(label, input)
        {
            this.AddStyleSheetFromResources(StyleSheetPath)
                .AddClass(SwitchClass);
            style.alignItems = Align.Center;

            labelElement.style.flexGrow = 1;
            labelElement.style.minWidth = StyleKeyword.Auto;
            labelElement.style.marginRight = 10;
            labelElement.style.unityTextAlign = TextAnchor.MiddleLeft;

            // BaseField sets flex-basis to zero, which otherwise overrides the input width.
            input.style.flexBasis = TrackWidth;
            input.style.flexGrow = 0;
            input.style.flexShrink = 0;

            _track = new VisualElement()
                .SetFlexShrink(0)
                .SetSize(TrackWidth, TrackHeight)
                .SetBorderWidth(TrackBorderWidth)
                .SetBorderRadius(TrackHeight / 2)
                .SetPickingMode(PickingMode.Ignore);

            _handle = new VisualElement()
                .SetSize(HandleSize)
                .SetPosition(Position.Absolute)
                .SetBorderWidth(1)
                .SetBorderRadius(HandleSize / 2)
                .SetBackgroundColor(DefaultHandleColor)
                .SetBorderColor(DefaultHandleShadowColor)
                .SetPickingMode(PickingMode.Ignore);
            _handle.style.top = HandleInset;

            input.AddChild(_track.AddChild(_handle));

            RegisterCallback<ClickEvent>(_ => value = !value);
            RegisterCallback<KeyDownEvent>(OnKeyDown);
            RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);

            SetValueWithoutNotify(false);
        }

        public override void SetValueWithoutNotify(bool newValue)
        {
            base.SetValueWithoutNotify(newValue);
            // BaseField may seed the value before the track is constructed.
            if (_track == null) return;
            MoveTo(newValue);
        }

        private void OnCustomStyleResolved(CustomStyleResolvedEvent evt)
        {
            var customStyle = evt.customStyle;

            _accentColor = customStyle.TryGetValue(_accentProperty, out var accent) ? accent : DefaultAccentColor;
            _trackOffBorderColor = customStyle.TryGetValue(_trackOffBorderProperty, out var border)
                ? border
                : DefaultTrackOffBorderColor;

            _handle.SetBackgroundColor(customStyle.TryGetValue(_handleProperty, out var handle) ? handle : DefaultHandleColor);
            _handle.SetBorderColor(customStyle.TryGetValue(_handleShadowProperty, out var shadow)
                ? shadow
                : DefaultHandleShadowColor);

            UpdateVisuals();
        }

        private void OnKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode is not (KeyCode.Space or KeyCode.Return or KeyCode.KeypadEnter)) return;
            value = !value;
            evt.StopPropagation();
        }

        private void MoveTo(bool on)
        {
            var target = on ? 1f : 0f;

            if (panel == null)
            {
                _handlePosition = target;
                UpdateVisuals();
                return;
            }

            _animation?.Pause();
            var start = _handlePosition;
            var startTime = Time.realtimeSinceStartup;

            _animation = schedule.Execute(() =>
            {
                var t = Mathf.Clamp01((Time.realtimeSinceStartup - startTime) / AnimationDuration);
                t = 1f - Mathf.Pow(1f - t, 3f);
                _handlePosition = Mathf.Lerp(start, target, t);
                UpdateVisuals();

                if (t >= 1f) _animation?.Pause();
            }).Every(16);
        }

        private void UpdateVisuals()
        {
            const float maxLeft = TrackInnerWidth - HandleSize - HandleInset;
            _handle.style.left = Mathf.Lerp(HandleInset, maxLeft, _handlePosition);

            // The light skin gives an off track a faint fill, so the white handle does not vanish into a white card.
            var onFill = new Color(_accentColor.r, _accentColor.g, _accentColor.b, OnFillAlpha);
            var offFill = EditorGUIUtility.isProSkin
                ? new Color(_accentColor.r, _accentColor.g, _accentColor.b, 0f)
                : new Color(0f, 0f, 0f, LightOffFillAlpha);

            _track.style.backgroundColor = Color.Lerp(offFill, onFill, _handlePosition);

            var onBorder = Color.Lerp(_accentColor, Color.black, OnBorderDarkening);
            _track.SetBorderColor(Color.Lerp(_trackOffBorderColor, onBorder, _handlePosition));
        }
    }
}
