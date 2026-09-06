using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors.Internal
{
    // An iOS-style on/off switch: a rounded outlined track whose handle slides between off and on, with the fill
    // alpha and border color interpolating along the handle's ease-out-cubic path. A near drop-in replacement for
    // Toggle — it derives from BaseField<bool>, so it carries a left caption, raises change events and binds like
    // any other field. Built and animated in code, since a position-synced color lerp cannot be expressed in USS.
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

        // UIToolkit absolute offsets start at the padding edge (inside the border), so the handle's travel is measured
        // against the inner box — otherwise the border pushes the on-position handle past the right edge.
        private const float TrackInnerWidth = TrackWidth - 2f * TrackBorderWidth;     // 41
        private const float TrackInnerHeight = TrackHeight - 2f * TrackBorderWidth;   // 21
        private const float HandleInset = (TrackInnerHeight - HandleSize) / 2f;       // 1.5

        // The switch also renders on Unity's NATIVE light-theme pages (Project Settings / Preferences), where the
        // dark-skin neutrals would be all but invisible — so they flip with the editor skin. Read once per domain:
        // a mid-session skin switch catches up on the next reload.
        private static readonly Color AccentColor = new(0.333f, 0.686f, 0.392f, 1f);

        private static readonly Color TrackOffBorderColor = EditorGUIUtility.isProSkin
            ? new Color(0.32f, 0.32f, 0.34f, 1f)
            : new Color(0.45f, 0.45f, 0.47f, 1f);

        // Semi-transparent so the handle picks up a touch of the track tint behind it.
        private static readonly Color HandleColor = EditorGUIUtility.isProSkin
            ? new Color(0.74f, 0.74f, 0.77f, 0.85f)
            : new Color(0.35f, 0.35f, 0.38f, 0.9f);

        private static readonly Color HandleShadowColor = new(0f, 0f, 0f, 0.15f);

        private readonly VisualElement _track;
        private readonly VisualElement _handle;

        // 0 = off (handle left), 1 = on (handle right). The track color lerps along the same 0..1 axis.
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

            // The caption fills the row so the switch pins to the right edge (the iOS settings-row layout); drop the
            // inherited fixed label column so it grows naturally instead.
            labelElement.style.flexGrow = 1;
            labelElement.style.minWidth = StyleKeyword.Auto;
            labelElement.style.marginRight = 10;
            labelElement.style.unityTextAlign = TextAnchor.MiddleLeft;

            // BaseField's USS pins `.unity-base-field__input` to flex-basis:0, which overrides `width` and collapses
            // the input to 0 — override flex-basis to the track's width and freeze grow/shrink.
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
            _handle.style.top = HandleInset; // vertically centered in the inner box; left is driven by the animation

            input.AddChild(_track.AddChild(_handle));

            // The whole field is clickable (caption included, like Toggle); the track/handle ignore picking so the
            // click resolves to the field itself.
            RegisterCallback<ClickEvent>(_ => value = !value);
            RegisterCallback<KeyDownEvent>(OnKeyDown);

            SetValueWithoutNotify(false);
        }

        public sealed override void SetValueWithoutNotify(bool newValue)
        {
            base.SetValueWithoutNotify(newValue);
            // BaseField may seed the value before our visuals exist; skip until the track is built.
            if (_track == null) return;
            MoveTo(newValue);
        }

        // Space / Enter flips the switch while it holds keyboard focus, matching Toggle's keyboard behavior.
        private void OnKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode is not (KeyCode.Space or KeyCode.Return or KeyCode.KeypadEnter)) return;
            value = !value;
            evt.StopPropagation();
        }

        // Before the field is attached to a panel there is no scheduler, so the handle snaps instead of animating —
        // no animation runs on window open.
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
                t = 1f - Mathf.Pow(1f - t, 3f); // ease-out cubic for a smooth deceleration
                _handlePosition = Mathf.Lerp(start, target, t);
                UpdateVisuals();

                if (t >= 1f) _animation?.Pause();
            }).Every(16); // ~60fps
        }

        private void UpdateVisuals()
        {
            const float maxLeft = TrackInnerWidth - HandleSize - HandleInset;
            _handle.style.left = Mathf.Lerp(HandleInset, maxLeft, _handlePosition);

            // The fill alpha and the border color lerp with the handle position so the on-tint sweeps in as it slides.
            _track.style.backgroundColor =
                new Color(AccentColor.r, AccentColor.g, AccentColor.b, Mathf.Lerp(0f, OnFillAlpha, _handlePosition));
            _track.SetBorderColor(Color.Lerp(TrackOffBorderColor, AccentColor, _handlePosition));
        }
    }
}
