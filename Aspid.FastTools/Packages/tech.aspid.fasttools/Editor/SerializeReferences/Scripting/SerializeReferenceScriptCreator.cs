using System;
using System.IO;
using System.Text;
using System.Linq;
using UnityEditor;
using System.Reflection;
using System.Globalization;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    // Generates and imports a [Serializable] subclass stub for a managed-reference field's base type, so an author
    // can create a subtype without leaving the inspector. An interface base has its members emitted as
    // auto-properties, field-like events and throwing method stubs, so the file compiles.
    internal static class SerializeReferenceScriptCreator
    {
        // Returns the created asset path and the new type's full name, for the deferred assignment.
        public static bool TryCreateSubclassStub(Type baseType, out string assetPath, out string fullTypeName)
        {
            assetPath = null;
            fullTypeName = null;
            if (baseType is null) return false;

            var suggested = "New" + (baseType.IsInterface ? baseType.Name.TrimStart('I') : baseType.Name);
            var path = EditorUtility.SaveFilePanelInProject(
                "Create Managed-Reference Script", suggested, "cs",
                $"Create a new class deriving from {baseType.Name}.");
            if (string.IsNullOrEmpty(path)) return false;

            var className = Path.GetFileNameWithoutExtension(path);
            if (!IsValidClassName(className))
            {
                EditorUtility.DisplayDialog(
                    "Invalid Class Name",
                    $"\"{className}\" is not a valid C# class name. Use a name that starts with a letter or " +
                    "underscore, contains only letters, digits and underscores, and is not a C# keyword.",
                    "OK");
                return false;
            }

            var nspace = baseType.Namespace;

            File.WriteAllText(path, GenerateStub(className, nspace, baseType));
            AssetDatabase.ImportAsset(path);

            assetPath = path;
            fullTypeName = string.IsNullOrEmpty(nspace) ? className : $"{nspace}.{className}";
            return true;
        }

        private static string GenerateStub(string className, string nspace, Type baseType)
        {
            var builder = new StringBuilder();
            builder.AppendLine("using System;");
            builder.AppendLine("using UnityEngine;");
            builder.AppendLine();

            var indent = string.Empty;
            if (!string.IsNullOrEmpty(nspace))
            {
                builder.AppendLine($"namespace {nspace}");
                builder.AppendLine("{");
                indent = "    ";
            }

            builder.AppendLine($"{indent}[Serializable]");
            builder.AppendLine($"{indent}public class {className} : {TypeName(baseType)}");
            builder.AppendLine($"{indent}{{");

            if (baseType.IsInterface)
                AppendInterfaceMembers(builder, indent + "    ", baseType);

            builder.AppendLine($"{indent}}}");

            if (!string.IsNullOrEmpty(nspace)) builder.AppendLine("}");

            return builder.ToString();
        }

        // The interface hierarchy is flattened, so the same signature can arrive from several branches. Each group
        // emits one public implicit member; a duplicate differing only in return type cannot share it and becomes an
        // explicit implementation on its declaring interface instead.
        private static void AppendInterfaceMembers(StringBuilder builder, string indent, Type interfaceType)
        {
            const string body = "throw new NotImplementedException()";

            var members = EnumerateInterfaceMembers(interfaceType).ToArray();
            var events = members.OfType<EventInfo>().ToArray();
            var properties = members.OfType<PropertyInfo>().ToArray();

            var handledAccessors = new HashSet<MethodInfo>();
            foreach (var @event in events)
            {
                if (@event.AddMethod is not null) handledAccessors.Add(@event.AddMethod);
                if (@event.RemoveMethod is not null) handledAccessors.Add(@event.RemoveMethod);
            }
            foreach (var property in properties)
                foreach (var accessor in property.GetAccessors())
                    handledAccessors.Add(accessor);

            foreach (var group in events.GroupBy(@event => @event.Name))
            {
                foreach (var (typeGroup, index) in group.GroupBy(@event => TypeName(@event.EventHandlerType))
                    .Select((typeGroup, index) => (typeGroup, index)))
                {
                    if (index is 0)
                    {
                        builder.AppendLine($"{indent}public event {typeGroup.Key} {group.Key};");
                        continue;
                    }

                    foreach (var @event in typeGroup)
                        builder.AppendLine($"{indent}event {typeGroup.Key} {TypeName(@event.DeclaringType)}.{group.Key} {{ add => {body}; remove => {body}; }}");
                }
            }

            foreach (var group in properties.GroupBy(property => property.Name))
            {
                foreach (var (typeGroup, index) in group.GroupBy(property => TypeName(property.PropertyType))
                    .Select((typeGroup, index) => (typeGroup, index)))
                {
                    if (index is 0)
                    {
                        // An auto-property requires a getter even when the interface declares only a setter.
                        var setter = typeGroup.FirstOrDefault(property => property.CanWrite);
                        var set = setter is null ? string.Empty : IsInitOnly(setter) ? " init;" : " set;";
                        builder.AppendLine($"{indent}public {typeGroup.Key} {group.Key} {{ get;{set} }}");
                        continue;
                    }

                    foreach (var property in typeGroup)
                    {
                        var get = property.CanRead ? $" get => {body};" : string.Empty;
                        var set = property.CanWrite ? $" {(IsInitOnly(property) ? "init" : "set")} => {body};" : string.Empty;
                        builder.AppendLine($"{indent}{typeGroup.Key} {TypeName(property.DeclaringType)}.{group.Key} {{{get}{set} }}");
                    }
                }
            }

            var methods = members.OfType<MethodInfo>()
                .Where(method => !method.IsSpecialName && !handledAccessors.Contains(method)) // property/event accessors
                .ToArray();

            foreach (var group in methods.GroupBy(method =>
                $"{method.Name}({string.Join(",", method.GetParameters().Select(p => p.ParameterType.AssemblyQualifiedName ?? p.ParameterType.Name))})"))
            {
                foreach (var (returnGroup, index) in group.GroupBy(method => TypeName(method.ReturnType))
                    .Select((returnGroup, index) => (returnGroup, index)))
                {
                    if (index is 0)
                    {
                        var method = returnGroup.First();
                        var parameters = string.Join(", ", method.GetParameters().Select(p => $"{TypeName(p.ParameterType)} {p.Name}"));
                        builder.AppendLine($"{indent}public {returnGroup.Key} {method.Name}({parameters}) => {body};");
                        continue;
                    }

                    foreach (var method in returnGroup)
                    {
                        var parameters = string.Join(", ", method.GetParameters().Select(p => $"{TypeName(p.ParameterType)} {p.Name}"));
                        builder.AppendLine($"{indent}{returnGroup.Key} {TypeName(method.DeclaringType)}.{method.Name}({parameters}) => {body};");
                    }
                }
            }
        }

        // C# has no set-only auto-properties, and an init accessor — detected by the IsExternalInit modreq on the
        // setter's return parameter — can only be implemented by another init.
        private static bool IsInitOnly(PropertyInfo property) =>
            property.SetMethod is { ReturnParameter: { } returnParameter }
            && returnParameter.GetRequiredCustomModifiers().Any(modifier => modifier.FullName == "System.Runtime.CompilerServices.IsExternalInit");

        private static IEnumerable<MemberInfo> EnumerateInterfaceMembers(Type interfaceType)
        {
            foreach (var member in interfaceType.GetMembers()) yield return member;
            foreach (var inherited in interfaceType.GetInterfaces())
                foreach (var member in inherited.GetMembers())
                    yield return member;
        }

        // Reserved keywords a type name cannot use verbatim, since they would need a leading '@'.
        private static readonly HashSet<string> _csharpKeywords = new()
        {
            "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked", "class",
            "const", "continue", "decimal", "default", "delegate", "do", "double", "else", "enum", "event",
            "explicit", "extern", "false", "finally", "fixed", "float", "for", "foreach", "goto", "if",
            "implicit", "in", "int", "interface", "internal", "is", "lock", "long", "namespace", "new",
            "null", "object", "operator", "out", "override", "params", "private", "protected", "public",
            "readonly", "ref", "return", "sbyte", "sealed", "short", "sizeof", "stackalloc", "static",
            "string", "struct", "switch", "this", "throw", "true", "try", "typeof", "uint", "ulong",
            "unchecked", "unsafe", "ushort", "using", "virtual", "void", "volatile", "while",
        };

        // Hand-rolled because Unity's .NET Standard profile ships no System.CodeDom.
        private static bool IsValidClassName(string className)
        {
            if (string.IsNullOrEmpty(className)) return false;
            if (_csharpKeywords.Contains(className)) return false;

            var first = className[0];
            if (first != '_' && !char.IsLetter(first)) return false;

            for (var i = 1; i < className.Length; i++)
            {
                var c = className[i];
                if (c == '_' || char.IsLetterOrDigit(c)) continue;

                // Unicode combining and formatting marks are permitted in C# identifiers.
                var category = CharUnicodeInfo.GetUnicodeCategory(c);
                if (category is UnicodeCategory.NonSpacingMark or UnicodeCategory.SpacingCombiningMark
                    or UnicodeCategory.ConnectorPunctuation or UnicodeCategory.Format) continue;

                return false;
            }

            return true;
        }

        // Generic-aware, so emitted signatures compile instead of leaking reflection's "Name`1[[…]]" strings.
        private static string TypeName(Type type)
        {
            if (type == typeof(void)) return "void";

            if (type.IsArray)
                return $"{TypeName(type.GetElementType())}[{new string(',', type.GetArrayRank() - 1)}]";

            if (type.IsByRef || type.IsPointer)
                return TypeName(type.GetElementType());

            if (type.IsGenericParameter) return type.Name;

            if (type.IsGenericType)
            {
                var definition = type.GetGenericTypeDefinition();
                var rawName = (definition.FullName ?? definition.Name).Replace('+', '.');

                var tick = rawName.IndexOf('`');
                if (tick >= 0) rawName = rawName.Substring(0, tick);

                var arguments = string.Join(", ", type.GetGenericArguments().Select(TypeName));
                return $"{rawName}<{arguments}>";
            }

            var name = type.FullName ?? type.Name;
            return name.Replace('+', '.');
        }
    }
}
