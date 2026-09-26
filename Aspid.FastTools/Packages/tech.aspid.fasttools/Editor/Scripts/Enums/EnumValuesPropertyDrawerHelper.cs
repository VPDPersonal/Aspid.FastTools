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
        private const string NothingCaption = "Nothing";
        private const string EverythingCaption = "Everything";
        private const string NoneCaption = "<None>";

        public static Type? GetEnumType(SerializedProperty enumTypeProperty)
        {
            var type = Type.GetType(enumTypeProperty.stringValue, throwOnError: false);
            return type is { IsEnum: true } ? type : null;
        }

        public static bool HasMembers(Type enumType) =>
            Enum.GetValues(enumType).Length > 0;

        // Drawing never rewrites the key: one the enum cannot parse is kept until a member is picked,
        // so switching the enum type back or restoring a member brings the row back.
        public static Enum? ParseKey(string key, Type enumType) =>
            Enum.TryParse(enumType, key, out var parsed) ? (Enum)parsed : null;

        // Unity's flags fields hold a 32-bit mask: they throw for a 64-bit enum or cut its high bits off.
        public static bool IsWideFlags(Type enumType)
        {
            if (!EnumInfo.IsFlags(enumType)) return false;

            var underlyingType = Enum.GetUnderlyingType(enumType);
            return underlyingType == typeof(long) || underlyingType == typeof(ulong);
        }

        public static string GetKeyCaption(string key, Enum? enumValue)
        {
            if (enumValue is null)
                return string.IsNullOrWhiteSpace(key) ? NoneCaption : $"<Missing {key}>";

            if (EnumInfo.ToInt64(enumValue) is 0L && !Enum.IsDefined(enumValue.GetType(), enumValue))
                return NothingCaption;

            return enumValue.ToString();
        }

        public static Enum ToggleFlag(Enum current, Enum flag)
        {
            var mask = EnumInfo.ToInt64(current);
            var bits = EnumInfo.ToInt64(flag);

            mask = (mask & bits) == bits ? mask & ~bits : mask | bits;
            return (Enum)Enum.ToObject(current.GetType(), mask);
        }

        public static void ShowKeyMenu(Rect rect, SerializedObject serializedObject, string keyPath, string enumTypePath)
        {
            var keyProperty = serializedObject.FindProperty(keyPath);
            if (GetEnumType(serializedObject.FindProperty(enumTypePath)) is not { } enumType) return;

            var menu = new GenericMenu();
            var current = ParseKey(keyProperty.stringValue, enumType);

            if (current is null || !EnumInfo.IsFlags(enumType))
            {
                foreach (var member in GetDistinctMembers(enumType))
                {
                    var isChecked = current is not null && EnumInfo.ToInt64(current) == EnumInfo.ToInt64(member);
                    AddKeyItem(member.ToString(), isChecked, member);
                }
            }
            else
            {
                var all = GetDistinctMembers(enumType).Aggregate(0L, (mask, member) => mask | EnumInfo.ToInt64(member));
                var mask = EnumInfo.ToInt64(current);

                AddKeyItem(NothingCaption, mask is 0L, (Enum)Enum.ToObject(enumType, 0L));
                AddKeyItem(EverythingCaption, mask == all, (Enum)Enum.ToObject(enumType, all));
                menu.AddSeparator(string.Empty);

                foreach (var member in GetDistinctMembers(enumType))
                {
                    var bits = EnumInfo.ToInt64(member);
                    if (bits is 0L) continue;

                    AddKeyItem(member.ToString(), (mask & bits) == bits, ToggleFlag(current, member));
                }
            }

            menu.DropDown(rect);

            void AddKeyItem(string text, bool isChecked, Enum key) =>
                menu.AddItem(new GUIContent(text), isChecked, () => serializedObject
                    .FindProperty(keyPath)
                    .SetStringAndApply(key.ToString()));
        }

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

        internal static void PopulateMissing(
            SerializedProperty values,
            SerializedProperty enumType,
            SerializedProperty defaultValue)
        {
            if (GetEnumType(enumType) is not { } type) return;

            var existing = CollectExistingKeys(values, type);
            var added = false;

            // Compare numeric values: an alias shares its member's value, and ToString() names only one of them.
            foreach (var member in GetDistinctMembers(type))
            {
                if (!existing.Add(EnumInfo.ToInt64(member))) continue;

                values.arraySize++;

                var element = values.GetArrayElementAtIndex(values.arraySize - 1);
                element.FindPropertyRelative("_key").stringValue = member.ToString();
                element.FindPropertyRelative("_enumType").stringValue = enumType.stringValue;
                CopyValue(defaultValue, element.FindPropertyRelative("_value"));

                added = true;
            }

            if (added)
                values.serializedObject.ApplyModifiedProperties();
        }

        internal static bool HasMissingMembers(SerializedProperty values, SerializedProperty enumType)
        {
            if (GetEnumType(enumType) is not { } type) return false;

            var existing = CollectExistingKeys(values, type);
            return GetDistinctMembers(type).Any(member => !existing.Contains(EnumInfo.ToInt64(member)));
        }

        private static IEnumerable<Enum> GetDistinctMembers(Type enumType)
        {
            var seen = new HashSet<long>();

            foreach (Enum member in Enum.GetValues(enumType))
            {
                if (seen.Add(EnumInfo.ToInt64(member)))
                    yield return member;
            }
        }

        private static HashSet<long> CollectExistingKeys(SerializedProperty values, Type enumType)
        {
            var set = new HashSet<long>();
            for (var i = 0; i < values.arraySize; i++)
            {
                var element = values.GetArrayElementAtIndex(i);

                if (ParseKey(element.FindPropertyRelative("_key").stringValue, enumType) is { } key)
                    set.Add(EnumInfo.ToInt64(key));
            }

            return set;
        }

        private static void CopyValue(SerializedProperty source, SerializedProperty destination)
        {
            // boxedValue throws for an array or a list, so they are copied element by element.
            if (source.isArray && source.propertyType is not SerializedPropertyType.String)
            {
                destination.arraySize = source.arraySize;

                for (var i = 0; i < source.arraySize; i++)
                    CopyValue(source.GetArrayElementAtIndex(i), destination.GetArrayElementAtIndex(i));

                return;
            }

            destination.boxedValue = source.boxedValue;
        }
    }
}
