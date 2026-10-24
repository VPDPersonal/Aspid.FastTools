using System;
using UnityEditor;
using UnityEngine;
using NUnit.Framework;
using System.Collections;
using UnityEngine.TestTools;
using Aspid.FastTools.Types;
using UnityEngine.UIElements;
using Aspid.FastTools.Editors;
using System.Collections.Generic;
using Object = UnityEngine.Object;
using Aspid.FastTools.Types.Editors;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Tests.Editors.Types
{
    /// <summary>
    /// Covers the callbacks of the Types drawers that run after the property they were set up for may be gone: a
    /// picker, a suggestion or a deferred refresh. They open a copy of the object only when they run
    /// (<see cref="DetachedProperty"/> disposes it), so a destroyed target or a vanished path skips them instead of
    /// throwing, and a cancelled picker holds nothing.
    /// </summary>
    [TestFixture]
    internal sealed class TypeDrawerDetachedCopyTests
    {
        private static readonly Type _scriptedType = typeof(SerializableMonoScript);

        private static readonly string _intName = typeof(int).AssemblyQualifiedName;
        private static readonly string _longName = typeof(long).AssemblyQualifiedName;
        private static readonly string _stringName = typeof(string).AssemblyQualifiedName;

        [Serializable]
        private sealed class Slot
        {
            public string baseName;
            public string typeName;
        }

        private sealed class Holder : ScriptableObject
        {
            public string typeName;
            public List<string> typeNames = new();

            public List<Slot> slots = new();

            // The wrappers have no public constructor: only Unity's serializer creates them.
            public SerializableMonoScript script;
            public List<SerializableMonoScript> scripts = new();
        }

        private Holder _holder;
        private TestPanel _panel;
        private SerializedObject _serialized;

        [SetUp]
        public void SetUp()
        {
            _panel = new TestPanel();
            _holder = CreateHolder();
            _serialized = new SerializedObject(_holder);
        }

        [TearDown]
        public void TearDown()
        {
            _serialized.Dispose();
            _panel.Dispose();
            Object.DestroyImmediate(_holder);
        }

        // Unity's serializer, not a constructor, creates the wrappers and the list elements, so the object goes
        // through a SerializedObject before the tests read it.
        private static Holder CreateHolder()
        {
            var holder = ScriptableObject.CreateInstance<Holder>();

            using var serialized = new SerializedObject(holder);
            serialized.Update();
            serialized.FindProperty(nameof(Holder.typeNames)).arraySize = 2;
            serialized.FindProperty(nameof(Holder.scripts)).arraySize = 2;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            return holder;
        }

        private DetachedProperty Detach(string path) => new(property: _serialized.FindProperty(path));

        // The script reference is a private editor-only field, so a test reads it the way the drawers do.
        private static Type ScriptClassOf(Holder holder, string path)
        {
            using var serialized = new SerializedObject(holder);
            var wrapper = serialized.FindProperty(path);

            return (wrapper.FindPropertyRelative(SerializableMonoScriptUtility.ScriptFieldName)
                .objectReferenceValue as MonoScript)?.GetClass();
        }

        private static string StoredNameOf(Holder holder, string path)
        {
            using var serialized = new SerializedObject(holder);
            return serialized.FindProperty(path).FindPropertyRelative(SerializableTypeUtility.BackingFieldName).stringValue;
        }

        // Destroys the object after the detached property was made, as a picker answers once its inspector is gone.
        private static DetachedProperty DetachFromDestroyedObject(string path)
        {
            var other = CreateHolder();
            DetachedProperty detached;

            using (var source = new SerializedObject(other))
                detached = new DetachedProperty(property: source.FindProperty(path));

            Object.DestroyImmediate(other);
            return detached;
        }

        [Test]
        public void SetName_WritesTheNameThroughACopyOpenedWhenCalled()
        {
            var detached = Detach(path: nameof(Holder.typeName));

            TypeIMGUIPropertyDrawer.SetName(detached: detached, assemblyQualifiedName: _stringName);

            Assert.AreEqual(_stringName, _holder.typeName);
        }

        [Test]
        public void SetName_NullName_WritesAnEmptyName()
        {
            _holder.typeName = _stringName;
            var detached = Detach(path: nameof(Holder.typeName));

            TypeIMGUIPropertyDrawer.SetName(detached: detached, assemblyQualifiedName: null);

            Assert.AreEqual(string.Empty, _holder.typeName);
        }

        [Test]
        public void SetName_WritesOnlyThePathOfTheDetachedProperty()
        {
            _holder.typeNames[0] = _intName;
            var detached = Detach(path: nameof(Holder.typeName));

            TypeIMGUIPropertyDrawer.SetName(detached: detached, assemblyQualifiedName: _longName);

            Assert.AreEqual(_longName, _holder.typeName);
            Assert.AreEqual(_intName, _holder.typeNames[0]);
        }

        [Test]
        public void SetName_ATargetDestroyedMeanwhile_DoesNothing()
        {
            var detached = DetachFromDestroyedObject(path: nameof(Holder.typeName));

            Assert.DoesNotThrow(() => TypeIMGUIPropertyDrawer.SetName(detached: detached, assemblyQualifiedName: _stringName));
        }

        [Test]
        public void SetName_APathThatNoLongerExists_DoesNothing()
        {
            _holder.typeNames[0] = _intName;
            var detached = Detach(path: $"{nameof(Holder.typeNames)}.Array.data[1]");
            _holder.typeNames.RemoveAt(1);

            Assert.DoesNotThrow(() => TypeIMGUIPropertyDrawer.SetName(detached: detached, assemblyQualifiedName: _stringName));
            CollectionAssert.AreEqual(new[] { _intName }, _holder.typeNames);
        }

        [Test]
        public void AssignWrapper_WritesTheNameAndTheScriptThroughACopyOpenedWhenCalled()
        {
            var detached = Detach(path: nameof(Holder.script));

            SerializableMonoScriptUtility.Assign(detached: detached, type: _scriptedType);

            Assert.AreEqual(_scriptedType, ScriptClassOf(holder: _holder, path: nameof(Holder.script)));
            Assert.AreEqual(_scriptedType.AssemblyQualifiedName, StoredNameOf(holder: _holder, path: nameof(Holder.script)));
        }

        [Test]
        public void AssignWrapper_NullType_ClearsTheNameAndTheScript()
        {
            var detached = Detach(path: nameof(Holder.script));
            SerializableMonoScriptUtility.Assign(detached: detached, type: _scriptedType);

            SerializableMonoScriptUtility.Assign(detached: detached, type: null);

            Assert.IsNull(ScriptClassOf(holder: _holder, path: nameof(Holder.script)));
            Assert.AreEqual(string.Empty, StoredNameOf(holder: _holder, path: nameof(Holder.script)));
        }

        [Test]
        public void AssignWrapper_ATargetDestroyedMeanwhile_DoesNothing()
        {
            var detached = DetachFromDestroyedObject(path: nameof(Holder.script));

            Assert.DoesNotThrow(() => SerializableMonoScriptUtility.Assign(detached: detached, type: _scriptedType));
        }

        [Test]
        public void AssignWrapper_APathThatNoLongerExists_DoesNothing()
        {
            var path = $"{nameof(Holder.scripts)}.Array.data[1]";
            var detached = Detach(path: path);
            _holder.scripts.RemoveAt(1);

            Assert.DoesNotThrow(() => SerializableMonoScriptUtility.Assign(detached: detached, type: _scriptedType));
            Assert.AreEqual(1, _holder.scripts.Count);
            Assert.IsNull(ScriptClassOf(holder: _holder, path: $"{nameof(Holder.scripts)}.Array.data[0]"));
        }

        // The refresh of the constraint of a [TypeSelector] field runs long after the drawer was built: it needs the
        // instance that declares the field, which the targets and the path give with no copy of the object.
        [Test]
        public void Resolve_ByDetachedProperty_ReadsTheMemberOfTheDeclaringElementOfAnObjectWithNoOpenCopy()
        {
            _holder.slots.Add(new Slot { baseName = _intName });
            _holder.slots.Add(new Slot { baseName = _stringName });
            _serialized.Update();

            DetachedProperty detached;
            using (var source = new SerializedObject(_holder))
                detached = new DetachedProperty(
                    property: source.FindProperty($"{nameof(Holder.slots)}.Array.data[1].{nameof(Slot.typeName)}"));

            var result = TypeSelectorConstraintResolver.Resolve(
                attributedProperty: detached, assemblyQualifiedNames: new[] { nameof(Slot.baseName) });

            CollectionAssert.AreEqual(new[] { typeof(string) }, result.Types);
            Assert.IsEmpty(result.Warnings);
        }

        [Test]
        public void Resolve_ByDetachedProperty_MatchesTheResolutionByProperty()
        {
            _holder.slots.Add(new Slot { baseName = _longName });
            _serialized.Update();
            var property = _serialized.FindProperty($"{nameof(Holder.slots)}.Array.data[0].{nameof(Slot.typeName)}");

            var byProperty = TypeSelectorConstraintResolver.Resolve(
                attributedProperty: property, assemblyQualifiedNames: new[] { nameof(Slot.baseName), "Missing" });
            var byDetached = TypeSelectorConstraintResolver.Resolve(
                attributedProperty: new DetachedProperty(property: property),
                assemblyQualifiedNames: new[] { nameof(Slot.baseName), "Missing" });

            CollectionAssert.AreEqual(byProperty.Types, byDetached.Types);
            CollectionAssert.AreEqual(byProperty.Warnings, byDetached.Warnings);
        }

        // The drawer of a stored type refreshes from a copy of the object when its field reports a change. The
        // inspector disposes its own SerializedObject when it goes, while such a refresh can still be queued.
        [UnityTest]
        public IEnumerator TypeDrawer_RefreshAfterTheCallersSerializedObjectIsDisposed_ShowsTheStoredType()
        {
            _holder.typeName = _stringName;
            var serialized = new SerializedObject(_holder);

            var root = TypeUIToolkitPropertyDrawer.Draw(
                label: "Type",
                property: serialized.FindProperty(nameof(Holder.typeName)),
                allow: TypeAllow.All,
                types: new[] { typeof(object) },
                field: out var field);
            _panel.Root.Add(root);

            // The panel binds the drawer's trackers on its first updates, so the inspector's object goes after them.
            yield return null;
            yield return null;
            serialized.Dispose();

            field.value = typeof(int);

            yield return WaitUntil(() => field.value == typeof(string));

            Assert.AreEqual(typeof(string), field.value, "The refresh shows the stored type, whatever the field reported.");
        }

        [UnityTest]
        public IEnumerator MonoScriptDrawer_PickAfterTheCallersSerializedObjectIsDisposed_StillWritesTheWrapper()
        {
            var serialized = new SerializedObject(_holder);

            var root = MonoScriptUIToolkitPropertyDrawer.Draw(
                label: "Type",
                wrapperProperty: serialized.FindProperty(nameof(Holder.script)),
                allow: TypeAllow.All,
                types: new[] { typeof(object) },
                field: out var field);
            _panel.Root.Add(root);
            yield return null;
            yield return null;
            serialized.Dispose();

            field.ApplyPicked(assemblyQualifiedName: _scriptedType.AssemblyQualifiedName);

            yield return WaitUntil(() => field.value == _scriptedType);

            Assert.AreEqual(_scriptedType, ScriptClassOf(holder: _holder, path: nameof(Holder.script)));
            Assert.AreEqual(_scriptedType, field.value);
        }

        // The notice follows the object through the trackers of the drawer, with no copy of its own to read from.
        [UnityTest]
        public IEnumerator MonoScriptDrawer_RepairingAMissingType_WritesTheScriptAndClearsTheNotice()
        {
            using (var store = new SerializedObject(_holder))
            {
                var stored = store.FindProperty(nameof(Holder.script));
                stored.FindPropertyRelative(SerializableMonoScriptUtility.ScriptFieldName).objectReferenceValue = null;
                stored.FindPropertyRelative(SerializableTypeUtility.BackingFieldName).stringValue = "Old.Name, Old";
                store.ApplyModifiedPropertiesWithoutUndo();
            }

            using var serialized = new SerializedObject(_holder);
            var root = MonoScriptUIToolkitPropertyDrawer.Draw(
                label: "Type",
                wrapperProperty: serialized.FindProperty(nameof(Holder.script)),
                allow: TypeAllow.All,
                types: new[] { typeof(object) },
                field: out var field);
            _panel.Root.Add(root);

            Assert.IsTrue(root.ClassListContains(TypeUIToolkitPropertyDrawer.MissingClass), "Precondition: the stored type is missing.");

            field.ApplyPicked(assemblyQualifiedName: _scriptedType.AssemblyQualifiedName);

            yield return WaitUntil(() => !root.ClassListContains(TypeUIToolkitPropertyDrawer.MissingClass));

            Assert.IsFalse(root.ClassListContains(TypeUIToolkitPropertyDrawer.MissingClass));
            Assert.AreEqual(_scriptedType, ScriptClassOf(holder: _holder, path: nameof(Holder.script)));
        }

        private static IEnumerator WaitUntil(Func<bool> condition)
        {
            var deadline = EditorApplication.timeSinceStartup + 2d;
            while (!condition() && EditorApplication.timeSinceStartup < deadline)
                yield return null;
        }
    }
}
