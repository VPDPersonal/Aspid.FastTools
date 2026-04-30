#nullable enable
using UnityEditor;
using UnityEngine;
using NUnit.Framework;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Ids.EditorTests
{
    [TestFixture]
    internal sealed class RuntimeRegistryCacheTests
    {
        [Test]
        public void IdRegistry_Contains_ReturnsTrueAfterInvalidate()
        {
            var reg = ScriptableObject.CreateInstance<IdRegistry>();
            try
            {
                SetIdRegistryIds(reg, 1, 7, 42);
                Assert.IsTrue(reg.Contains(1));
                Assert.IsTrue(reg.Contains(7));
                Assert.IsTrue(reg.Contains(42));
                Assert.IsFalse(reg.Contains(99));
                Assert.AreEqual(3, reg.Count);
            }
            finally
            {
                Object.DestroyImmediate(reg);
            }
        }

        [Test]
        public void IdRegistry_InvalidateCache_PicksUpNewIds()
        {
            var reg = ScriptableObject.CreateInstance<IdRegistry>();
            try
            {
                SetIdRegistryIds(reg, 1);
                Assert.IsTrue(reg.Contains(1));
                Assert.IsFalse(reg.Contains(2));

                SetIdRegistryIds(reg, 1, 2);
                reg.InvalidateCache();

                Assert.IsTrue(reg.Contains(2));
            }
            finally
            {
                Object.DestroyImmediate(reg);
            }
        }

        [Test]
        public void StringIdRegistry_Lookup_WorksBothWays()
        {
            var reg = ScriptableObject.CreateInstance<StringIdRegistry>();
            try
            {
                SetEntries(reg, (1, "Goblin"), (2, "Orc"));

                Assert.IsTrue(reg.Contains(1));
                Assert.IsTrue(reg.Contains("Goblin"));
                Assert.IsTrue(reg.TryGetId("Goblin", out var goblinId));
                Assert.AreEqual(1, goblinId);
                Assert.IsTrue(reg.TryGetName(2, out var name));
                Assert.AreEqual("Orc", name);
            }
            finally
            {
                Object.DestroyImmediate(reg);
            }
        }

        [Test]
        public void StringIdRegistry_Misses_ReportFailure()
        {
            var reg = ScriptableObject.CreateInstance<StringIdRegistry>();
            try
            {
                SetEntries(reg, (1, "Goblin"));

                Assert.IsFalse(reg.TryGetId("Unknown", out _));
                Assert.IsFalse(reg.TryGetName(99, out var name));
                Assert.AreEqual(string.Empty, name);
            }
            finally
            {
                Object.DestroyImmediate(reg);
            }
        }

        [Test]
        public void StringIdRegistry_InvalidateCache_RebuildsAfterMutation()
        {
            var reg = ScriptableObject.CreateInstance<StringIdRegistry>();
            try
            {
                SetEntries(reg, (1, "Goblin"));
                Assert.IsTrue(reg.Contains("Goblin"));

                SetEntries(reg, (1, "Goblin"), (2, "Orc"));
                reg.InvalidateCache();

                Assert.IsTrue(reg.Contains("Orc"));
                Assert.IsTrue(reg.TryGetId("Orc", out var orcId));
                Assert.AreEqual(2, orcId);
            }
            finally
            {
                Object.DestroyImmediate(reg);
            }
        }

        private static void SetIdRegistryIds(IdRegistry reg, params int[] ids)
        {
            var so = new SerializedObject(reg);
            var prop = so.FindProperty("_ids");
            prop.arraySize = ids.Length;
            for (var i = 0; i < ids.Length; i++)
                prop.GetArrayElementAtIndex(i).intValue = ids[i];
            so.ApplyModifiedPropertiesWithoutUndo();
            reg.InvalidateCache();
        }

        private static void SetEntries(StringIdRegistry reg, params (int id, string name)[] entries)
        {
            var so = new SerializedObject(reg);
            var prop = so.FindProperty("_entries");
            prop.arraySize = entries.Length;
            for (var i = 0; i < entries.Length; i++)
            {
                var element = prop.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("Id").intValue = entries[i].id;
                element.FindPropertyRelative("Name").stringValue = entries[i].name;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            reg.InvalidateCache();
        }
    }
}
