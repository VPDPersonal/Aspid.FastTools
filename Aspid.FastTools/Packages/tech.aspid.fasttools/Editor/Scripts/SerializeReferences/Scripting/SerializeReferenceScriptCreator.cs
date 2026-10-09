using System;
using System.IO;
using System.Text;
using System.Linq;
using UnityEditor;
using System.Reflection;
using System.Globalization;
using UnityEditor.Compilation;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    internal static class SerializeReferenceScriptCreator
    {
        private const string Body = "throw new NotImplementedException()";

        public static bool TryCreateSubclassStub(Type baseType, out string assetPath, out string fullTypeName)
        {
            assetPath = null;
            fullTypeName = null;
            if (baseType is null) return false;

            var path = EditorUtility.SaveFilePanelInProject(
                "Create Managed-Reference Script", SuggestClassName(baseType), "cs",
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

            if (!TryGenerateStub(className, nspace, baseType, IsSameAssembly(path, baseType), out var stub, out var error))
            {
                EditorUtility.DisplayDialog("Cannot Create Script", error, "OK");
                return false;
            }

            File.WriteAllText(path, stub);
            AssetDatabase.ImportAsset(path);

            assetPath = path;
            fullTypeName = string.IsNullOrEmpty(nspace) ? className : $"{nspace}.{className}";
            return true;
        }

        // Only a single interface prefix is dropped (IInteractable -> NewInteractable, not Newnteractable), and the
        // generic arity suffix goes, since "NewEffect`1" is not a valid class name.
        internal static string SuggestClassName(Type baseType)
        {
            var name = baseType.Name;

            var aritySeparator = name.IndexOf('`');
            if (aritySeparator >= 0) name = name[..aritySeparator];

            if (baseType.IsInterface && name.Length > 1 && name[0] == 'I' && char.IsUpper(name[1]))
                name = name[1..];

            return "New" + name;
        }

        // Accessibility decides how a stub overrides a member, and that depends on whether the new script lands in
        // the assembly of the base type. The name of a script that does not exist yet still resolves from its folder.
        internal static bool IsSameAssembly(string scriptPath, Type baseType) =>
            CompilationPipeline.GetAssemblyNameFromScriptPath(scriptPath) == baseType.Assembly.GetName().Name + ".dll";

        internal static bool TryGenerateStub(string className, string nspace, Type baseType, bool sameAssembly,
            out string stub, out string error)
        {
            stub = null;
            error = null;

            // Also covers structs, enums and static classes.
            if (baseType.IsSealed)
            {
                error = $"\"{baseType.Name}\" is sealed, so no class can derive from it.";
                return false;
            }

            var memberIndent = string.IsNullOrEmpty(nspace) ? "    " : "        ";
            var members = new StringBuilder();

            if (baseType.IsInterface) AppendInterfaceMembers(members, memberIndent, baseType);
            else if (!TryAppendBaseClassMembers(members, memberIndent, className, baseType, sameAssembly, out error))
                return false;

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
            builder.Append(members);
            builder.AppendLine($"{indent}}}");

            if (!string.IsNullOrEmpty(nspace)) builder.AppendLine("}");

            stub = builder.ToString();
            return true;
        }

        // The interface hierarchy is flattened, so the same signature can arrive from several branches. Each group
        // emits one public implicit member; a duplicate differing only in return type cannot share it and becomes an
        // explicit implementation on its declaring interface instead. Members with a default body need no stub.
        private static void AppendInterfaceMembers(StringBuilder builder, string indent, Type interfaceType)
        {
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

            foreach (var group in events.Where(@event => @event.AddMethod is { IsAbstract: true }).GroupBy(@event => @event.Name))
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
                        builder.AppendLine($"{indent}event {typeGroup.Key} {TypeName(@event.DeclaringType)}.{group.Key} {{ add => {Body}; remove => {Body}; }}");
                }
            }

            foreach (var group in properties.Where(IsAbstract).GroupBy(PropertyKey))
            {
                foreach (var (typeGroup, index) in group.GroupBy(PropertyTypeName)
                    .Select((typeGroup, index) => (typeGroup, index)))
                {
                    if (index is 0)
                    {
                        var property = typeGroup.First();
                        var setter = typeGroup.FirstOrDefault(candidate => candidate.CanWrite);
                        var initOnly = setter is not null && IsInitOnly(setter);

                        // An auto-property requires a getter even when the interface declares only a setter. An
                        // indexer and a ref property cannot be an auto-property.
                        if (property.GetIndexParameters().Length > 0 || property.PropertyType.IsByRef)
                        {
                            var write = setter is not null && !property.PropertyType.IsByRef;
                            builder.AppendLine($"{indent}public {PropertyDeclaration(property, string.Empty)} {Accessors(read: true, write: write, initOnly: initOnly)}");
                            continue;
                        }

                        var set = setter is null ? string.Empty : initOnly ? " init;" : " set;";
                        builder.AppendLine($"{indent}public {PropertyDeclaration(property, string.Empty)} {{ get;{set} }}");
                        continue;
                    }

                    foreach (var property in typeGroup)
                    {
                        var declaration = PropertyDeclaration(property, TypeName(property.DeclaringType) + ".");
                        builder.AppendLine($"{indent}{declaration} {Accessors(read: property.CanRead, write: property.CanWrite, initOnly: IsInitOnly(property))}");
                    }
                }
            }

            var methods = members.OfType<MethodInfo>()
                .Where(method => method.IsAbstract && !method.IsSpecialName && !handledAccessors.Contains(method))
                .ToArray();

            foreach (var group in methods.GroupBy(MethodKey))
            {
                foreach (var (returnGroup, index) in group.GroupBy(ReturnTypeName)
                    .Select((returnGroup, index) => (returnGroup, index)))
                {
                    if (index is 0)
                    {
                        builder.AppendLine($"{indent}{MethodDeclaration(returnGroup.First(), "public ", string.Empty, withConstraints: true)}");
                        continue;
                    }

                    foreach (var method in returnGroup)
                        builder.AppendLine($"{indent}{MethodDeclaration(method, string.Empty, TypeName(method.DeclaringType) + ".", withConstraints: false)}");
                }
            }
        }

        // A class base needs a constructor the stub can call, and an override for every abstract member that is
        // still open. Reflection lists only the most derived override of a virtual member, so a member that an
        // intermediate class already implements does not appear as abstract. A member that the stub cannot reach
        // stops the generation: the file would not compile.
        private static bool TryAppendBaseClassMembers(StringBuilder builder, string indent, string className,
            Type baseType, bool sameAssembly, out string error)
        {
            error = null;

            if (!TryAppendConstructor(builder, indent, className, baseType, sameAssembly))
            {
                error = $"\"{baseType.Name}\" has no constructor that a derived class can call.";
                return false;
            }

            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var hidden = new List<string>();
            var overrides = new StringBuilder();

            foreach (var method in baseType.GetMethods(flags).Where(method => method.IsAbstract && !method.IsSpecialName))
            {
                var access = AccessKeyword(method, sameAssembly);
                if (access is null) hidden.Add(method.Name);
                else overrides.AppendLine($"{indent}{MethodDeclaration(method, $"{access} override ", string.Empty, withConstraints: false)}");
            }

            foreach (var property in baseType.GetProperties(flags).Where(IsAbstract))
            {
                var getter = property.GetMethod is { IsAbstract: true } ? property.GetMethod : null;
                var setter = property.SetMethod is { IsAbstract: true } ? property.SetMethod : null;

                var getAccess = getter is null ? null : AccessKeyword(getter, sameAssembly);
                var setAccess = setter is null ? null : AccessKeyword(setter, sameAssembly);
                if ((getter is not null && getAccess is null) || (setter is not null && setAccess is null))
                {
                    hidden.Add(property.Name);
                    continue;
                }

                // The property takes the more visible accessor's keyword, and the other accessor repeats its own.
                var access = getAccess is null ? setAccess
                    : setAccess is null ? getAccess
                    : Array.IndexOf(_accessByVisibility, getAccess) <= Array.IndexOf(_accessByVisibility, setAccess) ? getAccess : setAccess;

                var accessors = Accessors(
                    read: getter is not null, write: setter is not null, initOnly: IsInitOnly(property),
                    readModifier: getAccess == access ? string.Empty : getAccess + " ",
                    writeModifier: setAccess == access ? string.Empty : setAccess + " ");

                overrides.AppendLine($"{indent}{access} override {PropertyDeclaration(property, string.Empty)} {accessors}");
            }

            foreach (var @event in baseType.GetEvents(flags).Where(@event => @event.AddMethod is { IsAbstract: true }))
            {
                var access = AccessKeyword(@event.AddMethod, sameAssembly);
                if (access is null) hidden.Add(@event.Name);
                else overrides.AppendLine($"{indent}{access} override event {TypeName(@event.EventHandlerType)} {@event.Name};");
            }

            if (hidden.Count is 0)
            {
                if (builder.Length > 0 && overrides.Length > 0) builder.AppendLine();
                builder.Append(overrides);
                return true;
            }

            error = $"\"{baseType.Name}\" has abstract members that cannot be overridden from the new script's assembly: {string.Join(", ", hidden)}.";
            return false;
        }

        // A constructor that takes no arguments needs no declaration: the implicit base() call binds to it. Without
        // one, the stub declares a constructor that passes its arguments on to the one with the fewest parameters.
        private static bool TryAppendConstructor(StringBuilder builder, string indent, string className,
            Type baseType, bool sameAssembly)
        {
            var constructors = baseType
                .GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(constructor => AccessKeyword(constructor, sameAssembly) is not null)
                .ToArray();

            if (constructors.Length is 0) return false;
            if (constructors.Any(constructor => constructor.GetParameters().All(IsOptionalParameter))) return true;

            var parameters = constructors.Select(constructor => constructor.GetParameters()).OrderBy(list => list.Length).First();
            var arguments = string.Join(", ", parameters.Select(parameter => ParameterModifier(parameter) + ParameterName(parameter)));

            builder.AppendLine($"{indent}public {className}({ParameterList(parameters)}) : base({arguments}) {{ }}");
            return true;
        }

        // The keyword a class in the same or another assembly writes to override or call the member; null when hidden.
        private static string AccessKeyword(MethodBase member, bool sameAssembly)
        {
            if (member.IsPublic) return "public";
            if (member.IsFamilyOrAssembly) return sameAssembly ? "protected internal" : "protected";
            if (member.IsFamily) return "protected";
            if (member.IsAssembly) return sameAssembly ? "internal" : null;
            return member.IsFamilyAndAssembly && sameAssembly ? "private protected" : null;
        }

        private static readonly string[] _accessByVisibility = { "public", "protected internal", "protected", "internal", "private protected" };

        private static bool IsAbstract(PropertyInfo property) =>
            property.GetMethod is { IsAbstract: true } || property.SetMethod is { IsAbstract: true };

        private static bool IsOptionalParameter(ParameterInfo parameter) =>
            parameter.IsOptional || parameter.IsDefined(typeof(ParamArrayAttribute), inherit: false);

        // Same name and parameters (generic arity included) means the same signature, whatever the return type is.
        private static string MethodKey(MethodInfo method) =>
            $"{method.Name}`{method.GetGenericArguments().Length}({ParameterKey(method.GetParameters())})";

        private static string PropertyKey(PropertyInfo property) =>
            $"{property.Name}[{ParameterKey(property.GetIndexParameters())}]";

        private static string ParameterKey(IEnumerable<ParameterInfo> parameters) =>
            string.Join(",", parameters.Select(parameter => ParameterModifier(parameter) + TypeName(parameter.ParameterType, byPosition: true)));

        private static string MethodDeclaration(MethodInfo method, string modifiers, string qualifier, bool withConstraints)
        {
            var generics = method.IsGenericMethodDefinition
                ? $"<{string.Join(", ", method.GetGenericArguments().Select(argument => argument.Name))}>"
                : string.Empty;

            // An override or an explicit implementation inherits the constraints and cannot restate them.
            var constraints = withConstraints ? GenericConstraints(method) : string.Empty;
            return $"{modifiers}{ReturnTypeName(method)} {qualifier}{method.Name}{generics}({ParameterList(method.GetParameters())}){constraints} => {Body};";
        }

        // "int Value", "string this[int index]" or "ref int Count" without the accessors and the modifiers.
        private static string PropertyDeclaration(PropertyInfo property, string qualifier)
        {
            var parameters = property.GetIndexParameters();
            var name = parameters.Length is 0 ? property.Name : $"this[{ParameterList(parameters)}]";
            return $"{PropertyTypeName(property)} {qualifier}{name}";
        }

        private static string Accessors(bool read, bool write, bool initOnly, string readModifier = "", string writeModifier = "")
        {
            var builder = new StringBuilder("{");
            if (read) builder.Append($" {readModifier}get => {Body};");
            if (write) builder.Append($" {writeModifier}{(initOnly ? "init" : "set")} => {Body};");
            return builder.Append(" }").ToString();
        }

        private static string GenericConstraints(MethodInfo method)
        {
            var builder = new StringBuilder();

            foreach (var parameter in method.GetGenericArguments())
            {
                var attributes = parameter.GenericParameterAttributes;
                var isStruct = (attributes & GenericParameterAttributes.NotNullableValueTypeConstraint) != 0;

                var constraints = new List<string>();
                if (isStruct) constraints.Add(IsUnmanaged(parameter) ? "unmanaged" : "struct");
                else if ((attributes & GenericParameterAttributes.ReferenceTypeConstraint) != 0) constraints.Add("class");

                // The compiler lists ValueType among the constraints of a struct; the keyword already says it.
                foreach (var constraint in parameter.GetGenericParameterConstraints())
                    if (!isStruct || constraint != typeof(ValueType)) constraints.Add(TypeName(constraint));

                if (!isStruct && (attributes & GenericParameterAttributes.DefaultConstructorConstraint) != 0)
                    constraints.Add("new()");

                if (constraints.Count > 0) builder.Append($" where {parameter.Name} : {string.Join(", ", constraints)}");
            }

            return builder.ToString();
        }

        private static bool IsUnmanaged(Type parameter) =>
            parameter.CustomAttributes.Any(attribute => attribute.AttributeType.FullName == "System.Runtime.CompilerServices.IsUnmanagedAttribute");

        private static string ParameterList(IEnumerable<ParameterInfo> parameters) =>
            string.Join(", ", parameters.Select(parameter => $"{ParameterModifier(parameter)}{TypeName(parameter.ParameterType)} {ParameterName(parameter)}"));

        // A by-reference parameter keeps its modifier: an implementation with another one is a different method.
        private static string ParameterModifier(ParameterInfo parameter)
        {
            if (!parameter.ParameterType.IsByRef) return string.Empty;
            if (parameter.IsOut && !parameter.IsIn) return "out ";
            return parameter.IsIn && !parameter.IsOut ? "in " : "ref ";
        }

        private static string ParameterName(ParameterInfo parameter)
        {
            var name = parameter.Name ?? $"arg{parameter.Position}";
            return _csharpKeywords.Contains(name) ? "@" + name : name;
        }

        private static string ReturnTypeName(MethodInfo method) =>
            RefReturnPrefix(method.ReturnType, method.ReturnParameter) + TypeName(method.ReturnType);

        private static string PropertyTypeName(PropertyInfo property) =>
            RefReturnPrefix(property.PropertyType, property.GetMethod?.ReturnParameter) + TypeName(property.PropertyType);

        // A readonly ref return carries the InAttribute modifier on its return parameter.
        private static string RefReturnPrefix(Type type, ParameterInfo returnParameter)
        {
            if (!type.IsByRef) return string.Empty;
            return returnParameter?.GetRequiredCustomModifiers().Any(modifier => modifier.FullName == "System.Runtime.InteropServices.InAttribute") is true
                ? "ref readonly "
                : "ref ";
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

                var category = CharUnicodeInfo.GetUnicodeCategory(c);
                if (category is UnicodeCategory.NonSpacingMark or UnicodeCategory.SpacingCombiningMark
                    or UnicodeCategory.ConnectorPunctuation or UnicodeCategory.Format) continue;

                return false;
            }

            return true;
        }

        // With byPosition, a generic method parameter is named by its index, so Get<T>(T) and Get<U>(U) compare equal.
        private static string TypeName(Type type, bool byPosition = false)
        {
            if (type == typeof(void)) return "void";

            if (type.IsArray)
                return $"{TypeName(type.GetElementType(), byPosition)}[{new string(',', type.GetArrayRank() - 1)}]";

            if (type.IsByRef || type.IsPointer)
                return TypeName(type.GetElementType(), byPosition);

            if (type.IsGenericParameter)
                return byPosition && type.DeclaringMethod is not null ? $"!!{type.GenericParameterPosition}" : type.Name;

            if (type.IsGenericType)
            {
                var definition = type.GetGenericTypeDefinition();
                var rawName = (definition.FullName ?? definition.Name).Replace('+', '.');

                var tick = rawName.IndexOf('`');
                if (tick >= 0) rawName = rawName.Substring(0, tick);

                var arguments = string.Join(", ", type.GetGenericArguments().Select(argument => TypeName(argument, byPosition)));
                return $"{rawName}<{arguments}>";
            }

            var name = type.FullName ?? type.Name;
            return name.Replace('+', '.');
        }
    }
}
