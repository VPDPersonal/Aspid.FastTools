using System;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Globalization;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Aspid.FastTools.Editors.Tests
{
    // The public API of assemblies as text: a section per assembly, a block per type, a line per member.
    // Public members count; protected ones count only in a type that a user can derive from.
    internal sealed class PublicApiSnapshot
    {
        private const string Indent = "    ";

        private const BindingFlags DeclaredMembers = BindingFlags.Public | BindingFlags.NonPublic |
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

        private static readonly Dictionary<Type, string> Keywords = new()
        {
            [typeof(void)] = "void",
            [typeof(bool)] = "bool",
            [typeof(byte)] = "byte",
            [typeof(sbyte)] = "sbyte",
            [typeof(char)] = "char",
            [typeof(short)] = "short",
            [typeof(ushort)] = "ushort",
            [typeof(int)] = "int",
            [typeof(uint)] = "uint",
            [typeof(long)] = "long",
            [typeof(ulong)] = "ulong",
            [typeof(float)] = "float",
            [typeof(double)] = "double",
            [typeof(decimal)] = "decimal",
            [typeof(string)] = "string",
            [typeof(object)] = "object",
        };

        // Assembly name → type name → lines. The first line of a type is its declaration.
        private readonly SortedDictionary<string, SortedDictionary<string, List<string>>> _assemblies =
            new(StringComparer.Ordinal);

        public static PublicApiSnapshot Read(IEnumerable<Assembly> assemblies)
        {
            var snapshot = new PublicApiSnapshot();

            foreach (var assembly in assemblies)
            {
                var types = new SortedDictionary<string, List<string>>(StringComparer.Ordinal);

                foreach (var type in assembly.GetTypes().Where(IsVisible))
                    types[FormatType(type)] = DescribeType(type);

                snapshot._assemblies[assembly.GetName().Name] = types;
            }

            return snapshot;
        }

        public static PublicApiSnapshot Parse(string text)
        {
            var snapshot = new PublicApiSnapshot();
            SortedDictionary<string, List<string>> types = null;
            List<string> lines = null;

            foreach (var rawLine in text.Split('\n'))
            {
                var line = rawLine.TrimEnd('\r');
                if (line.Length is 0 || line.StartsWith("#", StringComparison.Ordinal)) continue;

                if (line.StartsWith("[", StringComparison.Ordinal) && line.EndsWith("]", StringComparison.Ordinal))
                {
                    types = new SortedDictionary<string, List<string>>(StringComparer.Ordinal);
                    snapshot._assemblies[line.Substring(startIndex: 1, length: line.Length - 2)] = types;
                    lines = null;
                }
                else if (line.StartsWith(Indent, StringComparison.Ordinal))
                {
                    if (lines is null) throw new FormatException($"A member line has no type: '{line}'.");
                    lines.Add(line.Substring(Indent.Length));
                }
                else
                {
                    if (types is null) throw new FormatException($"A type has no assembly section: '{line}'.");
                    lines = new List<string>();
                    types[line] = lines;
                }
            }

            return snapshot;
        }

        public string Render()
        {
            var builder = new StringBuilder();
            builder.Append("# Public API of tech.aspid.fasttools, written by PublicApiSnapshotTests. Do not edit by hand.\n");
            builder.Append("# Update it as AGENTS.md says: run the test with ASPID_FASTTOOLS_WRITE_PUBLIC_API=1.\n");

            foreach (var assembly in _assemblies)
            {
                builder.Append('\n').Append('[').Append(assembly.Key).Append("]\n");

                foreach (var type in assembly.Value)
                {
                    builder.Append('\n').Append(type.Key).Append('\n');

                    foreach (var line in type.Value)
                        builder.Append(Indent).Append(line).Append('\n');
                }
            }

            return builder.ToString();
        }

        // Each entry is "[Assembly] Type: line", so equal lines of different types stay apart.
        public HashSet<string> GetEntries(ICollection<string> excludedAssemblies)
        {
            var entries = new HashSet<string>(StringComparer.Ordinal);

            foreach (var assembly in _assemblies.Where(assembly => !excludedAssemblies.Contains(assembly.Key)))
            {
                foreach (var type in assembly.Value)
                {
                    foreach (var line in type.Value)
                        entries.Add($"[{assembly.Key}] {type.Key}: {line}");
                }
            }

            return entries;
        }

        public void CopyAssembly(PublicApiSnapshot source, string assemblyName)
        {
            if (source._assemblies.TryGetValue(assemblyName, out var types))
                _assemblies[assemblyName] = types;
        }

        private static bool IsVisible(Type type)
        {
            // Generated types are Unity's API, not ours: the UxmlSerializedData that the [UxmlElement] generator nests
            // in a public element is internal on 6000.0 and public on 6000.4.
            if (type.Name.IndexOf('<') >= 0 || type.IsDefined(typeof(CompilerGeneratedAttribute), inherit: false))
                return false;

            if (!type.IsNested) return type.IsPublic;
            if (!IsVisible(type.DeclaringType)) return false;

            return type.IsNestedPublic
                || ((type.IsNestedFamily || type.IsNestedFamORAssem) && !type.DeclaringType.IsSealed);
        }

        private static bool IsVisible(MethodBase method) =>
            method is not null
            && (method.IsPublic || ((method.IsFamily || method.IsFamilyOrAssembly) && !method.DeclaringType.IsSealed));

        private static bool IsVisible(FieldInfo field) =>
            field.IsPublic || ((field.IsFamily || field.IsFamilyOrAssembly) && !field.DeclaringType.IsSealed);

        private static bool IsDelegate(Type type) =>
            type.BaseType == typeof(MulticastDelegate);

        private static List<string> DescribeType(Type type)
        {
            var members = new List<(string Name, string Line)>();

            if (type.IsEnum)
            {
                members.AddRange(type.GetFields(BindingFlags.Public | BindingFlags.Static)
                    .Select(field => (field.Name, $"{field.Name} = {FormatConstant(field.GetRawConstantValue(), type)}")));
            }
            else if (!IsDelegate(type))
            {
                members.AddRange(DescribeMembers(type));
            }

            var lines = members
                .OrderBy(member => member.Name, StringComparer.Ordinal)
                .ThenBy(member => member.Line, StringComparer.Ordinal)
                .Select(member => member.Line)
                .ToList();

            lines.Insert(index: 0, DescribeDeclaration(type));
            return lines;
        }

        private static string DescribeDeclaration(Type type)
        {
            var access = type.IsPublic || type.IsNestedPublic ? "public" : "protected";
            var genericParameters = GetOwnGenericParameters(type);
            var name = StripArity(type.Name) + FormatGenericParameters(genericParameters);

            if (IsDelegate(type))
            {
                var invoke = type.GetMethod("Invoke");
                return $"{access} delegate {FormatReturnType(invoke)} {name}({FormatParameters(invoke)}){FormatConstraints(genericParameters)}";
            }

            if (type.IsEnum)
            {
                var flags = type.IsDefined(typeof(FlagsAttribute), inherit: false) ? "[Flags] " : string.Empty;
                var underlyingType = Enum.GetUnderlyingType(type);
                var bases = underlyingType == typeof(int) ? string.Empty : $" : {FormatType(underlyingType)}";
                return $"{flags}{access} enum {name}{bases}";
            }

            string kind;
            var modifiers = string.Empty;
            var baseTypes = new List<string>();

            if (type.IsInterface)
            {
                kind = "interface";
            }
            else if (type.IsValueType)
            {
                kind = "struct";
            }
            else
            {
                kind = "class";
                modifiers = type.IsAbstract && type.IsSealed ? "static " : type.IsAbstract ? "abstract " : type.IsSealed ? "sealed " : string.Empty;
                if (type.BaseType != typeof(object)) baseTypes.Add(FormatType(type.BaseType));
            }

            // Only the interfaces the type adds: the ones of a Unity base class differ between Unity versions.
            var inheritedInterfaces = type.BaseType?.GetInterfaces() ?? Type.EmptyTypes;
            baseTypes.AddRange(type.GetInterfaces()
                .Except(inheritedInterfaces)
                .Where(@interface => @interface.IsVisible)
                .Select(FormatType)
                .OrderBy(@interface => @interface, StringComparer.Ordinal));

            var baseList = baseTypes.Count is 0 ? string.Empty : $" : {string.Join(", ", baseTypes)}";
            return $"{access} {modifiers}{kind} {name}{baseList}{FormatConstraints(genericParameters)}";
        }

        private static IEnumerable<(string Name, string Line)> DescribeMembers(Type type)
        {
            foreach (var constructor in type.GetConstructors(DeclaredMembers).Where(IsVisible))
                yield return (string.Empty, $"{FormatAccess(constructor)} {StripArity(type.Name)}({FormatParameters(constructor)})");

            foreach (var field in type.GetFields(DeclaredMembers).Where(field => IsVisible(field) && !field.IsSpecialName))
            {
                var modifiers = field.IsLiteral
                    ? "const "
                    : (field.IsStatic ? "static " : string.Empty) + (field.IsInitOnly ? "readonly " : string.Empty);
                var value = field.IsLiteral ? $" = {FormatConstant(field.GetRawConstantValue(), field.FieldType)}" : string.Empty;

                yield return (field.Name, $"{FormatAccess(field)} {modifiers}{FormatType(field.FieldType)} {field.Name}{value}");
            }

            foreach (var property in type.GetProperties(DeclaredMembers))
            {
                var getter = property.GetGetMethod(nonPublic: true);
                var setter = property.GetSetMethod(nonPublic: true);
                if (!IsVisible(getter)) getter = null;
                if (!IsVisible(setter)) setter = null;
                if (getter is null && setter is null) continue;

                var main = getter is { IsPublic: true } || setter is null ? getter : setter;
                var access = FormatAccess(main);
                var accessors = new List<string>();
                if (getter is not null) accessors.Add(FormatAccessor(getter, access, "get;"));
                if (setter is not null) accessors.Add(FormatAccessor(setter, access, "set;"));

                var indexParameters = property.GetIndexParameters();
                var name = indexParameters.Length is 0
                    ? property.Name
                    : $"this[{string.Join(", ", indexParameters.Select(parameter => FormatParameter(parameter, isThis: false)))}]";

                yield return (property.Name,
                    $"{access} {FormatModifiers(main)}{FormatType(property.PropertyType)} {name} {{ {string.Join(" ", accessors)} }}");
            }

            foreach (var @event in type.GetEvents(DeclaredMembers))
            {
                var add = @event.GetAddMethod(nonPublic: true);
                if (!IsVisible(add)) continue;

                yield return (@event.Name,
                    $"{FormatAccess(add)} {FormatModifiers(add)}event {FormatType(@event.EventHandlerType)} {@event.Name}");
            }

            foreach (var method in type.GetMethods(DeclaredMembers).Where(IsVisible))
            {
                // Accessors are listed with their property or event; operators stay as op_* methods.
                if (method.IsSpecialName && !method.Name.StartsWith("op_", StringComparison.Ordinal)) continue;

                var genericParameters = method.IsGenericMethodDefinition ? method.GetGenericArguments() : Type.EmptyTypes;
                yield return (method.Name,
                    $"{FormatAccess(method)} {FormatModifiers(method)}{FormatReturnType(method)} {method.Name}" +
                    $"{FormatGenericParameters(genericParameters)}({FormatParameters(method)}){FormatConstraints(genericParameters)}");
            }
        }

        private static string FormatAccess(MethodBase method) =>
            method.IsPublic ? "public" : "protected";

        private static string FormatAccess(FieldInfo field) =>
            field.IsPublic ? "public" : "protected";

        private static string FormatAccessor(MethodBase accessor, string propertyAccess, string keyword)
        {
            var access = FormatAccess(accessor);
            return access == propertyAccess ? keyword : $"{access} {keyword}";
        }

        private static string FormatModifiers(MethodBase method)
        {
            if (method.IsStatic) return "static ";
            if (!method.IsVirtual || method.DeclaringType.IsInterface) return string.Empty;

            var isOverride = (method.Attributes & MethodAttributes.VtableLayoutMask) == MethodAttributes.ReuseSlot;
            if (method.IsAbstract) return isOverride ? "abstract override " : "abstract ";
            if (isOverride) return method.IsFinal ? "sealed override " : "override ";

            // A new slot that is final is a non-virtual interface implementation.
            return method.IsFinal ? string.Empty : "virtual ";
        }

        private static string FormatReturnType(MethodInfo method) =>
            method.ReturnType.IsByRef ? $"ref {FormatType(method.ReturnType.GetElementType())}" : FormatType(method.ReturnType);

        private static string FormatParameters(MethodBase method)
        {
            var isExtension = method.IsDefined(typeof(ExtensionAttribute), inherit: false);
            return string.Join(", ", method.GetParameters().Select((parameter, index) =>
                FormatParameter(parameter, isThis: isExtension && index is 0)));
        }

        private static string FormatParameter(ParameterInfo parameter, bool isThis)
        {
            var builder = new StringBuilder();
            if (isThis) builder.Append("this ");

            var type = parameter.ParameterType;

            if (type.IsByRef)
            {
                builder.Append(parameter.IsOut ? "out " : parameter.IsIn ? "in " : "ref ");
                type = type.GetElementType();
            }
            else if (parameter.IsDefined(typeof(ParamArrayAttribute), inherit: false))
            {
                builder.Append("params ");
            }

            builder.Append(FormatType(type)).Append(' ').Append(parameter.Name);

            if (parameter.HasDefaultValue)
                builder.Append(" = ").Append(FormatConstant(parameter.RawDefaultValue, type));

            return builder.ToString();
        }

        private static string FormatConstant(object value, Type type) => value switch
        {
            null => type.IsValueType && Nullable.GetUnderlyingType(type) is null ? "default" : "null",
            string text => $"\"{text.Replace("\\", "\\\\").Replace("\"", "\\\"")}\"",
            char symbol => $"'{symbol}'",
            bool flag => flag ? "true" : "false",
            float number => number.ToString("R", CultureInfo.InvariantCulture),
            double number => number.ToString("R", CultureInfo.InvariantCulture),
            IFormattable formattable => formattable.ToString(format: null, CultureInfo.InvariantCulture),
            _ => value.ToString(),
        };

        private static Type[] GetOwnGenericParameters(Type type)
        {
            var parameters = type.GetGenericArguments();
            var inherited = type.IsNested ? type.DeclaringType.GetGenericArguments().Length : 0;
            return parameters.Skip(inherited).ToArray();
        }

        private static string FormatGenericParameters(Type[] parameters) =>
            parameters.Length is 0 ? string.Empty : $"<{string.Join(", ", parameters.Select(FormatGenericParameter))}>";

        private static string FormatGenericParameter(Type parameter) =>
            (parameter.GenericParameterAttributes & GenericParameterAttributes.VarianceMask) switch
            {
                GenericParameterAttributes.Covariant => $"out {parameter.Name}",
                GenericParameterAttributes.Contravariant => $"in {parameter.Name}",
                _ => parameter.Name,
            };

        private static string FormatConstraints(IEnumerable<Type> parameters)
        {
            var builder = new StringBuilder();

            foreach (var parameter in parameters)
            {
                var attributes = parameter.GenericParameterAttributes;
                var isStruct = (attributes & GenericParameterAttributes.NotNullableValueTypeConstraint) != 0;
                var constraints = new List<string>();

                if ((attributes & GenericParameterAttributes.ReferenceTypeConstraint) != 0) constraints.Add("class");
                if (isStruct) constraints.Add("struct");

                constraints.AddRange(parameter.GetGenericParameterConstraints()
                    .Where(constraint => !(isStruct && constraint == typeof(ValueType)))
                    .OrderBy(constraint => constraint.IsInterface)
                    .ThenBy(FormatType, StringComparer.Ordinal)
                    .Select(FormatType));

                if (!isStruct && (attributes & GenericParameterAttributes.DefaultConstructorConstraint) != 0)
                    constraints.Add("new()");

                if (constraints.Count > 0)
                    builder.Append(" where ").Append(parameter.Name).Append(" : ").Append(string.Join(", ", constraints));
            }

            return builder.ToString();
        }

        // Full names, so a type that moves to another namespace shows up in every signature that uses it.
        private static string FormatType(Type type)
        {
            if (type.IsGenericParameter) return type.Name;
            if (type.IsByRef) return FormatType(type.GetElementType());
            if (type.IsPointer) return $"{FormatType(type.GetElementType())}*";
            if (type.IsArray) return $"{FormatType(type.GetElementType())}[{new string(',', type.GetArrayRank() - 1)}]";
            if (Keywords.TryGetValue(type, out var keyword)) return keyword;

            var underlyingType = Nullable.GetUnderlyingType(type);
            if (underlyingType is not null) return $"{FormatType(underlyingType)}?";

            var definition = type.IsGenericType ? type.GetGenericTypeDefinition() : type;
            return FormatTypeName(definition, type.GetGenericArguments());
        }

        // A nested type carries the generic arguments of its declaring types first.
        private static string FormatTypeName(Type definition, Type[] arguments)
        {
            var inherited = 0;
            string prefix;

            if (definition.IsNested)
            {
                var declaringType = definition.DeclaringType;
                inherited = declaringType.GetGenericArguments().Length;
                prefix = $"{FormatTypeName(declaringType, arguments)}.";
            }
            else
            {
                prefix = string.IsNullOrEmpty(definition.Namespace) ? string.Empty : $"{definition.Namespace}.";
            }

            var count = definition.GetGenericArguments().Length;
            var name = prefix + StripArity(definition.Name);
            if (count <= inherited) return name;

            var own = arguments.Skip(inherited).Take(count - inherited).Select(FormatType);
            return $"{name}<{string.Join(", ", own)}>";
        }

        private static string StripArity(string name)
        {
            var index = name.IndexOf('`');
            return index < 0 ? name : name.Substring(startIndex: 0, length: index);
        }
    }
}
