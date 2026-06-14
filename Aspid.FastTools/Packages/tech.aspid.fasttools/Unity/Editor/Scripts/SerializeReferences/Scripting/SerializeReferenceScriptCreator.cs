using System;
using System.IO;
using System.Text;
using System.Linq;
using UnityEditor;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    /// <summary>
    /// Generates a <c>[Serializable]</c> subclass stub for a managed-reference field's base type and imports it, so an
    /// author can create a new subtype without leaving the inspector. For an interface base the interface members are
    /// emitted as auto-properties / <see cref="NotImplementedException"/> method stubs so the file compiles.
    /// </summary>
    internal static class SerializeReferenceScriptCreator
    {
        /// <summary>
        /// Prompts for a file (name + folder), writes a subclass stub deriving from <paramref name="baseType"/>, imports
        /// it, and returns the created asset path and the new type's full name (for the deferred assignment).
        /// </summary>
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

        private static void AppendInterfaceMembers(StringBuilder builder, string indent, Type interfaceType)
        {
            var handledByProperties = new System.Collections.Generic.HashSet<MethodInfo>();

            foreach (var property in EnumerateInterfaceMembers(interfaceType).OfType<PropertyInfo>())
            {
                var accessors = property.GetAccessors();
                foreach (var accessor in accessors) handledByProperties.Add(accessor);

                var get = property.CanRead ? " get;" : string.Empty;
                var set = property.CanWrite ? " set;" : string.Empty;
                builder.AppendLine($"{indent}public {TypeName(property.PropertyType)} {property.Name} {{{get}{set} }}");
            }

            foreach (var method in EnumerateInterfaceMembers(interfaceType).OfType<MethodInfo>())
            {
                if (method.IsSpecialName || handledByProperties.Contains(method)) continue; // property/event accessors

                var parameters = string.Join(", ", method.GetParameters().Select(p => $"{TypeName(p.ParameterType)} {p.Name}"));
                builder.AppendLine($"{indent}public {TypeName(method.ReturnType)} {method.Name}({parameters}) => throw new NotImplementedException();");
            }
        }

        private static System.Collections.Generic.IEnumerable<MemberInfo> EnumerateInterfaceMembers(Type interfaceType)
        {
            foreach (var member in interfaceType.GetMembers()) yield return member;
            foreach (var inherited in interfaceType.GetInterfaces())
                foreach (var member in inherited.GetMembers())
                    yield return member;
        }

        // A best-effort C# type name for the stub. Handles void and nested types; generic edge cases fall back to a
        // form the author can refine in the opened file.
        private static string TypeName(Type type)
        {
            if (type == typeof(void)) return "void";
            var name = type.FullName ?? type.Name;
            return name.Replace('+', '.');
        }
    }
}
