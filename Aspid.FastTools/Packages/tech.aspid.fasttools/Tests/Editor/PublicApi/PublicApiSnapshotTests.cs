using System;
using System.IO;
using System.Linq;
using UnityEngine;
using NUnit.Framework;
using System.Collections.Generic;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace Aspid.FastTools.Editors.Tests
{
    // A renamed or removed public member passes every other check and breaks users in a minor release. This test
    // compares the public API of the package assemblies with PublicApi.txt; AGENTS.md says how to update the file.
    internal sealed class PublicApiSnapshotTests
    {
        private const string PackageName = "tech.aspid.fasttools";
        private const string BaselinePath = "Tests/Editor/PublicApi/PublicApi.txt";
        private const string WriteVariable = "ASPID_FASTTOOLS_WRITE_PUBLIC_API";
        private const int MaxListedEntries = 50;

        private const string BreakTitle =
            "Removed or changed (breaks user code: keep the old member as [Obsolete], or record the break in " +
            "CHANGELOG.md and CHANGELOG.ru.md and mark the PR title with '!'):";

        // UNITY_6000_4_OR_NEWER is the newest version guard around public API, so an older Editor compiles a part of
        // the baseline: there the test only checks that nothing outside the baseline appears.
        // Move this guard and WholeApiVersion together with the first public member under a newer one.
        private const string WholeApiVersion = "6000.4";
#if UNITY_6000_4_OR_NEWER
        private static readonly bool HasWholeApi = true;
#else
        private static readonly bool HasWholeApi = false;
#endif

        [Test]
        public void PublicApi_MatchesBaseline()
        {
            var package = PackageInfo.FindForAssetPath($"Packages/{PackageName}");
            Assert.IsNotNull(package, $"Package {PackageName} is not resolved.");

            var loaded = AppDomain.CurrentDomain.GetAssemblies()
                .GroupBy(assembly => assembly.GetName().Name)
                .ToDictionary(group => group.Key, group => group.First());

            var assemblyNames = GetAssemblyNames(package.resolvedPath);
            var current = PublicApiSnapshot.Read(assemblyNames.Where(loaded.ContainsKey).Select(name => loaded[name]));

            // An asmdef whose define constraints are off in this project (an optional integration) is not compiled.
            var notCompiled = assemblyNames.Where(name => !loaded.ContainsKey(name)).ToList();

            var path = Path.Combine(package.resolvedPath, BaselinePath);
            var baseline = File.Exists(path) ? PublicApiSnapshot.Parse(File.ReadAllText(path)) : null;

            if (Environment.GetEnvironmentVariable(WriteVariable) == "1")
            {
                Assert.IsTrue(HasWholeApi, $"Write the baseline on Unity {WholeApiVersion} or newer: an older Editor lacks part of the API.");

                if (baseline is not null)
                {
                    foreach (var name in notCompiled)
                        current.CopyAssembly(baseline, name);
                }

                File.WriteAllText(path, current.Render());
                Assert.Pass($"Wrote {path}.");
            }

            Assert.IsNotNull(baseline, $"{BaselinePath} is missing. {UpdateHint}");

            var expected = baseline.GetEntries(excludedAssemblies: notCompiled);
            var actual = current.GetEntries(excludedAssemblies: Array.Empty<string>());
            var removed = HasWholeApi ? expected.Where(entry => !actual.Contains(entry)).ToList() : new List<string>();
            var added = actual.Where(entry => !expected.Contains(entry)).ToList();

            if (removed.Count is 0 && added.Count is 0) return;

            Assert.Fail(
                $"The public API differs from {BaselinePath}.\n" +
                FormatEntries(BreakTitle, removed) +
                FormatEntries("Added:", added) +
                UpdateHint);
        }

        private static string UpdateHint =>
            $"If the change is intended, run this test with {WriteVariable}=1 on Unity {WholeApiVersion} or newer " +
            "and commit the file (AGENTS.md shows the command).";

        private static List<string> GetAssemblyNames(string packagePath) =>
            new[] { "Runtime", "Editor" }
                .SelectMany(folder => Directory.EnumerateFiles(Path.Combine(packagePath, folder), "*.asmdef", SearchOption.AllDirectories))
                .Select(asmdef => JsonUtility.FromJson<AssemblyDefinition>(File.ReadAllText(asmdef)).name)
                .ToList();

        private static string FormatEntries(string title, List<string> entries)
        {
            if (entries.Count is 0) return string.Empty;

            entries.Sort(StringComparer.Ordinal);
            var lines = entries.Take(MaxListedEntries).Select(entry => $"  {entry}\n");
            var rest = entries.Count > MaxListedEntries ? $"  … and {entries.Count - MaxListedEntries} more\n" : string.Empty;
            return $"{title}\n{string.Concat(lines)}{rest}";
        }

        [Serializable]
        private sealed class AssemblyDefinition
        {
            // ReSharper disable once InconsistentNaming — the asmdef JSON key.
            public string name = string.Empty;
        }
    }
}
