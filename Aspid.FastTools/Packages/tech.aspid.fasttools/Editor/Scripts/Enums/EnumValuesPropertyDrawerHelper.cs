#nullable enable
using System;
using UnityEditor;
using System.Linq;
using UnityEngine;
using System.Reflection;
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
        private const string EntrySegment = "._values.Array.data[";

        private static readonly Dictionary<Type, HashSet<long>> _obsoleteValues = new();

        public static Type? GetEnumType(SerializedProperty? enumTypeProperty)
        {
            if (enumTypeProperty is null) return null;

            var type = Type.GetType(enumTypeProperty.stringValue, throwOnError: false);
            return type is { IsEnum: true } ? type : null;
        }

        // A row stores no enum of its own: it belongs to the _values array of an EnumValues, whose _enumType names it.
        public static SerializedProperty? FindEnumTypeProperty(SerializedProperty entry)
        {
            var path = entry.propertyPath;
            var index = path.LastIndexOf(EntrySegment, StringComparison.Ordinal);

            return index < 0
                ? null
                : entry.serializedObject.FindProperty($"{path.Substring(0, index)}._enumType");
        }

        public static bool HasMembers(Type enumType) =>
            Enum.GetValues(enumType).Length > 0;

        // Drawing never rewrites the key: one the enum cannot parse is kept until a member is picked,
        // so switching the enum type back or restoring a member brings the row back.
        public static Enum? ParseKey(string key, Type enumType) =>
            Enum.TryParse(enumType, key, out var parsed) ? (Enum)parsed : null;

        // Unity's flags fields hold a signed 32-bit mask: they throw for a 64-bit enum or cut its high bits off,
        // and misread a uint enum that uses bit 31.
        public static bool IsWideFlags(Type enumType)
        {
            if (!EnumInfo.IsFlags(enumType)) return false;

            var underlyingType = Enum.GetUnderlyingType(enumType);
            if (underlyingType == typeof(long) || underlyingType == typeof(ulong)) return true;

            return underlyingType == typeof(uint)
                && Enum.GetValues(enumType).Cast<Enum>().Any(value => (EnumInfo.ToInt64(value) & 0x80000000L) != 0);
        }

        // Unity's enum fields leave out [Obsolete] members, so a key on one reads blank in them.
        // A key the fields cannot show goes to the menu field, which names it.
        public static bool UsesKeyMenu(Type enumType, Enum? enumValue) =>
            enumValue is null || IsWideFlags(enumType) || HasObsoleteMember(enumType, enumValue);

        public static bool HasObsoleteMember(Type enumType, Enum value)
        {
            var obsolete = GetObsoleteValues(enumType);
            if (obsolete.Count is 0) return false;

            var key = EnumInfo.ToInt64(value);
            if (obsolete.Contains(key)) return true;

            return EnumInfo.IsFlags(enumType) && obsolete.Any(bits => bits is not 0L && (key & bits) == bits);
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

        public static ContextualMenuManipulator CreatePopulateMenuManipulator(
            SerializedObject serializedObject,
            string values,
            string enumType,
            string defaultValue) => new(evt =>
        {
            var valuesProperty = serializedObject.FindProperty(values);
            var enumTypeProperty = serializedObject.FindProperty(enumType);
            var defaultValueProperty = serializedObject.FindProperty(defaultValue);

            var status = CanPopulate(valuesProperty, enumTypeProperty)
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

            if (CanPopulate(valuesProperty, enumTypeProperty))
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

        // Several selected objects share one array size, so rows built from the first object would overwrite the others.
        internal static bool CanPopulate(SerializedProperty values, SerializedProperty enumType) =>
            !values.serializedObject.isEditingMultipleObjects && HasMissingMembers(values, enumType);

        internal static void PopulateMissing(
            SerializedProperty values,
            SerializedProperty enumType,
            SerializedProperty defaultValue)
        {
            if (values.serializedObject.isEditingMultipleObjects) return;
            if (GetEnumType(enumType) is not { } type) return;

            var existing = CollectExistingKeys(values, type);
            var added = false;

            // Compare numeric values: an alias shares its member's value, and ToString() names only one of them.
            foreach (var member in GetPopulateMembers(type))
            {
                if (!existing.Add(EnumInfo.ToInt64(member))) continue;

                values.arraySize++;

                var element = values.GetArrayElementAtIndex(values.arraySize - 1);
                element.FindPropertyRelative("_key").stringValue = member.ToString();
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
            return GetPopulateMembers(type).Any(member => !existing.Contains(EnumInfo.ToInt64(member)));
        }

        // A member marked [Obsolete] is kept out of new rows, like it is kept out of Unity's enum fields.
        private static IEnumerable<Enum> GetPopulateMembers(Type enumType)
        {
            var obsolete = GetObsoleteValues(enumType);
            return GetDistinctMembers(enumType).Where(member => !obsolete.Contains(EnumInfo.ToInt64(member)));
        }

        // A value is obsolete when every name it has is [Obsolete]: an alias without the attribute keeps it shown.
        private static HashSet<long> GetObsoleteValues(Type enumType)
        {
            if (_obsoleteValues.TryGetValue(enumType, out var cached)) return cached;

            var live = new HashSet<long>();
            var obsolete = new HashSet<long>();

            foreach (var field in enumType.GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                var value = EnumInfo.ToInt64((Enum)field.GetValue(null));
                (field.IsDefined(typeof(ObsoleteAttribute), inherit: false) ? obsolete : live).Add(value);
            }

            obsolete.ExceptWith(live);
            _obsoleteValues[enumType] = obsolete;
            return obsolete;
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
