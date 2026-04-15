using System;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Editors
{
    public static class EnumFieldExtensions
    {
        public static T Initialize<T>(this T element, Enum defaultValue, bool includeObsoleteValues = false)
            where T : EnumField
        {
            element.Init(defaultValue, includeObsoleteValues);
            return element;
        }
    }
}