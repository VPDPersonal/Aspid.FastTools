using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors.Internal
{
    [UxmlElement(nameof(AspidAnimatedTitle), libraryPath = "Aspid/FastTools")]
    public sealed partial class AspidAnimatedTitle : VisualElement
    {
        private const string WordClass = "aspid-fasttools-animated-title__word";

        private const int PaletteCount = 3;

        // Default palette mirrors the gemstone text-dark ramp from Aspid-FastTools-Default-Dark.uss.
        private static readonly Color _defaultColor1 = new(r:85f  / 255f, g: 175f / 255f, b: 100f / 255f, a: 1f); // #55AF64
        private static readonly Color _defaultColor2 = new(r:185f / 255f, g: 135f / 255f, b: 060f / 255f, a: 1f); // #B9873C
        private static readonly Color _defaultColor3 = new(r:185f / 255f, g: 065f / 255f, b: 065f / 255f, a: 1f); // #B94141

        private static readonly CustomStyleProperty<float> _colorStrideProperty =
            new("--aspid-fasttools-prop-animated_title-color_stride");

        private static readonly CustomStyleProperty<float> _colorSpeedProperty =
            new("--aspid-fasttools-prop-animated_title-color_speed");

        private static readonly CustomStyleProperty<float> _waveStrideProperty =
            new("--aspid-fasttools-prop-animated_title-wave_stride");

        private static readonly CustomStyleProperty<float> _waveSpeedProperty =
            new("--aspid-fasttools-prop-animated_title-wave_speed");

        private static readonly CustomStyleProperty<float> _waveAmplitudeProperty =
            new("--aspid-fasttools-prop-animated_title-wave_amplitude");

        private static readonly CustomStyleProperty<Color> _color1Property =
            new("--aspid-fasttools-colors-animated_title-color_1");

        private static readonly CustomStyleProperty<Color> _color2Property =
            new("--aspid-fasttools-colors-animated_title-color_2");

        private static readonly CustomStyleProperty<Color> _color3Property =
            new("--aspid-fasttools-colors-animated_title-color_3");

        private string _text = string.Empty;
        private Label[] _chars = Array.Empty<Label>();

        [UxmlAttribute(name: "text")]
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

        [UxmlAttribute(name: "color-stride")]
        public float ColorStride { get; set; } = 0.12f;

        [UxmlAttribute(name: "color-speed")]
        public float ColorSpeed { get; set; } = 0.4f;

        [UxmlAttribute(name: "wave-stride")]
        public float WaveStride { get; set; } = 0.55f;

        [UxmlAttribute(name: "wave-speed")]
        public float WaveSpeed { get; set; } = 1.6f;

        [UxmlAttribute(name: "wave-amplitude")]
        public float WaveAmplitude { get; set; } = 3f;

        [UxmlAttribute(name: "color-1")]
        public Color Color1 { get; set; } = _defaultColor1;

        [UxmlAttribute(name: "color-2")]
        public Color Color2 { get; set; } = _defaultColor2;

        [UxmlAttribute(name: "color-3")]
        public Color Color3 { get; set; } = _defaultColor3;

        public AspidAnimatedTitle()
        {
            this.AddStyleSheetsFromResource("UI/Components/Aspid-FastTools-AspidAnimatedTitle");

            RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);
            schedule.Execute(UpdateAnimation).Every(33);
        }

        public AspidAnimatedTitle(string text) 
            : this()
        {
            Text = text;
        }

        private void OnCustomStyleResolved(CustomStyleResolvedEvent evt)
        {
            if (evt.customStyle.TryGetValue(_colorStrideProperty, out var colorStride))
                ColorStride = colorStride;
            if (evt.customStyle.TryGetValue(_colorSpeedProperty, out var colorSpeed))
                ColorSpeed = colorSpeed;
            if (evt.customStyle.TryGetValue(_waveStrideProperty, out var waveStride))
                WaveStride = waveStride;
            if (evt.customStyle.TryGetValue(_waveSpeedProperty, out var waveSpeed))
                WaveSpeed = waveSpeed;
            if (evt.customStyle.TryGetValue(_waveAmplitudeProperty, out var waveAmplitude))
                WaveAmplitude = waveAmplitude;

            if (evt.customStyle.TryGetValue(_color1Property, out var color1))
                Color1 = color1;
            if (evt.customStyle.TryGetValue(_color2Property, out var color2))
                Color2 = color2;
            if (evt.customStyle.TryGetValue(_color3Property, out var color3))
                Color3 = color3;
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
                    t: i * ColorStride + time * ColorSpeed, 
                    length: PaletteCount);
                
                var i0 = (int)colorPhase;
                var t = colorPhase - i0;

                var c0 = GetPaletteColor(index: i0);
                var c1 = GetPaletteColor(index: (i0 + 1) % PaletteCount);
                _chars[i].style.color = Color.Lerp(c0, c1, t);

                var yOffset = Mathf.Sin(f: i * WaveStride + time * WaveSpeed) * WaveAmplitude;
                _chars[i].style.translate = new Translate(0f, yOffset);
            }
        }

        private Color GetPaletteColor(int index) => index switch
        {
            0 => Color1,
            1 => Color2,
            _ => Color3,
        };
    }
}
