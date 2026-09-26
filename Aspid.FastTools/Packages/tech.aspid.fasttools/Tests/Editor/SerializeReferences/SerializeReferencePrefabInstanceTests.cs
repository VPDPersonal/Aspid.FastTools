using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using NUnit.Framework;
using UnityEngine.TestTools;
using System.Collections.Generic;
using Aspid.FastTools.Types.Editors;
using Aspid.FastTools.SerializeReferences.Tests;
using Object = UnityEngine.Object;

namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    /// <summary>
    /// Missing-type detection on objects that are part of a prefab instance — the inherited component of a prefab
    /// variant — whose reference lives in the source prefab or in the variant's overrides, not in a document of its own.
    /// </summary>
    [TestFixture]
    internal sealed class SerializeReferencePrefabInstanceTests
    {
        private const string BasePath = "Assets/__AspidPrefabInstanceBase__.prefab";
        private const string VariantPath = "Assets/__AspidPrefabInstanceVariant__.prefab";

        [TearDown]
        public void TearDown()
        {
            AssetDatabase.DeleteAsset(VariantPath);
            AssetDatabase.DeleteAsset(BasePath);
            ResetProbes();
        }

        [Test]
        public void InheritedMissingType_IsReportedOnTheVariant_AndRepairedInTheSourcePrefab()
        {
            CreateBaseAndVariant(onBase: probe => probe.weapon = new PrefabTestSword { damage = 3 }, onVariant: null);
            BreakType(BasePath, "class: PrefabTestSword,", "class: PrefabTestSwordRemoved,");

            using (var variant = new SerializedObject(LoadProbe(VariantPath)))
            {
                var weapon = variant.FindProperty(nameof(PrefabReferenceProbe.weapon));

                Assert.IsNull(weapon.managedReferenceValue);
                Assert.IsTrue(SerializeReferenceHelpers.IsMissingType(weapon),
                    "A missing type inherited from the source prefab must not read as an empty <None> field.");
                Assert.AreEqual("PrefabTestSwordRemoved", SerializeReferenceHelpers.GetMissingTypeName(weapon).Class);
                Assert.IsFalse(SerializeReferenceHelpers.TryGetRepairLocation(weapon, out _, out _, out _),
                    "The variant has no document for the inherited component, so Fix must not target it.");
                StringAssert.Contains(BasePath, SerializeReferenceHelpers.GetMissingTypeRepairHint(weapon));
            }

            using (var source = new SerializedObject(LoadProbe(BasePath)))
            {
                var weapon = source.FindProperty(nameof(PrefabReferenceProbe.weapon));

                Assert.IsTrue(SerializeReferenceHelpers.IsMissingType(weapon));
                Assert.IsTrue(SerializeReferenceHelpers.TryGetRepairLocation(weapon, out var assetPath, out _, out var inMemory));
                Assert.AreEqual(BasePath, assetPath);
                Assert.IsFalse(inMemory);
            }
        }

        [Test]
        public void OverrideMissingType_IsReportedOnTheVariant_AndGatedAsMissing()
        {
            CreateBaseAndVariant(
                onBase: probe =>
                {
                    probe.weapon = new PrefabTestSword();
                    probe.requiredWeapon = new PrefabTestSword();
                },
                onVariant: probe =>
                {
                    probe.weapon = new PrefabTestBow { arrows = 5 };
                    probe.requiredWeapon = new PrefabTestBow { arrows = 2 };
                });
            // Unity logs an error for each override it cannot apply, at import and at load alike.
            LogAssert.ignoreFailingMessages = true;
            BreakType(VariantPath, "Tests.PrefabTestBow", "Tests.PrefabTestBowRemoved");

            using (var variant = new SerializedObject(LoadProbe(VariantPath)))
            {
                var weapon = variant.FindProperty(nameof(PrefabReferenceProbe.weapon));

                Assert.IsNull(weapon.managedReferenceValue);
                Assert.IsTrue(SerializeReferenceHelpers.IsMissingType(weapon),
                    "A missing type stored in the variant's override must not read as an empty <None> field.");
                Assert.AreEqual("PrefabTestBowRemoved", SerializeReferenceHelpers.GetMissingTypeName(weapon).Class);
                Assert.IsTrue(SerializeReferenceHelpers.TryGetPrefabOverrideMissingType(weapon, out _, out _));
                Assert.IsFalse(SerializeReferenceHelpers.TryGetRepairLocation(weapon, out _, out _, out _));
                StringAssert.Contains("prefab override", SerializeReferenceHelpers.GetMissingTypeRepairHint(weapon));

                var required = variant.FindProperty(nameof(PrefabReferenceProbe.requiredWeapon));
                Assert.IsFalse(TypeSelectorRequiredGate.IsViolation(required),
                    "A present reference with a missing type is set, not unset.");
            }

            AssertRequiredReportedAsMissing(SerializeReferenceGateScanner.Scan(GateOptions.Full), "Scan(Full)");
            AssertRequiredReportedAsMissing(SerializeReferenceGateScanner.Scan(GateOptions.RequiredOnly), "Scan(RequiredOnly)");
            AssertRequiredReportedAsMissing(SerializeReferenceGateScanner.ScanAssetRequiredFields(VariantPath), "ScanAssetRequiredFields");
        }

        // Every required scan must report it: the missing-type document scan never sees a type inside an override.
        private static void AssertRequiredReportedAsMissing(IReadOnlyList<GateViolation> violations, string scan)
        {
            var forField = violations
                .Where(v => v.AssetPath == VariantPath && v.FieldPath == nameof(PrefabReferenceProbe.requiredWeapon))
                .ToList();

            Assert.AreEqual(1, forField.Count, $"{scan} must report the required field exactly once.");
            Assert.AreEqual(GateViolationKind.MissingType, forField[0].Kind, scan);
            Assert.AreEqual("PrefabTestBowRemoved", forField[0].StoredType.Class, scan);
        }

        [Test]
        public void OverrideToNone_HidesTheSourcePrefabMissingType()
        {
            CreateBaseAndVariant(
                onBase: probe => probe.weapon = new PrefabTestSword(),
                onVariant: probe => probe.weapon = null);
            BreakType(BasePath, "class: PrefabTestSword,", "class: PrefabTestSwordRemoved,");

            using var variant = new SerializedObject(LoadProbe(VariantPath));
            var weapon = variant.FindProperty(nameof(PrefabReferenceProbe.weapon));

            Assert.IsNull(weapon.managedReferenceValue);
            Assert.IsFalse(SerializeReferenceHelpers.IsMissingType(weapon),
                "The variant overrides the field to <None>, so the source prefab's missing type does not reach it.");
        }

        [Test]
        public void ParseManagedReferenceTypename_SplitsNamespaceBeforeNestedAndGenericSegments()
        {
            var nested = SerializeReferenceHelpers.ParseManagedReferenceTypename("Asm Game.Weapons.Outer/Inner");
            Assert.AreEqual("Asm", nested.Assembly);
            Assert.AreEqual("Game.Weapons", nested.Namespace);
            Assert.AreEqual("Outer/Inner", nested.Class);

            var generic = SerializeReferenceHelpers.ParseManagedReferenceTypename("Asm Game.Box`1[[Other.Item, Lib]]");
            Assert.AreEqual("Game", generic.Namespace);
            Assert.AreEqual("Box`1[[Other.Item, Lib]]", generic.Class);

            var global = SerializeReferenceHelpers.ParseManagedReferenceTypename("Asm Sword");
            Assert.AreEqual(string.Empty, global.Namespace);
            Assert.AreEqual("Sword", global.Class);

            Assert.IsTrue(SerializeReferenceHelpers.ParseManagedReferenceTypename("NoTypeName").IsEmpty);
        }

        private static void CreateBaseAndVariant(Action<PrefabReferenceProbe> onBase, Action<PrefabReferenceProbe> onVariant)
        {
            var go = new GameObject("Base");
            try
            {
                onBase(go.AddComponent<PrefabReferenceProbe>());
                PrefabUtility.SaveAsPrefabAsset(go, BasePath);
            }
            finally { Object.DestroyImmediate(go); }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(BasePath));
            try
            {
                onVariant?.Invoke(instance.GetComponent<PrefabReferenceProbe>());
                PrefabUtility.SaveAsPrefabAsset(instance, VariantPath);
            }
            finally { Object.DestroyImmediate(instance); }
        }

        // Renames the stored type in the file, the state a deleted or renamed class leaves behind.
        private static void BreakType(string path, string from, string to)
        {
            var text = File.ReadAllText(path);
            StringAssert.Contains(from, text, $"{path} must store the type the test breaks.");
            File.WriteAllText(path, text.Replace(from, to));

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            AssetDatabase.ImportAsset(VariantPath, ImportAssetOptions.ForceUpdate);
            ResetProbes();
        }

        private static PrefabReferenceProbe LoadProbe(string path) =>
            AssetDatabase.LoadAssetAtPath<GameObject>(path).GetComponent<PrefabReferenceProbe>();

        private static void ResetProbes()
        {
            SerializeReferenceYamlProbeCache.ClearCache();
            SerializeReferenceHelpers.InvalidateMissingTypeMemo();
        }
    }
}
