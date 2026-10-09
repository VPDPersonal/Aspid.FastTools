using System;
using UnityEngine;
using NUnit.Framework;
using System.Collections;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using System.Collections.Generic;
using Aspid.FastTools.UIElements;
using Aspid.FastTools.SerializeReferences.Editors;
using Aspid.FastTools.UIElements.Editors.Internal;
using Aspid.FastTools.UIElements.Editors.Internal.Tests;

namespace Aspid.FastTools.Editors.Tests
{
    /// <summary>
    /// Behavioural coverage for the Settings tab: the keyboard handling of the Build / CI gate row (arrows step the
    /// value without wrapping, Fail and Off ask for confirmation first, Enter does not change the value) and the
    /// contrast of the field borders and the slider track on both palettes.
    /// </summary>
    [TestFixture]
    internal sealed class SettingsViewTests
    {
        [TestCase(GateSeverity.Off, -1, GateSeverity.Off)]
        [TestCase(GateSeverity.Off, +1, GateSeverity.Warn)]
        [TestCase(GateSeverity.Warn, -1, GateSeverity.Off)]
        [TestCase(GateSeverity.Warn, +1, GateSeverity.Fail)]
        [TestCase(GateSeverity.Fail, -1, GateSeverity.Warn)]
        [TestCase(GateSeverity.Fail, +1, GateSeverity.Fail)]
        public void StepEnum_MovesOneValueWithoutWrapping(GateSeverity start, int delta, GateSeverity expected)
        {
            var field = new EnumField(start);

            SettingsView.StepEnum(field, delta, confirm: _ => true);

            Assert.AreEqual(expected, field.value);
        }

        [Test]
        public void StepEnum_AsksOnlyBeforeFailAndOff()
        {
            var asked = new List<Enum>();
            var field = new EnumField(GateSeverity.Off);

            SettingsView.StepEnum(field, -1, Confirm);
            SettingsView.StepEnum(field, +1, Confirm);
            SettingsView.StepEnum(field, +1, Confirm);
            SettingsView.StepEnum(field, +1, Confirm);
            SettingsView.StepEnum(field, -1, Confirm);
            SettingsView.StepEnum(field, -1, Confirm);

            CollectionAssert.AreEqual(new object[] { GateSeverity.Fail, GateSeverity.Off }, asked);
            Assert.AreEqual(GateSeverity.Off, field.value);
            return;

            bool Confirm(Enum next)
            {
                asked.Add(next);
                return true;
            }
        }

        [TestCase(GateSeverity.Warn, +1)]
        [TestCase(GateSeverity.Warn, -1)]
        public void StepEnum_DeclinedConfirmation_KeepsValue(GateSeverity start, int delta)
        {
            var field = new EnumField(start);

            SettingsView.StepEnum(field, delta, confirm: _ => false);

            Assert.AreEqual(start, field.value);
        }

        [TestCase(GateSeverity.Off, true)]
        [TestCase(GateSeverity.Warn, false)]
        [TestCase(GateSeverity.Fail, true)]
        public void NeedsConfirmation_OnlyForFailAndOff(GateSeverity value, bool expected) =>
            Assert.AreEqual(expected, SettingsView.NeedsConfirmation(value));

        [Test]
        public void NeedsConfirmation_IgnoresOtherEnums() =>
            Assert.IsFalse(SettingsView.NeedsConfirmation(DayOfWeek.Monday));

        [UnityTest]
        public IEnumerator Enter_OnBuildGateRow_KeepsValue()
        {
            // The setter would write the shared ProjectSettings asset, so a failing run must put the value back.
            var severity = SerializeReferenceSettings.BuildSeverity;
            var panel = new TestPanel();

            try
            {
                var view = new SettingsView();
                panel.Root.Add(view);
                yield return null;

                var gate = view.Q<EnumField>();
                Assert.IsNotNull(gate, "The Settings tab must contain the Build / CI gate dropdown.");

                var before = gate.value;
                for (var i = 0; i < 20 && !gate.ClassListContains(AspidSettingsUI.NavTargetFocusedClass); i++)
                    SendKey(view, KeyCode.DownArrow);

                Assert.IsTrue(gate.ClassListContains(AspidSettingsUI.NavTargetFocusedClass),
                    "The Down arrow must reach the Build / CI gate row.");

                SendKey(view, KeyCode.Return);
                SendKey(view, KeyCode.KeypadEnter);

                Assert.AreEqual(before, gate.value, "Enter must not change the Build / CI gate.");
            }
            finally
            {
                panel.Root.Clear();
                panel.Dispose();
                SerializeReferenceSettings.BuildSeverity = severity;
            }
        }

        [UnityTest]
        public IEnumerator FieldBordersAndSliderTrack_OnDarkPalette_ReachThreeToOneContrast() =>
            AssertControlBorderContrast(lightPalette: false);

        [UnityTest]
        public IEnumerator FieldBordersAndSliderTrack_OnLightPalette_ReachThreeToOneContrast() =>
            AssertControlBorderContrast(lightPalette: true);

        private static IEnumerator AssertControlBorderContrast(bool lightPalette)
        {
            var panel = new TestPanel();

            try
            {
                // The palette is declared on the element that carries the sheets, so the page can sit under either one.
                var theme = new VisualElement()
                    .AddStyleSheetFromResources(AspidStyles.DefaultStyleSheet)
                    .AddStyleSheetFromResources(AspidStyles.LightStyleSheet);
                theme.EnableInClassList(AspidStyles.PaletteLightClass, lightPalette);

                var view = new SettingsView();
                theme.Add(view);
                panel.Root.Add(theme);
                yield return null;
                yield return null;

                var border = Token(theme, "--aspid-colors-control-border");
                var card = Blend(Token(theme, "--aspid-colors-surface-card"), Token(theme, "--aspid-colors-surface-canvas"));
                var fieldFill = Blend(Token(theme, "--aspid-colors-surface-card"), card);

                var slider = view.Q<SliderInt>();
                var inputs = new[]
                {
                    view.Q(className: "unity-enum-field__input"),
                    slider.Q(className: "unity-base-text-field__input"),
                };

                foreach (var input in inputs)
                {
                    Assert.AreEqual(border, input.resolvedStyle.borderTopColor, "A field border must use the control border token.");
                    Assert.GreaterOrEqual(Contrast(border, card), 3f, "The border must stand out from the card.");
                    Assert.GreaterOrEqual(Contrast(border, fieldFill), 3f, "The border must stand out from the field fill.");
                }

                var track = slider.Q(className: "unity-base-slider__tracker");
                Assert.AreEqual(border, track.resolvedStyle.backgroundColor, "The slider track must use the control border token.");
            }
            finally
            {
                panel.Root.Clear();
                panel.Dispose();
            }
        }

        private static Color Token(VisualElement element, string name)
        {
            Assert.IsTrue(element.customStyle.TryGetValue(new CustomStyleProperty<Color>(name), out var color),
                $"The palette must declare {name}.");
            return color;
        }

        private static Color Blend(Color top, Color bottom) =>
            new(
                r: top.r * top.a + bottom.r * (1f - top.a),
                g: top.g * top.a + bottom.g * (1f - top.a),
                b: top.b * top.a + bottom.b * (1f - top.a));

        // WCAG relative luminance and contrast ratio of two opaque colors.
        private static float Contrast(Color first, Color second)
        {
            var a = Luminance(first);
            var b = Luminance(second);
            return (Mathf.Max(a, b) + 0.05f) / (Mathf.Min(a, b) + 0.05f);
        }

        private static float Luminance(Color color) =>
            0.2126f * Linear(color.r) + 0.7152f * Linear(color.g) + 0.0722f * Linear(color.b);

        private static float Linear(float channel) =>
            channel <= 0.03928f ? channel / 12.92f : Mathf.Pow((channel + 0.055f) / 1.055f, 2.4f);

        private static void SendKey(VisualElement host, KeyCode key)
        {
            using var evt = KeyDownEvent.GetPooled('\0', key, EventModifiers.None);
            evt.target = host;
            host.SendEvent(evt);
        }
    }
}
