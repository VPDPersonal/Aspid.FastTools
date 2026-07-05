using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Aspid.FastTools.UIElements;
using Aspid.FastTools.UIElements.Editors;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.VisualElements.Editors
{
    // A custom inspector built entirely from the fluent VisualElement extension API — every card, label and badge
    // below is composed in code, never in UXML/USS. Each STEP card is one lesson of TUTORIAL.md; the Card() helper
    // at the bottom is itself an example of the same chaining. Select the VisualElements Tutorial GameObject.
    [CustomEditor(typeof(VisualElementsTutorial))]
    internal sealed class VisualElementsTutorialEditor : Editor
    {
        private static readonly Color _cardBackground = new(0.16f, 0.17f, 0.19f);
        private static readonly Color _cardBorder = new(0.26f, 0.28f, 0.31f);
        private static readonly Color _titleColor = new(0.93f, 0.93f, 0.94f);
        private static readonly Color _subtleColor = new(0.58f, 0.61f, 0.66f);
        private static readonly Color _accent = new(0.42f, 0.69f, 1.00f);
        private static readonly Color _warning = new(1.00f, 0.76f, 0.30f);

        public override VisualElement CreateInspectorGUI() =>
            new VisualElement()
                .AddChild(Step1FluentStyle())
                .AddChild(Step2TextAndPresets())
                .AddChild(Step3Layout())
                .AddChild(Step4Reactive())
                .AddChild(Step5Elements());

        // STEP 1 — Fluent style basics. Every Set* returns the element, so the whole style block is one
        // uninterrupted chain — no local variable, no `element.style.paddingTop = …` noise.
        private VisualElement Step1FluentStyle()
        {
            var swatch = new VisualElement()
                .SetHeight(44)
                .SetBackgroundColor(_accent)
                .SetBorderColor(_cardBorder)
                .SetBorderWidth(1)
                .SetBorderRadius(8);

            return Card(1, "Fluent style basics",
                ".SetHeight .SetBackgroundColor .SetBorderColor .SetBorderWidth .SetBorderRadius",
                swatch);
        }

        // STEP 2 — Text & font presets. The Add*/Set*UnityFontStyleAndWeight presets read better than passing a
        // raw FontStyle enum and compose with the other text setters.
        private VisualElement Step2TextAndPresets()
        {
            var normal = new Label("SetNormalUnityFontStyleAndWeight")
                .SetColor(_titleColor)
                .SetNormalUnityFontStyleAndWeight();

            var bold = new Label("AddBoldUnityFontStyleAndWeight")
                .SetColor(_titleColor)
                .AddBoldUnityFontStyleAndWeight();

            var italic = new Label("AddItalicUnityFontStyleAndWeight")
                .SetColor(_subtleColor)
                .AddItalicUnityFontStyleAndWeight();

            var spaced = new Label("LETTER SPACING")
                .SetColor(_subtleColor)
                .SetFontSize(11)
                .SetLetterSpacing(4);

            var body = new VisualElement()
                .AddChild(normal)
                .AddChild(bold)
                .AddChild(italic)
                .AddChild(spaced);

            return Card(2, "Text & font presets",
                "presets: SetNormal/AddBold/AddItalicUnityFontStyleAndWeight · SetLetterSpacing",
                body);
        }

        // STEP 3 — Layout & composition. A flex row plus AddChild is all it takes to build a header that pushes
        // the name to the left and the tag to the right.
        private VisualElement Step3Layout()
        {
            var tutorial = (VisualElementsTutorial)target;

            var name = new Label(tutorial.AbilityName)
                .SetColor(_titleColor)
                .SetFontSize(14)
                .AddBoldUnityFontStyleAndWeight();

            var tag = new Label("ABILITY")
                .SetColor(_subtleColor)
                .SetFontSize(10)
                .SetLetterSpacing(2);

            var row = new VisualElement()
                .SetFlexDirection(FlexDirection.Row)
                .SetAlignItems(Align.Center)
                .SetJustifyContent(Justify.SpaceBetween)
                .AddChild(name)
                .AddChild(tag);

            return Card(3, "Layout & composition",
                ".SetFlexDirection(Row) .SetAlignItems .SetJustifyContent + .AddChild",
                row);
        }

        // STEP 4 — Reactive UI. PropertyField.AddValueChanged re-runs the badge logic on every edit — the same
        // wiring the AbilityConfig demo inspector uses.
        private VisualElement Step4Reactive()
        {
            var tutorial = (VisualElementsTutorial)target;

            var badge = new Label()
                .SetUnityTextAlign(TextAnchor.MiddleCenter)
                .SetAlignSelf(Align.FlexStart)
                .SetFontSize(11)
                .SetMarginTop(8)
                .SetPadding(top: 3, bottom: 3, left: 10, right: 10)
                .SetBorderRadius(10)
                .SetBorderWidth(1)
                .AddBoldUnityFontStyleAndWeight();

            var manaField = new PropertyField(serializedObject.FindProperty("_manaCost"))
                .AddValueChanged(_ => Refresh());

            var body = new VisualElement()
                .AddChild(manaField)
                .AddChild(badge);

            Refresh();
            return Card(4, "Reactive UI",
                "PropertyField(...).AddValueChanged(_ => …) drives the badge below",
                body);

            void Refresh()
            {
                var isFree = tutorial.ManaCost is 0;
                var color = isFree ? _warning : _accent;
                badge.SetText(isFree ? "FREE" : $"{tutorial.ManaCost} MP")
                    .SetColor(color)
                    .SetBorderColor(color);
            }
        }

        // STEP 5 — Element extensions breadth. The same fluent style extends every built-in element: a ProgressBar
        // fed a live value, a HelpBox toggled by SetDisplay, and a Button wired with AddClicked.
        private VisualElement Step5Elements()
        {
            var tutorial = (VisualElementsTutorial)target;

            var bar = new ProgressBar()
                .SetLowValue(0f)
                .SetHighValue(1f);

            var hint = new HelpBox()
                .SetMessageType(HelpBoxMessageType.Info)
                .SetMarginTop(8);

            var chargeField = new PropertyField(serializedObject.FindProperty("_charge"))
                .AddValueChanged(_ => RefreshBar());

            var button = new Button()
                .SetText("Log charge")
                .SetMarginTop(8)
                .AddClicked(() => Debug.Log($"Charge is {tutorial.Charge:P0}"));

            var body = new VisualElement()
                .AddChild(chargeField)
                .AddChild(bar)
                .AddChild(hint)
                .AddChild(button);

            RefreshBar();
            return Card(5, "Element extensions breadth",
                "ProgressBar .SetLowValue/.SetHighValue/.SetValue/.SetTitle · HelpBox · Button .AddClicked",
                body);

            void RefreshBar()
            {
                var charge = tutorial.Charge;
                bar.SetValue(charge).SetTitle($"{charge:P0} charged");
                hint.SetText(charge >= 1f ? "Fully charged." : "Raise Charge to fill the bar.")
                    .SetDisplay(charge >= 1f ? DisplayStyle.Flex : DisplayStyle.None);
            }
        }

        // Shared card chrome — itself built with the fluent API this sample teaches.
        private VisualElement Card(int step, string title, string api, VisualElement content)
        {
            var heading = new Label($"STEP {step} — {title}")
                .SetColor(_titleColor)
                .SetFontSize(13)
                .AddBoldUnityFontStyleAndWeight();

            var subtitle = new Label(api)
                .SetColor(_subtleColor)
                .SetFontSize(10)
                .SetMarginTop(2);

            return new VisualElement()
                .SetBackgroundColor(_cardBackground)
                .SetBorderColor(_cardBorder)
                .SetBorderWidth(1)
                .SetBorderRadius(10)
                .SetMarginTop(8)
                .SetPadding(top: 10, bottom: 12, left: 12, right: 12)
                .AddChild(heading)
                .AddChild(subtitle)
                .AddChild(content.SetMarginTop(8));
        }
    }
}
