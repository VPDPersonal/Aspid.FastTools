using System;
using System.Linq;
using NUnit.Framework;
using Unity.Profiling;
using System.Reflection;
using Aspid.FastTools.Types;
using System.CodeDom.Compiler;

namespace Aspid.FastTools.Editors.Tests
{
    // Checks that the generator DLL committed into the package ran on the runtime assembly. A this.Marker() call
    // compiles without it: it binds to the empty fallback, so the markers vanish and no error appears.
    // The overloads the generator emits are the only trace of a run.
    [TestFixture]
    internal sealed class GeneratedProfilerMarkersTests
    {
        private const string GeneratorName = "Aspid.FastTools.Generators.ProfilerMarkersGenerator";

        private static Type[] GeneratedClasses() => typeof(SerializableTypeBase).Assembly
            .GetTypes()
            .Where(type => type.GetCustomAttributes<GeneratedCodeAttribute>().Any(attribute => attribute.Tool == GeneratorName))
            .ToArray();

        [Test]
        public void RuntimeAssembly_HasTheOverloadsGeneratedForItsMarkerCalls()
        {
            var generated = GeneratedClasses();

            Assert.IsNotEmpty(generated, "The generator did not run on Aspid.FastTools: its this.Marker() calls open no markers.");

            foreach (var type in generated)
            {
                var overloads = type
                    .GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .Where(method => method.Name == "Marker")
                    .ToArray();

                Assert.AreEqual(1, overloads.Length, $"{type.FullName} must declare one Marker overload.");
                Assert.AreEqual(typeof(ProfilerMarker.AutoScope), overloads[0].ReturnType, type.FullName);
            }
        }

#if ENABLE_PROFILER
        [Test]
        public void GeneratedClasses_CreateProfilerMarkers()
        {
            const BindingFlags fields = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly;

            // A generic class keeps its markers in a nested class, one per closed type.
            var markers = GeneratedClasses()
                .SelectMany(type => new[] { type }.Concat(type.GetNestedTypes(BindingFlags.NonPublic)))
                .SelectMany(type => type.GetFields(fields))
                .Count(field => field.FieldType == typeof(ProfilerMarker));

            Assert.Greater(markers, 0, "The generated classes hold no ProfilerMarker fields: the markers are compiled out.");
        }
#endif
    }
}
