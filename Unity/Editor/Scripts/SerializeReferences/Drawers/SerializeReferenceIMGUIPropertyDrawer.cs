using System;
using UnityEditor;
using UnityEngine;
using Aspid.FastTools.Types;
using Aspid.FastTools.Editors;
using Aspid.FastTools.Types.Editors;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    /// <summary>
    /// IMGUI rendering for the <c>[SerializeReferenceSelector]</c> drawer: a foldout-and-dropdown header
    /// row, an optional missing-type warning, and the nested properties of the assigned instance.
    /// </summary>
    internal static class SerializeReferenceIMGUIPropertyDrawer
    {
        public static float GetHeight(SerializedProperty property)
        {
            var spacing = EditorGUIUtility.standardVerticalSpacing;
            var height = EditorGUIUtility.singleLineHeight;

            if (SerializeReferenceHelpers.IsMissingType(property))
            {
                height += spacing + GetWarningHeight();

                if (SerializeReferenceHelpers.TryGetAssetLocation(property, out _, out _))
                    height += spacing + EditorGUIUtility.singleLineHeight;
            }

            if (SerializeReferenceHelpers.HasSharedReference(property))
                height += spacing + GetWarningHeight();

            if (property.managedReferenceValue is not null && property.isExpanded)
                height += GetChildrenHeight(property, spacing);

            return height;
        }

        public static void Draw(Rect position, GUIContent label, SerializedProperty property, params Type[] types)
        {
            var spacing = EditorGUIUtility.standardVerticalSpacing;
            var currentType = SerializeReferenceHelpers.GetCurrentType(property);
            var hasValue = currentType is not null;

            var line = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

            var contextEvent = Event.current;
            if (contextEvent.type == EventType.ContextClick && line.Contains(contextEvent.mousePosition))
            {
                var fieldType = types.Length > 0 ? types[0] : typeof(object);
                ShowContextMenu(property, fieldType);
                contextEvent.Use();
            }

            var labelRect = new Rect(line.x, line.y, EditorGUIUtility.labelWidth, line.height);
            if (hasValue) property.isExpanded = EditorGUI.Foldout(labelRect, property.isExpanded, label, toggleOnLabelClick: true);
            else EditorGUI.LabelField(labelRect, label);

            var dropdownRect = new Rect(
                line.x + EditorGUIUtility.labelWidth + 2f,
                line.y,
                line.width - EditorGUIUtility.labelWidth - 2f,
                line.height);

            var openRect = Rect.zero;
            if (hasValue)
            {
                var openSize = line.height;
                openRect = new Rect(dropdownRect.xMax - openSize, dropdownRect.y, openSize, openSize);
                dropdownRect.width -= openSize + 1f;
            }

            var caption = GetCaption(property, currentType);
            if (EditorGUI.DropdownButton(dropdownRect, new GUIContent(caption), FocusType.Passive))
                ShowSelector(property, types, currentType, dropdownRect);

            if (hasValue)
                TypeIMGUIPropertyDrawer.DrawOpenScriptButton(openRect, currentType);

            var y = line.yMax + spacing;

            if (SerializeReferenceHelpers.IsMissingType(property))
            {
                var warningHeight = GetWarningHeight();
                var warningRect = new Rect(position.x, y, position.width, warningHeight);
                EditorGUI.HelpBox(warningRect, $"Missing type: {SerializeReferenceHelpers.GetMissingTypeDisplayName(property)}", MessageType.Warning);
                y += warningHeight + spacing;

                if (SerializeReferenceHelpers.TryGetAssetLocation(property, out _, out _))
                {
                    var buttonRect = new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight);
                    if (GUI.Button(buttonRect, "Fix"))
                    {
                        var screenPosition = GUIUtility.GUIToScreenPoint(new Vector2(buttonRect.x, buttonRect.yMax));
                        var screenRect = new Rect(screenPosition.x, screenPosition.y, buttonRect.width, buttonRect.height);
                        SerializeReferenceHelpers.ShowFixTypeSelector(property.Persistent(), screenRect, null);
                    }
                    y += EditorGUIUtility.singleLineHeight + spacing;
                }
            }

            if (SerializeReferenceHelpers.HasSharedReference(property))
            {
                var sharedHeight = GetWarningHeight();
                var sharedRect = new Rect(position.x, y, position.width, sharedHeight);
                EditorGUI.HelpBox(sharedRect,
                    "Shared reference — editing one field changes both. Right-click → Make Unique Reference.",
                    MessageType.Info);
                y += sharedHeight + spacing;
            }

            if (!hasValue || !property.isExpanded) return;

            EditorGUI.indentLevel++;
            DrawChildren(property, position.x, position.width, spacing, ref y);
            EditorGUI.indentLevel--;
        }

        private static void DrawChildren(SerializedProperty property, float x, float width, float spacing, ref float y)
        {
            var iterator = property.Copy();
            var end = property.GetEndProperty();
            var enterChildren = true;

            while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, end))
            {
                enterChildren = false;

                var height = EditorGUI.GetPropertyHeight(iterator, includeChildren: true);
                EditorGUI.PropertyField(new Rect(x, y, width, height), iterator, includeChildren: true);
                y += height + spacing;
            }
        }

        private static float GetChildrenHeight(SerializedProperty property, float spacing)
        {
            var height = 0f;
            var iterator = property.Copy();
            var end = property.GetEndProperty();
            var enterChildren = true;

            while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, end))
            {
                enterChildren = false;
                height += EditorGUI.GetPropertyHeight(iterator, includeChildren: true) + spacing;
            }

            return height;
        }

        private static void ShowSelector(SerializedProperty property, Type[] types, Type currentType, Rect dropdownRect)
        {
            var persistent = property.Persistent();
            var fieldType = types.Length > 0 ? types[0] : typeof(object);
            var screenPosition = GUIUtility.GUIToScreenPoint(new Vector2(dropdownRect.x, dropdownRect.y));
            var screenRect = new Rect(screenPosition.x, screenPosition.y, dropdownRect.width, dropdownRect.height);

            TypeSelectorWindow.Show(
                screenRect: screenRect,
                types: types,
                currentAqn: currentType?.AssemblyQualifiedName ?? string.Empty,
                allow: TypeAllow.None,
                onSelected: assemblyQualifiedName => Apply(string.IsNullOrEmpty(assemblyQualifiedName)
                    ? null
                    : Type.GetType(assemblyQualifiedName, throwOnError: false)),
                filter: SerializeReferenceHelpers.IsAssignableManagedReference,
                additionalTypes: GenericTypeResolver.GetAssignableGenericDefinitions(fieldType),
                argumentFilter: SerializeReferenceHelpers.IsValidGenericArgument);

            return;

            void Apply(Type type)
            {
                var previous = persistent.managedReferenceValue;
                persistent.SetManagedReferenceAndApply(SerializeReferenceHelpers.CreateInstancePreservingData(type, previous));
                persistent.isExpanded = type is not null;
            }
        }

        private static void ShowContextMenu(SerializedProperty property, Type fieldType)
        {
            var persistent = property.Persistent();
            var menu = new GenericMenu();

            menu.AddItem(new GUIContent("Copy Serialize Reference"), false,
                () => SerializeReferenceClipboard.Copy(persistent.managedReferenceValue));

            var pasteLabel = new GUIContent("Paste Serialize Reference");
            if (SerializeReferenceClipboard.CanPasteInto(fieldType))
                menu.AddItem(pasteLabel, false, () => Paste(persistent));
            else
                menu.AddDisabledItem(pasteLabel);

            if (SerializeReferenceHelpers.HasSharedReference(property))
                menu.AddItem(new GUIContent("Make Unique Reference"), false,
                    () => SerializeReferenceHelpers.MakeReferenceUnique(persistent));

            menu.ShowAsContext();

            void Paste(SerializedProperty target)
            {
                var value = SerializeReferenceClipboard.CreateInstance();
                target.SetManagedReferenceAndApply(value);
                target.isExpanded = value is not null;
            }
        }


        private static string GetCaption(SerializedProperty property, Type currentType)
        {
            if (currentType is not null)
                return TypeSelectorHelpers.GetTypeSelectorTitle(currentType);

            var missingName = SerializeReferenceHelpers.IsMissingType(property)
                ? SerializeReferenceHelpers.GetMissingTypeDisplayName(property)
                : null;

            return TypeSelectorHelpers.GetTypeSelectorTitle(null, missingName);
        }

        private static float GetWarningHeight() => EditorGUIUtility.singleLineHeight * 2f;
    }
}
