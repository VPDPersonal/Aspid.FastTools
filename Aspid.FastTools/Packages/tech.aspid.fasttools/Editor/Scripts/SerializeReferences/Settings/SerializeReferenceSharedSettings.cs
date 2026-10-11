using System;
using System.IO;
using UnityEditor;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    [FilePath(RelativePath, FilePathAttribute.Location.ProjectFolder)]
    internal sealed class SerializeReferenceSharedSettings : ScriptableSingleton<SerializeReferenceSharedSettings>
    {
        private const string RelativePath = "ProjectSettings/SerializeReferenceSharedSettings.asset";

        [Tooltip("How a build and a CI run react to missing or unset-required managed references.")]
        [SerializeField] private GateSeverity _buildSeverity = GateSeverity.Warn;

        [Tooltip("Give a duplicated list element its own instance instead of sharing the original's rid.")]
        [SerializeField] private bool _autoDeAlias = true;

        [Tooltip("Project folders the reference scans skip.")]
        [SerializeField] private string[] _excludedFolders = Array.Empty<string>();

        // A pull or a checkout rewrites the file behind the open Editor, and the loaded copy stays old. Destroying the
        // copy makes the next access read the file again. Hold no reference to the instance across this call.
        public static void Reload() => DestroyImmediate(instance);

        // ScriptableSingleton.Save does not check the file out, so a file that version control keeps read-only is never
        // written. Logs the reason and returns false when the file stays read-only; a missing file is writable.
        public static bool TryMakeEditable() =>
            !File.Exists(RelativePath) || SerializeReferenceYamlEditor.TryMakeEditable(RelativePath);

        public GateSeverity BuildSeverity
        {
            get => _buildSeverity;
            set
            {
                if (_buildSeverity == value) return;
                _buildSeverity = value;
                Save(saveAsText: true);
            }
        }

        public bool AutoDeAlias
        {
            get => _autoDeAlias;
            set
            {
                if (_autoDeAlias == value) return;
                _autoDeAlias = value;
                Save(saveAsText: true);
            }
        }

        public string[] ExcludedFolders
        {
            // A defensive copy: mutating the live array would change the asset without a Save, and the facade's
            // equality check would then swallow the follow-up assignment.
            get => _excludedFolders is { Length: > 0 } ? (string[])_excludedFolders.Clone() : Array.Empty<string>();
            set
            {
                _excludedFolders = value is { Length: > 0 } ? (string[])value.Clone() : Array.Empty<string>();
                Save(saveAsText: true);
            }
        }
    }
}
