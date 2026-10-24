using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Reflection;
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
        private const string DotNetReferenceUrl = "https://learn.microsoft.com/dotnet/api/";
        private const string UnityScriptReferencePage = "file:///unity/ScriptReference/";

        /// <summary>
        /// Searches script assets for a declaration of <paramref name="type"/>.
        /// </summary>
        /// <remarks>
        /// Text matching is limited to assets matching the type name and, for nested types, the declaring type's script.
        /// Check <see cref="MonoScript.GetClass"/> before assigning the result to a component's <c>m_Script</c> property.
        /// </remarks>
        /// <param name="type">The type to locate, or <see langword="null"/> for no match.</param>
        /// <returns>The matching script asset; otherwise, <see langword="null"/> if no declaration is found.</returns>
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
        /// <remarks>
        /// A type that ships with the Unity Editor and has no script in the project opens its API reference page
        /// instead: a <c>UnityEngine</c> or <c>UnityEditor</c> type its Scripting API page for the running Editor
        /// version, a <c>System</c> or <c>Microsoft</c> type its .NET API page. Logs a warning when neither exists;
        /// a <see langword="null"/> type is ignored.
        /// </remarks>
        /// <param name="type">The type whose script to open.</param>
        public static void OpenInScriptEditor(this Type type)
        {
            if (type is null) return;
            var monoScript = type.FindMonoScript();

            if (monoScript is not null)
            {
                AssetDatabase.OpenAsset(monoScript, FindTypeLineNumber(monoScript, type));
                return;
            }

            var page = GetApiReferencePage(type);

            if (page is null)
            {
                Debug.LogWarning($"MonoScript for type {type.AssemblyQualifiedName} not found.");
                return;
            }

            // Help resolves the Unity page to the installed offline docs, or to the online docs of this Editor version.
            if (page.StartsWith(UnityScriptReferencePage, StringComparison.Ordinal))
                Help.ShowHelpPage(page);
            else
                Application.OpenURL(page);
        }

        // The API reference page of a type that ships with the Editor; null for a type of the project or a plugin.
        internal static string GetApiReferencePage(Type type)
        {
            var lookupType = GetLookupType(type);
            if (!IsEditorBuiltIn(lookupType.Assembly)) return null;

            var typeNamespace = lookupType.Namespace ?? string.Empty;
            var fullName = lookupType.FullName?.Replace('+', '.');
            if (fullName is null) return null;

            if (IsInNamespace(typeNamespace, root: "UnityEngine") || IsInNamespace(typeNamespace, root: "UnityEditor"))
            {
                // The Scripting API drops the root namespace and names ObjectPool<T> ObjectPool_1.
                var page = fullName.Substring(fullName.IndexOf('.') + 1).Replace('`', '_');
                return $"{UnityScriptReferencePage}{page}.html";
            }

            if (IsInNamespace(typeNamespace, root: "System") || IsInNamespace(typeNamespace, root: "Microsoft"))
                return DotNetReferenceUrl + fullName.Replace('`', '-').ToLowerInvariant();

            return null;
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

        private static bool IsInNamespace(string typeNamespace, string root) =>
            typeNamespace == root || typeNamespace.StartsWith(root + ".", StringComparison.Ordinal);

        // A plugin DLL may declare UnityEngine or System namespaces too, so only the Editor's own assemblies count.
        private static bool IsEditorBuiltIn(Assembly assembly)
        {
            if (assembly.IsDynamic || string.IsNullOrEmpty(assembly.Location)) return false;

            var editorPath = Path.GetFullPath(EditorApplication.applicationContentsPath) + Path.DirectorySeparatorChar;
            var assemblyPath = Path.GetFullPath(assembly.Location);

            return assemblyPath.StartsWith(editorPath, StringComparison.OrdinalIgnoreCase);
        }

        private static string GetDeclarationPattern(bool isEnum, string typeName) => isEnum
            ? $@"\benum\s+{Regex.Escape(typeName)}\b"
            : $@"\b(class|struct|record|interface)\s+{Regex.Escape(typeName)}\b";
    }
}
