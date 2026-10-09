using System;
using System.Linq;
using NUnit.Framework;
using System.Reflection;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Aspid.FastTools.UIElements.Tests
{
    /// <summary>
    /// A C# instance method wins over an extension method of the same name and a compatible signature, so a method
    /// that a new Unity version adds to a UI Toolkit type silently turns a fluent call into a call of Unity's method.
    /// CI runs the tests on every supported Unity version, so this test fails on the version that adds the clash.
    /// </summary>
    [TestFixture]
    internal sealed class UnityMethodShadowingTests
    {
        // Clashes that are accepted on purpose, as "ExtensionClass.Method" with the reason. Empty: the package avoids
        // Unity's names, for example FocusSelf and SetTextSelf, so a hit is a new clash to rename around.
        private static readonly HashSet<string> Accepted = new();

        private static readonly BindingFlags InstanceMethods = BindingFlags.Public | BindingFlags.Instance;

        [Test]
        public void Extensions_AreNotShadowedByUnityInstanceMethods()
        {
            var unityTypes = GetUnityTypes();
            var instanceMethods = new Dictionary<Type, ILookup<string, MethodInfo>>();
            var clashes = new SortedSet<string>();

            foreach (var extension in GetExtensionMethods())
            {
                var key = $"{extension.DeclaringType.Name}.{extension.Name}";
                if (Accepted.Contains(key)) continue;

                var parameters = extension.GetParameters().Skip(1).ToArray();
                foreach (var receiver in GetReceivers(extension, unityTypes))
                {
                    if (!instanceMethods.TryGetValue(receiver, out var methods))
                        instanceMethods[receiver] = methods = receiver.GetMethods(InstanceMethods).ToLookup(method => method.Name);

                    foreach (var method in methods[extension.Name])
                    {
                        if (CanTakeTheCall(method, parameters))
                            clashes.Add($"{key}({FormatParameters(parameters)}) is shadowed by {method.DeclaringType.Name}.{method.Name}({FormatParameters(method.GetParameters())})");
                    }
                }
            }

            Assert.IsEmpty(clashes, "Rename the extension, or accept the clash with a reason in the list above:" +
                                    Environment.NewLine + string.Join(Environment.NewLine, clashes));
        }

        [Test]
        public void Extensions_AreFound()
        {
            Assert.Greater(GetExtensionMethods().Count(), 500);
            Assert.Greater(GetUnityTypes().Count, 100);
        }

        // An extension without a receiver is never compared, so a gap in the lookup would hide a clash.
        [Test]
        public void Extensions_HaveReceivers()
        {
            var unityTypes = GetUnityTypes();

            var withoutReceiver = GetExtensionMethods()
                .Where(extension => !GetReceivers(extension, unityTypes).Any())
                .Select(extension => $"{extension.DeclaringType.Name}.{extension.Name}({FormatParameters(extension.GetParameters())})")
                .ToArray();

            Assert.IsEmpty(withoutReceiver, "These extensions have no receiver type to check:" + Environment.NewLine + string.Join(Environment.NewLine, withoutReceiver));
        }

        private static IEnumerable<MethodInfo> GetExtensionMethods() =>
            typeof(VisualElementExtensions).Assembly
                .GetTypes()
                .Where(type => type.IsPublic && type.IsAbstract && type.IsSealed && type.Namespace == typeof(VisualElementExtensions).Namespace)
                .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.Static))
                .Where(method => method.IsDefined(typeof(ExtensionAttribute), inherit: false));

        // The type the extension extends, or for a generic one every Unity type that meets the constraints.
        private static IEnumerable<Type> GetReceivers(MethodInfo extension, IReadOnlyList<Type> unityTypes)
        {
            var thisType = extension.GetParameters()[0].ParameterType;
            var constraints = thisType.IsGenericParameter ? thisType.GetGenericParameterConstraints() : new[] { thisType };
            if (constraints.Length == 0) return Enumerable.Empty<Type>();

            // A constraint type from another assembly, such as Texture2D, is not among the Unity types.
            return unityTypes
                .Concat(constraints.Where(constraint => !constraint.ContainsGenericParameters))
                .Distinct()
                .Where(type => constraints.All(constraint => Meets(type, constraint)));
        }

        // A constraint such as BaseField<TValue> is met by every closed type built from the same generic type.
        private static bool Meets(Type type, Type constraint)
        {
            if (constraint.IsGenericParameter) return true;
            if (!constraint.ContainsGenericParameters) return constraint.IsAssignableFrom(type);
            if (!constraint.IsGenericType) return true;

            var definition = constraint.GetGenericTypeDefinition();
            return GetSupertypes(type).Any(supertype => supertype.IsGenericType && supertype.GetGenericTypeDefinition() == definition);
        }

        private static IEnumerable<Type> GetSupertypes(Type type)
        {
            for (var current = type; current is not null; current = current.BaseType)
                yield return current;

            foreach (var @interface in type.GetInterfaces())
                yield return @interface;
        }

        // Does `receiver.Name(arguments)` bind to the instance method, with the optional parameters left out as needed?
        private static bool CanTakeTheCall(MethodInfo method, ParameterInfo[] extensionParameters)
        {
            var instanceParameters = method.GetParameters();
            var required = extensionParameters.Count(parameter => !parameter.IsOptional);

            for (var count = required; count <= extensionParameters.Length; count++)
            {
                if (count > instanceParameters.Length) break;
                if (instanceParameters.Skip(count).Any(parameter => !parameter.IsOptional)) continue;

                var accepts = true;
                for (var i = 0; i < count && accepts; i++)
                    accepts = Accepts(instanceParameters[i].ParameterType, extensionParameters[i].ParameterType);

                if (accepts) return true;
            }

            return false;
        }

        // An open generic argument of the extension can be any type, so it counts as a match.
        private static bool Accepts(Type instanceParameter, Type extensionParameter) =>
            extensionParameter.ContainsGenericParameters || instanceParameter.IsAssignableFrom(extensionParameter);

        private static string FormatParameters(IEnumerable<ParameterInfo> parameters) =>
            string.Join(", ", parameters.Select(parameter => parameter.ParameterType.Name));

        private static IReadOnlyList<Type> GetUnityTypes() =>
            new[] { typeof(VisualElement).Assembly, typeof(UnityEditor.UIElements.ObjectField).Assembly }
                .Distinct()
                .SelectMany(assembly => assembly.GetExportedTypes())
                .Where(type => !type.ContainsGenericParameters && !type.IsEnum)
                .ToArray();
    }
}
