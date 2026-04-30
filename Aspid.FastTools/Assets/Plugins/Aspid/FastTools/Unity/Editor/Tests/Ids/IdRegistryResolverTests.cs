#nullable enable
using System.IO;
using UnityEditor;
using UnityEngine;
using NUnit.Framework;
using UnityEngine.TestTools;
using Aspid.FastTools.Ids.Editors;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Ids.EditorTests
{
    [TestFixture]
    internal sealed class IdRegistryResolverTests
    {
        private const string TempFolder = "Assets/__AspidFastToolsTestsTemp";

        [SetUp]
        public void Setup()
        {
            if (!AssetDatabase.IsValidFolder(TempFolder))
                AssetDatabase.CreateFolder("Assets", "__AspidFastToolsTestsTemp");
            IdRegistryResolver.ClearCache();
        }

        [TearDown]
        public void Teardown()
        {
            if (AssetDatabase.IsValidFolder(TempFolder))
                AssetDatabase.DeleteAsset(TempFolder);
            IdRegistryResolver.ClearCache();
        }

        [Test]
        public void Find_NullType_ReturnsNull()
        {
            Assert.IsNull(IdRegistryResolver.Find(null));
        }

        [Test]
        public void Find_NoMatchingRegistry_ReturnsNull()
        {
            Assert.IsNull(IdRegistryResolver.Find(typeof(StubIdAlpha)));
        }

        [Test]
        public void Find_LocatesIntOnlyRegistryByAqn()
        {
            var asset = CreateAt<IdRegistry>("Alpha.asset", typeof(StubIdAlpha));

            var found = IdRegistryResolver.Find(typeof(StubIdAlpha));

            Assert.AreSame(asset, found);
        }

        [Test]
        public void Find_LocatesStringMappedRegistryByAqn()
        {
            var asset = CreateAt<StringIdRegistry>("Beta.asset", typeof(StubIdBeta));

            var found = IdRegistryResolver.Find(typeof(StubIdBeta));

            Assert.AreSame(asset, found);
        }

        [Test]
        public void Find_DuplicateRegistries_LogsErrorAndReturnsFirst()
        {
            CreateAt<IdRegistry>("Dup1.asset", typeof(StubIdGamma));
            CreateAt<StringIdRegistry>("Dup2.asset", typeof(StubIdGamma));

            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("Multiple registries"));

            var found = IdRegistryResolver.Find(typeof(StubIdGamma));
            Assert.IsNotNull(found);
        }

        [Test]
        public void ClearCache_ForcesReread()
        {
            CreateAt<IdRegistry>("Alpha.asset", typeof(StubIdAlpha));
            IdRegistryResolver.Find(typeof(StubIdAlpha));

            IdRegistryResolver.ClearCache();

            Assert.IsNotNull(IdRegistryResolver.Find(typeof(StubIdAlpha)));
        }

        [Test]
        public void RegistryChanged_FiresOnClearCache()
        {
            var fired = 0;
            void Handler() => fired++;
            IdRegistryResolver.RegistryChanged += Handler;
            try
            {
                IdRegistryResolver.ClearCache();
                Assert.AreEqual(1, fired);
            }
            finally
            {
                IdRegistryResolver.RegistryChanged -= Handler;
            }
        }

        [Test]
        public void RegistryChanged_FiresOnAssetImport()
        {
            // Warm up so the resolver is in the "patch index in place" path,
            // where OnAssetImported notifies subscribers.
            IdRegistryResolver.Find(typeof(StubIdAlpha));

            var fired = 0;
            void Handler() => fired++;
            IdRegistryResolver.RegistryChanged += Handler;
            try
            {
                CreateAt<IdRegistry>("AlphaImport.asset", typeof(StubIdAlpha));
                Assert.GreaterOrEqual(fired, 1);
            }
            finally
            {
                IdRegistryResolver.RegistryChanged -= Handler;
            }
        }

        [Test]
        public void RegistryChanged_FiresOnCreateStringMapped()
        {
            IdRegistryResolver.Find(typeof(StubIdDelta));

            var fired = 0;
            void Handler() => fired++;
            IdRegistryResolver.RegistryChanged += Handler;

            StringIdRegistry? created = null;
            try
            {
                created = IdRegistryResolver.CreateStringMapped(typeof(StubIdDelta));
                Assert.GreaterOrEqual(fired, 1);
            }
            finally
            {
                IdRegistryResolver.RegistryChanged -= Handler;
                DeleteIfCreated(created);
            }
        }

        [Test]
        public void GetOrCreateStringMapped_ReturnsExistingWhenPresent()
        {
            var existing = CreateAt<StringIdRegistry>("Existing.asset", typeof(StubIdAlpha));

            var result = IdRegistryResolver.GetOrCreateStringMapped(typeof(StubIdAlpha));

            Assert.AreSame(existing, result);
        }

        [Test]
        public void GetOrCreateStringMapped_CreatesOnceWhenMissing()
        {
            StringIdRegistry? created = null;
            try
            {
                var first = IdRegistryResolver.GetOrCreateStringMapped(typeof(StubIdEpsilon));
                created = first;
                var second = IdRegistryResolver.GetOrCreateStringMapped(typeof(StubIdEpsilon));

                Assert.IsNotNull(first);
                Assert.AreSame(first, second);
            }
            finally
            {
                DeleteIfCreated(created);
            }
        }

        private static void DeleteIfCreated(StringIdRegistry? registry)
        {
            if (registry == null) return;
            var path = AssetDatabase.GetAssetPath(registry);
            if (!string.IsNullOrEmpty(path)) AssetDatabase.DeleteAsset(path);
        }

        private static T CreateAt<T>(string fileName, System.Type targetStruct) where T : ScriptableObject
        {
            var asset = ScriptableObject.CreateInstance<T>();
            var path = Path.Combine(TempFolder, fileName).Replace('\\', '/');
            AssetDatabase.CreateAsset(asset, path);

            var so = new SerializedObject(asset);
            so.FindProperty("_targetStructType").stringValue = targetStruct.AssemblyQualifiedName;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            return asset;
        }

        private struct StubIdAlpha : IId { public int Id => 0; }
        private struct StubIdBeta : IId { public int Id => 0; }
        private struct StubIdGamma : IId { public int Id => 0; }
        private struct StubIdDelta : IId { public int Id => 0; }
        private struct StubIdEpsilon : IId { public int Id => 0; }
    }
}
