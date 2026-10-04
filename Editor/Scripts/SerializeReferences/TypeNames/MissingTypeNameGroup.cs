using System;
using System.IO;
using System.Linq;
using UnityEditor;
using Aspid.FastTools.Types;
using System.Collections.Generic;
using Aspid.FastTools.Types.Editors;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    // The fields that store one missing type name, whatever version the name was written with.
    internal sealed class MissingTypeNameGroup
    {
        public readonly string TypeName;
        public readonly List<MissingTypeNameLocation> Entries = new();

        private TypeNameConstraint? _constraint;
        private bool _mixedConstraints;

        private MissingTypeNameGroup(string typeName)
        {
            TypeName = typeName;
        }

        public string DisplayName => MissingTypeNames.FullName(TypeName);

        public string ShortName => MissingTypeNames.ShortName(TypeName);

        public int FileCount => Entries.Select(entry => entry.AssetPath).Distinct(StringComparer.Ordinal).Count();

        public static List<MissingTypeNameGroup> Build(IEnumerable<MissingTypeNameLocation> locations)
        {
            var byName = new Dictionary<string, MissingTypeNameGroup>(StringComparer.Ordinal);

            foreach (var location in locations)
            {
                var key = MissingTypeNames.GroupKey(location.Entry.TypeName);
                if (!byName.TryGetValue(key, out var group))
                {
                    group = new MissingTypeNameGroup(location.Entry.TypeName);
                    byName.Add(key, group);
                }

                group.Entries.Add(location);
            }

            return byName.Values
                .OrderBy(group => group.ShortName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(group => group.DisplayName, StringComparer.Ordinal)
                .ToList();
        }

        // The picker settings every entry's field shares; when the fields disagree or one cannot be read, the picker
        // is unconstrained apart from what all entries need (a script for SerializableMonoScript fields).
        public TypeNameConstraint ResolveConstraint(out bool mixed)
        {
            if (_constraint is { } cached)
            {
                mixed = _mixedConstraints;
                return cached;
            }

            TypeNameConstraint? shared = null;
            mixed = false;

            foreach (var entry in Entries)
            {
                if (!MissingTypeNames.TryGetFieldConstraint(entry, out var constraint))
                {
                    mixed = true;
                    break;
                }

                if (shared is null) shared = constraint;
                else if (!shared.Value.Equals(constraint))
                {
                    mixed = true;
                    break;
                }
            }

            var result = !mixed && shared is { } agreed
                ? agreed
                : new TypeNameConstraint(
                    new[] { typeof(object) },
                    TypeAllow.All,
                    requiresScript: Entries.All(entry => entry.Entry.HasScriptField),
                    excludeEditorOnly: true);

            _constraint = result;
            _mixedConstraints = mixed;
            return result;
        }

        // The one compatible type with the same class name, as the Inspector's → suggestion offers it.
        public bool TryGetSuggestion(TypeNameConstraint constraint, out Type suggestion)
        {
            suggestion = TypeMissingRepair.GetSuggestion(
                storedName: TypeName,
                types: constraint.Types,
                allow: constraint.Allow,
                excludeEditorOnly: constraint.ExcludeEditorOnly,
                predicate: constraint.RequiresScript ? SerializableMonoScriptUtility.HasScript : null);

            return suggestion is not null;
        }
    }

    // Rewrites stored type names in the asset files. Each file is written once and reimported.
    internal static class MissingTypeNameRepair
    {
        public static List<MissingTypeNameLocation> FilterWritable(IReadOnlyList<MissingTypeNameLocation> source, out int skipped)
        {
            var prefabStagePath = SerializeReferenceOpenCopyGuard.CurrentPrefabStagePath();
            var writable = new List<MissingTypeNameLocation>(source.Count);
            skipped = 0;

            foreach (var entry in source)
            {
                if (SerializeReferenceOpenCopyGuard.IsRewriteSafe(entry.AssetPath, prefabStagePath)) writable.Add(entry);
                else skipped++;
            }

            return writable;
        }

        // Stores `newType` in every entry; null clears the name. A SerializableMonoScript field gets the type's script
        // too, or a null script when the type has none.
        public static int Rewrite(IReadOnlyList<MissingTypeNameLocation> entries, Type newType, string progressTitle)
        {
            var newName = newType?.AssemblyQualifiedName ?? string.Empty;
            var newScript = FormatScriptOf(newType);

            return RunBatch(entries, progressTitle, entry =>
                new TypeNameEdit(entry.Entry, newName, entry.Entry.HasScriptField ? newScript : null));
        }

        // Puts back the name and script each entry had before a Rewrite to `appliedName`. An entry that no longer
        // stores `appliedName` is left alone.
        public static int Revert(IReadOnlyList<MissingTypeNameLocation> entries, string appliedName, string progressTitle) =>
            RunBatch(entries, progressTitle, entry =>
                new TypeNameEdit(
                    entry.Entry.WithTypeName(appliedName),
                    entry.Entry.TypeName,
                    entry.Entry.HasScriptField
                        ? SerializeReferenceYamlEditor.FormatScriptReference(entry.Entry.ScriptFileId, entry.Entry.ScriptGuid)
                        : null));

        public static int CountFiles(IEnumerable<MissingTypeNameLocation> entries) =>
            entries.Select(entry => entry.AssetPath).Distinct(StringComparer.Ordinal).Count();

        private static string FormatScriptOf(Type type)
        {
            if (type is null || !SerializableMonoScriptUtility.TryGetScript(type, out var script) || script == null)
                return SerializeReferenceYamlEditor.FormatScriptReference(0, null);

            return AssetDatabase.TryGetGUIDAndLocalFileIdentifier(script, out var guid, out long fileId)
                ? SerializeReferenceYamlEditor.FormatScriptReference(fileId, guid)
                : SerializeReferenceYamlEditor.FormatScriptReference(0, null);
        }

        private static int RunBatch(
            IReadOnlyList<MissingTypeNameLocation> entries, string progressTitle, Func<MissingTypeNameLocation, TypeNameEdit> edit)
        {
            var byFile = entries.GroupBy(entry => entry.AssetPath, StringComparer.Ordinal).ToArray();
            var applied = 0;

            AssetDatabase.StartAssetEditing();
            try
            {
                for (var i = 0; i < byFile.Length; i++)
                {
                    var file = byFile[i];
                    EditorUtility.DisplayProgressBar(progressTitle, $"{file.Key}  ({i + 1}/{byFile.Length})", (float)i / byFile.Length);

                    if (File.Exists(file.Key) && !SerializeReferenceYamlEditor.TryMakeEditable(file.Key)) continue;

                    var count = SerializeReferenceYamlEditor.RewriteTypeNames(file.Key, file.Select(edit).ToArray());
                    if (count == 0) continue;

                    applied += count;
                    AssetDatabase.ImportAsset(file.Key, ImportAssetOptions.ForceUpdate);
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                EditorUtility.ClearProgressBar();
            }

            return applied;
        }
    }
}
