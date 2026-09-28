using System.IO;
using UnityEditor;
using UnityEngine;
using NUnit.Framework;
using Object = UnityEngine.Object;

namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    // The current mark of the [SerializeReference] dropdown: a missing type must not preselect <None>, which Enter
    // right after opening would apply, clearing the reference and the data a Fix could still recover.
    [TestFixture]
    internal sealed class SerializeReferenceSelectorCurrentTests
    {
        private const string ProbeAssetPath = "Assets/__AspidSelectorCurrentProbe__.asset";

        [Test]
        public void GetSelectorCurrentAqn_MissingType_MarksNothing()
        {
            var probe = ScriptableObject.CreateInstance<LinkerTestObject>();
            probe.a = new TestSword();
            try
            {
                AssetDatabase.CreateAsset(probe, ProbeAssetPath);
                AssetDatabase.SaveAssets();

                // The saved type no longer resolves and the live value is null, as Unity loads a missing type. The
                // fixture class has no script file of its own, so the asset is not reimported.
                var yaml = File.ReadAllText(ProbeAssetPath);
                StringAssert.Contains("class: TestSword,", yaml);
                File.WriteAllText(ProbeAssetPath, yaml.Replace("class: TestSword,", "class: GoneTestSword,"));
                probe.a = null;

                var serialized = new SerializedObject(probe);
                var missing = serialized.FindProperty(nameof(LinkerTestObject.a));
                Assert.IsTrue(SerializeReferenceHelpers.IsMissingType(missing), "The probe must hold a missing type.");

                Assert.IsNull(SerializeReferenceHelpers.GetSelectorCurrentAqn(missing, null),
                    "A missing type must preselect nothing, not <None>.");
                Assert.AreEqual(string.Empty,
                    SerializeReferenceHelpers.GetSelectorCurrentAqn(serialized.FindProperty(nameof(LinkerTestObject.b)), null),
                    "An empty reference still marks <None>.");
            }
            finally
            {
                AssetDatabase.DeleteAsset(ProbeAssetPath);
            }
        }

        [Test]
        public void GetSelectorCurrentAqn_AssignedType_MarksIt()
        {
            var probe = ScriptableObject.CreateInstance<LinkerTestObject>();
            probe.a = new TestSword();
            try
            {
                var property = new SerializedObject(probe).FindProperty(nameof(LinkerTestObject.a));

                Assert.AreEqual(typeof(TestSword).AssemblyQualifiedName,
                    SerializeReferenceHelpers.GetSelectorCurrentAqn(property, typeof(TestSword)));
            }
            finally
            {
                Object.DestroyImmediate(probe);
            }
        }
    }
}
