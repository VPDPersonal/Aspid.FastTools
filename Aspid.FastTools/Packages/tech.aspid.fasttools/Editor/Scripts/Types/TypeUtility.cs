using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEditor.Compilation;
using System.Runtime.CompilerServices;
using Assembly = System.Reflection.Assembly;
using AssemblyFlags = UnityEditor.Compilation.AssemblyFlags;

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
        // A nested type lists only its own arguments: those of its outer types stay in their names.
        internal static string FormatGenericName(Type type) =>
            type.IsGenericType ? FormatWithArguments(type, type.GetGenericArguments()) : type.Name;

        // "Outer.Inner" for the types enclosing a nested type, each with its generic arguments; null for a top-level type.
        internal static string FormatDeclaringName(Type type)
        {
            // The declaring type of a constructed nested type is the open definition, so an outer type takes the
            // leading arguments of the nested one, which carries those of every outer type before its own.
            var arguments = type.IsGenericType ? type.GetGenericArguments() : Type.EmptyTypes;
            string name = null;

            for (var declaring = type.DeclaringType; declaring is not null; declaring = declaring.DeclaringType)
            {
                var count = declaring.IsGenericType ? declaring.GetGenericArguments().Length : 0;
                var formatted = count is 0 || count > arguments.Length
                    ? FormatGenericName(declaring)
                    : FormatWithArguments(declaring, arguments.Take(count).ToArray());

                name = name is null ? formatted : $"{formatted}.{name}";
            }

            return name;
        }

        // Namespace.Outer.Name without the assembly part.
        internal static string FormatQualifiedName(Type type)
        {
            var name = FormatGenericName(type);
            var declaringName = FormatDeclaringName(type);

            if (declaringName is not null)
                name = $"{declaringName}.{name}";

            return string.IsNullOrEmpty(type.Namespace) ? name : $"{type.Namespace}.{name}";
        }

        // A type the compiler synthesized, or one nested in such a type: the compiler's static-data holder
        // <PrivateImplementationDetails> carries __StaticArrayInitTypeSize=12, whose own name has no angle brackets.
        // The attribute is read on the type only: a source generator may mark an ordinary class with it,
        // and the types written inside that class stay selectable.
        internal static bool IsCompilerGenerated(Type type)
        {
            if (type.IsDefined(typeof(CompilerGeneratedAttribute), inherit: false))
                return true;

            for (var current = type; current is not null; current = current.DeclaringType)
            {
                if (current.Name.Contains('<') || current.Name.Contains('>'))
                    return true;
            }

            return false;
        }

        private static string FormatWithArguments(Type type, Type[] arguments)
        {
            var baseName = StripArity(type.Name);
            var ownCount = GetOwnArgumentCount(type, arguments.Length);

            if (ownCount is 0) return baseName;

            var own = arguments.Skip(arguments.Length - ownCount).Select(FormatGenericName);
            return $"{baseName}<{string.Join(", ", own)}>";
        }

        // A nested type's generic arguments start with those of its outer types, while the arity in its name
        // counts only its own.
        private static int GetOwnArgumentCount(Type type, int total)
        {
            if (!type.IsNested) return total;

            var tick = type.Name.IndexOf('`');
            if (tick < 0) return 0;

            return int.TryParse(type.Name[(tick + 1)..], out var count) ? Math.Min(count, total) : total;
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
