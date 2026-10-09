using System;
using System.Linq;
using System.Threading;
using System.Reflection;
using System.Collections.Generic;
using UnityEditor.Compilation;
using Assembly = System.Reflection.Assembly;
using AssemblyFlags = UnityEditor.Compilation.AssemblyFlags;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types.Editors
{
    internal static class TypeUtility
    {
        // Most passes SweepDomainTypes makes, so a steady stream of assembly loads cannot loop it forever.
        internal const int MaxDomainSweeps = 8;

        // Counts assembly loads. A cache built at an older count can miss a type that exists now.
        private static int _assemblyLoads;

        private static int _domainTypesLoads;
        private static int _resolvedTypesLoads;
        private static IReadOnlyList<Type> _domainTypes;
        private static Dictionary<string, bool> _compiledAssemblies;
        private static readonly Dictionary<string, Type> _resolvedTypes = new(StringComparer.Ordinal);
        private static readonly Dictionary<Assembly, bool> _editorOnlyAssemblies = new();

        static TypeUtility()
        {
            AppDomain.CurrentDomain.AssemblyLoad += (_, _) => Interlocked.Increment(ref _assemblyLoads);
        }

        internal static IReadOnlyList<Type> DomainTypes
        {
            get
            {
                if (_domainTypes is not null && _domainTypesLoads == Volatile.Read(ref _assemblyLoads))
                    return _domainTypes;

                _domainTypes = SweepDomainTypes(EnumerateDomainTypes, out _domainTypesLoads);
                return _domainTypes;
            }
        }

        // Walking the types can load another assembly, and the walk then misses its types. The walk repeats until a
        // pass loads nothing. loads is the count the last pass started at, so a walk cut off by the cap is redone later.
        internal static Type[] SweepDomainTypes(Func<IEnumerable<Type>> enumerate, out int loads)
        {
            Type[] types;
            var sweeps = 0;

            do
            {
                loads = Volatile.Read(ref _assemblyLoads);
                types = enumerate().ToArray();
            }
            while (loads != Volatile.Read(ref _assemblyLoads) && ++sweeps < MaxDomainSweeps);

            return types;
        }

        // IMGUI asks for the same stored name several times per field on every event. Each name is resolved once, a
        // missing type too; an assembly load can change the answer, so it drops the results.
        internal static Type GetTypeOrNull(string assemblyQualifiedName)
        {
            if (string.IsNullOrWhiteSpace(assemblyQualifiedName)) return null;

            var loads = Volatile.Read(ref _assemblyLoads);

            if (_resolvedTypesLoads != loads)
            {
                _resolvedTypes.Clear();
                _resolvedTypesLoads = loads;
            }

            if (_resolvedTypes.TryGetValue(assemblyQualifiedName, out var cached)) return cached;

            Type type;

            try
            {
                type = Type.GetType(assemblyQualifiedName, throwOnError: false);
            }
            catch (Exception)
            {
                type = null;
            }

            _resolvedTypes[assemblyQualifiedName] = type;
            return type;
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
