using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types.Editors
{
    // Resolves the string arguments of a TypeSelectorAttribute into base-constraint types.
    // Each name is resolved member-first — a field or property with that name on the target object's type
    // hierarchy supplies the constraint dynamically; only when no member matches is the string treated
    // as an assembly-qualified type name for GetType(string).
    internal static class TypeSelectorConstraintResolver
    {
        // Blank names are skipped, and every name that can supply no constraint adds a warning. A suitable member
        // whose current value is empty is not one: it means "no constraint yet".
        internal static Result Resolve(object targetObject, IReadOnlyList<string> assemblyQualifiedNames)
        {
            var types = new List<Type>();
            List<string> warnings = null;
            var targetType = targetObject.GetType();

            foreach (var name in assemblyQualifiedNames)
            {
                if (string.IsNullOrWhiteSpace(name)) continue;
                var member = GetMemberFromHierarchy(targetType, name);

                if (member is not null)
                {
                    var count = types.Count;
                    AddTypesFromMember(targetObject, member, types);
                    var isAdded = types.Count > count;

                    if (!isAdded && !IsSuitableMember(member))
                    {
                        (warnings ??= new List<string>()).Add(
                            $"Member '{name}' cannot supply base types — it must be an instance field or property " +
                            "of type Type, Type[], string, string[], SerializableType or SerializableMonoScript (plain or <T>).");
                    }

                    continue;
                }

                var type = TypeUtility.GetTypeOrNull(name);

                if (type is not null)
                {
                    types.Add(type);
                }
                else
                {
                    var message = IsValidIdentifier(name)
                        ? $"'{name}' is neither a member of {targetType.Name} nor a resolvable type name — " +
                          $"if a type was intended, qualify it with its assembly (\"{name}, MyAssembly\")."
                        : $"Type '{name}' could not be resolved to any loaded type.";

                    (warnings ??= new List<string>()).Add(message);
                }
            }

            return new Result(types.ToArray(), warnings);
        }

        private static MemberInfo GetMemberFromHierarchy(Type type, string memberName)
        {
            const BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly;

            for (var current = type; current is not null; current = current.BaseType)
            {
                var members = current.GetMember(memberName, bindingAttr);

                if (members.Length > 0)
                    return members[0];
            }

            return null;
        }

        private static void AddTypesFromMember(object targetObject, MemberInfo member, List<Type> types)
        {
            var value = member switch
            {
                FieldInfo fieldInfo => fieldInfo.GetValue(targetObject),
                PropertyInfo propertyInfo => propertyInfo.GetValue(targetObject),
                _ => null
            };

            switch (value)
            {
                case Type type:
                    types.Add(type);
                    break;

                case Type[] typeArray:
                    types.AddRange(typeArray.Where(type => type is not null));
                    break;

                case string assemblyQualifiedName:
                    AddTypeFromName(assemblyQualifiedName, types);
                    break;

                case string[] assemblyQualifiedNames:
                {
                    foreach (var assemblyQualifiedName in assemblyQualifiedNames)
                        AddTypeFromName(assemblyQualifiedName, types);

                    break;
                }

                case ISerializableType serializableType:
                    if (serializableType.Type is { } resolved)
                        types.Add(resolved);

                    break;

                case ISerializableType[] serializableTypes:
                {
                    foreach (var wrapper in serializableTypes)
                    {
                        if (wrapper?.Type is { } element)
                            types.Add(element);
                    }

                    break;
                }
            }
        }

        private static void AddTypeFromName(string assemblyQualifiedName, List<Type> types)
        {
            if (TypeUtility.GetTypeOrNull(assemblyQualifiedName) is { } type)
                types.Add(type);
        }

        private static bool IsValidIdentifier(string name)
        {
            if (!char.IsLetter(name[0]) && name[0] != '_')
                return false;

            for (var i = 1; i < name.Length; i++)
            {
                if (!char.IsLetterOrDigit(name[i]) && name[i] != '_')
                    return false;
            }

            return true;
        }

        private static bool IsSuitableMember(MemberInfo member)
        {
            var memberType = member switch
            {
                FieldInfo fieldInfo => fieldInfo.FieldType,
                PropertyInfo propertyInfo => propertyInfo.PropertyType,
                _ => null
            };

            if (memberType?.IsArray ?? false)
                memberType = memberType.GetElementType();

            if (memberType is null) return false;

            return memberType == typeof(string)
                || memberType == typeof(Type)
                || typeof(ISerializableType).IsAssignableFrom(memberType);
        }

        // The outcome of Resolve: the constraint types plus a warning for every name
        // that could not supply any.
        internal readonly struct Result
        {
            internal Type[] Types { get; }

            internal IReadOnlyList<string> Warnings { get; }

            internal Result(Type[] types, IReadOnlyList<string> warnings)
            {
                Types = types;
                Warnings = warnings ?? Array.Empty<string>();
            }
        }
    }
}
