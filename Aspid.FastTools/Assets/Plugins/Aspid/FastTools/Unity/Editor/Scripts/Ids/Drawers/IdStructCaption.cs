#nullable enable

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Ids.Editors
{
    /// <summary>
    /// Pure caption logic for the IdStruct drawer's dropdown button.
    /// Decides what to display for a given (id, stored-name, registry) triple,
    /// including the &lt;Missing&gt; state when a string-mapped entry is gone.
    /// Extracted so it can be unit-tested without spinning up a UIToolkit panel.
    /// </summary>
    internal static class IdStructCaption
    {
        /// <summary>
        /// Builds the dropdown caption.
        /// </summary>
        /// <param name="id">The current integer id (source of truth).</param>
        /// <param name="storedName">The name carried in the editor-only <c>__stringId</c> companion field; may be stale or empty.</param>
        /// <param name="registry">The registry currently bound to the field type, or <c>null</c> if none.</param>
        /// <param name="resolvedName">When this method returns, contains the registry-authoritative name when available; otherwise <paramref name="storedName"/>.</param>
        /// <param name="isMissing">When this method returns, <c>true</c> if the int id has no matching entry in a string-mapped registry (or the registry was deleted).</param>
        /// <returns>The caption text to render in the drawer.</returns>
        public static string Build(int id, string? storedName, IdRegistryBase? registry, out string resolvedName, out bool isMissing)
        {
            var name = storedName ?? string.Empty;
            var isIntOnly = registry is IdRegistry;
            var stringMapped = registry as StringIdRegistry;

            string? registryName = null;
            var hasName = stringMapped != null
                          && id > 0
                          && stringMapped.TryGetName(id, out registryName);

            if (hasName) name = registryName ?? string.Empty;

            resolvedName = name;
            isMissing = !isIntOnly && id > 0 && !hasName;

            if (isMissing)
                return string.IsNullOrEmpty(name) ? $"<Missing id {id}>" : $"<Missing '{name}'>";

            return string.IsNullOrEmpty(name) ? Constants.NoneOption : name;
        }
    }
}
