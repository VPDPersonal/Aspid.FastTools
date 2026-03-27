#nullable enable
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Aspid.FastTools.Editors
{
    internal sealed class StringIdRenameDialog : EditorWindow
    {
        private Action<RenameChoice>? _onChoice;

        public enum RenameChoice { Cancel, Rename, RenameEverywhere }

        public static void Show(string oldId, string newId, bool hasIdField, Action<RenameChoice> onChoice)
        {
            var window = CreateInstance<StringIdRenameDialog>();
            window.titleContent = new GUIContent("Rename ID");
            window._onChoice = onChoice;
            window.BuildUI(oldId, newId, hasIdField);
            window.ShowModal();
        }

        private void BuildUI(string oldId, string newId, bool hasIdField)
        {
            rootVisualElement
                .SetPadding(16)
                .SetMinSize(width: 400, height: 180)
                .SetFlexDirection(FlexDirection.Column);

            var title = new Label($"Rename '{oldId}' → '{newId}'?")
                .SetFontSize(14)
                .SetUnityFontStyleAndWeight(FontStyle.Bold)
                .SetMargin(bottom: 12);

            rootVisualElement.AddChild(title);

            if (hasIdField)
            {
                var warning = new Label(
                    "'Rename Everywhere' will replace all references in ScriptableObjects, Prefabs, and Scenes.\n\n" +
                    "This operation can be time-consuming depending on project size.")
                    .SetWhiteSpace(WhiteSpace.Normal)
                    .SetMargin(bottom: 16)
                    .SetColor(new Color(0.8f, 0.6f, 0.3f));

                rootVisualElement.AddChild(warning);
            }

            var spacer = new VisualElement().SetFlexGrow(1);
            rootVisualElement.AddChild(spacer);

            var buttonRow = new VisualElement()
                .SetFlexDirection(FlexDirection.Row)
                .SetJustifyContent(Justify.FlexEnd);

            var cancelBtn = new Button(() => Choose(RenameChoice.Cancel)) { text = "Cancel" }
                .SetMinSize(width: 80);

            var renameBtn = new Button(() => Choose(RenameChoice.Rename)) { text = "Rename" }
                .SetMinSize(width: 80)
                .SetMargin(left: 8);

            buttonRow.AddChild(cancelBtn).AddChild(renameBtn);

            if (hasIdField)
            {
                var renameEverywhereBtn = new Button(() => Choose(RenameChoice.RenameEverywhere))
                    { text = "Rename Everywhere" }
                    .SetMinSize(width: 140)
                    .SetMargin(left: 8)
                    .SetBackgroundColor(new Color(0.3f, 0.5f, 0.8f));

                buttonRow.AddChild(renameEverywhereBtn);
            }

            rootVisualElement.AddChild(buttonRow);
        }

        private void Choose(RenameChoice choice)
        {
            _onChoice?.Invoke(choice);
            Close();
        }
    }
}
