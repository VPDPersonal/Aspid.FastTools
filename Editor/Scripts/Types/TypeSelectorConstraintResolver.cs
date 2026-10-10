using System;
using UnityEditor;
using System.Linq;
using System.Reflection;
using Aspid.FastTools.Editors;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types.Editors
{
    internal static class TypeSelectorConstraintResolver
    {
        // Members are looked up on the instance that declares the attributed field — for a field inside a
        // [Serializable] class or a list element that is the nested instance, not the inspected root object —
        // matching the declaring type analyzer rules AFT0006–AFT0008 check against.
        internal static Result Resolve(SerializedProperty attributedProperty, IReadOnlyList<string> assemblyQualifiedNames) =>
            Resolve(
                attributedProperty.GetDeclaringInstance() ?? attributedProperty.serializedObject.targetObject,
                assemblyQualifiedNames);

        // For a list drawn through an API that takes its own constraints: [TypeSelector] applies to the collection,
        // so its elements no longer resolve the attribute themselves.
        internal static Type[] AppendFieldConstraints(SerializedProperty listProperty, Type[] baseTypes)
        {
            var selector = listProperty.GetFieldInfo()?.GetCustomAttribute<TypeSelectorAttribute>(inherit: true);
            if (selector is null || selector.AssemblyQualifiedNames.Length is 0) return baseTypes;

            var resolved = Resolve(listProperty, selector.AssemblyQualifiedNames).Types;
            if (resolved.Length is 0) return baseTypes;

            return baseTypes is null || baseTypes.Length is 0 ? resolved : baseTypes.Concat(resolved).ToArray();
        }

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
                    if (!IsSuitableMember(member))
                    {
                        (warnings ??= new List<string>()).Add(
                            $"Member '{name}' cannot supply base types — it must be an instance field or property " +
                            "of type Type, Type[], string, string[], SerializableType or SerializableMonoScript (plain or <T>); " +
                            "properties must be readable and cannot be indexers.");
                    }
                    else if (!TryAddTypesFromMember(targetObject, member, types, out var failure))
                    {
                        (warnings ??= new List<string>()).Add(
                            $"Member '{name}' threw {failure.GetType().Name} while supplying base types: {failure.Message}");
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

        // A member getter is user code: when it throws, the picker draws without this constraint and a warning explains why.
        private static bool TryAddTypesFromMember(object targetObject, MemberInfo member, List<Type> types, out Exception failure)
        {
            var count = types.Count;

            try
            {
                AddTypesFromMember(targetObject, member, types);
                failure = null;
                return true;
            }
            catch (Exception exception)
            {
                types.RemoveRange(index: count, count: types.Count - count);
                failure = exception is TargetInvocationException { InnerException: { } inner } ? inner : exception;
                return false;
            }
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
            if (member is PropertyInfo property &&
                (property.GetGetMethod(nonPublic: true) is null ||
                    property.GetIndexParameters().Length != 0))
                return false;

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
