using UnityEditor;
using UnityEngine;
using NUnit.Framework;
using UnityEngine.UIElements;
using Aspid.FastTools.Editors;
using Object = UnityEngine.Object;
using Aspid.FastTools.SerializeReferences.Tests;

namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    // The header of a [SerializeReference] field marks an override on a prefab instance and offers Revert and
    // Apply to Prefab, which Unity gives only to the fields it draws itself.
    [TestFixture]
    internal sealed class SerializeReferencePrefabOverrideTests
    {
        private const string BasePath = "Assets/__AspidPrefabOverrideMenuBase__.prefab";
        private const string VariantPath = "Assets/__AspidPrefabOverrideMenuVariant__.prefab";
        private const string OverrideClass = "aspid-fasttools-serialize-reference--prefab-override";

        private GameObject _instance;
        private PrefabReferenceProbe _probe;

        [SetUp]
        public void SetUp()
        {
            var go = new GameObject("Base");
            try
            {
                go.AddComponent<PrefabReferenceProbe>().weapon = new PrefabTestSword { damage = 1 };
                PrefabUtility.SaveAsPrefabAsset(go, BasePath);
            }
            finally { Object.DestroyImmediate(go); }

            _instance = Instantiate();
            _probe = _instance.GetComponent<PrefabReferenceProbe>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_instance);
            AssetDatabase.DeleteAsset(BasePath);
        }

        [Test]
        public void IsOverridden_UntouchedInstance_IsFalse()
        {
            using var serialized = new SerializedObject(_probe);

            Assert.IsFalse(SerializeReferencePrefabOverride.IsOverridden(serialized.FindProperty(nameof(PrefabReferenceProbe.weapon))));
        }

        [Test]
        public void IsOverridden_ChangedType_IsTrue()
        {
            Override(_probe, new PrefabTestBow { arrows = 3 });

            using var serialized = new SerializedObject(_probe);

            Assert.IsTrue(SerializeReferencePrefabOverride.IsOverridden(serialized.FindProperty(nameof(PrefabReferenceProbe.weapon))));
        }

        [Test]
        public void IsOverridden_ObjectOutsideAPrefab_IsFalse()
        {
            var plain = new GameObject("Plain");
            try
            {
                var probe = plain.AddComponent<PrefabReferenceProbe>();
                probe.weapon = new PrefabTestBow();

                using var serialized = new SerializedObject(probe);

                Assert.IsFalse(SerializeReferencePrefabOverride.IsOverridden(serialized.FindProperty(nameof(PrefabReferenceProbe.weapon))));
            }
            finally { Object.DestroyImmediate(plain); }
        }

        [Test]
        public void IsOverridden_SeveralObjectsSelected_IsFalse()
        {
            var other = Instantiate();
            try
            {
                var otherProbe = other.GetComponent<PrefabReferenceProbe>();
                Override(_probe, new PrefabTestBow { arrows = 3 });
                Override(otherProbe, new PrefabTestBow { arrows = 4 });

                using var serialized = new SerializedObject(new Object[] { _probe, otherProbe });

                Assert.IsFalse(SerializeReferencePrefabOverride.IsOverridden(serialized.FindProperty(nameof(PrefabReferenceProbe.weapon))),
                    "With several objects the override state is not one value, so there is nothing to bold or revert.");
            }
            finally { Object.DestroyImmediate(other); }
        }

        // The USS rule that bolds the label matches this exact chain: field > foldout > toggle > label.
        [Test]
        public void Field_OverriddenOnAnInstance_BoldsTheHeaderLabel()
        {
            using var serialized = new SerializedObject(_probe);
            var weapon = serialized.FindProperty(nameof(PrefabReferenceProbe.weapon));

            Assert.IsFalse(SerializeReferenceEditorGUI.CreateField(weapon).ClassListContains(OverrideClass));

            Override(_probe, new PrefabTestBow { arrows = 3 });
            serialized.Update();

            var field = SerializeReferenceEditorGUI.CreateField(weapon);
            Assert.IsTrue(field.ClassListContains(OverrideClass));

            var toggle = field.Q(className: "unity-foldout__toggle");
            var label = toggle.Q(className: "unity-base-field__label");
            Assert.AreSame(toggle, label.parent);
            Assert.IsTrue(toggle.parent.ClassListContains("unity-foldout"));
            Assert.AreSame(field, toggle.parent.parent);
        }

        // The Overrides dropdown applies the whole instance: the value stays, the override goes, and
        // TrackPropertyValue never fires, so the field listens to the prefab update itself.
        [Test]
        public void Field_ApplyDoneElsewhere_DropsTheBoldLabelOnPrefabInstanceUpdate()
        {
            Override(_probe, new PrefabTestBow { arrows = 3 });

            using var serialized = new SerializedObject(_probe);
            var field = (SerializeReferenceField)SerializeReferenceEditorGUI.CreateField(
                serialized.FindProperty(nameof(PrefabReferenceProbe.weapon)));
            Assert.IsTrue(field.ClassListContains(OverrideClass));

            PrefabUtility.ApplyPrefabInstance(_instance, InteractionMode.AutomatedAction);
            field.OnPrefabInstanceUpdated(_instance);

            Assert.IsFalse(field.ClassListContains(OverrideClass));
        }

        [Test]
        public void Revert_ReturnsTheClassAndDataOfThePrefab()
        {
            Override(_probe, new PrefabTestBow { arrows = 3 });

            using (var serialized = new SerializedObject(_probe))
                SerializeReferencePrefabOverride.Revert(serialized.FindProperty(nameof(PrefabReferenceProbe.weapon)));

            Assert.IsInstanceOf<PrefabTestSword>(_probe.weapon);
            Assert.AreEqual(1, ((PrefabTestSword)_probe.weapon).damage);

            using (var serialized = new SerializedObject(_probe))
                Assert.IsFalse(SerializeReferencePrefabOverride.IsOverridden(serialized.FindProperty(nameof(PrefabReferenceProbe.weapon))));
        }

        [Test]
        public void Apply_WritesTheClassAndDataIntoThePrefab()
        {
            Override(_probe, new PrefabTestBow { arrows = 4 });

            using (var serialized = new SerializedObject(_probe))
            {
                var weapon = serialized.FindProperty(nameof(PrefabReferenceProbe.weapon));

                Assert.IsTrue(SerializeReferencePrefabOverride.TryGetApplyTarget(weapon, out var assetPath));
                Assert.AreEqual(BasePath, assetPath);
                Assert.AreEqual("Apply to Prefab '__AspidPrefabOverrideMenuBase__'", SerializeReferencePrefabOverride.GetApplyLabel(assetPath));

                SerializeReferencePrefabOverride.Apply(weapon, assetPath);
            }

            var source = AssetDatabase.LoadAssetAtPath<GameObject>(BasePath).GetComponent<PrefabReferenceProbe>();
            Assert.IsInstanceOf<PrefabTestBow>(source.weapon);
            Assert.AreEqual(4, ((PrefabTestBow)source.weapon).arrows);

            using (var serialized = new SerializedObject(_probe))
                Assert.IsFalse(SerializeReferencePrefabOverride.IsOverridden(serialized.FindProperty(nameof(PrefabReferenceProbe.weapon))));
        }

        // Apply throws on an object that is an asset, so the menu must not offer it; Revert works there.
        [Test]
        public void VariantAsset_OverriddenField_OffersRevertButNotApply()
        {
            Override(_probe, new PrefabTestBow { arrows = 3 });
            PrefabUtility.SaveAsPrefabAsset(_instance, VariantPath);

            try
            {
                var variantProbe = AssetDatabase.LoadAssetAtPath<GameObject>(VariantPath).GetComponent<PrefabReferenceProbe>();

                using var serialized = new SerializedObject(variantProbe);
                var weapon = serialized.FindProperty(nameof(PrefabReferenceProbe.weapon));

                Assert.IsTrue(SerializeReferencePrefabOverride.IsOverridden(weapon));
                Assert.IsFalse(SerializeReferencePrefabOverride.TryGetApplyTarget(weapon, out _));

                SerializeReferencePrefabOverride.Revert(weapon);
                serialized.Update();

                Assert.IsFalse(SerializeReferencePrefabOverride.IsOverridden(weapon));
            }
            finally { AssetDatabase.DeleteAsset(VariantPath); }
        }

        [Test]
        public void TryGetApplyTarget_ObjectOutsideAPrefab_ReturnsFalse()
        {
            var plain = new GameObject("Plain");
            try
            {
                using var serialized = new SerializedObject(plain.AddComponent<PrefabReferenceProbe>());

                Assert.IsFalse(SerializeReferencePrefabOverride.TryGetApplyTarget(
                    serialized.FindProperty(nameof(PrefabReferenceProbe.weapon)), out _));
            }
            finally { Object.DestroyImmediate(plain); }
        }

        private static GameObject Instantiate() =>
            (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(BasePath));

        private static void Override(PrefabReferenceProbe probe, IPrefabTestWeapon weapon)
        {
            using var serialized = new SerializedObject(probe);
            serialized.FindProperty(nameof(PrefabReferenceProbe.weapon)).SetManagedReferenceAndApply(weapon);
        }
    }
}
