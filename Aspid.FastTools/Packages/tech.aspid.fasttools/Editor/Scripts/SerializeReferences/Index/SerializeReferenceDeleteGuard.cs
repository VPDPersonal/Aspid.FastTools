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

        // A C# identifier, which may start with any Unicode letter ("Оружие").
        private const string Identifier = @"@?[\p{L}\p{Nl}_][\p{L}\p{Mn}\p{Mc}\p{Nd}\p{Nl}\p{Pc}\p{Cf}]*";

        // Declarations and the braces and semicolons that scope them. "record struct" / "record class" come first, or
        // the bare "record" alternative would swallow the keyword; "where" after "class" or "struct" is a constraint
        // clause, not a name. The type parameter list is captured so that Command and Command<T> are not confused.
        private static readonly Regex _declarationToken = new(
            @"\bnamespace\s+(?<namespace>" + Identifier + @"(?:\s*\.\s*" + Identifier + @")*)" +
            @"|(?<partial>\bpartial\s+)?\b(?:record\s+(?:class|struct)|class|struct|record|interface)\s+(?!where\b)" +
            @"(?<name>" + Identifier + @")(?:\s*<(?<parameters>[^<>]*)>)?" +
            @"|(?<open>\{)|(?<close>\})|(?<end>;)",
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

            var types = ResolveCandidateTypes(new[] { assetPath });
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
            var scriptPaths = AssetDatabase.FindAssets("t:MonoScript", new[] { folderPath })
                .Select(AssetDatabase.GUIDToAssetPath)
                .ToList();

            var types = ResolveCandidateTypes(scriptPaths);
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

        // MonoScript.GetClass() only knows the class named after the file, so every type the scripts declare is looked
        // up in their assembly by namespace and nesting path. Only types that can be a managed-reference value are
        // kept: a script holding just components or ScriptableObjects never needs the project sweep. A partial type is
        // kept only when no script left after the delete declares another part of it.
        internal static List<Type> ResolveCandidateTypes(IReadOnlyCollection<string> scriptPaths)
        {
            var result = new List<Type>();
            var deleted = new HashSet<string>(scriptPaths, StringComparer.Ordinal);
            var assemblyTypes = new Dictionary<string, Type[]>(StringComparer.Ordinal);
            Dictionary<string, string[]> assemblySources = null;

            foreach (var scriptPath in scriptPaths)
            {
                var script = AssetDatabase.LoadAssetAtPath<MonoScript>(scriptPath);
                if (script == null) continue;

                var declarations = ScanDeclarations(script.text);
                if (declarations.Count == 0) continue;

                var assemblyName = Path.GetFileNameWithoutExtension(CompilationPipeline.GetAssemblyNameFromScriptPath(scriptPath));
                foreach (var type in GetAssemblyTypes(assemblyName, assemblyTypes))
                {
                    if (result.Contains(type) || !CanBeManagedReference(type)) continue;
                    if (!declarations.TryGetValue(DeclarationKey(type), out var isPartial)) continue;

                    if (isPartial)
                    {
                        assemblySources ??= CompilationPipeline.GetAssemblies(AssembliesType.Editor)
                            .GroupBy(assembly => assembly.name, StringComparer.Ordinal)
                            .ToDictionary(group => group.Key, group => group.First().sourceFiles, StringComparer.Ordinal);

                        if (HasPartInOtherScript(type, assemblyName, assemblySources, deleted)) continue;
                    }

                    result.Add(type);
                }
            }

            return result;
        }

        // Every type declaration keyed the way DeclarationKey names a type ("Namespace|Outer`1/Inner"), with whether it
        // is partial. Namespace blocks nest, and a file-scoped namespace covers the rest of the file.
        private static Dictionary<string, bool> ScanDeclarations(string source)
        {
            var declarations = new Dictionary<string, bool>(StringComparer.Ordinal);
            var text = _commentOrLiteral.Replace(source ?? string.Empty, " ");

            // Each open brace pushes what it opens: a namespace, a type path, or neither for any other block.
            var scopes = new List<(string @namespace, string typePath)>();
            var fileNamespace = string.Empty;
            string pendingNamespace = null;
            string pendingType = null;

            foreach (Match match in _declarationToken.Matches(text))
            {
                if (match.Groups["namespace"].Success)
                {
                    pendingNamespace = Regex.Replace(match.Groups["namespace"].Value, @"[\s@]", string.Empty);
                    pendingType = null;
                }
                else if (match.Groups["name"].Success)
                {
                    var outer = scopes.LastOrDefault(scope => scope.typePath is not null).typePath;
                    var name = GetDeclaredName(match);
                    pendingType = outer is null ? name : $"{outer}/{name}";

                    var key = $"{CurrentNamespace(fileNamespace, scopes)}|{pendingType}";
                    var isPartial = match.Groups["partial"].Success;
                    declarations[key] = isPartial || (declarations.TryGetValue(key, out var partial) && partial);
                }
                else if (match.Groups["open"].Success)
                {
                    scopes.Add((pendingNamespace, pendingNamespace is null ? pendingType : null));
                    pendingNamespace = null;
                    pendingType = null;
                }
                else if (match.Groups["close"].Success)
                {
                    if (scopes.Count > 0) scopes.RemoveAt(scopes.Count - 1);
                }
                else
                {
                    // "namespace A;" is file-scoped; a declaration ending in ';' (a positional record) has no body.
                    if (pendingNamespace is not null) fileNamespace = pendingNamespace;
                    pendingNamespace = null;
                    pendingType = null;
                }
            }

            return declarations;
        }

        private static string CurrentNamespace(string fileNamespace, List<(string @namespace, string typePath)> scopes)
        {
            var parts = new List<string>();
            if (fileNamespace.Length > 0) parts.Add(fileNamespace);

            foreach (var scope in scopes)
                if (scope.@namespace is not null) parts.Add(scope.@namespace);

            return string.Join(".", parts);
        }

        private static string DeclarationKey(Type type)
        {
            var path = type.Name;
            for (var declaring = type.DeclaringType; declaring is not null; declaring = declaring.DeclaringType)
                path = $"{declaring.Name}/{path}";

            return $"{type.Namespace ?? string.Empty}|{path}";
        }

        private static bool HasPartInOtherScript(
            Type type,
            string assemblyName,
            Dictionary<string, string[]> assemblySources,
            HashSet<string> deleted)
        {
            if (string.IsNullOrEmpty(assemblyName) || !assemblySources.TryGetValue(assemblyName, out var sources)) return false;

            var key = DeclarationKey(type);
            var name = TypeUtility.StripArity(type.Name);

            foreach (var source in sources)
            {
                if (deleted.Contains(source)) continue;

                string text;
                try
                {
                    text = File.ReadAllText(source);
                }
                catch (Exception)
                {
                    continue;
                }

                // Most scripts never name the type, so the declaration scan runs on few of them.
                if (text.IndexOf(name, StringComparison.Ordinal) < 0) continue;
                if (ScanDeclarations(text).ContainsKey(key)) return true;
            }

            return false;
        }

        private static IEnumerable<Type> GetAssemblyTypes(string assemblyName, Dictionary<string, Type[]> cache)
        {
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

        // Names carry the generic arity the way Type.Name does ("Command`1").
        private static string GetDeclaredName(Match match)
        {
            var name = match.Groups["name"].Value.TrimStart('@');
            var parameters = match.Groups["parameters"];
            if (!parameters.Success || parameters.Value.Trim().Length == 0) return name;

            return $"{name}`{parameters.Value.Count(character => character == ',') + 1}";
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

                    var lines = ReadIfMayHoldUsages(path, classTokens);
                    if (lines is null) continue;

                    var usedHere = false;
                    // A pure text pass rather than an asset load; prefab instance overrides count like RefIds entries.
                    foreach (var usage in SerializeReferenceTypeUsageIndex.CollectUsages(lines, guid: null))
                    {
                        if (!typesByKey.TryGetValue(SerializeReferenceHelpers.OpenTypeKey(usage.StoredType), out var type)) continue;

                        counts[type]++;
                        usedHere = true;
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

        // A line probe before the parse: most assets hold no managed references at all, and the rest rarely name the
        // class being deleted. Returns the lines for the parse, so each asset is read once, or null when the asset can
        // be skipped.
        private static string[] ReadIfMayHoldUsages(string path, HashSet<string> classTokens)
        {
            var lines = SerializeReferenceYaml.ReadLines(path);
            if (lines is null) return null;

            // Unity writes a character outside printable ASCII as a YAML escape ("\u041F"), so a non-ASCII class name
            // never appears as written: any line with an escape may name it.
            var namesEscaped = classTokens.Any(token => token.Any(c => c < ' ' || c > '~'));

            var mayHoldUsages = false;
            var namesType = false;

            foreach (var line in lines)
            {
                mayHoldUsages = mayHoldUsages || SerializeReferenceTypeUsageIndex.MayHoldUsages(line);
                namesType = namesType ||
                    (namesEscaped && line.IndexOf('\\') >= 0) ||
                    classTokens.Any(token => line.IndexOf(token, StringComparison.Ordinal) >= 0);

                if (mayHoldUsages && namesType) return lines;
            }

            return null;
        }

        private static void AddSample(ICollection<string> samplePaths, string path)
        {
            if (samplePaths is null || string.IsNullOrEmpty(path) || samplePaths.Count >= SamplePathCount) return;
            if (!samplePaths.Contains(path)) samplePaths.Add(path);
        }
    }
}
