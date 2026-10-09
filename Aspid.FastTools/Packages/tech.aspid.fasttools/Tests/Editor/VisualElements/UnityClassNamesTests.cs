using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using System.Reflection;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace Aspid.FastTools.UIElements.Tests
{
    /// <summary>
    /// Guards the Unity class names that the package matches. A stylesheet selector cannot use a C# constant, so a
    /// Unity release that renames a class would stop the rule from matching and no other test would fail.
    /// </summary>
    /// <remarks>
    /// A name counts as declared when a UI Toolkit element type of the running Unity holds it in a field whose name
    /// ends with <c>ClassName</c>, public or not.
    /// </remarks>
    [TestFixture]
    internal sealed class UnityClassNamesTests
    {
        private const string PackagePath = "Packages/tech.aspid.fasttools";
        private const string UnityPrefix = "unity-";

        // Classes that Unity applies from a type outside UI Toolkit elements, so no class-name field exposes them.
        private static readonly HashSet<string> LiteralNames = new() { "unity-header-drawer__label" };

        private static readonly Regex Comment = new(@"/\*.*?\*/", RegexOptions.Singleline);
        private static readonly Regex QuotedString = new(@"""[^""]*""|'[^']*'");
        private static readonly Regex StyleSheetClass = new(@"\.(unity-[\w-]+)");
        private static readonly Regex LayoutClassAttribute = new(@"\bclass\s*=\s*""([^""]*)""");
        private static readonly Regex SourceLiteral = new(@"""(unity-[\w-]+)""");

        [Test]
        public void PackageStyleSheetsAndLayouts_UseClassNamesThatUnityDeclares()
        {
            var declared = CollectDeclaredClassNames();
            var used = CollectUsedNames(extensions: new[] { ".uss", ".uxml" }, read: ReadStyleSheetOrLayoutNames);

            Assert.IsNotEmpty(used, "No Unity class names found in the package stylesheets and layouts.");

            var missing = used
                .Where(pair => !declared.Contains(pair.Key))
                .Select(pair => $"{pair.Key} ({string.Join(", ", pair.Value)})")
                .ToArray();

            CollectionAssert.IsEmpty(missing, "Unity declares none of these classes, so a rule or layout that uses " +
                "them never matches. Rename them to the classes the current Unity applies.");
        }

        [Test]
        public void PackageSource_UsesUnityConstantsInsteadOfClassNameLiterals()
        {
            var declared = CollectDeclaredClassNames();
            var used = CollectUsedNames(extensions: new[] { ".cs" }, read: ReadSourceNames);

            var literals = used
                .Where(pair => !LiteralNames.Contains(pair.Key))
                .Select(pair => $"{pair.Key} ({string.Join(", ", pair.Value)})")
                .ToArray();

            CollectionAssert.IsEmpty(literals, "Use the Unity constant (such as BaseField<T>.inputUssClassName) " +
                "instead of a class-name literal, which a Unity release can rename without a compile error. " +
                "List the name in LiteralNames only when Unity declares no constant for it.");

            Assert.IsTrue(LiteralNames.All(name => !declared.Contains(name)),
                "Unity now declares a constant for a listed literal; use it and drop the name from LiteralNames.");
        }

        // Name -> package-relative files that use it. The Tests folder is skipped: it holds fixtures, not shipped code.
        private static Dictionary<string, List<string>> CollectUsedNames(string[] extensions, Func<string, IEnumerable<string>> read)
        {
            var package = PackageInfo.FindForAssetPath(PackagePath);
            Assert.IsNotNull(package, $"{PackagePath} is not a package.");

            var used = new Dictionary<string, List<string>>();
            foreach (var file in Directory.GetFiles(package.resolvedPath, "*.*", SearchOption.AllDirectories))
            {
                var relative = Path.GetRelativePath(package.resolvedPath, file).Replace('\\', '/');
                if (relative.StartsWith("Tests/") || !extensions.Contains(Path.GetExtension(file))) continue;

                foreach (var name in read(file))
                {
                    if (!used.TryGetValue(name, out var files))
                        used[name] = files = new List<string>();

                    if (!files.Contains(relative)) files.Add(relative);
                }
            }

            return used;
        }

        private static IEnumerable<string> ReadStyleSheetOrLayoutNames(string path)
        {
            var content = File.ReadAllText(path);

            if (path.EndsWith(".uxml"))
            {
                return LayoutClassAttribute.Matches(content)
                    .SelectMany(match => match.Groups[1].Value.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                    .Where(name => name.StartsWith(UnityPrefix));
            }

            content = QuotedString.Replace(Comment.Replace(content, string.Empty), string.Empty);
            return StyleSheetClass.Matches(content).Select(match => match.Groups[1].Value);
        }

        private static IEnumerable<string> ReadSourceNames(string path)
        {
            return SourceLiteral.Matches(File.ReadAllText(path)).Select(match => match.Groups[1].Value);
        }

        private static HashSet<string> CollectDeclaredClassNames()
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic |
                                       BindingFlags.Static | BindingFlags.DeclaredOnly;

            var names = new HashSet<string>();
            var assemblies = new[] { typeof(VisualElement).Assembly, typeof(PropertyField).Assembly };

            foreach (var type in assemblies.SelectMany(GetLoadableTypes))
            {
                var closed = TryCloseGenericType(type);
                if (closed is null || !typeof(VisualElement).IsAssignableFrom(closed)) continue;

                foreach (var field in closed.GetFields(flags))
                {
                    if (field.FieldType != typeof(string) || !field.Name.EndsWith("ClassName")) continue;

                    try
                    {
                        if (field.GetValue(null) is string value && value.StartsWith(UnityPrefix))
                            names.Add(value);
                    }
                    catch (Exception)
                    {
                        // A type whose static constructor fails holds nothing readable.
                    }
                }
            }

            // Without these the scan found nothing, and every other check would pass for the wrong reason.
            Assert.IsTrue(names.Contains(Foldout.ussClassName) && names.Contains(PropertyField.ussClassName),
                "Reflection did not find Unity's class-name fields; update this test for the current Unity.");

            return names;
        }

        private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
        {
            try { return assembly.GetTypes(); }
            catch (ReflectionTypeLoadException exception) { return exception.Types.Where(type => type is not null); }
        }

        // Class-name fields do not depend on the type arguments, so any arguments that satisfy the constraints do.
        private static Type TryCloseGenericType(Type type)
        {
            if (!type.IsGenericTypeDefinition) return type;

            foreach (var argument in new[] { typeof(object), typeof(float) })
            {
                try { return type.MakeGenericType(Enumerable.Repeat(argument, type.GetGenericArguments().Length).ToArray()); }
                catch (ArgumentException) { }
            }

            return null;
        }
    }
}
