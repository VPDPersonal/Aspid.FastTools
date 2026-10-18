using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors.Internal
{
    // The Theme dropdown: the built-in themes, then the custom themes of the project, then the command that makes one.
    // The popup of an EnumField holds neither a separator nor a command, so the field opens its own menu and shows the
    // name of the current theme.
    internal sealed class AspidThemeField : BaseField<string>
    {
        internal const string CreateThemeItem = "Create theme…";

        private readonly Action _createTheme;
        private readonly TextElement _text;
        private readonly VisualElement _input;

        public AspidThemeField(string label, Action createTheme)
            : this(label, createTheme, new VisualElement()) { }

        private AspidThemeField(string label, Action createTheme, VisualElement visualInput)
            : base(label, visualInput)
        {
            _createTheme = createTheme;
            _input = visualInput;

            this.AddClass(EnumField.ussClassName);
            labelElement.AddClass(EnumField.labelUssClassName);

            _text = new TextElement()
                .AddClass(EnumField.textUssClassName)
                .SetPickingMode(PickingMode.Ignore);

            visualInput
                .AddClass(EnumField.inputUssClassName)
                .AddChild(_text)
                .AddChild(new VisualElement()
                    .AddClass(EnumField.arrowUssClassName)
                    .SetPickingMode(PickingMode.Ignore));

            visualInput.RegisterCallback<PointerDownEvent>(evt =>
            {
                if (evt.button is not 0) return;

                ShowMenu();
                evt.StopPropagation();
            });

            RegisterCallback<NavigationSubmitEvent>(evt =>
            {
                ShowMenu();
                evt.StopPropagation();
            });

            // Another surface can pick a theme, so the field follows the store while it is in a panel.
            RegisterCallback<AttachToPanelEvent>(_ =>
            {
                AspidThemeSettings.Changed -= Refresh;
                AspidThemeSettings.Changed += Refresh;
                Refresh();
            });
            RegisterCallback<DetachFromPanelEvent>(_ => AspidThemeSettings.Changed -= Refresh);

            Refresh();
        }

        public override void SetValueWithoutNotify(string newValue)
        {
            base.SetValueWithoutNotify(newValue);
            _text.text = newValue;
        }

        // Steps through the themes without wrapping; the command is not a theme, so the arrows do not reach it.
        internal void Step(int delta)
        {
            var themes = GetThemes();
            var index = Mathf.Clamp(themes.FindIndex(theme => theme.IsCurrent) + delta, 0, themes.Count - 1);

            themes[index].Pick();
        }

        internal static List<Theme> GetThemes()
        {
            var themes = new List<Theme>();

            foreach (AspidTheme builtIn in Enum.GetValues(typeof(AspidTheme)))
                themes.Add(new Theme(builtIn, custom: null));

            var current = AspidThemeSettings.CustomTheme;
            var found = AspidThemeSettings.FindCustomThemes();

            // A picked sheet without the label, or outside Assets, stays in the list while it is the current theme.
            if (current != null && !found.Contains(current)) found.Add(current);

            foreach (var custom in found)
                themes.Add(new Theme(AspidTheme.Default, custom));

            return themes;
        }

        internal void ShowMenu()
        {
            var window = _input.GetOwnerWindow();
            if (!window) return;

            var menu = new GenericMenu { allowDuplicateNames = true };
            var themes = GetThemes();

            for (var i = 0; i < themes.Count; i++)
            {
                var theme = themes[i];

                // A line parts the custom themes from the built-in ones.
                if (theme.Custom != null && themes[i - 1].Custom == null) menu.AddSeparator(string.Empty);
                menu.AddItem(new GUIContent(theme.Name), theme.IsCurrent, theme.Pick);
            }

            menu.AddSeparator(string.Empty);
            menu.AddItem(new GUIContent(CreateThemeItem), on: false, () => _createTheme?.Invoke());

            // GenericMenu takes GUI coordinates, which a UI Toolkit event does not set up: go through screen space.
            var bound = _input.worldBound;
            menu.DropDown(GUIUtility.ScreenToGUIRect(new Rect(
                window.position.x + bound.xMin,
                window.position.y + bound.yMin,
                bound.width,
                bound.height)));
        }

        private void Refresh()
        {
            var custom = AspidThemeSettings.CustomTheme;
            SetValueWithoutNotify(custom != null ? custom.name : AspidThemeSettings.Theme.ToString());
        }

        internal readonly struct Theme
        {
            public readonly AspidTheme BuiltIn;
            public readonly StyleSheet Custom;

            public Theme(AspidTheme builtIn, StyleSheet custom)
            {
                BuiltIn = builtIn;
                Custom = custom;
            }

            public string Name => Custom != null ? Custom.name : BuiltIn.ToString();

            public bool IsCurrent
            {
                get
                {
                    var current = AspidThemeSettings.CustomTheme;
                    return Custom != null ? current == Custom : current == null && AspidThemeSettings.Theme == BuiltIn;
                }
            }

            public void Pick()
            {
                if (Custom != null) AspidThemeSettings.CustomTheme = Custom;
                else AspidThemeSettings.Theme = BuiltIn;
            }
        }
    }
}
