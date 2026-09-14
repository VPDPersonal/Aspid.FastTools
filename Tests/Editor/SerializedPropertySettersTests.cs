using System;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Aspid.FastTools.Editors.Tests
{
    internal sealed class SerializedPropertySettersTests
    {
        private SetterTarget _target;
        private SerializedObject _serializedObject;
        private int _undoGroup;

        [SetUp]
        public void SetUp()
        {
            Undo.IncrementCurrentGroup();
            _undoGroup = Undo.GetCurrentGroup();
            _target = ScriptableObject.CreateInstance<SetterTarget>();
            _serializedObject = new SerializedObject(_target);
        }

        [TearDown]
        public void TearDown()
        {
            Undo.RevertAllDownToGroup(_undoGroup);
            _serializedObject.Dispose();
            Object.DestroyImmediate(_target);
            Undo.IncrementCurrentGroup();
        }

        [TestCase(false)]
        [TestCase(true)]
        public void WithoutUndo_AppliesPendingValuesAndReturnsSameProperty(bool useOverload)
        {
            var property = _serializedObject.FindProperty(nameof(SetterTarget.Count));
            _serializedObject.FindProperty(nameof(SetterTarget.Weight)).SetFloat(0.5f);

            var result = useOverload
                ? property.SetValueAndApplyWithoutUndo(42)
                : property.SetIntAndApplyWithoutUndo(42);

            Assert.AreSame(property, result);
            Assert.AreEqual(42, _target.Count);
            Assert.AreEqual(0.5f, _target.Weight);
            Assert.IsFalse(_serializedObject.hasModifiedProperties);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void WithoutUndo_LeavesValueWhenUndoRevertsRecordedChange(bool useOverload)
        {
            var control = ScriptableObject.CreateInstance<SetterTarget>();
            try
            {
                using var controlObject = new SerializedObject(control);
                controlObject.FindProperty(nameof(SetterTarget.Count)).SetIntAndApply(7);
                var property = _serializedObject.FindProperty(nameof(SetterTarget.Count));
                if (useOverload)
                    property.SetValueAndApplyWithoutUndo(42);
                else
                    property.SetIntAndApplyWithoutUndo(42);

                Undo.FlushUndoRecordObjects();
                Undo.RevertAllDownToGroup(_undoGroup);

                Assert.AreEqual(0, control.Count, "The regular AndApply must be undoable.");
                Assert.AreEqual(42, _target.Count, "The WithoutUndo write must remain applied.");
            }
            finally
            {
                Undo.ClearUndo(control);
                Object.DestroyImmediate(control);
            }
        }

        [Test]
        public void WithoutUndo_AppliesToEverySelectedObject()
        {
            var other = ScriptableObject.CreateInstance<SetterTarget>();
            try
            {
                using var selection = new SerializedObject(new Object[] { _target, other });
                selection.FindProperty(nameof(SetterTarget.Count)).SetIntAndApplyWithoutUndo(42);

                Assert.AreEqual(42, _target.Count);
                Assert.AreEqual(42, other.Count);
            }
            finally
            {
                Object.DestroyImmediate(other);
            }
        }

        [Test]
        public void ArrayWithoutUndo_AppliesSizeAndPreservesDefaultIncrements()
        {
            var items = _serializedObject.FindProperty(nameof(SetterTarget.Items));
            Assert.AreSame(items, items.SetArraySizeAndApplyWithoutUndo(3));
            Assert.AreEqual(3, _target.Items.Length);
            Assert.AreSame(items, items.AddArraySizeAndApplyWithoutUndo());
            Assert.AreEqual(4, _target.Items.Length);
            Assert.AreSame(items, items.RemoveArraySizeAndApplyWithoutUndo(2));
            Assert.AreEqual(2, _target.Items.Length);
            items.RemoveArraySizeAndApplyWithoutUndo();
            Assert.AreEqual(1, _target.Items.Length);
        }

        [Test]
        public void ReferenceAndBoxedWithoutUndo_ApplyAssignedValues()
        {
            var reference = _serializedObject.FindProperty(nameof(SetterTarget.Reference));
            var managed = _serializedObject.FindProperty(nameof(SetterTarget.Managed));
            var count = _serializedObject.FindProperty(nameof(SetterTarget.Count));
            var value = new ManagedValue { Number = 7 };

            Assert.AreSame(reference, reference.SetObjectReferenceAndApplyWithoutUndo(_target));
            Assert.AreSame(managed, managed.SetManagedReferenceAndApplyWithoutUndo(value));
            Assert.AreSame(count, count.SetBoxedAndApplyWithoutUndo(42));
            Assert.AreSame(_target, _target.Reference);
            Assert.AreSame(value, _target.Managed);
            Assert.AreEqual(42, _target.Count);
        }

        [Test]
        public void EveryAndApplyOverload_HasMatchingWithoutUndoSignature()
        {
            var methods = typeof(SerializePropertyExtensions).GetMethods()
                .Where(method => method.Name.EndsWith("AndApply", StringComparison.Ordinal))
                .ToArray();
            Assert.IsNotEmpty(methods);
            foreach (var method in methods)
            {
                var original = method.MakeGenericMethod(typeof(SerializedProperty));
                var counterpart = typeof(SerializePropertyExtensions).GetMethods()
                    .Where(candidate => candidate.Name == method.Name + "WithoutUndo")
                    .Select(candidate => candidate.MakeGenericMethod(typeof(SerializedProperty)))
                    .SingleOrDefault(candidate => candidate.GetParameters().Select(p => p.ParameterType)
                        .SequenceEqual(original.GetParameters().Select(p => p.ParameterType)));

                Assert.IsNotNull(counterpart, original.ToString());
                Assert.AreEqual(original.ReturnType, counterpart.ReturnType);
                CollectionAssert.AreEqual(original.GetParameters().Select(p => p.DefaultValue),
                    counterpart.GetParameters().Select(p => p.DefaultValue));
            }
        }

        private sealed class SetterTarget : ScriptableObject
        {
            public int Count = 0;
            public float Weight = 0;
            public int[] Items = Array.Empty<int>();
            public Object Reference = null;
            [SerializeReference] public ManagedValue Managed = null;
        }

        [Serializable]
        private sealed class ManagedValue
        {
            public int Number;
        }
    }
}
