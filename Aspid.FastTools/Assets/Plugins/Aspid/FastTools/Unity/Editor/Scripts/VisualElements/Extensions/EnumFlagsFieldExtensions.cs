using System;
using UnityEditor.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Editors
{
    public static class EnumFlagsFieldExtensions
    {
        public static T Initialize<T>(this T element, Enum defaultValue, bool includeObsoleteValues = false)
            where T : EnumFlagsField
        {
            element.Init(defaultValue, includeObsoleteValues);
            return element;
        }
    }
}