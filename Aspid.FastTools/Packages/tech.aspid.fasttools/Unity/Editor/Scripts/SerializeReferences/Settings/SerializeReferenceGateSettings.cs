using UnityEditor;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    /// <summary>
    /// Project-scoped, version-controlled home for the build/CI gate severity. Persisted as a YAML asset under
    /// <c>ProjectSettings/</c> (via <see cref="ScriptableSingleton{T}"/> + <see cref="FilePathAttribute"/>) so the
    /// chosen severity is committed to VCS and travels to a clean CI runner — unlike the per-machine
    /// <see cref="EditorPrefs"/> that back the rest of <see cref="SerializeReferenceSettings"/>. Commit
    /// <c>ProjectSettings/SerializeReferenceGateSettings.asset</c> for the value to reach CI.
    /// </summary>
    [FilePath("ProjectSettings/SerializeReferenceGateSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    internal sealed class SerializeReferenceGateSettings : ScriptableSingleton<SerializeReferenceGateSettings>
    {
        [SerializeField] private GateSeverity _buildSeverity = GateSeverity.Warn;

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
    }
}
