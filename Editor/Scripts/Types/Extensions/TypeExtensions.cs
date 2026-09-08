using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Text.RegularExpressions;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types.Editors
{
    /// <summary>
    /// Provides extension methods for locating and opening the <see cref="MonoScript"/> defining a
    /// <see cref="Type"/>.
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        /// Searches script assets for a declaration of <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type to locate, or <see langword="null"/> for no match.</param>
        /// <returns>The matching script asset; otherwise, <see langword="null"/> if no declaration is found.</returns>
        /// <remarks>
        /// Text matching is limited to assets matching the type name and, for nested types, the declaring type's script.
        /// Check <see cref="MonoScript.GetClass"/> before assigning the result to a component's <c>m_Script</c> property.
        /// </remarks>
        public static MonoScript FindMonoScript(this Type type)
        {
            if (type is null) return null;

            var lookupType = GetLookupType(type);
            var typeNamespace = lookupType.Namespace;
            var typeName = TypeUtility.StripArity(lookupType.Name);

            var scripts = AssetDatabase.FindAssets(filter: $"t:MonoScript {typeName}")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<MonoScript>)
                .Where(script => script is not null)
                .ToArray();

            var exact = scripts.FirstOrDefault(script => script.GetClass() == lookupType);
            if (exact is not null) return exact;

            var pattern = GetDeclarationPattern(lookupType.IsEnum, typeName);

            foreach (var script in scripts)
            {
                var text = script.text;
                if (string.IsNullOrWhiteSpace(text)) continue;
                if (!string.IsNullOrWhiteSpace(typeNamespace) && !text.Contains($"namespace {typeNamespace}")) continue;
                if (!Regex.IsMatch(text, pattern)) continue;

                return script;
            }

            // A nested type may live in another partial file or generated code; accept the outer script only if
            // it contains the declaration.
            if (lookupType.DeclaringType is not { } declaringType) return null;

            var declaringScript = declaringType.FindMonoScript();
            if (declaringScript is null || string.IsNullOrWhiteSpace(declaringScript.text)) return null;

            return Regex.IsMatch(declaringScript.text, pattern) ? declaringScript : null;
        }

        /// <summary>
        /// Opens the script defining <paramref name="type"/> at its declaration line.
        /// </summary>
        /// <remarks>Logs a warning when no script can be located; a <see langword="null"/> type is ignored.</remarks>
        /// <param name="type">The type whose script to open.</param>
        public static void OpenInScriptEditor(this Type type)
        {
            if (type is null) return;
            var monoScript = type.FindMonoScript();

            if (monoScript is null)
            {
                Debug.LogWarning($"MonoScript for type {type.AssemblyQualifiedName} not found.");
                return;
            }

            AssetDatabase.OpenAsset(monoScript, FindTypeLineNumber(monoScript, type));
        }

        private static int FindTypeLineNumber(MonoScript script, Type type)
        {
            var lookupType = GetLookupType(type);
            return FindTypeLineNumber(script.text, lookupType.IsEnum, TypeUtility.StripArity(lookupType.Name));
        }

        private static int FindTypeLineNumber(string text, bool isEnum, string typeName)
        {
            if (string.IsNullOrWhiteSpace(text)) return 1;

            var pattern = GetDeclarationPattern(isEnum, typeName);
            var lines = text.Split('\n');

            for (var i = 0; i < lines.Length; i++)
            {
                if (Regex.IsMatch(lines[i], pattern))
                    return i + 1;
            }

            return 1;
        }

        private static Type GetLookupType(Type type) =>
            type.IsGenericType ? type.GetGenericTypeDefinition() : type;

        private static string GetDeclarationPattern(bool isEnum, string typeName) => isEnum
            ? $@"\benum\s+{Regex.Escape(typeName)}\b"
            : $@"\b(class|struct|record|interface)\s+{Regex.Escape(typeName)}\b";
    }
}
