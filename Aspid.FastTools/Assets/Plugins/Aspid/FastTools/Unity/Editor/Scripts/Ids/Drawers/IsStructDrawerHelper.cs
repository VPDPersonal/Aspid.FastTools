using Aspid.FastTools.Editors;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Ids.Editors
{
    internal static class IsStructDrawerHelper
    {
        public static string BuildCaption(IsStructDrawerContext ctx, out bool isMissing)
        {
            var registry = ctx.FindRegistry();
            var id = ctx.IntIdProperty.intValue;
            var nameId = ctx.StringIdProperty.stringValue ?? string.Empty; 

            var hasName = registry is not null
                && id > 0
                && registry.TryGetName(id, out nameId);

            nameId ??= string.Empty;
            var hasNotNameId = string.IsNullOrEmpty(nameId);
            isMissing = registry is not null && id > 0 && !hasName;
            
            return isMissing 
                ? hasNotNameId ? $"<Missing id {id}>" : $"<Missing '{nameId}'>"
                : hasNotNameId ? Constants.NoneOption : nameId;
        }
        
        public static void SyncStringFromInt(IsStructDrawerContext ctx)
        {
            var registry = ctx.FindRegistry();
            
            var currentId = ctx.IntIdProperty.intValue;
            if (currentId <= 0 || registry is null) return;

            if (!registry.TryGetName(currentId, out var registryName)) return;
            if (registryName == ctx.StringIdProperty.stringValue) return;

            ctx.StringIdProperty.SetString(registryName);
            ctx.Property.ApplyModifiedProperties();
        }
        
        public static void ApplySelection(
            string selected,
            IsStructDrawerContext ctx)
        {
            var id = 0;
            var nameId = selected ?? string.Empty;
            
            if (!string.IsNullOrEmpty(nameId))
            {
                var registry = ctx.FindRegistry();
                if (registry is not null && registry.TryGetId(nameId, out var foundId))
                    id = foundId;
            }
            
            SetFields(id, nameId, ctx);
        }
        
        private static void SetFields(
            int id,
            string nameId,
            IsStructDrawerContext ctx)
        {
            ctx.IntIdProperty.SetInt(id);
            ctx.StringIdProperty.SetString(nameId);
            ctx.Property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
