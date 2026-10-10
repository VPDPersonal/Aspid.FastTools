using System;
using System.Linq;
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
    /// value without wrapping, Fail and Off ask for confirmation first, Enter does not change the value), the
    /// contrast of the field borders and the slider track, and the keyboard focus border, on both palettes.
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

                var gate = FocusBuildGate(view);
                var before = gate.value;

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
        public IEnumerator Arrows_OnBuildGateRow_StepToWarnWithoutDialog()
        {
            var severity = SerializeReferenceSettings.BuildSeverity;
            var panel = new TestPanel();

            try
            {
                // Warn is the only value that both arrows reach without a confirmation dialog.
                SerializeReferenceSettings.BuildSeverity = GateSeverity.Fail;

                var view = new SettingsView();
                panel.Root.Add(view);
                yield return null;

                var gate = FocusBuildGate(view);

                SendKey(view, KeyCode.LeftArrow);
                Assert.AreEqual(GateSeverity.Warn, gate.value, "Left must step Fail down to Warn.");

                gate.value = GateSeverity.Off;
                SendKey(view, KeyCode.RightArrow);
                Assert.AreEqual(GateSeverity.Warn, gate.value, "Right must step Off up to Warn.");
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

        [UnityTest]
        public IEnumerator FocusedRows_GetStrongBorderWithoutShiftingLayout()
        {
            var folders = SerializeReferenceSettings.ExcludedFolders;
            var panel = new TestPanel();

            try
            {
                // A folder entry exists only for a listed folder; the setting is put back below.
                SerializeReferenceSettings.ExcludedFolders = new[] { "Assets/SettingsViewTests" };

                // Components that carry their own sheets keep the dark palette outside an Aspid window, so the test stays on it.
                var theme = CreateTheme(lightPalette: false);
                var view = new SettingsView();
                theme.Add(view);
                panel.Root.Add(theme);
                yield return null;
                yield return null;

                var success = Token(theme, "--aspid-colors-status-success-text-light");
                var info = Token(theme, "--aspid-colors-status-info-text-light");
                var error = Token(theme, "--aspid-colors-status-error-text-light");

                var gate = view.Q<EnumField>();
                var header = view.Q(className: "aspid-fasttools-excluded-folders__header");
                var entry = view.Q(className: "aspid-fasttools-excluded-folders__entry");
                var footer = view.Q(className: AspidSettingsUI.FooterClass);
                var shared = footer?.Q<Button>(className: AspidSettingsUI.SharedScopeClass);
                var user = footer?.Q<Button>(className: AspidSettingsUI.UserScopeClass);
                var danger = view.Q<Button>(className: AspidSettingsUI.ActionDangerClass);

                // The row backplate is the child of the focused field; the other targets carry their own border.
                var cases = new[]
                {
                    (Name: "row", Focus: (VisualElement)gate, Bordered: gate?.Q(className: AspidSettingsUI.RowBackplateClass), Color: success),
                    (Name: "Excluded folders header", Focus: header, Bordered: header, Color: success),
                    (Name: "Excluded folders entry", Focus: entry, Bordered: entry, Color: success),
                    (Name: "Reset Shared button", Focus: (VisualElement)shared, Bordered: shared, Color: success),
                    (Name: "Reset Per-user button", Focus: (VisualElement)user, Bordered: user, Color: info),
                    (Name: "danger button", Focus: (VisualElement)danger, Bordered: danger, Color: error),
                };

                foreach (var item in cases)
                {
                    Assert.IsNotNull(item.Focus, $"The Settings tab must contain the {item.Name}.");
                    Assert.IsNotNull(item.Bordered, $"The {item.Name} must have an element that carries the focus border.");
                }

                var before = cases.Select(item => item.Bordered.layout).ToArray();

                // The buttons ease their border color, and the test panel does not advance time-based transitions.
                var noTransition = new StyleList<TimeValue>(new List<TimeValue> { new(0f) });

                foreach (var item in cases)
                {
                    item.Focus.style.transitionDuration = noTransition;
                    item.Focus.AddToClassList(AspidSettingsUI.NavTargetFocusedClass);
                }

                yield return null;
                yield return null;

                for (var i = 0; i < cases.Length; i++)
                {
                    var item = cases[i];
                    var style = item.Bordered.resolvedStyle;

                    // The panel snaps the 1px width to its pixel grid, so only a positive width is checked.
                    Assert.Greater(style.borderTopWidth, 0f, $"The focused {item.Name} must have a border.");
                    Assert.AreEqual(item.Color, style.borderTopColor, $"The focused {item.Name} must use the strong status color.");
                    Assert.AreEqual(before[i], item.Bordered.layout, $"The focus border must not shift the {item.Name}.");
                }
            }
            finally
            {
                panel.Root.Clear();
                panel.Dispose();
                SerializeReferenceSettings.ExcludedFolders = folders;
            }
        }

        [UnityTest]
        public IEnumerator FocusBorderColors_OnDarkPalette_ReachThreeToOneContrast() =>
            AssertFocusBorderContrast(lightPalette: false);

        [UnityTest]
        public IEnumerator FocusBorderColors_OnLightPalette_ReachThreeToOneContrast() =>
            AssertFocusBorderContrast(lightPalette: true);

        private static IEnumerator AssertFocusBorderContrast(bool lightPalette)
        {
            var panel = new TestPanel();

            try
            {
                var theme = CreateTheme(lightPalette);
                panel.Root.Add(theme);
                yield return null;
                yield return null;

                var card = Blend(Token(theme, "--aspid-colors-surface-card"), Token(theme, "--aspid-colors-surface-canvas"));
                var focusFill = Token(theme, "--aspid-colors-bg-dark");

                foreach (var name in new[]
                {
                    "--aspid-colors-status-success-text-light",
                    "--aspid-colors-status-error-text-light",
                    "--aspid-colors-status-info-text-light",
                })
                {
                    var color = Token(theme, name);

                    Assert.GreaterOrEqual(Contrast(color, card), 3f, $"{name} must stand out from the card.");
                    Assert.GreaterOrEqual(Contrast(color, focusFill), 3f, $"{name} must stand out from the focus fill.");
                }
            }
            finally
            {
                panel.Root.Clear();
                panel.Dispose();
            }
        }

        private static IEnumerator AssertControlBorderContrast(bool lightPalette)
        {
            var panel = new TestPanel();

            try
            {
                var theme = CreateTheme(lightPalette);
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

        // The palette is declared on the element that carries the sheets, so the page can sit under either one.
        private static VisualElement CreateTheme(bool lightPalette)
        {
            var theme = new VisualElement()
                .AddStyleSheetFromResources(AspidStyles.DefaultStyleSheet)
                .AddStyleSheetFromResources(AspidStyles.LightStyleSheet);

            theme.EnableInClassList(AspidStyles.PaletteLightClass, lightPalette);
            return theme;
        }

        private static EnumField FocusBuildGate(SettingsView view)
        {
            var gate = view.Q<EnumField>();
            Assert.IsNotNull(gate, "The Settings tab must contain the Build / CI gate dropdown.");

            for (var i = 0; i < 20 && !gate.ClassListContains(AspidSettingsUI.NavTargetFocusedClass); i++)
                SendKey(view, KeyCode.DownArrow);

            Assert.IsTrue(gate.ClassListContains(AspidSettingsUI.NavTargetFocusedClass),
                "The Down arrow must reach the Build / CI gate row.");

            return gate;
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
