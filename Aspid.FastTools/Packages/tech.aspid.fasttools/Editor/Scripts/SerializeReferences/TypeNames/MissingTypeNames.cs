using System;
using System.Linq;
using UnityEditor;
using System.Reflection;
using Aspid.FastTools.Types;
using System.Collections.Generic;
using Aspid.FastTools.Types.Editors;
using Object = UnityEngine.Object;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    internal readonly struct MissingTypeNameLocation
    {
        public readonly string AssetPath;
        public readonly StoredTypeNameEntry Entry;

        public MissingTypeNameLocation(string assetPath, StoredTypeNameEntry entry)
        {
            AssetPath = assetPath;
            Entry = entry;
        }
    }

    // What the type picker of a wrapper field offers, as the Inspector would set it up for that field.
    internal readonly struct TypeNameConstraint : IEquatable<TypeNameConstraint>
    {
        public static readonly TypeNameConstraint Unconstrained = new(new[] { typeof(object) }, TypeAllow.All,
            requiresScript: false, excludeEditorOnly: true);

        public readonly Type[] Types;
        public readonly TypeAllow Allow;

        // A SerializableMonoScript field takes only a type that has a script asset.
        public readonly bool RequiresScript;

        // A type stored in a runtime object must resolve in a player, so editor-only types are left out.
        public readonly bool ExcludeEditorOnly;

        public TypeNameConstraint(Type[] types, TypeAllow allow, bool requiresScript, bool excludeEditorOnly)
        {
            Types = types is { Length: > 0 } ? types : new[] { typeof(object) };
            Allow = allow;
            RequiresScript = requiresScript;
            ExcludeEditorOnly = excludeEditorOnly;
        }

        public bool IsUnconstrained => Types.All(type => type == typeof(object)) && Allow == TypeAllow.All;

        public bool Equals(TypeNameConstraint other) =>
            Allow == other.Allow && RequiresScript == other.RequiresScript && ExcludeEditorOnly == other.ExcludeEditorOnly &&
            new HashSet<Type>(Types).SetEquals(other.Types);

        public override bool Equals(object obj) => obj is TypeNameConstraint other && Equals(other);

        public override int GetHashCode() => unchecked(((int)Allow * 397) ^ (RequiresScript ? 2 : 0) ^ (ExcludeEditorOnly ? 1 : 0));
    }

    // Stored names of SerializableType and SerializableMonoScript wrappers that no longer resolve, found by a text scan
    // of the asset files: the project-wide counterpart of the Inspector's Missing type notice.
    internal static class MissingTypeNames
    {
        private const BindingFlags FieldFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

        private static readonly Dictionary<string, bool> _nameResolves = new(StringComparer.Ordinal);
        private static readonly Dictionary<(string Guid, long FileId), Type> _scriptClasses = new();
        private static readonly Dictionary<(string Guid, long FileId), Type> _overrideTargets = new();

        // Drops what earlier sweeps resolved; a sweep calls it first, so a class added or removed since is seen.
        public static void ClearCache()
        {
            _nameResolves.Clear();
            _scriptClasses.Clear();
            _overrideTargets.Clear();
        }

        // An empty name stores no type. A SerializableMonoScript resolves through its script first, as in the Editor,
        // where a player build re-syncs the name from the script before it is written.
        public static bool Resolves(StoredTypeNameEntry entry) =>
            entry.TypeName.Trim().Length == 0 ||
            NameResolves(entry.TypeName) ||
            (entry.HasScript && GetScriptClass(entry.ScriptGuid, entry.ScriptFileId) is not null);

        public static bool NameResolves(string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName)) return false;
            if (_nameResolves.TryGetValue(typeName, out var resolves)) return resolves;

            resolves = TypeUtility.GetTypeOrNull(assemblyQualifiedName: typeName) is not null;
            _nameResolves[typeName] = resolves;
            return resolves;
        }

        // The class a script asset declares; a .cs file holds one script, a DLL one per class.
        public static Type GetScriptClass(string guid, long fileId)
        {
            if (string.IsNullOrEmpty(guid)) return null;
            if (_scriptClasses.TryGetValue((guid, fileId), out var cached)) return cached;

            var script = FindSubAsset(guid, fileId) as MonoScript;
            var type = script != null ? script.GetClass() : null;

            _scriptClasses[(guid, fileId)] = type;
            return type;
        }

        public static List<MissingTypeNameLocation> Find(string assetPath, string[] lines)
        {
            var result = new List<MissingTypeNameLocation>();
            if (lines is null) return result;

            foreach (var entry in SerializeReferenceYamlEditor.FindStoredTypeNames(lines))
            {
                if (!Resolves(entry))
                    result.Add(new MissingTypeNameLocation(assetPath, entry));
            }

            return result;
        }

        public static List<MissingTypeNameLocation> FindInFile(string assetPath)
        {
            var lines = SerializeReferenceYaml.ReadLines(assetPath);
            return lines is null || !lines.Any(SerializeReferenceYamlEditor.MayHoldTypeNames)
                ? new List<MissingTypeNameLocation>()
                : Find(assetPath, lines);
        }

        // Every scanned file under Assets/, apart from Excluded scan folders. Null when the progress bar is cancelled,
        // since a partial list would pass for a complete one.
        public static List<MissingTypeNameLocation> ScanProject()
        {
            ClearCache();

            var result = new List<MissingTypeNameLocation>();
            var paths = AssetDatabase.GetAllAssetPaths().Where(SerializeReferenceHelpers.IsScanCandidate).ToArray();

            try
            {
                for (var i = 0; i < paths.Length; i++)
                {
                    if (EditorUtility.DisplayCancelableProgressBar(
                            "Scanning Type Names",
                            $"{paths[i]}  ({i + 1}/{paths.Length})",
                            (float)i / Math.Max(1, paths.Length)))
                    {
                        return null;
                    }

                    result.AddRange(FindInFile(paths[i]));
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            return result;
        }

        // Re-reads the given files and swaps their entries in an earlier result; every other file keeps its entries.
        public static List<MissingTypeNameLocation> Rescan(IEnumerable<MissingTypeNameLocation> cached, IEnumerable<string> assetPaths)
        {
            ClearCache();

            var paths = new HashSet<string>(assetPaths, StringComparer.Ordinal);
            var result = cached.Where(location => !paths.Contains(location.AssetPath)).ToList();

            foreach (var path in paths.Where(SerializeReferenceHelpers.IsScanCandidate))
                result.AddRange(FindInFile(path));

            return result;
        }

        // "Ns.Box`1[[Ns.T, A, Version=…]], A, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null" -> "Ns.Box`1[[Ns.T, A, Version=…]], A":
        // the identity a group shares, whatever version the name was written with.
        public static string GroupKey(string typeName)
        {
            var parts = SplitTopLevel(typeName);
            return parts.Count >= 2 ? $"{parts[0]}, {parts[1]}" : typeName?.Trim() ?? string.Empty;
        }

        // The full type name, without the assembly.
        public static string FullName(string typeName)
        {
            var parts = SplitTopLevel(typeName);
            return parts.Count > 0 ? parts[0] : string.Empty;
        }

        // The class name the way the picker shows it: no namespace, declaring types, arity or generic arguments.
        public static string ShortName(string typeName)
        {
            var fullName = FullName(typeName);
            var bracket = fullName.IndexOf('[');
            if (bracket >= 0) fullName = fullName[..bracket];

            var name = fullName[(Math.Max(fullName.LastIndexOf('.'), fullName.LastIndexOf('+')) + 1)..];
            return TypeUtility.StripArity(name);
        }

        // The picker settings of the wrapper field, read from the type that declares it: the wrapper's T, the
        // [TypeSelector] base types given by name, and its Allow. Base types supplied by a member need the live
        // object, so they are left out. False when the declaring type or the field cannot be found.
        public static bool TryGetFieldConstraint(MissingTypeNameLocation location, out TypeNameConstraint constraint)
        {
            constraint = TypeNameConstraint.Unconstrained;

            var host = GetDeclaringType(location.Entry);
            var field = host is null ? null : FindField(host, location.Entry.FieldPath);
            if (field is null) return false;

            var types = new List<Type>();
            if (SerializableTypeUtility.TryGetBaseType(field.FieldType, out var baseType) && baseType is not null)
                types.Add(baseType);

            var selector = field.GetCustomAttribute<TypeSelectorAttribute>(inherit: true);
            foreach (var name in selector?.AssemblyQualifiedNames ?? Array.Empty<string>())
            {
                if (TypeUtility.GetTypeOrNull(assemblyQualifiedName: name) is { } type)
                    types.Add(type);
            }

            if (types.Count > 1) types.Remove(typeof(object));

            constraint = new TypeNameConstraint(
                types.ToArray(),
                selector?.Allow ?? TypeAllow.All,
                requiresScript: location.Entry.HasScriptField || SerializableMonoScriptUtility.IsMonoScriptWrapperField(field.FieldType),
                excludeEditorOnly: !TypeUtility.IsEditorOnlyAssembly(host.Assembly));
            return true;
        }

        // The class whose field holds the wrapper: the managed reference type inside a reference's data, the m_Script
        // class of a MonoBehaviour or ScriptableObject, or the overridden component of a prefab instance override.
        private static Type GetDeclaringType(StoredTypeNameEntry entry)
        {
            if (!entry.HostReferenceType.IsEmpty)
                return ResolveManagedType(entry.HostReferenceType);

            if (entry.IsOverride)
                return GetOverrideTargetType(entry.TargetGuid, entry.TargetFileId);

            return GetScriptClass(entry.HostScriptGuid, entry.HostScriptFileId);
        }

        private static Type ResolveManagedType(ManagedTypeName name)
        {
            var className = name.Class.Replace('/', '+');
            var fullName = string.IsNullOrEmpty(name.Namespace) ? className : $"{name.Namespace}.{className}";
            return TypeUtility.GetTypeOrNull(assemblyQualifiedName: string.IsNullOrEmpty(name.Assembly) ? fullName : $"{fullName}, {name.Assembly}");
        }

        private static Type GetOverrideTargetType(string guid, long fileId)
        {
            if (string.IsNullOrEmpty(guid)) return null;
            if (_overrideTargets.TryGetValue((guid, fileId), out var cached)) return cached;

            var target = FindSubAsset(guid, fileId);
            var type = target != null ? target.GetType() : null;

            _overrideTargets[(guid, fileId)] = type;
            return type;
        }

        private static Object FindSubAsset(string guid, long fileId)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(path)) return null;

            foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(path))
            {
                if (asset == null) continue;
                if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(asset, out _, out long localId) && localId == fileId)
                    return asset;
            }

            return null;
        }

        // Walks Unity's property path ("_waves.Array.data[0]._enemies") over declared fields; returns the last field,
        // which is the wrapper or the list of wrappers.
        private static FieldInfo FindField(Type host, string fieldPath)
        {
            if (string.IsNullOrEmpty(fieldPath)) return null;

            var segments = fieldPath.Split('.');
            var current = host;
            FieldInfo field = null;

            for (var i = 0; i < segments.Length; i++)
            {
                if (segments[i] == "Array" && i + 1 < segments.Length && segments[i + 1].StartsWith("data[", StringComparison.Ordinal))
                {
                    current = GetElementType(current);
                    i++;
                    continue;
                }

                field = GetFieldFromHierarchy(current, segments[i]);
                if (field is null) return null;

                current = field.FieldType;
            }

            return field;
        }

        private static FieldInfo GetFieldFromHierarchy(Type type, string name)
        {
            for (var current = type; current is not null && current != typeof(object); current = current.BaseType)
            {
                var field = current.GetField(name, FieldFlags);
                if (field is not null) return field;
            }

            return null;
        }

        private static Type GetElementType(Type type)
        {
            if (type is null) return null;
            if (type.IsArray) return type.GetElementType();

            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>)
                ? type.GetGenericArguments()[0]
                : null;
        }

        // Splits an assembly-qualified name at its top-level commas; the generic arguments' own commas sit in brackets.
        private static List<string> SplitTopLevel(string typeName)
        {
            var parts = new List<string>();
            if (string.IsNullOrWhiteSpace(typeName)) return parts;

            var depth = 0;
            var start = 0;

            for (var i = 0; i < typeName.Length; i++)
            {
                switch (typeName[i])
                {
                    case '[': depth++; break;
                    case ']': depth--; break;
                    case ',' when depth == 0:
                        parts.Add(typeName[start..i].Trim());
                        start = i + 1;
                        break;
                }
            }

            parts.Add(typeName[start..].Trim());
            return parts;
        }
    }
}
