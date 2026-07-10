using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types.Editors
{
    /// <summary>
    /// Resolves the string arguments of a <see cref="TypeSelectorAttribute"/> into base-constraint types.
    /// Each name is resolved member-first: a field or property with that name on the target object's type
    /// hierarchy supplies the constraint dynamically (value shapes: <see cref="Type"/>, <see cref="Type"/>[],
    /// <c>string</c>, <c>string[]</c>, <see cref="ISerializableType"/> and arrays of it); only when no member
    /// matches is the string treated as an assembly-qualified type name for <see cref="Type.GetType(string)"/>.
    /// A name that resolves to nothing is reported through the warnings list so the drawer can surface it.
    /// </summary>
    internal static class TypeSelectorConstraintResolver
    {
        /// <summary>
        /// Resolves <paramref name="assemblyQualifiedNames"/> against <paramref name="targetObject"/>.
        /// Blank names are skipped. When <paramref name="warnings"/> is non-null, every name that yields
        /// no constraint (unknown member and unresolvable type, or a member the drawer cannot read) appends
        /// a human-readable message.
        /// </summary>
        public static Type[] Resolve(IReadOnlyList<string> assemblyQualifiedNames, object targetObject, List<string> warnings = null)
        {
            var types = new List<Type>();
            var targetType = targetObject.GetType();

            foreach (var name in assemblyQualifiedNames)
            {
                if (string.IsNullOrWhiteSpace(name)) continue;

                var member = GetMemberFromHierarchy(targetType, name);
                if (member is not null)
                {
                    if (!AddTypesFromMember(types, member, targetObject) && !IsSuitableMember(member))
                    {
                        warnings?.Add(
                            $"Member '{name}' cannot supply base types — it must be an instance field or property " +
                            "of type Type, Type[], string, string[], SerializableType or SerializableType<T>.");
                    }

                    continue;
                }

                var type = Type.GetType(name, throwOnError: false);
                if (type is not null)
                {
                    types.Add(type);
                    continue;
                }

                warnings?.Add(IsValidIdentifier(name)
                    ? $"'{name}' is neither a member of {targetType.Name} nor a resolvable type name — " +
                      $"if a type was intended, qualify it with its assembly (\"{name}, MyAssembly\")."
                    : $"Type '{name}' could not be resolved to any loaded type.");
            }

            return types.ToArray();
        }

        private static MemberInfo GetMemberFromHierarchy(Type type, string memberName)
        {
            const BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly;

            var currentType = type;
            while (currentType is not null)
            {
                var members = currentType.GetMember(memberName, bindingAttr);
                if (members.Length > 0)
                    return members[0];

                currentType = currentType.BaseType;
            }

            return null;
        }

        // Reads the member's current value and appends every type it names. Returns true when at least one
        // type was appended — a suitable member holding null / empty values is legitimate ("no constraint yet")
        // and is reported as success by IsSuitableMember instead.
        private static bool AddTypesFromMember(List<Type> types, MemberInfo member, object targetObject)
        {
            var value = member switch
            {
                FieldInfo fieldInfo => fieldInfo.GetValue(targetObject),
                PropertyInfo propertyInfo => propertyInfo.GetValue(targetObject),
                _ => null
            };

            var count = types.Count;

            switch (value)
            {
                case Type type:
                    types.Add(type);
                    break;

                case Type[] typeArray:
                    types.AddRange(typeArray.Where(type => type is not null));
                    break;

                case string assemblyQualifiedName:
                    AddTypeFromName(types, assemblyQualifiedName);
                    break;

                case string[] assemblyQualifiedNames:
                {
                    foreach (var assemblyQualifiedName in assemblyQualifiedNames)
                        AddTypeFromName(types, assemblyQualifiedName);
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

            return types.Count > count;
        }

        private static void AddTypeFromName(List<Type> types, string assemblyQualifiedName)
        {
            if (string.IsNullOrWhiteSpace(assemblyQualifiedName)) return;

            var type = Type.GetType(assemblyQualifiedName, throwOnError: false);
            if (type is not null)
                types.Add(type);
        }

        // A member the drawer can read at all: an instance field or property whose declared (element) type is
        // Type, string or an ISerializableType wrapper. A declared type of object still passes when the value
        // matched in AddTypesFromMember; here only the static shape is judged.
        private static bool IsSuitableMember(MemberInfo member)
        {
            var memberType = member switch
            {
                FieldInfo fieldInfo => fieldInfo.FieldType,
                PropertyInfo propertyInfo => propertyInfo.PropertyType,
                _ => null
            };

            if (memberType is null) return false;
            if (memberType.IsArray) memberType = memberType.GetElementType();
            if (memberType is null) return false;

            return memberType == typeof(string) ||
                memberType == typeof(Type) ||
                typeof(ISerializableType).IsAssignableFrom(memberType);
        }

        // Syntactic split: a valid C# identifier is a member reference, anything else is a type name.
        private static bool IsValidIdentifier(string name)
        {
            if (!char.IsLetter(name[0]) && name[0] != '_') return false;

            for (var i = 1; i < name.Length; i++)
            {
                if (!char.IsLetterOrDigit(name[i]) && name[i] != '_')
                    return false;
            }

            return true;
        }
    }
}
