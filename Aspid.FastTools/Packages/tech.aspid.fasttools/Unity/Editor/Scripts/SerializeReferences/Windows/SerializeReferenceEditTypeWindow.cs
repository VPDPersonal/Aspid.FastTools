using System;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using Aspid.FastTools.UIElements;
using Aspid.FastTools.UIElements.Editors.Internal;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    /// <summary>
    /// Small utility window for repairing a missing managed-reference type. Shows the stored Assembly / Namespace /
    /// Class of the unresolved type as editable fields and, on Apply, hands the corrected
    /// <see cref="ManagedTypeName"/> back to the drawer, which rewrites it into the asset YAML.
    /// </summary>
    internal sealed class SerializeReferenceEditTypeWindow : EditorWindow
    {
        private Action<ManagedTypeName> _onApply;

        public static void Show(ManagedTypeName current, Action<ManagedTypeName> onApply)
        {
            var window = GetWindow<SerializeReferenceEditTypeWindow>(utility: true, title: "Edit Reference Type");
            window._onApply = onApply;
            window.minSize = new Vector2(380f, 168f);
            window.Build(current);
            window.ShowUtility();
        }

        private void Build(ManagedTypeName current)
        {
            var root = rootVisualElement;
            root.Clear();
            root.AddStyleSheetsFromResource(AspidStyles.DefaultStyleSheet)
                .SetPadding(8f);

            root.AddChild(new AspidHelpBox(AspidHelpBoxPreset.Default.SetMessageType(HelpBoxMessageType.Info))
            {
                Message = "Re-point this reference to an existing type. The type is written straight into the asset " +
                          "file (Unity cannot reassign a missing reference through the Inspector)."
            });

            var classField = new TextField("Class") { value = current.Class };
            var namespaceField = new TextField("Namespace") { value = current.Namespace };
            var assemblyField = new TextField("Assembly") { value = current.Assembly };

            root.AddChild(classField)
                .AddChild(namespaceField)
                .AddChild(assemblyField);

            var buttons = new VisualElement()
                .SetFlexDirection(FlexDirection.Row)
                .SetJustifyContent(Justify.FlexEnd)
                .SetMarginTop(8f);

            var cancel = new Button(Close) { text = "Cancel" };
            var apply = new Button(() =>
            {
                _onApply?.Invoke(new ManagedTypeName(assemblyField.value, namespaceField.value, classField.value));
                Close();
            }) { text = "Apply" };

            buttons.AddChild(cancel).AddChild(apply);
            root.AddChild(buttons);
        }
    }
}
