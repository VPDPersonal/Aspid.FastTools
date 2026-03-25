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
    internal static class IdDropdownDrawer
    {
        private const string NoneOption = "<None>";
        private const float CreateButtonWidth = 55f;

        // IMGUI per-property state: (isCreating, inputText)
        private static readonly Dictionary<string, (bool creating, string input)> _imguiState = new();

        #region IMGUI

        internal static float GetIMGUIHeight(SerializedProperty property)
        {
            var h = EditorGUIUtility.singleLineHeight;
            if (_imguiState.TryGetValue(PropertyKey(property), out var s) && s.creating)
                h += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            return h;
        }

        internal static void DrawIMGUI(Rect position, SerializedProperty property, GUIContent label, Type? declaringType)
        {
            if (!string.IsNullOrWhiteSpace(label.text))
            {
                EditorGUI.LabelField(position, label);
                position.x += EditorGUIUtility.labelWidth;
                position.width -= EditorGUIUtility.labelWidth;
            }

            var key = PropertyKey(property);
            _imguiState.TryGetValue(key, out var state);

            // Main row
            var mainRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            var dropRect = new Rect(mainRect.x, mainRect.y, mainRect.width - CreateButtonWidth - 2f, mainRect.height);
            var btnRect  = new Rect(dropRect.xMax + 2f, mainRect.y, CreateButtonWidth, mainRect.height);

            if (EditorGUI.DropdownButton(dropRect, new GUIContent(Caption(property.stringValue)), FocusType.Passive))
            {
                var reg = FindRegistry(declaringType);
                var sp  = GUIUtility.GUIToScreenPoint(new Vector2(dropRect.x, dropRect.y));
                var sr  = new Rect(sp.x, sp.y, dropRect.width, dropRect.height);
                StringIdSelectorWindow.Show(reg?.Ids ?? Array.Empty<string>(), sr, property.stringValue,
                    id => property.SetStringAndApply(id ?? string.Empty));
            }

            if (GUI.Button(btnRect, state.creating ? "Cancel" : "Create"))
            {
                _imguiState[key] = state.creating ? (false, string.Empty) : (true, string.Empty);
                state = _imguiState[key];
            }

            if (!state.creating) return;

            // Inline create row
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

            var reg2    = FindRegistry(declaringType);
            var trimmed = state.input?.Trim() ?? string.Empty;
            var canAdd  = !string.IsNullOrEmpty(trimmed) && (reg2 == null || !reg2.Contains(trimmed));

            using (new EditorGUI.DisabledScope(!canAdd))
            {
                if (GUI.Button(addRect, "Add"))
                {
                    var registry = reg2 ?? CreateRegistryForType(declaringType);
                    if (registry != null)
                    {
                        registry.Add(trimmed);
                        EditorUtility.SetDirty(registry);
                        AssetDatabase.SaveAssetIfDirty(registry);
                        property.SetStringAndApply(trimmed);
                    }
                    _imguiState[key] = (false, string.Empty);
                }
            }

            if (GUI.Button(cancelRect, "✗"))
                _imguiState[key] = (false, string.Empty);
        }

        #endregion

        #region UIToolkit

        internal static VisualElement DrawUIToolkit(SerializedProperty property, string label, Type? declaringType)
        {
            var outerContainer = new VisualElement()
                .SetFlexDirection(FlexDirection.Column)
                .AddChild(new PropertyField(property).SetDisplay(DisplayStyle.None));

            var mainRow = new VisualElement()
                .SetFlexDirection(FlexDirection.Row);

            var dropdownButton = new Button()
                .SetMargin(0)
                .SetFlexGrow(1)
                .SetFlexShrink(1)
                .SetOverflow(Overflow.Hidden)
                .SetWhiteSpace(WhiteSpace.NoWrap)
                .SetUnityTextAlign(TextAnchor.MiddleLeft)
                .SetTextOverflow(TextOverflow.Ellipsis)
                .SetText(Caption(property.stringValue));

            var createToggleButton = new Button()
                .SetText("Create")
                .SetMargin(left: 4);

            // Inline create row (hidden by default)
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

            // Input validation
            inputField.RegisterValueChangedCallback(e =>
            {
                var val = e.newValue?.Trim() ?? string.Empty;
                var reg = FindRegistry(declaringType);

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

            // Toggle create row
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

            // Confirm: add to registry and set value
            addButton.clicked += () =>
            {
                var id = inputField.value?.Trim();
                if (string.IsNullOrEmpty(id)) return;

                var reg = FindRegistry(declaringType) ?? CreateRegistryForType(declaringType);
                if (reg == null) return;

                reg.Add(id);
                EditorUtility.SetDirty(reg);
                AssetDatabase.SaveAssetIfDirty(reg);

                var p = GetProperty(serializedObject, propertyPath);
                p.SetStringAndApply(id);
                dropdownButton.SetText(Caption(id));

                inputField.value = string.Empty;
                createRow.SetDisplay(DisplayStyle.None);
                errorLabel.SetDisplay(DisplayStyle.None);
            };

            // Cancel
            cancelRowButton.clicked += () =>
            {
                inputField.value = string.Empty;
                createRow.SetDisplay(DisplayStyle.None);
                errorLabel.SetDisplay(DisplayStyle.None);
            };

            // Open selector dropdown
            dropdownButton.clicked += () =>
            {
                var reg    = FindRegistry(declaringType);
                var window = EditorWindow.focusedWindow;
                var wb     = dropdownButton.worldBound;
                var sr     = new Rect(window.position.x + wb.xMin, window.position.y + wb.yMin, wb.width, wb.height);

                var current = GetProperty(serializedObject, propertyPath).stringValue ?? string.Empty;
                StringIdSelectorWindow.Show(reg?.Ids ?? Array.Empty<string>(), sr, current, id =>
                {
                    var p = GetProperty(serializedObject, propertyPath);
                    p.SetStringAndApply(id ?? string.Empty);
                    dropdownButton.SetText(Caption(p.stringValue));
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

        #region Registry

        internal static StringIdRegistry? GetRegistry(Type? declaringType) => FindRegistry(declaringType);

        private static StringIdRegistry? FindRegistry(Type? declaringType)
        {
            if (declaringType == null) return null;

            var aqn   = declaringType.AssemblyQualifiedName ?? string.Empty;
            var guids = AssetDatabase.FindAssets("t:StringIdRegistry");

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var reg  = AssetDatabase.LoadAssetAtPath<StringIdRegistry>(path);
                if (reg != null && reg.TargetStructType == aqn)
                    return reg;
            }

            return null;
        }

        private static StringIdRegistry? CreateRegistryForType(Type? declaringType)
        {
            if (declaringType == null) return null;

            var path = AssetDatabase.GenerateUniqueAssetPath($"Assets/StringIdRegistry_{declaringType.Name}.asset");
            var reg  = ScriptableObject.CreateInstance<StringIdRegistry>();
            AssetDatabase.CreateAsset(reg, path);

            var so = new SerializedObject(reg);
            so.FindProperty("_targetStructType").stringValue = declaringType.AssemblyQualifiedName ?? string.Empty;
            so.ApplyModifiedPropertiesWithoutUndo();

            AssetDatabase.SaveAssets();
            return reg;
        }

        #endregion

        #region Helpers

        private static SerializedProperty GetProperty(SerializedObject so, string path) =>
            so.FindProperty(path);

        private static string Caption(string? id) =>
            string.IsNullOrEmpty(id) ? NoneOption : id!;

        private static string PropertyKey(SerializedProperty p) =>
            $"{p.serializedObject.targetObject.GetInstanceID()}:{p.propertyPath}";

        #endregion
    }
}
