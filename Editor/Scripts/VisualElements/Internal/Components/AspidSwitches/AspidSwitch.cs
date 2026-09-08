using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors.Internal
{
    [UxmlElement(libraryPath = "Aspid/FastTools")]
    internal sealed partial class AspidSwitch : BaseField<bool>
    {
        private const string SwitchClass = "aspid-fasttools-switch";
        private const float TrackWidth = 44f;
        private const float TrackHeight = 24f;
        private const float HandleSize = 18f;
        private const float TrackBorderWidth = 1.5f;
        private const float OnFillAlpha = 0.30f;
        private const float AnimationDuration = 0.15f;

        // UI Toolkit absolute offsets start inside the border, so handle travel uses the inner box.
        private const float TrackInnerWidth = TrackWidth - 2f * TrackBorderWidth;
        private const float TrackInnerHeight = TrackHeight - 2f * TrackBorderWidth;
        private const float HandleInset = (TrackInnerHeight - HandleSize) / 2f;

        private static readonly Color AccentColor = new(0.333f, 0.686f, 0.392f, 1f);

        private static readonly Color TrackOffBorderColor = EditorGUIUtility.isProSkin
            ? new Color(0.32f, 0.32f, 0.34f, 1f)
            : new Color(0.45f, 0.45f, 0.47f, 1f);

        private static readonly Color HandleColor = EditorGUIUtility.isProSkin
            ? new Color(0.74f, 0.74f, 0.77f, 0.85f)
            : new Color(0.35f, 0.35f, 0.38f, 0.9f);

        private static readonly Color HandleShadowColor = new(0f, 0f, 0f, 0.15f);

        private readonly VisualElement _track;
        private readonly VisualElement _handle;

        private float _handlePosition;
        private IVisualElementScheduledItem _animation;

        public AspidSwitch()
            : this(null) { }

        public AspidSwitch(string label)
            : this(label, new VisualElement()) { }

        private AspidSwitch(string label, VisualElement input)
            : base(label, input)
        {
            this.AddClass(SwitchClass);
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
                .SetBackgroundColor(HandleColor)
                .SetBorderColor(HandleShadowColor)
                .SetPickingMode(PickingMode.Ignore);
            _handle.style.top = HandleInset;

            input.AddChild(_track.AddChild(_handle));

            RegisterCallback<ClickEvent>(_ => value = !value);
            RegisterCallback<KeyDownEvent>(OnKeyDown);

            SetValueWithoutNotify(false);
        }

        public sealed override void SetValueWithoutNotify(bool newValue)
        {
            base.SetValueWithoutNotify(newValue);
            // BaseField may seed the value before the track is constructed.
            if (_track == null) return;
            MoveTo(newValue);
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

            _track.style.backgroundColor =
                new Color(AccentColor.r, AccentColor.g, AccentColor.b, Mathf.Lerp(0f, OnFillAlpha, _handlePosition));
            _track.SetBorderColor(Color.Lerp(TrackOffBorderColor, AccentColor, _handlePosition));
        }
    }
}
