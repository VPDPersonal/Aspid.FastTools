using System;
using UnityEditor;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    // The SerializeReference settings that must be the same for every teammate and for CI: the gate severity, the
    // auto-de-alias behavior and the excluded scan folders. Persisted as a YAML asset under ProjectSettings/ so the
    // values are committed and travel to a clean CI runner, unlike the per-machine EditorPrefs behind the rest.
    [FilePath("ProjectSettings/SerializeReferenceSharedSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    internal sealed class SerializeReferenceSharedSettings : ScriptableSingleton<SerializeReferenceSharedSettings>
    {
        [Tooltip("How a build and a CI run react to missing or unset-required managed references.")]
        [SerializeField] private GateSeverity _buildSeverity = GateSeverity.Warn;

        [Tooltip("Give a duplicated list element its own instance instead of sharing the original's rid.")]
        [SerializeField] private bool _autoDeAlias = true;

        [Tooltip("Project folders the reference scans skip.")]
        [SerializeField] private string[] _excludedFolders = Array.Empty<string>();

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
                _excludedFolders = value ?? Array.Empty<string>();
                Save(saveAsText: true);
            }
        }
    }
}
