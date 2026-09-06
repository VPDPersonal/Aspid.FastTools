#nullable enable
using System;
using UnityEditor;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Aspid.FastTools.Editors;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Enums.Editors
{
    internal static class EnumValuesPropertyDrawerHelper
    {
        private const string PopulateMenuItem = "Populate Missing Enum Members";

        // Resolves the row's key against the configured enum type; null means the type is not
        // an enum (or has no members) and the raw string field should be shown instead. A key
        // that no longer matches any member falls back to the first one and is persisted, so a
        // renamed enum migrates instead of leaving the row unusable.
        public static Enum? ResolveKey(SerializedProperty keyProperty, SerializedProperty enumTypeProperty)
        {
            var enumType = Type.GetType(enumTypeProperty.stringValue, throwOnError: false);
            if (enumType is null || !enumType.IsEnum) return null;

            if (!Enum.TryParse(enumType, keyProperty.stringValue, out var parsed))
            {
                var values = Enum.GetValues(enumType);
                if (values.Length is 0) return null;

                parsed = values.GetValue(0);
            }

            var enumValue = (Enum)parsed;

            if (keyProperty.stringValue != enumValue.ToString())
                keyProperty.SetStringAndApply(enumValue.ToString());

            return enumValue;
        }

        // Every entry mirrors the parent's _enumType so its own drawer can pick the right field.
        public static void SyncEntryEnumTypes(SerializedProperty values, SerializedProperty enumType)
        {
            var enumTypeValue = enumType.stringValue;

            for (var i = 0; i < values.arraySize; i++)
            {
                var element = values
                    .GetArrayElementAtIndex(i)
                    .FindPropertyRelative("_enumType");

                if (element.stringValue != enumTypeValue)
                    element.SetStringAndApply(enumTypeValue);
            }
        }

        public static ContextualMenuManipulator CreatePopulateMenuManipulator(
            SerializedObject serializedObject,
            string values,
            string enumType,
            string defaultValue) => new(evt =>
        {
            var valuesProperty = serializedObject.FindProperty(values);
            var enumTypeProperty = serializedObject.FindProperty(enumType);
            var defaultValueProperty = serializedObject.FindProperty(defaultValue);

            var status = HasMissingMembers(valuesProperty, enumTypeProperty)
                ? DropdownMenuAction.Status.Normal
                : DropdownMenuAction.Status.Disabled;

            evt.menu.AppendAction(
                PopulateMenuItem,
                _ => PopulateMissing(valuesProperty, enumTypeProperty, defaultValueProperty),
                status);
        });

        public static void ShowPopulateContextMenu(
            Rect rect,
            SerializedObject serializedObject,
            string values,
            string enumType,
            string defaultValue)
        {
            var current = Event.current;
            if (current.type != EventType.ContextClick || !rect.Contains(current.mousePosition)) return;

            var valuesProperty = serializedObject.FindProperty(values);
            var enumTypeProperty = serializedObject.FindProperty(enumType);

            var menu = new GenericMenu();
            var menuLabel = new GUIContent(PopulateMenuItem);

            if (HasMissingMembers(valuesProperty, enumTypeProperty))
            {
                menu.AddItem(menuLabel, false, () => PopulateMissing(
                    serializedObject.FindProperty(values),
                    serializedObject.FindProperty(enumType),
                    serializedObject.FindProperty(defaultValue)));
            }
            else
            {
                menu.AddDisabledItem(menuLabel);
            }

            menu.ShowAsContext();
            current.Use();
        }

        private static void PopulateMissing(
            SerializedProperty values,
            SerializedProperty enumType,
            SerializedProperty defaultValue)
        {
            var type = Type.GetType(enumType.stringValue, throwOnError: false);
            if (type is null || !type.IsEnum) return;

            var existing = CollectExistingKeys(values);
            var added = false;

            // For [Flags] enums this includes named composites (All = A | B) as separate rows.
            foreach (var name in Enum.GetNames(type))
            {
                if (!existing.Add(name)) continue;

                values.arraySize++;

                var element = values.GetArrayElementAtIndex(values.arraySize - 1);
                element.FindPropertyRelative("_key").stringValue = name;
                element.FindPropertyRelative("_enumType").stringValue = enumType.stringValue;
                element.FindPropertyRelative("_value").boxedValue = defaultValue.boxedValue;

                added = true;
            }

            if (added)
                values.serializedObject.ApplyModifiedProperties();
        }

        private static bool HasMissingMembers(SerializedProperty values, SerializedProperty enumType)
        {
            var type = Type.GetType(enumType.stringValue, throwOnError: false);
            if (type is null || !type.IsEnum) return false;

            var existing = CollectExistingKeys(values);
            return Enum.GetNames(type).Any(name => !existing.Contains(name));
        }

        private static HashSet<string> CollectExistingKeys(SerializedProperty values)
        {
            var set = new HashSet<string>(values.arraySize);
            for (var i = 0; i < values.arraySize; i++)
            {
                var element = values.GetArrayElementAtIndex(i);
                set.Add(element.FindPropertyRelative("_key").stringValue);
            }

            return set;
        }
    }
}
