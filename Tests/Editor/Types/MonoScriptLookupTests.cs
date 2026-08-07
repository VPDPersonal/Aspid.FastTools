using NUnit.Framework;

namespace Aspid.FastTools.Types.Editors.Tests
{
    /// <summary>
    /// Coverage for <see cref="TypeExtensions.FindMonoScript"/> on nested types — what the field's open-script
    /// button resolves. A nested type owns no script asset, so without the walk out to its declaring type the
    /// button silently does nothing.
    /// </summary>
    [TestFixture]
    internal sealed class MonoScriptLookupTests
    {
        [Test]
        public void FindMonoScript_NestedType_ResolvesToTheDeclaringTypeScript()
        {
            var script = typeof(MonoScriptLookupFixtures.Concealed).FindMonoScript();

            Assert.IsNotNull(script, "A nested type must resolve to the script its declaration lives in.");
            StringAssert.Contains(nameof(MonoScriptLookupFixtures), UnityEditor.AssetDatabase.GetAssetPath(script));
        }

        [Test]
        public void FindMonoScript_TopLevelType_StillResolvesToItsOwnScript()
        {
            var script = typeof(MonoScriptLookupFixtures).FindMonoScript();

            Assert.IsNotNull(script);
            StringAssert.Contains(nameof(MonoScriptLookupFixtures), UnityEditor.AssetDatabase.GetAssetPath(script));
        }

        [Test]
        public void FindMonoScript_NestedTypeDeclaredElsewhere_ReportsNotFound()
        {
            // Detached sits in the other half of the partial fixture, so the declaring type's script does not
            // contain its declaration. Answering with that script would send the open-script button to line 1 of a
            // file the type is not in — a wrong answer is worse than the warning a null produces.
            Assert.IsNull(typeof(MonoScriptLookupFixtures.Detached).FindMonoScript(),
                "A declaring script that does not declare the nested type must not be offered as its script.");
        }
    }
}
