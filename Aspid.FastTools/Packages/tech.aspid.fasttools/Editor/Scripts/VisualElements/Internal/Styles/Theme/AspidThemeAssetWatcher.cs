using System;
using System.Linq;
using UnityEditor;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors.Internal
{
    // A deleted or moved sheet can be the picked theme of the project: the windows then fall back to the built-in theme
    // and the Theme dropdown shows the new name, which no setting change would tell them.
    internal sealed class AspidThemeAssetWatcher : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom)
        {
            if (!AspidThemeSettings.HasCustomThemePick) return;
            if (!HasStyleSheet(deleted) && !HasStyleSheet(moved)) return;

            AspidThemeSettings.NotifyChanged();
        }

        private static bool HasStyleSheet(string[] paths) =>
            paths.Any(path => path.EndsWith(".uss", StringComparison.OrdinalIgnoreCase));
    }
}
