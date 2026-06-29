using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;
using System.Text.RegularExpressions;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types.Editors
{
    /// <summary>
    /// Editor-side extension methods for <see cref="Type"/> that locate the
    /// <see cref="MonoScript"/> asset corresponding to a given type using the Asset Database.
    /// </summary>
    public static class TypeExtensions
    {
        private static readonly Dictionary<string, Regex> RegexCache = new();

        /// <summary>
        /// Searches the Asset Database for the <see cref="MonoScript"/> that defines the given type.
        /// The search first tries an exact match via <see cref="MonoScript.GetClass"/>, then falls back
        /// to a regex match against the script text, checking the namespace and the type declaration keyword
        /// (<c>class</c>, <c>struct</c>, <c>record</c>, or <c>enum</c>).
        /// </summary>
        /// <param name="type">The type to locate a script asset for.</param>
        /// <returns>
        /// The matching <see cref="MonoScript"/> asset, or <see langword="null"/> if none is found.
        /// </returns>
        public static MonoScript FindMonoScript(this Type type)
        {
            if (type is null) return null;

            // A closed generic (e.g. Modifier<Modifier<int>>) has no script of its own — the source file
            // declares the open definition, so look that up and match by its arity-stripped name.
            var lookupType = type.IsGenericType ? type.GetGenericTypeDefinition() : type;
            var isEnum = lookupType.IsEnum;
            var typeName = StripArity(lookupType.Name);
            var typeNamespace = lookupType.Namespace;

            var scripts = AssetDatabase.FindAssets(filter: $"t:MonoScript {typeName}")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<MonoScript>)
                .Where(script => script is not null)
                .ToArray();

            var regex = GetRegex(isEnum, typeName);

            foreach (var script in scripts)
            {
                if (script.GetClass() != lookupType) continue;
                return script;
            }

            foreach (var script in scripts)
            {
                var text = script.text;
                if (string.IsNullOrWhiteSpace(text)) continue;
                if (!string.IsNullOrWhiteSpace(typeNamespace) && !text.Contains($"namespace {typeNamespace}")) continue;
                if (!regex.IsMatch(text)) continue;

                return script;
            }

            return null;
        }

        /// <summary>
        /// Opens the script that defines <paramref name="type"/> in the configured external
        /// editor at the line of the type declaration. Logs a warning and is a no-op when
        /// no <see cref="MonoScript"/> can be located.
        /// </summary>
        public static void OpenInScriptEditor(this Type type)
        {
            if (type is null) return;
            var (monoScript, lineNumber) = type.FindMonoScriptWithLine();

            if (monoScript is null)
            {
                Debug.LogWarning($"MonoScript for type {type.AssemblyQualifiedName} not found.");
                return;
            }

            AssetDatabase.OpenAsset(monoScript, lineNumber);
        }

        /// <summary>
        /// Searches the Asset Database for the <see cref="MonoScript"/> that defines the given type
        /// and also determines the 1-based line number of the type declaration within that script.
        /// </summary>
        /// <param name="type">The type to locate.</param>
        /// <returns>
        /// A tuple of the matched <see cref="MonoScript"/> and the 1-based line number of the type declaration.
        /// If no script is found, returns <c>(null, 0)</c>.
        /// </returns>
        public static (MonoScript script, int line) FindMonoScriptWithLine(this Type type)
        {
            var script = type.FindMonoScript();
            if (script is null) return (script: null, line: 0);

            var lookupType = type.IsGenericType ? type.GetGenericTypeDefinition() : type;
            var line = FindTypeLineNumber(script.text, StripArity(lookupType.Name), lookupType.IsEnum);
            return (script, line);
        }

        /// <summary>
        /// Removes the CLR generic-arity suffix (<c>Modifier`1</c> → <c>Modifier</c>) from a raw type name.
        /// Names without a backtick are returned unchanged. Shared by the type-name formatters.
        /// </summary>
        internal static string StripArity(string name)
        {
            var tick = name.IndexOf('`');
            return tick >= 0 ? name[..tick] : name;
        }

        /// <summary>
        /// Short display name for a type: open generic definitions and closed generics are rendered with
        /// angle-bracket arguments (<c>Modifier&lt;Single&gt;</c>) instead of the raw arity form (<c>Modifier`1</c>),
        /// recursing into the arguments so nested generics render fully (<c>Modifier&lt;Modifier&lt;Int32&gt;&gt;</c>).
        /// Non-generic types are returned unchanged. Shared by the type-selector display formatters.
        /// </summary>
        internal static string FormatGenericName(Type type)
        {
            if (!type.IsGenericType) return type.Name;

            var baseName = StripArity(type.Name);
            var arguments = string.Join(", ", type.GetGenericArguments().Select(FormatGenericName));
            return $"{baseName}<{arguments}>";
        }

        /// <summary>
        /// Enumerates every type across all currently loaded assemblies, dropping the entries that fail to load in a
        /// partially-loadable assembly (<see cref="ReflectionTypeLoadException"/>). Shared by the type-selector
        /// candidate scan and the open-generic resolver so both walk the domain the same way.
        /// </summary>
        internal static IEnumerable<Type> EnumerateDomainTypes()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;

                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException exception)
                {
                    types = exception.Types.Where(type => type is not null).ToArray();
                }

                foreach (var type in types)
                    yield return type;
            }
        }
        
        private static int FindTypeLineNumber(string text, string typeName, bool isEnum)
        {
            if (string.IsNullOrEmpty(text)) return 1;

            var regex = GetRegex(isEnum, typeName);
            var lines = text.Split('\n');

            for (var i = 0; i < lines.Length; i++)
            {
                if (regex.IsMatch(lines[i]))
                    return i + 1;
            }

            return 1;
        }
        
        private static string GetPattern(bool isEnum, string typeName) => isEnum
            ? $@"\benum\s+{Regex.Escape(typeName)}\b"
            : $@"\b(class|struct|record)\s+{Regex.Escape(typeName)}\b";
        
        private static Regex GetRegex(bool isEnum, string typeName)
        {
            var key = $"{isEnum}:{typeName}";
            if (RegexCache.TryGetValue(key, out var cached))
                return cached;

            var regex = new Regex(GetPattern(isEnum, typeName), RegexOptions.Compiled);
            RegexCache[key] = regex;
            return regex;
        }
    }
}
