using System;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using NUnit.Framework;
using UnityEngine.UIElements;
using Aspid.FastTools.SerializeReferences.Editors;

namespace Aspid.FastTools.Types.Editors.Tests
{
    /// <summary>
    /// Guards the script-backed wrappers: the code-side contract (base type, implicit conversion), the editor
    /// utility that maps types to their script assets and writes a wrapper property, and the editor-side sync
    /// that re-reads the stored type name from the referenced script.
    /// </summary>
    [TestFixture]
    internal sealed class SerializableMonoScriptTests
    {
        // A type of the package's runtime assembly declared in a file of its own name — exactly the shape
        // MonoScript.GetClass() reports, so it is guaranteed to have a script asset.
        private static readonly Type ScriptedType = typeof(SerializableMonoScript);

        // A scripted type the constrained wrapper accepts.
        private static readonly Type ConstrainedType = typeof(SerializableType);

        private sealed class Holder : ScriptableObject
        {
            // The wrappers have no public constructor: only Unity's serializer creates them.
            [SerializeField] public SerializableMonoScript wrapper;

            [TypeSelector(Required = true)]
            [SerializeField] public SerializableMonoScript<SerializableType> required;

            // A plain wrapper, the picker's contrast case: it stores only the name, so any type fits it.
            [TypeSelector(Required = true)]
            [SerializeField] public SerializableType requiredType;
        }

        // Unity's serializer, not a constructor, creates the wrappers, and it only runs once the object is
        // serialized — a freshly created instance still carries null fields.
        private static Holder CreateHolder()
        {
            var holder = ScriptableObject.CreateInstance<Holder>();
            new SerializedObject(holder).Update();
            return holder;
        }

        // The script reference is a private editor-only field, so a test reads it the way the drawers do.
        private static MonoScript ScriptOf(SerializedProperty wrapperProperty) =>
            wrapperProperty.FindPropertyRelative(SerializableMonoScriptUtility.ScriptFieldName).objectReferenceValue as MonoScript;

        // FromJsonOverwrite only deserializes, like loading an asset not saved since a class rename: the wrapper's
        // OnBeforeSerialize gets no chance to re-sync the name.
        private static void LoadWithStaleName(Holder holder)
        {
            const string staleName = "Old.Name, Old";

            SerializableMonoScriptUtility.Assign(new SerializedObject(holder).FindProperty(nameof(Holder.wrapper)), ScriptedType);

            var json = EditorJsonUtility.ToJson(holder).Replace(ScriptedType.AssemblyQualifiedName, staleName);
            EditorJsonUtility.FromJsonOverwrite(json, holder);

            Assert.AreEqual(staleName, holder.wrapper.AssemblyQualifiedName, "Precondition: the stored name is stale.");
        }

        // Writes the two serialized fields directly, so a test can reach states Assign never produces.
        private static void Store(Holder holder, string assemblyQualifiedName, MonoScript script)
        {
            using var serialized = new SerializedObject(holder);
            var wrapper = serialized.FindProperty(nameof(Holder.wrapper));

            wrapper.FindPropertyRelative(SerializableMonoScriptUtility.ScriptFieldName).objectReferenceValue = script;
            wrapper.FindPropertyRelative(SerializableTypeUtility.BackingFieldName).stringValue = assemblyQualifiedName;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        // A fresh SerializedObject per call, as the drawers read a wrapper after Unity re-serialized its target.
        private static bool IsMissing(params Holder[] holders)
        {
            using var serialized = new SerializedObject(holders);
            return TypeMissingRepair.IsMissingMonoScript(serialized.FindProperty(nameof(Holder.wrapper)));
        }

        [Test]
        public void ImplicitConversion_NullWrapper_YieldsNull()
        {
            SerializableMonoScript plain = null;
            SerializableMonoScript<IComparable> constrained = null;

            Assert.IsNull((Type)plain);
            Assert.IsNull((Type)constrained);
        }

        [Test]
        public void ConstrainedWrapper_IsAMonoScriptWrapper()
        {
            var holder = CreateHolder();
            try
            {
                var serialized = new SerializedObject(holder);
                SerializableMonoScriptUtility.Assign(serialized.FindProperty(nameof(Holder.required)), ConstrainedType);

                SerializableMonoScript wrapper = holder.required;

                Assert.AreEqual(typeof(SerializableType), wrapper.BaseType, "BaseType must stay virtual through the base reference.");
                Assert.AreEqual(ConstrainedType, (Type)wrapper);
            }
            finally { UnityEngine.Object.DestroyImmediate(holder); }
        }

        [Test]
        public void Wrappers_ExposeTheirBaseType()
        {
            var holder = CreateHolder();
            try
            {
                Assert.AreEqual(typeof(object), holder.wrapper.BaseType);
                Assert.AreEqual(typeof(SerializableType), holder.required.BaseType);
            }
            finally { UnityEngine.Object.DestroyImmediate(holder); }

            Assert.IsTrue(SerializableTypeUtility.TryGetBaseType(typeof(SerializableMonoScript<Exception>[]), out var baseType));
            Assert.AreEqual(typeof(Exception), baseType);
        }

        [Test]
        public void Utility_RecognisesWrapperFields()
        {
            Assert.IsTrue(SerializableMonoScriptUtility.IsMonoScriptWrapperField(typeof(SerializableMonoScript)));
            Assert.IsTrue(SerializableMonoScriptUtility.IsMonoScriptWrapperField(typeof(SerializableMonoScript<Exception>[])));
            Assert.IsFalse(SerializableMonoScriptUtility.IsMonoScriptWrapperField(typeof(SerializableType)));
            Assert.IsTrue(SerializableTypeUtility.IsSerializableTypeField(typeof(SerializableMonoScript)), "The gate treats it as a type wrapper.");
        }

        [Test]
        public void ScriptsByType_ContainsAScriptedRuntimeType_ButNotANestedOne()
        {
            Assert.IsTrue(SerializableMonoScriptUtility.TryGetScript(ScriptedType, out var script));
            Assert.AreEqual(ScriptedType, script.GetClass());
            Assert.IsFalse(SerializableMonoScriptUtility.HasScript(typeof(Holder)), "A nested type owns no script asset.");
        }

        [Test]
        public void Assign_WritesScriptAndName_AndNullClearsBoth()
        {
            var holder = CreateHolder();
            try
            {
                var serialized = new SerializedObject(holder);
                var wrapper = serialized.FindProperty(nameof(Holder.wrapper));

                SerializableMonoScriptUtility.Assign(wrapper, ScriptedType);
                Assert.AreEqual(ScriptedType, holder.wrapper.Type);
                Assert.AreEqual(ScriptedType, ScriptOf(wrapper)?.GetClass());

                serialized.Update();
                Assert.AreEqual(ScriptedType, SerializableMonoScriptUtility.GetCurrentType(wrapper, out var name));
                Assert.AreEqual(ScriptedType.AssemblyQualifiedName, name);

                SerializableMonoScriptUtility.Assign(wrapper, null);
                Assert.IsNull(holder.wrapper.Type);
                Assert.IsNull(ScriptOf(wrapper));
                Assert.AreEqual(string.Empty, holder.wrapper.AssemblyQualifiedName);
            }
            finally { UnityEngine.Object.DestroyImmediate(holder); }
        }

        [Test]
        public void Serialization_ResyncsTheNameFromTheScript()
        {
            var holder = CreateHolder();
            try
            {
                var serialized = new SerializedObject(holder);
                SerializableMonoScriptUtility.Assign(serialized.FindProperty(nameof(Holder.wrapper)), ScriptedType);

                // Simulate a stale name (what a class rename leaves behind) while the script reference is intact.
                serialized.Update();
                serialized.FindProperty($"{nameof(Holder.wrapper)}.{SerializableTypeUtility.BackingFieldName}").stringValue = "Old.Name, Old";
                serialized.ApplyModifiedProperties();

                // Building a SerializedObject serializes the target, which runs the wrapper's OnBeforeSerialize.
                using var fresh = new SerializedObject(holder);
                var name = fresh.FindProperty($"{nameof(Holder.wrapper)}.{SerializableTypeUtility.BackingFieldName}").stringValue;

                Assert.AreEqual(ScriptedType.AssemblyQualifiedName, name, "The script asset is the source of truth for the stored name.");
                Assert.AreEqual(ScriptedType, holder.wrapper.Type);
            }
            finally { UnityEngine.Object.DestroyImmediate(holder); }
        }

        [Test]
        public void StaleName_LoadedWithoutSerialization_ResolvesFromTheScript()
        {
            var holder = CreateHolder();
            try
            {
                LoadWithStaleName(holder);

                Assert.AreEqual(ScriptedType, holder.wrapper.Type, "The editor falls back to the script's class.");
            }
            finally { UnityEngine.Object.DestroyImmediate(holder); }
        }

        [Test]
        public void StaleName_ReadOffTheMainThread_YieldsNullUntilAMainThreadRead()
        {
            var holder = CreateHolder();
            try
            {
                LoadWithStaleName(holder);

                // MonoScript.GetClass throws off the main thread, so a worker gets no fallback and nothing is cached.
                Assert.IsNull(Task.Run(() => holder.wrapper.Type).Result);
                Assert.IsNull(Task.Run(() => holder.wrapper.Type).Result);
                Assert.AreEqual(ScriptedType, holder.wrapper.Type, "A main-thread read still falls back to the script's class.");
            }
            finally { UnityEngine.Object.DestroyImmediate(holder); }
        }

        [Test]
        public void SyncScriptFromName_PointsTheScriptAtTheWrittenType()
        {
            var holder = CreateHolder();
            try
            {
                var serialized = new SerializedObject(holder);
                var name = serialized.FindProperty($"{nameof(Holder.wrapper)}.{SerializableTypeUtility.BackingFieldName}");
                name.stringValue = ScriptedType.AssemblyQualifiedName;
                serialized.ApplyModifiedProperties();

                SerializableMonoScriptUtility.SyncScriptFromName(name);

                Assert.AreEqual(ScriptedType, ScriptOf(serialized.FindProperty(nameof(Holder.wrapper)))?.GetClass());
                Assert.AreEqual(ScriptedType, holder.wrapper.Type);
            }
            finally { UnityEngine.Object.DestroyImmediate(holder); }
        }

        [Test]
        public void RequiredGate_CoversTheWrapper()
        {
            var holder = CreateHolder();
            try
            {
                var serialized = new SerializedObject(holder);
                var backing = serialized.FindProperty($"{nameof(Holder.required)}.{SerializableTypeUtility.BackingFieldName}");

                Assert.IsTrue(TypeSelectorRequiredGate.IsViolation(backing), "An empty required wrapper is a violation.");

                SerializableMonoScriptUtility.Assign(serialized.FindProperty(nameof(Holder.required)), ScriptedType);
                serialized.Update();

                Assert.IsFalse(TypeSelectorRequiredGate.IsViolation(
                    serialized.FindProperty($"{nameof(Holder.required)}.{SerializableTypeUtility.BackingFieldName}")));
            }
            finally { UnityEngine.Object.DestroyImmediate(holder); }
        }

        [Test]
        public void MissingPredicate_EmptyWrapper_IsNotMissing()
        {
            var holder = CreateHolder();
            try { Assert.IsFalse(IsMissing(holder)); }
            finally { UnityEngine.Object.DestroyImmediate(holder); }
        }

        [Test]
        public void MissingPredicate_UnresolvedNameWithoutScript_IsMissing()
        {
            var holder = CreateHolder();
            try
            {
                Store(holder, "Old.Name, Old", script: null);

                Assert.IsTrue(IsMissing(holder));
            }
            finally { UnityEngine.Object.DestroyImmediate(holder); }
        }

        [Test]
        public void MissingPredicate_NameResolvesWithoutScript_IsNotMissing()
        {
            var holder = CreateHolder();
            try
            {
                Store(holder, typeof(string).AssemblyQualifiedName, script: null);

                Assert.IsFalse(SerializableMonoScriptUtility.HasScript(typeof(string)), "Precondition: no script owns the type.");
                Assert.IsFalse(IsMissing(holder));
            }
            finally { UnityEngine.Object.DestroyImmediate(holder); }
        }

        [Test]
        public void MissingPredicate_ScriptResolvesWhileTheNameIsStale_IsNotMissing()
        {
            var holder = CreateHolder();
            try
            {
                LoadWithStaleName(holder);

                Assert.IsFalse(IsMissing(holder), "The script's class keeps the field resolved.");
            }
            finally { UnityEngine.Object.DestroyImmediate(holder); }
        }

        [Test]
        public void MissingPredicate_RepairedByAssign_IsNoLongerMissing()
        {
            var holder = CreateHolder();
            try
            {
                Store(holder, "Old.Name, Old", script: null);
                Assert.IsTrue(IsMissing(holder), "Precondition: the stored name is unresolved.");

                using (var serialized = new SerializedObject(holder))
                    SerializableMonoScriptUtility.Assign(serialized.FindProperty(nameof(Holder.wrapper)), ScriptedType);

                Assert.IsFalse(IsMissing(holder));
            }
            finally { UnityEngine.Object.DestroyImmediate(holder); }
        }

        [Test]
        public void MissingPredicate_DifferentValuesAcrossTargets_IsNotMissing()
        {
            var first = CreateHolder();
            var second = CreateHolder();
            try
            {
                Store(first, "Old.First, Old", script: null);
                Store(second, "Old.Second, Old", script: null);

                Assert.IsTrue(IsMissing(first));
                Assert.IsFalse(IsMissing(first, second), "A mixed selection shows no notice.");

                Store(second, "Old.First, Old", script: null);
                Assert.IsTrue(IsMissing(first, second), "Equal missing values across targets are still missing.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(first);
                UnityEngine.Object.DestroyImmediate(second);
            }
        }

        [Test]
        public void MissingWrapper_ReservesTheNoticeRowInTheImguiHeight()
        {
            var holder = CreateHolder();
            try
            {
                using (var empty = new SerializedObject(holder))
                    Assert.AreEqual(EditorGUIUtility.singleLineHeight,
                        MonoScriptIMGUIPropertyDrawer.GetHeight(empty.FindProperty(nameof(Holder.wrapper))));

                Store(holder, "Old.Name, Old", script: null);

                using var missing = new SerializedObject(holder);
                Assert.Greater(MonoScriptIMGUIPropertyDrawer.GetHeight(missing.FindProperty(nameof(Holder.wrapper))),
                    EditorGUIUtility.singleLineHeight);
            }
            finally { UnityEngine.Object.DestroyImmediate(holder); }
        }

        [Test]
        public void MissingField_ShowsStripeNoticeAndTooltip_AndRepairWritesTheScript()
        {
            const string storedName = "Old.Name, Old";
            var holder = CreateHolder();
            var window = ScriptableObject.CreateInstance<EditorWindow>();
            try
            {
                Store(holder, storedName, script: null);
                window.ShowUtility();

                using var serialized = new SerializedObject(holder);
                var root = MonoScriptUIToolkitPropertyDrawer.Draw(label: "Type",
                    wrapperProperty: serialized.FindProperty(nameof(Holder.wrapper)),
                    allow: TypeAllow.All, types: new[] { typeof(object) }, out var field);
                window.rootVisualElement.Add(root);

                Assert.IsTrue(root.ClassListContains("aspid-fasttools-type-property--missing"));
                Assert.IsNotNull(root.Q(className: "aspid-fasttools-type-property__stripe"));
                Assert.IsNotNull(root.Q(className: "aspid-fasttools-inspector-notice"));
                Assert.AreEqual($"Missing type: {storedName}",
                    field.Q<VisualElement>(className: EnumField.inputUssClassName).tooltip);
                Assert.AreEqual("<Missing Old.Name>", field.Q<TextElement>(className: EnumField.textUssClassName).text);

                // The picker's repair path: the field reports the pick and the drawer writes name and script.
                field.ApplyPicked(assemblyQualifiedName: ScriptedType.AssemblyQualifiedName);

                using var repaired = new SerializedObject(holder);
                var wrapper = repaired.FindProperty(nameof(Holder.wrapper));
                Assert.AreEqual(ScriptedType, ScriptOf(wrapper)?.GetClass());
                Assert.IsFalse(IsMissing(holder));
            }
            finally
            {
                window.rootVisualElement.Clear();
                UnityEngine.Object.DestroyImmediate(window);
                UnityEngine.Object.DestroyImmediate(holder);
            }
        }

        // The Asset References "Assign Required" picker must offer a script-backed wrapper the same types as its
        // inspector, never one without a script asset.
        [Test]
        public void RequiredPickerFilter_MonoScriptWrapper_OffersOnlyScriptBackedTypes()
        {
            var holder = CreateHolder();
            try
            {
                var serialized = new SerializedObject(holder);
                var monoScript = serialized.FindProperty($"{nameof(Holder.required)}.{SerializableTypeUtility.BackingFieldName}");
                var plain = serialized.FindProperty($"{nameof(Holder.requiredType)}.{SerializableTypeUtility.BackingFieldName}");

                var filter = SerializeReferenceGraphView.BuildRequiredStringFilter(serialized, monoScript);

                Assert.IsNotNull(filter.Predicate, "A script-backed wrapper needs the script predicate.");
                Assert.IsTrue(filter.Predicate(ScriptedType));
                Assert.IsFalse(filter.Predicate(typeof(Holder)), "A nested type owns no script asset.");

                Assert.IsNull(SerializeReferenceGraphView.BuildRequiredStringFilter(serialized, plain).Predicate,
                    "A plain SerializableType accepts types without a script.");
            }
            finally { UnityEngine.Object.DestroyImmediate(holder); }
        }
    }
}
