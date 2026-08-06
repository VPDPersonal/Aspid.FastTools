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
    }
}
