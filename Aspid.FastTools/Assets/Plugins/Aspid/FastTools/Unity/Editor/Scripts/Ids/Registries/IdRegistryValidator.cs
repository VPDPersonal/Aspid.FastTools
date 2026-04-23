#nullable enable
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

        public static CleanUpSummary Summarize(IRegistryAccessor accessor)
        {
            var empty = 0;
            var duplicates = 0;
            var structural = accessor.HasStructuralDamage(out _) ? 1 : 0;

            var seen = new HashSet<string>();
            for (var i = 0; i < accessor.Count; i++)
            {
                var name = accessor.GetName(i);
                if (string.IsNullOrEmpty(name)) empty++;
                else if (!seen.Add(name)) duplicates++;
            }

            return new CleanUpSummary(empty, duplicates, structural);
        }
    }

    internal readonly struct CleanUpSummary
    {
        public readonly int EmptyCount;
        public readonly int DuplicateCount;
        public readonly int StructuralIssues;

        public CleanUpSummary(int emptyCount, int duplicateCount, int structuralIssues)
        {
            EmptyCount = emptyCount;
            DuplicateCount = duplicateCount;
            StructuralIssues = structuralIssues;
        }

        public int Total => EmptyCount + DuplicateCount + StructuralIssues;

        public string ToShortLabel()
        {
            var parts = new List<string>();
            if (DuplicateCount > 0) parts.Add($"{DuplicateCount} duplicates");
            if (EmptyCount > 0) parts.Add($"{EmptyCount} empty name" + (EmptyCount == 1 ? string.Empty : "s"));
            if (StructuralIssues > 0) parts.Add("structural issues");
            return $"⚠ {Total} invalid entr{(Total == 1 ? "y" : "ies")} ({string.Join(", ", parts)})";
        }
    }
}
