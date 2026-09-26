using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEditor.Compilation;
using Assembly = System.Reflection.Assembly;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types.Editors
{
    internal static class TypeUtility
    {
        private static IReadOnlyList<Type> _domainTypes;
        private static Dictionary<string, bool> _compiledAssemblies;
        private static readonly Dictionary<Assembly, bool> _editorOnlyAssemblies = new();

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

        // Whether a player build leaves the assembly out, so a type from it cannot be resolved there. Assemblies Unity
        // compiles answer by their Editor flag (asmdefs limited to the Editor, Editor folders); precompiled ones are
        // editor-only when they are UnityEditor itself or reference it. UnityEngine modules are always runtime.
        internal static bool IsEditorOnlyAssembly(Assembly assembly)
        {
            if (assembly is null) return false;
            if (_editorOnlyAssemblies.TryGetValue(assembly, out var cached)) return cached;

            var name = assembly.GetName().Name;

            _compiledAssemblies ??= CompilationPipeline.GetAssemblies(AssembliesType.Editor)
                .GroupBy(compiled => compiled.name)
                .ToDictionary(group => group.Key, group => group.Any(compiled =>
                    (compiled.flags & AssemblyFlags.EditorAssembly) != 0));

            var editorOnly = _compiledAssemblies.TryGetValue(name, out var flagged)
                ? flagged
                : !name.StartsWith("UnityEngine", StringComparison.Ordinal) &&
                  (IsUnityEditorAssemblyName(name) ||
                   assembly.GetReferencedAssemblies().Any(reference => IsUnityEditorAssemblyName(reference.Name)));

            _editorOnlyAssemblies[assembly] = editorOnly;
            return editorOnly;
        }

        private static bool IsUnityEditorAssemblyName(string name) =>
            name is not null && name.StartsWith("UnityEditor", StringComparison.Ordinal);

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
