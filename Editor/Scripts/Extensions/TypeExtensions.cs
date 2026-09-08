using System;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Editors
{
    internal static class TypeExtensions
    {
        internal static Type GetCollectionElementTypeOrSelf(this Type type)
        {
            if (type.IsArray) return type.GetElementType();

            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>)
                ? type.GetGenericArguments()[0]
                : type;
        }
    }
}
