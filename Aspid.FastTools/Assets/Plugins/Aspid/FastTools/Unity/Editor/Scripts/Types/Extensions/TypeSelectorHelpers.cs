using System;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types.Editors
{
    internal class TypeSelectorHelpers
    {
        public const string NoneOption = "<None>";
        public const string GlobalNamespace = "<Global>";

        public static string GetTypeSelectorTitle(Type value, string assemblyQualifiedName = null)
        {
            if (assemblyQualifiedName is not null)
                return $"<Missing {assemblyQualifiedName}>";
            
            return value is null ? NoneOption : value.Name;
        }
    }
}
