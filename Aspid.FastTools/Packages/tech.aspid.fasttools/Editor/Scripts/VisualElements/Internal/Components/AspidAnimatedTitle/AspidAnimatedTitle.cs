using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors.Internal
{
    [UxmlElement(nameof(AspidAnimatedTitle), libraryPath = "Aspid/FastTools", visibility = LibraryVisibility.Hidden)]
    internal sealed partial class AspidAnimatedTitle : VisualElement
    {
        private const long TickMs = 33;

        // The longest step one tick may advance the title: a stalled Editor resumes the wave instead of jumping.
        private const float MaxTickStep = TickMs * 2 / 1000f;
        private const int PaletteCount = 3;
        private const string WordClass = "aspid-fasttools-animated-title__word";
        private const string StyleSheetPath = "UI/Components/Aspid-FastTools-AspidAnimatedTitle";

        private readonly AspidAnimatedTitleColorsStyle _colors;
        private readonly AspidAnimatedTitleColorAnimationStyle _colorAnimation;
        private readonly AspidAnimatedTitleWaveAnimationStyle _waveAnimation;

        private string _text = string.Empty;
        private Label[] _chars = Array.Empty<Label>();

        // The title's own clock: it advances only while the title is watched, so a paused wave resumes where it stopped.
        private float _time;
        private double _lastTick = EditorApplication.timeSinceStartup;
        private readonly IVisualElementScheduledItem _animation;

        [UxmlAttribute]
        public string Text
        {
            get => _text;
            set
            {
                value ??= string.Empty;
                if (_text == value) return;

                _text = value;
                Rebuild();
            }
        }

        [UxmlAttribute]
        public float ColorStride
        {
            get => _colorAnimation.Stride;
            set => _colorAnimation.SetStride(value);
        }

        [UxmlAttribute]
        public float ColorSpeed
        {
            get => _colorAnimation.Speed;
            set => _colorAnimation.SetSpeed(value);
        }

        [UxmlAttribute]
        public float WaveStride
        {
            get => _waveAnimation.Stride;
            set => _waveAnimation.SetStride(value);
        }

        [UxmlAttribute]
        public float WaveSpeed
        {
            get => _waveAnimation.Speed;
            set => _waveAnimation.SetSpeed(value);
        }

        [UxmlAttribute]
        public float WaveAmplitude
        {
            get => _waveAnimation.Amplitude;
            set => _waveAnimation.SetAmplitude(value);
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

        public AspidAnimatedTitle()
            : this(string.Empty, AspidAnimatedTitlePreset.Default) { }

        public AspidAnimatedTitle(string text)
            : this(text, AspidAnimatedTitlePreset.Default) { }

        public AspidAnimatedTitle(string text, AspidAnimatedTitlePreset preset)
        {
            this.AddStyleSheetFromResources(StyleSheetPath);

            _colors = new AspidAnimatedTitleColorsStyle(
                this, preset.Color1, preset.Color2, preset.Color3, onChanged: OnColorsChanged);

            _colorAnimation = new AspidAnimatedTitleColorAnimationStyle(
                this, preset.ColorStride, preset.ColorSpeed, onChanged: null);

            _waveAnimation = new AspidAnimatedTitleWaveAnimationStyle(
                this, preset.WaveStride, preset.WaveSpeed, preset.WaveAmplitude, onChanged: null);

            _animation = schedule.Execute(UpdateAnimation).Every(TickMs);
            _animation.Pause();

            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);

            Text = text;
        }

        internal bool IsAnimating => _animation.isActive;

        private void OnAttachToPanel(AttachToPanelEvent _)
        {
            AspidAnimatedDotsBackgroundSettings.Changed += ApplyAnimationSetting;
            ApplyAnimationSetting();
        }

        private void OnDetachFromPanel(DetachFromPanelEvent _)
        {
            AspidAnimatedDotsBackgroundSettings.Changed -= ApplyAnimationSetting;
            _animation.Pause();
        }

        // With the "Animated background" setting off nothing ticks and the title rests: the letters sit on one line in
        // the middle color of the palette, the accent of the title.
        private void ApplyAnimationSetting()
        {
            if (AspidAnimatedDotsBackgroundSettings.Enabled)
            {
                _animation.Resume();
                return;
            }

            _animation.Pause();
            ApplyRest();
        }

        // A theme change while the title rests recolors it; while it animates the next tick does. The color store calls
        // this from its constructor, before the timer exists, and before it saves a new value, so the recolor waits a
        // frame and reads the saved colors.
        private void OnColorsChanged()
        {
            if (_animation is null || _animation.isActive) return;

            schedule.Execute(() =>
            {
                if (!_animation.isActive) ApplyRest();
            });
        }

        private void ApplyRest()
        {
            foreach (var label in _chars)
            {
                label.style.color = _colors.Color2;
                label.style.translate = new Translate(0f, 0f);
            }
        }

        private void Rebuild()
        {
            Clear();

            if (string.IsNullOrWhiteSpace(_text))
            {
                _chars = Array.Empty<Label>();
                return;
            }

            var charsList = new List<Label>(_text.Length);

            foreach (var word in _text.Split(' '))
            {
                if (string.IsNullOrEmpty(word)) continue;

                var wordContainer = new VisualElement()
                    .AddClass(WordClass);

                foreach (var ch in word)
                {
                    var label = new Label(text: ch.ToString());

                    wordContainer.AddChild(label);
                    charsList.Add(label);
                }

                this.AddChild(wordContainer);
            }

            _chars = charsList.ToArray();
            if (!_animation.isActive) ApplyRest();
        }

        internal void UpdateAnimation()
        {
            var now = EditorApplication.timeSinceStartup;
            var elapsed = (float)(now - _lastTick);
            _lastTick = now;

            if (_chars.Length is 0 || !EditorWindowWatch.IsWatched(this)) return;

            _time += Mathf.Min(elapsed, MaxTickStep);
            var time = _time;

            for (var i = 0; i < _chars.Length; i++)
            {
                var colorPhase = Mathf.Repeat(
                    t: i * _colorAnimation.Stride + time * _colorAnimation.Speed,
                    length: PaletteCount);

                var i0 = (int)colorPhase;
                var t = colorPhase - i0;

                var c0 = _colors[i0];
                var c1 = _colors[(i0 + 1) % PaletteCount];
                _chars[i].style.color = Color.Lerp(c0, c1, t);

                var yOffset = Mathf.Sin(f: i * _waveAnimation.Stride + time * _waveAnimation.Speed) * _waveAnimation.Amplitude;
                _chars[i].style.translate = new Translate(0f, yOffset);
            }
        }
    }
}
