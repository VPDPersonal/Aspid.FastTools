using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using NUnit.Framework;
using UnityEngine.TestTools;
using System.Collections.Generic;
using Aspid.FastTools.Types.Editors;
using System.Text.RegularExpressions;
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
        private const string VariantOfVariantPath = "Assets/__AspidPrefabInstanceVariantOfVariant__.prefab";

        // The two errors Unity logs for an override whose type is gone. How many of them and in which order differs
        // between Unity versions, so the tests let these through and fail on any other error.
        private static readonly Regex UnappliedOverrideError = new(
            @"^(Could not update a managed instance value at property path 'managedReferences\[\d+\]', with value '.*PrefabTestBowRemoved'"
            + @"|The serialized array of \[SerializeReference\] objects is missing entry for Refid \d+)$");

        private readonly List<string> _unexpectedErrors = new();

        [TearDown]
        public void TearDown()
        {
            Application.logMessageReceived -= CollectUnexpectedError;
            AssetDatabase.DeleteAsset(VariantOfVariantPath);
            AssetDatabase.DeleteAsset(VariantPath);
            AssetDatabase.DeleteAsset(BasePath);
            ResetProbes();

            var unexpected = _unexpectedErrors.ToArray();
            _unexpectedErrors.Clear();
            CollectionAssert.IsEmpty(unexpected, "Only the errors of the unapplied override may be logged.");
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
            AllowUnappliedOverrideErrors();
            BreakType(VariantPath, "Tests.PrefabTestBow", "Tests.PrefabTestBowRemoved");

            using (var variant = new SerializedObject(LoadProbe(VariantPath)))
            {
                var weapon = variant.FindProperty(nameof(PrefabReferenceProbe.weapon));

                Assert.IsNull(weapon.managedReferenceValue);
                Assert.IsTrue(SerializeReferenceHelpers.IsMissingType(weapon),
                    "A missing type stored in the variant's override must not read as an empty <None> field.");
                Assert.AreEqual("PrefabTestBowRemoved", SerializeReferenceHelpers.GetMissingTypeName(weapon).Class);
                Assert.IsFalse(SerializeReferenceHelpers.TryGetRepairLocation(weapon, out _, out _, out _));
                StringAssert.Contains("prefab override", SerializeReferenceHelpers.GetMissingTypeRepairHint(weapon));

                var required = variant.FindProperty(nameof(PrefabReferenceProbe.requiredWeapon));
                Assert.IsFalse(TypeSelectorRequiredGate.IsViolation(required),
                    "A present reference with a missing type is set, not unset.");
            }

            // The missing-type scan reports the override once; the required pass counts the field as set.
            var full = ForField(SerializeReferenceGateScanner.Scan(GateOptions.Full), VariantPath, nameof(PrefabReferenceProbe.requiredWeapon));
            Assert.AreEqual(1, full.Count, "Scan(Full) must report the required field exactly once.");
            Assert.AreEqual(GateViolationKind.MissingType, full[0].Kind);
            Assert.IsTrue(full[0].IsOverride);
            Assert.AreEqual("PrefabTestBowRemoved", full[0].StoredType.Class);

            CollectionAssert.IsEmpty(ForField(SerializeReferenceGateScanner.Scan(GateOptions.RequiredOnly), VariantPath,
                nameof(PrefabReferenceProbe.requiredWeapon)), "A present reference with a missing type is not unset.");
            CollectionAssert.IsEmpty(ForField(SerializeReferenceGateScanner.ScanAssetRequiredFields(VariantPath), VariantPath,
                nameof(PrefabReferenceProbe.requiredWeapon)));
        }

        [Test]
        public void OverrideHigherUpTheChain_IsRepairedInTheAssetThatHoldsIt()
        {
            CreateBaseAndVariant(
                onBase: probe => probe.weapon = new PrefabTestSword(),
                onVariant: probe => probe.weapon = new PrefabTestBow { arrows = 5 });
            CreateVariant(VariantPath, VariantOfVariantPath, onVariant: null);
            AllowUnappliedOverrideErrors();
            BreakType(VariantPath, "Tests.PrefabTestBow", "Tests.PrefabTestBowRemoved");

            using (var variantOfVariant = new SerializedObject(LoadProbe(VariantOfVariantPath)))
            {
                var weapon = variantOfVariant.FindProperty(nameof(PrefabReferenceProbe.weapon));

                Assert.IsTrue(SerializeReferenceHelpers.IsMissingType(weapon));
                Assert.AreEqual("PrefabTestBowRemoved", SerializeReferenceHelpers.GetMissingTypeName(weapon).Class);

                var hint = SerializeReferenceHelpers.GetMissingTypeRepairHint(weapon);
                StringAssert.Contains(VariantPath, hint, "The hint must name the variant that holds the override.");
                StringAssert.DoesNotContain("prefab override", hint, "The variant of the variant has no override to revert.");
            }

            using (var variant = new SerializedObject(LoadProbe(VariantPath)))
            {
                StringAssert.Contains("prefab override",
                    SerializeReferenceHelpers.GetMissingTypeRepairHint(variant.FindProperty(nameof(PrefabReferenceProbe.weapon))));
            }

            var full = SerializeReferenceGateScanner.Scan(GateOptions.Full);
            Assert.AreEqual(1, ForField(full, VariantPath, nameof(PrefabReferenceProbe.weapon)).Count);
            CollectionAssert.IsEmpty(ForField(full, VariantOfVariantPath, nameof(PrefabReferenceProbe.weapon)),
                "The variant of the variant stores nothing to fix.");
        }

        [Test]
        public void NestedOverrideMissingType_IsReportedOnTheVariant()
        {
            CreateBaseAndVariant(
                onBase: probe => probe.holder = new PrefabTestHolder { inner = new PrefabTestSword() },
                onVariant: probe => ((PrefabTestHolder)probe.holder).inner = new PrefabTestBow { arrows = 1 });
            AllowUnappliedOverrideErrors();
            BreakType(VariantPath, "Tests.PrefabTestBow", "Tests.PrefabTestBowRemoved");

            using var variant = new SerializedObject(LoadProbe(VariantPath));
            var inner = variant.FindProperty(InnerPath);

            Assert.IsNull(inner.managedReferenceValue);
            Assert.IsTrue(SerializeReferenceHelpers.IsMissingType(inner),
                "An override inside a managed reference is recorded as managedReferences[rid].inner.\n" + File.ReadAllText(VariantPath));
            Assert.AreEqual("PrefabTestBowRemoved", SerializeReferenceHelpers.GetMissingTypeName(inner).Class);
            StringAssert.Contains("prefab override", SerializeReferenceHelpers.GetMissingTypeRepairHint(inner));
        }

        [Test]
        public void NestedInheritedMissingType_IsReadFromTheSourcePrefab()
        {
            CreateBaseAndVariant(onBase: probe => probe.holder = new PrefabTestHolder { inner = new PrefabTestSword() }, onVariant: null);
            BreakType(BasePath, "class: PrefabTestSword,", "class: PrefabTestSwordRemoved,");

            using var variant = new SerializedObject(LoadProbe(VariantPath));
            var inner = variant.FindProperty(InnerPath);

            Assert.IsTrue(SerializeReferenceHelpers.IsMissingType(inner));
            Assert.AreEqual("PrefabTestSwordRemoved", SerializeReferenceHelpers.GetMissingTypeName(inner).Class);
            StringAssert.Contains(BasePath, SerializeReferenceHelpers.GetMissingTypeRepairHint(inner));
        }

        [Test]
        public void ReplacedEnclosingReference_OwnsItsNestedFields()
        {
            CreateBaseAndVariant(
                onBase: probe => probe.holder = new PrefabTestHolder
                {
                    inner = new PrefabTestSword(),
                    requiredInner = new PrefabTestSword(),
                },
                onVariant: probe => probe.holder = new PrefabTestHolder());
            BreakType(BasePath, "class: PrefabTestSword,", "class: PrefabTestSwordRemoved,");

            using var variant = new SerializedObject(LoadProbe(VariantPath));

            Assert.IsFalse(SerializeReferenceHelpers.IsMissingType(variant.FindProperty(InnerPath)),
                "The variant replaced the holder, so the source prefab's nested data is not its own.\n" + File.ReadAllText(VariantPath));
            Assert.IsTrue(TypeSelectorRequiredGate.IsViolation(variant.FindProperty(RequiredInnerPath)),
                "The replacing holder leaves its required field unset.");
        }

        private const string InnerPath = nameof(PrefabReferenceProbe.holder) + "." + nameof(PrefabTestHolder.inner);
        private const string RequiredInnerPath = nameof(PrefabReferenceProbe.holder) + "." + nameof(PrefabTestHolder.requiredInner);

        private static List<GateViolation> ForField(IReadOnlyList<GateViolation> violations, string assetPath, string fieldPath) =>
            violations.Where(v => v.AssetPath == assetPath && v.FieldPath == fieldPath).ToList();

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

        private static void CreateBaseAndVariant(Action<PrefabReferenceProbe> onBase, Action<PrefabReferenceProbe> onVariant)
        {
            var go = new GameObject("Base");
            try
            {
                onBase(go.AddComponent<PrefabReferenceProbe>());
                PrefabUtility.SaveAsPrefabAsset(go, BasePath);
            }
            finally { Object.DestroyImmediate(go); }

            CreateVariant(BasePath, VariantPath, onVariant);
        }

        private static void CreateVariant(string sourcePath, string variantPath, Action<PrefabReferenceProbe> onVariant)
        {
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(sourcePath));
            try
            {
                onVariant?.Invoke(instance.GetComponent<PrefabReferenceProbe>());
                PrefabUtility.SaveAsPrefabAsset(instance, variantPath);
            }
            finally { Object.DestroyImmediate(instance); }
        }

        private void AllowUnappliedOverrideErrors()
        {
            Application.logMessageReceived += CollectUnexpectedError;
            LogAssert.ignoreFailingMessages = true;
        }

        private void CollectUnexpectedError(string condition, string stackTrace, LogType type)
        {
            if (type is LogType.Log or LogType.Warning) return;
            if (UnappliedOverrideError.IsMatch(condition)) return;

            _unexpectedErrors.Add($"[{type}] {condition}");
        }

        // Renames the stored type in the file, the state a deleted or renamed class leaves behind.
        private static void BreakType(string path, string from, string to)
        {
            var text = File.ReadAllText(path);
            StringAssert.Contains(from, text, $"{path} must store the type the test breaks.");
            File.WriteAllText(path, text.Replace(from, to));

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            AssetDatabase.ImportAsset(VariantPath, ImportAssetOptions.ForceUpdate);
            if (File.Exists(VariantOfVariantPath)) AssetDatabase.ImportAsset(VariantOfVariantPath, ImportAssetOptions.ForceUpdate);
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
