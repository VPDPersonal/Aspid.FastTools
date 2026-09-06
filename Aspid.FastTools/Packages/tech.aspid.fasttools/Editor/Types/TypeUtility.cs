using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types.Editors
{
    // Editor-side utilities for working with Type names and the loaded type domain,
    // shared by the type-selector infrastructure.
    internal static class TypeUtility
    {
        // Cached once per domain: the picker sweeps every domain type on each open (and the open-generic flow does
        // it again per parameter page), which stalls large projects. Static state is cleared on every domain reload,
        // and an assembly loaded later without a reload invalidates the cache through the AssemblyLoad hook.
        private static IReadOnlyList<Type> _domainTypes;

        static TypeUtility()
        {
            AppDomain.CurrentDomain.AssemblyLoad += (_, _) => _domainTypes = null;
        }

        internal static IReadOnlyList<Type> DomainTypes => _domainTypes ??= EnumerateDomainTypes().ToArray();

        internal static Type GetTypeOrNull(string assemblyQualifiedName)
        {
            if (string.IsNullOrWhiteSpace(assemblyQualifiedName)) return null;

            try
            {
                return Type.GetType(assemblyQualifiedName, throwOnError: false);
            }
            catch (Exception)
            {
                return null;
            }
        }

        // Removes the CLR generic-arity suffix (Modifier`1 → Modifier) from a raw type name.
        // Names without a backtick are returned unchanged.
        internal static string StripArity(string name)
        {
            var tick = name.IndexOf('`');
            return tick >= 0 ? name[..tick] : name;
        }

        // Short display name for a type: generic types are rendered with angle-bracket arguments
        // (Modifier<Single>, nested — Modifier<Modifier<Int32>>)
        // instead of the raw arity form (Modifier`1). Non-generic types are returned unchanged.
        internal static string FormatGenericName(Type type)
        {
            if (!type.IsGenericType) return type.Name;

            var baseName = StripArity(type.Name);
            var arguments = string.Join(", ", type.GetGenericArguments().Select(FormatGenericName));

            return $"{baseName}<{arguments}>";
        }

        // Enumerates every type across all currently loaded assemblies, dropping the entries that fail to load
        // in a partially-loadable assembly (ReflectionTypeLoadException).
        internal static IEnumerable<Type> EnumerateDomainTypes()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;

                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException exception)
                {
                    types = exception.Types.Where(type => type is not null).ToArray();
                }

                foreach (var type in types)
                    yield return type;
            }
        }
    }
}
