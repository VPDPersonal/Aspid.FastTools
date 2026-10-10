using System;
using UnityEditor;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    internal sealed class SerializeReferenceNamePrompt : EditorWindow
    {
        private const string NameFieldControl = "nameField";

        private string _value = string.Empty;
        private Action<string> _onConfirm;
        private bool _focused;

        public static void Show(string title, string initial, Action<string> onConfirm)
        {
            var window = CreateInstance<SerializeReferenceNamePrompt>();
            window.titleContent = new GUIContent(title);
            window._value = initial ?? string.Empty;
            window._onConfirm = onConfirm;
            var size = new Vector2(340f, 96f);
            window.position = CenterOn(EditorGUIUtility.GetMainWindowPosition(), size);
            window.minSize = window.maxSize = size;
            window.ShowModalUtility();
        }

        // Centred on the editor window: the primary display's resolution is in pixels and misses the editor on a
        // second monitor.
        internal static Rect CenterOn(Rect owner, Vector2 size) =>
            new(Mathf.Round(owner.center.x - size.x / 2f), Mathf.Round(owner.center.y - size.y / 2f), size.x, size.y);

        private void OnGUI()
        {
            GUILayout.Space(8f);

            GUI.SetNextControlName(NameFieldControl);
            _value = EditorGUILayout.TextField("Name", _value);
            if (!_focused)
            {
                EditorGUI.FocusTextInControl(NameFieldControl);
                _focused = true;
            }

            var current = Event.current;
            var valid = !string.IsNullOrWhiteSpace(_value);

            if (current.type == EventType.KeyDown)
            {
                if (current.keyCode is KeyCode.Return or KeyCode.KeypadEnter && valid)
                {
                    Confirm();
                    current.Use();
                    return;
                }

                if (current.keyCode == KeyCode.Escape)
                {
                    Close();
                    current.Use();
                    return;
                }
            }

            GUILayout.FlexibleSpace();
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Cancel", GUILayout.Width(90f))) Close();

                using (new EditorGUI.DisabledScope(!valid))
                    if (GUILayout.Button("Save", GUILayout.Width(90f))) Confirm();
            }

            GUILayout.Space(6f);
        }

        private void Confirm()
        {
            _onConfirm?.Invoke(_value.Trim());
            Close();
        }
    }
}
