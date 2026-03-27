#nullable enable
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Editors
{
    public static class IdStructDrawer
    {
        private const string NoneOption = "<None>";
        private const float CreateButtonWidth = 55f;

        // IMGUI per-property state: (isCreating, inputText)
        private static readonly Dictionary<string, (bool creating, string input)> _imguiState = new();

        #region IMGUI

        public static float GetIMGUIHeight(SerializedProperty property)
        {
            var h = EditorGUIUtility.singleLineHeight;
            if (_imguiState.TryGetValue(PropertyKey(property), out var s) && s.creating)
                h += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            return h;
        }

        public static void DrawIMGUI(Rect position, SerializedProperty property, GUIContent label, Type fieldType)
        {
            var stringIdProp = property.FindPropertyRelative("__stringId");
            var intIdProp    = property.FindPropertyRelative("_id");

            if (!string.IsNullOrWhiteSpace(label.text))
            {
                EditorGUI.LabelField(position, label);
                position.x += EditorGUIUtility.labelWidth;
                position.width -= EditorGUIUtility.labelWidth;
            }

            var key = PropertyKey(property);
            _imguiState.TryGetValue(key, out var state);

            var mainRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            var dropRect = new Rect(mainRect.x, mainRect.y, mainRect.width - CreateButtonWidth - 2f, mainRect.height);
            var btnRect  = new Rect(dropRect.xMax + 2f, mainRect.y, CreateButtonWidth, mainRect.height);

            var currentName = stringIdProp?.stringValue ?? string.Empty;

            if (EditorGUI.DropdownButton(dropRect, new GUIContent(Caption(currentName)), FocusType.Passive))
            {
                var reg = StringIdRegistryHelper.FindRegistry(fieldType);
                var sp  = GUIUtility.GUIToScreenPoint(new Vector2(dropRect.x, dropRect.y));
                var sr  = new Rect(sp.x, sp.y, dropRect.width, dropRect.height);
                StringIdSelectorWindow.Show(reg?.Ids ?? Array.Empty<string>(), sr, currentName,
                    selected => ApplySelection(property, stringIdProp, intIdProp, fieldType, selected));
            }

            if (GUI.Button(btnRect, state.creating ? "Cancel" : "Create"))
            {
                _imguiState[key] = state.creating ? (false, string.Empty) : (true, string.Empty);
                state = _imguiState[key];
            }

            if (!state.creating) return;

            var gap     = 2f;
            var addW    = 40f;
            var cancelW = 22f;
            var rowY    = mainRect.yMax + EditorGUIUtility.standardVerticalSpacing;
            var inputW  = position.width - addW - cancelW - gap * 2f;

            var inputRect  = new Rect(position.x, rowY, inputW, EditorGUIUtility.singleLineHeight);
            var addRect    = new Rect(inputRect.xMax + gap, rowY, addW, EditorGUIUtility.singleLineHeight);
            var cancelRect = new Rect(addRect.xMax + gap, rowY, cancelW, EditorGUIUtility.singleLineHeight);

            var newInput = EditorGUI.TextField(inputRect, state.input ?? string.Empty);
            if (newInput != state.input)
            {
                state.input = newInput;
                _imguiState[key] = state;
            }

            var reg2    = StringIdRegistryHelper.FindRegistry(fieldType);
            var trimmed = state.input?.Trim() ?? string.Empty;
            var canAdd  = !string.IsNullOrEmpty(trimmed) && (reg2 == null || !reg2.Contains(trimmed));

            using (new EditorGUI.DisabledScope(!canAdd))
            {
                if (GUI.Button(addRect, "Add"))
                {
                    var registry = reg2 ?? StringIdRegistryHelper.CreateRegistry(fieldType);
                    var assignedId = registry.Add(trimmed);
                    EditorUtility.SetDirty(registry);
                    AssetDatabase.SaveAssetIfDirty(registry);
                    SetFields(property, stringIdProp, intIdProp, trimmed, assignedId);
                    _imguiState[key] = (false, string.Empty);
                }
            }

            if (GUI.Button(cancelRect, "✗"))
                _imguiState[key] = (false, string.Empty);
        }

        #endregion

        #region UIToolkit

        public static VisualElement DrawUIToolkit(SerializedProperty property, string label, Type fieldType)
        {
            var stringIdProp = property.FindPropertyRelative("__stringId");
            var intIdProp    = property.FindPropertyRelative("_id");

            var outerContainer = new VisualElement()
                .SetFlexDirection(FlexDirection.Column)
                .AddChild(new PropertyField(property).SetDisplay(DisplayStyle.None));

            var mainRow = new VisualElement()
                .SetFlexDirection(FlexDirection.Row);

            var currentName = stringIdProp?.stringValue ?? string.Empty;

            var dropdownButton = new Button()
                .SetMargin(0)
                .SetFlexGrow(1)
                .SetFlexShrink(1)
                .SetOverflow(Overflow.Hidden)
                .SetWhiteSpace(WhiteSpace.NoWrap)
                .SetUnityTextAlign(TextAnchor.MiddleLeft)
                .SetTextOverflow(TextOverflow.Ellipsis)
                .SetText(Caption(currentName));

            var createToggleButton = new Button()
                .SetText("Create")
                .SetMargin(left: 4);

            var createRow = new VisualElement()
                .SetFlexDirection(FlexDirection.Row)
                .SetDisplay(DisplayStyle.None)
                .SetMargin(top: 2);

            var inputField = new TextField();
            inputField.style.flexGrow = 1;
            inputField.style.flexShrink = 1;

            var addButton = new Button()
                .SetText("Add")
                .SetMargin(left: 4);
            addButton.SetEnabled(false);

            var cancelRowButton = new Button()
                .SetText("✗")
                .SetMargin(left: 2);

            var errorLabel = new Label()
                .SetDisplay(DisplayStyle.None)
                .SetMargin(top: 2);
            errorLabel.style.color = new Color(1f, 0.4f, 0.4f);
            errorLabel.style.fontSize = 10;

            var propertyPath     = property.propertyPath;
            var serializedObject = property.serializedObject;

            inputField.RegisterValueChangedCallback(e =>
            {
                var val = e.newValue?.Trim() ?? string.Empty;
                var reg = StringIdRegistryHelper.FindRegistry(fieldType);

                if (string.IsNullOrEmpty(val))
                {
                    errorLabel.SetDisplay(DisplayStyle.None);
                    addButton.SetEnabled(false);
                    return;
                }

                if (reg != null && reg.Contains(val))
                {
                    errorLabel.text = "ID already exists";
                    errorLabel.SetDisplay(DisplayStyle.Flex);
                    addButton.SetEnabled(false);
                    return;
                }

                errorLabel.SetDisplay(DisplayStyle.None);
                addButton.SetEnabled(true);
            });

            createToggleButton.clicked += () =>
            {
                var isVisible = createRow.style.display == DisplayStyle.Flex;
                createRow.SetDisplay(isVisible ? DisplayStyle.None : DisplayStyle.Flex);
                errorLabel.SetDisplay(DisplayStyle.None);
                if (!isVisible)
                {
                    inputField.value = string.Empty;
                    inputField.Focus();
                }
            };

            addButton.clicked += () =>
            {
                var name = inputField.value?.Trim();
                if (string.IsNullOrEmpty(name)) return;

                var reg = StringIdRegistryHelper.FindRegistry(fieldType) ?? StringIdRegistryHelper.CreateRegistry(fieldType);
                var assignedId = reg.Add(name);
                EditorUtility.SetDirty(reg);
                AssetDatabase.SaveAssetIfDirty(reg);

                var p        = serializedObject.FindProperty(propertyPath);
                var strProp  = p.FindPropertyRelative("__stringId");
                var intProp  = p.FindPropertyRelative("_id");
                SetFields(p, strProp, intProp, name, assignedId);
                dropdownButton.SetText(Caption(name));

                inputField.value = string.Empty;
                createRow.SetDisplay(DisplayStyle.None);
                errorLabel.SetDisplay(DisplayStyle.None);
            };

            cancelRowButton.clicked += () =>
            {
                inputField.value = string.Empty;
                createRow.SetDisplay(DisplayStyle.None);
                errorLabel.SetDisplay(DisplayStyle.None);
            };

            dropdownButton.clicked += () =>
            {
                var reg    = StringIdRegistryHelper.FindRegistry(fieldType);
                var window = EditorWindow.focusedWindow;
                var wb     = dropdownButton.worldBound;
                var sr     = new Rect(window.position.x + wb.xMin, window.position.y + wb.yMin, wb.width, wb.height);

                var p       = serializedObject.FindProperty(propertyPath);
                var strProp = p.FindPropertyRelative("__stringId");
                var current = strProp?.stringValue ?? string.Empty;

                StringIdSelectorWindow.Show(reg?.Ids ?? Array.Empty<string>(), sr, current, selected =>
                {
                    var p2       = serializedObject.FindProperty(propertyPath);
                    var strProp2 = p2.FindPropertyRelative("__stringId");
                    var intProp2 = p2.FindPropertyRelative("_id");
                    ApplySelection(p2, strProp2, intProp2, fieldType, selected);
                    dropdownButton.SetText(Caption(strProp2?.stringValue ?? string.Empty));
                });
            };

            if (!string.IsNullOrEmpty(label))
            {
                mainRow.AddChild(new Label(label)
                    .SetUnityTextAlign(TextAnchor.MiddleLeft)
                    .SetMargin(right: 4));
            }

            mainRow
                .AddChild(dropdownButton)
                .AddChild(createToggleButton);

            createRow
                .AddChild(inputField)
                .AddChild(addButton)
                .AddChild(cancelRowButton);

            outerContainer
                .AddChild(mainRow)
                .AddChild(createRow)
                .AddChild(errorLabel);

            return outerContainer;
        }

        #endregion

        #region Helpers

        private static void ApplySelection(
            SerializedProperty property,
            SerializedProperty? stringIdProp,
            SerializedProperty? intIdProp,
            Type fieldType,
            string? selected)
        {
            var name = selected ?? string.Empty;
            var id   = 0;
            if (!string.IsNullOrEmpty(name))
            {
                var reg = StringIdRegistryHelper.FindRegistry(fieldType);
                id = reg?.GetId(name) ?? 0;
            }
            SetFields(property, stringIdProp, intIdProp, name, id);
        }

        private static void SetFields(
            SerializedProperty property,
            SerializedProperty? stringIdProp,
            SerializedProperty? intIdProp,
            string name,
            int id)
        {
            if (stringIdProp != null)
                stringIdProp.stringValue = name;
            if (intIdProp != null)
                intIdProp.intValue = id;
            property.serializedObject.ApplyModifiedProperties();
        }

        private static string Caption(string? name) =>
            string.IsNullOrEmpty(name) ? NoneOption : name!;

        private static string PropertyKey(SerializedProperty p) =>
            $"{p.serializedObject.targetObject.GetInstanceID()}:{p.propertyPath}";

        #endregion
    }
}
