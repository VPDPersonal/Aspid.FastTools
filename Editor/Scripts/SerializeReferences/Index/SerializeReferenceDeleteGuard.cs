using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using System.Diagnostics;
using UnityEditor.Compilation;
using System.Collections.Generic;
using Aspid.FastTools.Types.Editors;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;
using Object = UnityEngine.Object;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    internal sealed class SerializeReferenceDeleteGuard : AssetModificationProcessor
    {
        private const int SamplePathCount = 8;
        private const long ProgressDelayMilliseconds = 250;
        private const string RefIdsMarker = "RefIds:";

        // "record struct" / "record class" come first, or the bare "record" alternative would swallow the keyword.
        // The type parameter list is captured so that Command and Command<T> of another file are not confused.
        private static readonly Regex _typeDeclaration = new(
            @"\b(?:record\s+(?:class|struct)|class|struct|record)\s+@?(?<name>[A-Za-z_]\w*)(?:\s*<(?<parameters>[^<>]*)>)?",
            RegexOptions.Compiled);

        private static readonly Regex _namespaceDeclaration = new(
            @"\bnamespace\s+@?(?<name>[A-Za-z_][\w.]*)",
            RegexOptions.Compiled);

        // Comments and string or char literals, so that "class Foo" written in them is not taken for a declaration.
        private static readonly Regex _commentOrLiteral = new(
            @"//[^\n]*|/\*.*?\*/|(?:@\$?|\$@)""(?:[^""]|"""")*""|\$?""(?:\\.|[^\\""\n])*""|'(?:\\.|[^\\'\n])*'",
            RegexOptions.Compiled | RegexOptions.Singleline);

        private static AssetDeleteResult OnWillDeleteAsset(string assetPath, RemoveAssetOptions options)
        {
            if (Application.isBatchMode) return AssetDeleteResult.DidNotDelete;
            if (string.IsNullOrEmpty(assetPath)) return AssetDeleteResult.DidNotDelete;

            // A folder delete fires once with the folder path; the scripts inside get no callback of their own.
            if (AssetDatabase.IsValidFolder(assetPath)) return GuardFolder(assetPath);

            if (!assetPath.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
                return AssetDeleteResult.DidNotDelete;

            var types = ResolveCandidateTypes(assetPath);
            if (types.Count == 0) return AssetDeleteResult.DidNotDelete;

            var sample = new SortedSet<string>(StringComparer.Ordinal);
            var counts = CountUsages(types, sample);
            if (counts is null) return AssetDeleteResult.FailedDelete;

            var used = types.Where(type => counts[type] > 0).ToList();
            if (used.Count == 0) return AssetDeleteResult.DidNotDelete;

            var count = used.Sum(type => counts[type]);
            var names = string.Join(", ", used.Select(type => $"\"{GetDisplayName(type)}\""));
            var subject = used.Count == 1
                ? $"{names} is used as a [SerializeReference] managed reference"
                : $"{names} are used as [SerializeReference] managed references";

            var message =
                $"{subject} in {count} place(s):\n\n" +
                $"{string.Join("\n", sample)}\n\n" +
                "Deleting the script will leave those references missing.";

            var proceed = EditorUtility.DisplayDialog("Delete Script", message, "Delete Anyway", "Cancel");

            return proceed ? AssetDeleteResult.DidNotDelete : AssetDeleteResult.FailedDelete;
        }

        private static AssetDeleteResult GuardFolder(string folderPath)
        {
            var types = new List<Type>();
            var assemblyTypes = new Dictionary<string, Type[]>(StringComparer.Ordinal);

            foreach (var guid in AssetDatabase.FindAssets("t:MonoScript", new[] { folderPath }))
            {
                foreach (var type in ResolveCandidateTypes(AssetDatabase.GUIDToAssetPath(guid), assemblyTypes))
                    if (!types.Contains(type)) types.Add(type);
            }

            if (types.Count == 0) return AssetDeleteResult.DidNotDelete;

            var counts = CountUsages(types, samplePaths: null);
            if (counts is null) return AssetDeleteResult.FailedDelete;

            var affected = new List<string>();
            var totalCount = 0;

            foreach (var type in types)
            {
                var count = counts[type];
                if (count <= 0) continue;

                totalCount += count;
                affected.Add($"{GetDisplayName(type)} — {count} place(s)");
            }

            if (affected.Count == 0) return AssetDeleteResult.DidNotDelete;

            var message =
                $"This folder contains {affected.Count} type(s) still used as [SerializeReference] managed " +
                $"references ({totalCount} place(s) total):\n\n{string.Join("\n", affected)}\n\n" +
                "Deleting the folder will leave those references missing.";

            var proceed = EditorUtility.DisplayDialog("Delete Folder", message, "Delete Anyway", "Cancel");
            return proceed ? AssetDeleteResult.DidNotDelete : AssetDeleteResult.FailedDelete;
        }

        internal static List<Type> ResolveCandidateTypes(string scriptPath) =>
            ResolveCandidateTypes(scriptPath, new Dictionary<string, Type[]>(StringComparer.Ordinal));

        // MonoScript.GetClass() only knows the class named after the file, so every type the script text declares is
        // looked up in the script's assembly, then their nested types. Only types that can be a managed-reference
        // value are kept: a script holding just components or ScriptableObjects never needs the project sweep.
        private static List<Type> ResolveCandidateTypes(string scriptPath, Dictionary<string, Type[]> assemblyTypes)
        {
            var result = new List<Type>();

            var script = AssetDatabase.LoadAssetAtPath<MonoScript>(scriptPath);
            if (script == null) return result;

            var text = _commentOrLiteral.Replace(script.text ?? string.Empty, " ");

            // Names carry the generic arity the way Type.Name does ("Command`1").
            var declaredNames = new HashSet<string>(StringComparer.Ordinal);
            foreach (Match match in _typeDeclaration.Matches(text))
                declaredNames.Add(GetDeclaredName(match));

            var declaredNamespaces = new HashSet<string>(StringComparer.Ordinal);
            foreach (Match match in _namespaceDeclaration.Matches(text))
                declaredNamespaces.Add(match.Groups["name"].Value);

            var declared = new List<Type>();
            // GetClass() ignores generic arity: for a script declaring only Command<T> it can return a Command of
            // another file.
            if (script.GetClass() is { } mainType && declaredNames.Contains(mainType.Name)) declared.Add(mainType);

            foreach (var type in GetAssemblyTypes(scriptPath, assemblyTypes))
            {
                if (type.DeclaringType is not null || declared.Contains(type)) continue;
                if (!declaredNames.Contains(type.Name)) continue;
                if (!DeclaresNamespace(declaredNamespaces, type.Namespace)) continue;

                declared.Add(type);
            }

            for (var i = 0; i < declared.Count; i++)
            {
                foreach (var nested in declared[i].GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic))
                {
                    // The name check drops compiler-generated closures and nested types of another partial file.
                    if (declared.Contains(nested) || !declaredNames.Contains(nested.Name)) continue;
                    declared.Add(nested);
                }
            }

            foreach (var type in declared)
                if (CanBeManagedReference(type)) result.Add(type);

            return result;
        }

        private static IEnumerable<Type> GetAssemblyTypes(string scriptPath, Dictionary<string, Type[]> cache)
        {
            var assemblyName = Path.GetFileNameWithoutExtension(CompilationPipeline.GetAssemblyNameFromScriptPath(scriptPath));
            if (string.IsNullOrEmpty(assemblyName)) return Array.Empty<Type>();
            if (cache.TryGetValue(assemblyName, out var cached)) return cached;

            var assembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(candidate => candidate.GetName().Name == assemblyName);

            Type[] types;
            try
            {
                types = assembly?.GetTypes() ?? Array.Empty<Type>();
            }
            catch (ReflectionTypeLoadException exception)
            {
                types = exception.Types.Where(type => type is not null).ToArray();
            }

            cache[assemblyName] = types;
            return types;
        }

        private static string GetDeclaredName(Match match)
        {
            var name = match.Groups["name"].Value;
            var parameters = match.Groups["parameters"];
            if (!parameters.Success || parameters.Value.Trim().Length == 0) return name;

            return $"{name}`{parameters.Value.Count(character => character == ',') + 1}";
        }

        // Nested blocks (namespace A { namespace B { } }) declare A.B piece by piece.
        private static bool DeclaresNamespace(HashSet<string> declaredNamespaces, string @namespace)
        {
            if (string.IsNullOrEmpty(@namespace) || declaredNamespaces.Contains(@namespace)) return true;

            foreach (var declared in declaredNamespaces)
            {
                if (@namespace.Length > declared.Length + 1 &&
                    @namespace[declared.Length] == '.' &&
                    @namespace.StartsWith(declared, StringComparison.Ordinal) &&
                    DeclaresNamespace(declaredNamespaces, @namespace[(declared.Length + 1)..]))
                {
                    return true;
                }
            }

            return false;
        }

        // Unlike IsAssignableManagedReference, an open generic definition passes: its script is matched against every
        // closed instantiation stored in YAML.
        private static bool CanBeManagedReference(Type type) =>
            type is { IsClass: true, IsAbstract: false } &&
            type != typeof(string) &&
            !typeof(Object).IsAssignableFrom(type) &&
            !typeof(Delegate).IsAssignableFrom(type) &&
            !type.IsDefined(typeof(CompilerGeneratedAttribute), inherit: false);

        private static string GetDisplayName(Type type)
        {
            var name = TypeUtility.StripArity(type.Name);
            for (var declaring = type.DeclaringType; declaring is not null; declaring = declaring.DeclaringType)
                name = $"{TypeUtility.StripArity(declaring.Name)}.{name}";

            return name;
        }

        // Null when the user cancels the sweep, which cancels the delete too: its outcome was never shown. samplePaths,
        // when given, collects up to SamplePathCount asset paths that hold a usage.
        internal static Dictionary<Type, int> CountUsages(IReadOnlyList<Type> types, ICollection<string> samplePaths)
        {
            var counts = types.ToDictionary(type => type, _ => 0);

            if (SerializeReferenceTypeUsageIndex.IsWarm)
            {
                foreach (var type in types)
                {
                    foreach (var usage in SerializeReferenceTypeUsageIndex.FindUsages(type))
                    {
                        counts[type]++;
                        AddSample(samplePaths, AssetDatabase.GUIDToAssetPath(usage.Guid));
                    }
                }

                return counts;
            }

            // A cold index is never warmed just to answer one delete, since that is a modal full-project build; one
            // text sweep matches every type's open key (so a generic script matches its closed keys) instead.
            var typesByKey = new Dictionary<string, Type>(StringComparer.Ordinal);
            var classTokens = new HashSet<string>(StringComparer.Ordinal);

            foreach (var type in types)
            {
                var name = ManagedTypeName.FromType(type);
                var key = SerializeReferenceHelpers.OpenTypeKey(name);
                typesByKey[key] = type;
                classTokens.Add(key[(key.LastIndexOf('|') + 1)..]);
            }

            var paths = AssetDatabase.GetAllAssetPaths().Where(SerializeReferenceHelpers.IsScanCandidate).ToArray();
            var stopwatch = Stopwatch.StartNew();

            try
            {
                for (var i = 0; i < paths.Length; i++)
                {
                    var path = paths[i];

                    if (stopwatch.ElapsedMilliseconds >= ProgressDelayMilliseconds &&
                        EditorUtility.DisplayCancelableProgressBar(
                            "Checking Managed References",
                            $"{path}  ({i + 1}/{paths.Length})",
                            (float)i / paths.Length))
                    {
                        return null;
                    }

                    var text = ReadIfMayHoldUsages(path, classTokens);
                    if (text is null) continue;

                    var usedHere = false;
                    // Skipping display-name resolution keeps this a pure text pass rather than an asset load.
                    foreach (var document in SerializeReferenceGraphScanner.Build(path, text, resolveTypeNames: false))
                    {
                        foreach (var node in document.Nodes)
                        {
                            if (node.StoredType.IsEmpty) continue;
                            if (!typesByKey.TryGetValue(SerializeReferenceHelpers.OpenTypeKey(node.StoredType), out var type)) continue;

                            counts[type]++;
                            usedHere = true;
                        }
                    }

                    if (usedHere) AddSample(samplePaths, path);
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            return counts;
        }

        // A substring probe before the line-by-line parse: most assets hold no managed references at all, and the rest
        // rarely name the class being deleted. Returns the text for the parse, so each asset is read once, or null when
        // the asset can be skipped.
        private static string ReadIfMayHoldUsages(string path, HashSet<string> classTokens)
        {
            string text;
            try
            {
                text = File.ReadAllText(path);
            }
            catch (Exception)
            {
                return null;
            }

            if (text.IndexOf(RefIdsMarker, StringComparison.Ordinal) < 0) return null;

            foreach (var token in classTokens)
                if (text.IndexOf(token, StringComparison.Ordinal) >= 0) return text;

            return null;
        }

        private static void AddSample(ICollection<string> samplePaths, string path)
        {
            if (samplePaths is null || string.IsNullOrEmpty(path) || samplePaths.Count >= SamplePathCount) return;
            if (!samplePaths.Contains(path)) samplePaths.Add(path);
        }
    }
}
