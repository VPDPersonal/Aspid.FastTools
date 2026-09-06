using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Text.RegularExpressions;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types.Editors
{
    /// <summary>
    /// Provides editor-side extension methods for locating and opening the <see cref="MonoScript"/> defining a
    /// <see cref="Type"/>.
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        /// Searches the Asset Database for the <see cref="MonoScript"/> defining a type.
        /// </summary>
        /// <remarks>
        /// Falls back to scanning script text when <see cref="MonoScript.GetClass"/> finds no match, so a type whose
        /// file name differs from its own is still found. A nested type owns no script asset, so the lookup walks out
        /// to the declaring type and accepts that script only when its text really declares the nested type.
        /// <para>
        /// The result is the file the type is declared in, which for a nested type is not the file whose own class it
        /// is, so a caller writing it into <c>m_Script</c> must check <see cref="MonoScript.GetClass"/> against the
        /// type it asked for.
        /// </para>
        /// </remarks>
        /// <param name="type">The type to locate a script asset for.</param>
        /// <returns>The matching asset, or <see langword="null"/> when none is found.</returns>
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

            // A nested type never has a script of its own, and its declaration is not always in the file the
            // declaring type resolves to: a partial outer is split across several, and a generated nested type has
            // no source line at all. The declaring script is therefore accepted only once its text carries the
            // nested declaration, so a miss answers "not found" instead of pointing at an unrelated file.
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

        // Enums are matched separately so a class/struct/record/interface lookup never lands on a same-named enum
        // declaration. `record struct Name` is covered by the `struct` alternative.
        private static string GetDeclarationPattern(bool isEnum, string typeName) => isEnum
            ? $@"\benum\s+{Regex.Escape(typeName)}\b"
            : $@"\b(class|struct|record|interface)\s+{Regex.Escape(typeName)}\b";
    }
}
