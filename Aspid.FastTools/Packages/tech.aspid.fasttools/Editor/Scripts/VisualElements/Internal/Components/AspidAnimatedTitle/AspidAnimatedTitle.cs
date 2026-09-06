using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors.Internal
{
    // A VisualElement that displays a title with per-character color cycling and a vertical wave animation. All
    // animation parameters and palette colors can be driven by USS custom properties or set explicitly in code.
    [UxmlElement(nameof(AspidAnimatedTitle), libraryPath = "Aspid/FastTools")]
    internal sealed partial class AspidAnimatedTitle : VisualElement
    {
        private const string StyleSheetPath = "UI/Components/Aspid-FastTools-AspidAnimatedTitle";
        private const int PaletteCount = 3;
        private const string WordClass = "aspid-fasttools-animated-title__word";

        private readonly AspidAnimatedTitleColorsStyle _colors;
        private readonly AspidAnimatedTitleColorAnimationStyle _colorAnimation;
        private readonly AspidAnimatedTitleWaveAnimationStyle _waveAnimation;

        private string _text = string.Empty;
        private Label[] _chars = Array.Empty<Label>();
        private IVisualElementScheduledItem _animation;

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
            this.AddStyleSheetsFromResource(StyleSheetPath);

            _colors = new AspidAnimatedTitleColorsStyle(
                this, preset.Color1, preset.Color2, preset.Color3, onChanged: null);

            _colorAnimation = new AspidAnimatedTitleColorAnimationStyle(
                this, preset.ColorStride, preset.ColorSpeed, onChanged: null);

            _waveAnimation = new AspidAnimatedTitleWaveAnimationStyle(
                this, preset.WaveStride, preset.WaveSpeed, preset.WaveAmplitude, onChanged: null);

            _animation = schedule.Execute(UpdateAnimation).Every(33);

            RegisterCallback<AttachToPanelEvent>(_ => _animation.Resume());
            RegisterCallback<DetachFromPanelEvent>(_ => _animation.Pause());

            Text = text;
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
        }

        private void UpdateAnimation()
        {
            if (_chars.Length is 0) return;
            var time = (float)EditorApplication.timeSinceStartup;

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
