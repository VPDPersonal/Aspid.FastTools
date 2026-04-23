#nullable enable
using UnityEditor;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Ids.Editors
{
    internal static class IdRegistryValidator
    {
        private const int MaxNameLength = 255;

        private static readonly Regex IdentifierPattern =
            new(@"^[A-Za-z_][A-Za-z0-9_]*$", RegexOptions.Compiled);

        private static readonly HashSet<string> ReservedKeywords = new()
        {
            "abstract", "as", "base", "bool", "break", "byte", "case", "catch",
            "char", "checked", "class", "const", "continue", "decimal", "default",
            "delegate", "do", "double", "else", "enum", "event", "explicit",
            "extern", "false", "finally", "fixed", "float", "for", "foreach",
            "goto", "if", "implicit", "in", "int", "interface", "internal", "is",
            "lock", "long", "namespace", "new", "null", "object", "operator",
            "out", "override", "params", "private", "protected", "public",
            "readonly", "ref", "return", "sbyte", "sealed", "short", "sizeof",
            "stackalloc", "static", "string", "struct", "switch", "this", "throw",
            "true", "try", "typeof", "uint", "ulong", "unchecked", "unsafe",
            "ushort", "using", "virtual", "void", "volatile", "while"
        };

        /// <summary>
        /// Validates a candidate id name. Rules, in order: not whitespace,
        /// valid C# identifier, not a reserved keyword, length ≤ 255,
        /// not in the optional <paramref name="existing"/> set.
        /// </summary>
        public static bool IsValidName(string? input, HashSet<string>? existing, out string? error)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                error = "Name cannot be empty.";
                return false;
            }

            if (!IdentifierPattern.IsMatch(input))
            {
                error = "Name must be a valid C# identifier (letters, digits, underscore; cannot start with a digit).";
                return false;
            }

            if (ReservedKeywords.Contains(input))
            {
                error = $"'{input}' is a reserved C# keyword.";
                return false;
            }

            if (input.Length > MaxNameLength)
            {
                error = $"Name is too long (max {MaxNameLength} chars).";
                return false;
            }

            if (existing != null && existing.Contains(input))
            {
                error = $"'{input}' already exists.";
                return false;
            }

            error = null;
            return true;
        }

        public static HashSet<string> GetDuplicates(SerializedProperty entriesProp)
        {
            var seen  = new HashSet<string>();
            var dupes = new HashSet<string>();

            for (var i = 0; i < entriesProp.arraySize; i++)
            {
                var val = entriesProp.GetArrayElementAtIndex(i).FindPropertyRelative("Name").stringValue;
                if (!string.IsNullOrEmpty(val) && !seen.Add(val))
                    dupes.Add(val);
            }

            return dupes;
        }

        public static bool HasDuplicate(StringIdRegistry registry, string entryName)
        {
            var seen = false;
            foreach (var name in registry.IdNames)
            {
                if (name != entryName) continue;
                if (seen) return true;
                seen = true;
            }
            return false;
        }

        // CleanUpInvalid stays for now — removed in Task 16 when the explicit clean-up row replaces it.
        public static void CleanUpInvalid(Object target)
        {
            var so       = new SerializedObject(target);
            var entries  = so.FindProperty("_entries");
            if (entries == null) return;

            var seen     = new HashSet<string>();
            var toRemove = new List<int>();

            for (int i = 0; i < entries.arraySize; i++)
            {
                var val = entries.GetArrayElementAtIndex(i).FindPropertyRelative("Name").stringValue;
                if (string.IsNullOrEmpty(val) || !seen.Add(val))
                    toRemove.Add(i);
            }

            for (var i = toRemove.Count - 1; i >= 0; i--)
                entries.DeleteArrayElementAtIndex(toRemove[i]);

            if (toRemove.Count > 0)
                so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
