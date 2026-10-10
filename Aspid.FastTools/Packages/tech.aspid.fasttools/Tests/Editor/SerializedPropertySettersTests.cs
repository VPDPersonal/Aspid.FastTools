using System;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;
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
                var original = Close(method);
                var counterpart = typeof(SerializePropertyExtensions).GetMethods()
                    .Where(candidate => candidate.Name == method.Name + "WithoutUndo")
                    .Select(Close)
                    .SingleOrDefault(candidate => candidate.GetParameters().Select(p => p.ParameterType)
                        .SequenceEqual(original.GetParameters().Select(p => p.ParameterType)));

                Assert.IsNotNull(counterpart, original.ToString());
                Assert.AreEqual(original.ReturnType, counterpart.ReturnType);
                CollectionAssert.AreEqual(original.GetParameters().Select(p => p.DefaultValue),
                    counterpart.GetParameters().Select(p => p.DefaultValue));
            }
        }

        [Test]
        public void IntSetters()
        {
            Verify(nameof(SetterTarget.Count), value: 42, read: target => target.Count,
                set: (property, value) => property.SetInt(value),
                setAndApply: (property, value) => property.SetIntAndApply(value),
                setAndApplyWithoutUndo: (property, value) => property.SetIntAndApplyWithoutUndo(value),
                aliasSet: (property, value) => property.SetValue(value),
                aliasAndApply: (property, value) => property.SetValueAndApply(value),
                aliasAndApplyWithoutUndo: (property, value) => property.SetValueAndApplyWithoutUndo(value));
        }

        [Test]
        public void UintSetters()
        {
            Verify(nameof(SetterTarget.Unsigned), value: 4_000_000_000u, read: target => target.Unsigned,
                set: (property, value) => property.SetUint(value),
                setAndApply: (property, value) => property.SetUintAndApply(value),
                setAndApplyWithoutUndo: (property, value) => property.SetUintAndApplyWithoutUndo(value),
                aliasSet: (property, value) => property.SetValue(value),
                aliasAndApply: (property, value) => property.SetValueAndApply(value),
                aliasAndApplyWithoutUndo: (property, value) => property.SetValueAndApplyWithoutUndo(value));
        }

        [Test]
        public void LongSetters()
        {
            Verify(nameof(SetterTarget.Long), value: 5_000_000_000L, read: target => target.Long,
                set: (property, value) => property.SetLong(value),
                setAndApply: (property, value) => property.SetLongAndApply(value),
                setAndApplyWithoutUndo: (property, value) => property.SetLongAndApplyWithoutUndo(value),
                aliasSet: (property, value) => property.SetValue(value),
                aliasAndApply: (property, value) => property.SetValueAndApply(value),
                aliasAndApplyWithoutUndo: (property, value) => property.SetValueAndApplyWithoutUndo(value));
        }

        [Test]
        public void UlongSetters()
        {
            Verify(nameof(SetterTarget.Ulong), value: 18_000_000_000_000_000_000UL, read: target => target.Ulong,
                set: (property, value) => property.SetUlong(value),
                setAndApply: (property, value) => property.SetUlongAndApply(value),
                setAndApplyWithoutUndo: (property, value) => property.SetUlongAndApplyWithoutUndo(value),
                aliasSet: (property, value) => property.SetValue(value),
                aliasAndApply: (property, value) => property.SetValueAndApply(value),
                aliasAndApplyWithoutUndo: (property, value) => property.SetValueAndApplyWithoutUndo(value));
        }

        [Test]
        public void FloatSetters()
        {
            Verify(nameof(SetterTarget.Weight), value: 0.5f, read: target => target.Weight,
                set: (property, value) => property.SetFloat(value),
                setAndApply: (property, value) => property.SetFloatAndApply(value),
                setAndApplyWithoutUndo: (property, value) => property.SetFloatAndApplyWithoutUndo(value),
                aliasSet: (property, value) => property.SetValue(value),
                aliasAndApply: (property, value) => property.SetValueAndApply(value),
                aliasAndApplyWithoutUndo: (property, value) => property.SetValueAndApplyWithoutUndo(value));
        }

        [Test]
        public void DoubleSetters()
        {
            Verify(nameof(SetterTarget.Precise), value: 0.123456789012345, read: target => target.Precise,
                set: (property, value) => property.SetDouble(value),
                setAndApply: (property, value) => property.SetDoubleAndApply(value),
                setAndApplyWithoutUndo: (property, value) => property.SetDoubleAndApplyWithoutUndo(value),
                aliasSet: (property, value) => property.SetValue(value),
                aliasAndApply: (property, value) => property.SetValueAndApply(value),
                aliasAndApplyWithoutUndo: (property, value) => property.SetValueAndApplyWithoutUndo(value));
        }

        [Test]
        public void BoolSetters()
        {
            Verify(nameof(SetterTarget.Enabled), value: true, read: target => target.Enabled,
                set: (property, value) => property.SetBool(value),
                setAndApply: (property, value) => property.SetBoolAndApply(value),
                setAndApplyWithoutUndo: (property, value) => property.SetBoolAndApplyWithoutUndo(value),
                aliasSet: (property, value) => property.SetValue(value),
                aliasAndApply: (property, value) => property.SetValueAndApply(value),
                aliasAndApplyWithoutUndo: (property, value) => property.SetValueAndApplyWithoutUndo(value));
        }

        // Each narrow integer binds to its own SetValue overload; without it the call is ambiguous with
        // SetValue(EntityId) on Unity 6000.2 and later, where EntityId converts implicitly to and from int.
        [Test]
        public void ByteSetters()
        {
            Verify(nameof(SetterTarget.Byte), value: (byte)200, read: target => target.Byte,
                set: (property, value) => property.SetInt(value),
                setAndApply: (property, value) => property.SetIntAndApply(value),
                setAndApplyWithoutUndo: (property, value) => property.SetIntAndApplyWithoutUndo(value),
                aliasSet: (property, value) => property.SetValue(value),
                aliasAndApply: (property, value) => property.SetValueAndApply(value),
                aliasAndApplyWithoutUndo: (property, value) => property.SetValueAndApplyWithoutUndo(value));
        }

        [Test]
        public void SbyteSetters()
        {
            Verify(nameof(SetterTarget.Sbyte), value: (sbyte)-100, read: target => target.Sbyte,
                set: (property, value) => property.SetInt(value),
                setAndApply: (property, value) => property.SetIntAndApply(value),
                setAndApplyWithoutUndo: (property, value) => property.SetIntAndApplyWithoutUndo(value),
                aliasSet: (property, value) => property.SetValue(value),
                aliasAndApply: (property, value) => property.SetValueAndApply(value),
                aliasAndApplyWithoutUndo: (property, value) => property.SetValueAndApplyWithoutUndo(value));
        }

        [Test]
        public void ShortSetters()
        {
            Verify(nameof(SetterTarget.Short), value: (short)-30000, read: target => target.Short,
                set: (property, value) => property.SetInt(value),
                setAndApply: (property, value) => property.SetIntAndApply(value),
                setAndApplyWithoutUndo: (property, value) => property.SetIntAndApplyWithoutUndo(value),
                aliasSet: (property, value) => property.SetValue(value),
                aliasAndApply: (property, value) => property.SetValueAndApply(value),
                aliasAndApplyWithoutUndo: (property, value) => property.SetValueAndApplyWithoutUndo(value));
        }

        [Test]
        public void UshortSetters()
        {
            Verify(nameof(SetterTarget.Ushort), value: (ushort)60000, read: target => target.Ushort,
                set: (property, value) => property.SetInt(value),
                setAndApply: (property, value) => property.SetIntAndApply(value),
                setAndApplyWithoutUndo: (property, value) => property.SetIntAndApplyWithoutUndo(value),
                aliasSet: (property, value) => property.SetValue(value),
                aliasAndApply: (property, value) => property.SetValueAndApply(value),
                aliasAndApplyWithoutUndo: (property, value) => property.SetValueAndApplyWithoutUndo(value));
        }

        [Test]
        public void CharSetters()
        {
            Verify(nameof(SetterTarget.Letter), value: 'x', read: target => target.Letter,
                set: (property, value) => property.SetInt(value),
                setAndApply: (property, value) => property.SetIntAndApply(value),
                setAndApplyWithoutUndo: (property, value) => property.SetIntAndApplyWithoutUndo(value),
                aliasSet: (property, value) => property.SetValue(value),
                aliasAndApply: (property, value) => property.SetValueAndApply(value),
                aliasAndApplyWithoutUndo: (property, value) => property.SetValueAndApplyWithoutUndo(value));
        }

        [Test]
        public void RectSetters()
        {
            Verify(nameof(SetterTarget.Rect), value: new Rect(1, 2, 3, 4), read: target => target.Rect,
                set: (property, value) => property.SetRect(value),
                setAndApply: (property, value) => property.SetRectAndApply(value),
                setAndApplyWithoutUndo: (property, value) => property.SetRectAndApplyWithoutUndo(value),
                aliasSet: (property, value) => property.SetValue(value),
                aliasAndApply: (property, value) => property.SetValueAndApply(value),
                aliasAndApplyWithoutUndo: (property, value) => property.SetValueAndApplyWithoutUndo(value));
        }

        [Test]
        public void RectIntSetters()
        {
            Verify(nameof(SetterTarget.RectInt), value: new RectInt(1, 2, 3, 4), read: target => target.RectInt,
                set: (property, value) => property.SetRectInt(value),
                setAndApply: (property, value) => property.SetRectIntAndApply(value),
                setAndApplyWithoutUndo: (property, value) => property.SetRectIntAndApplyWithoutUndo(value),
                aliasSet: (property, value) => property.SetValue(value),
                aliasAndApply: (property, value) => property.SetValueAndApply(value),
                aliasAndApplyWithoutUndo: (property, value) => property.SetValueAndApplyWithoutUndo(value));
        }

        [Test]
        public void BoundsSetters()
        {
            Verify(nameof(SetterTarget.Bounds), value: new Bounds(new Vector3(1, 2, 3), new Vector3(4, 5, 6)),
                read: target => target.Bounds,
                set: (property, value) => property.SetBounds(value),
                setAndApply: (property, value) => property.SetBoundsAndApply(value),
                setAndApplyWithoutUndo: (property, value) => property.SetBoundsAndApplyWithoutUndo(value),
                aliasSet: (property, value) => property.SetValue(value),
                aliasAndApply: (property, value) => property.SetValueAndApply(value),
                aliasAndApplyWithoutUndo: (property, value) => property.SetValueAndApplyWithoutUndo(value));
        }

        [Test]
        public void BoundsIntSetters()
        {
            Verify(nameof(SetterTarget.BoundsInt), value: new BoundsInt(new Vector3Int(1, 2, 3), new Vector3Int(4, 5, 6)),
                read: target => target.BoundsInt,
                set: (property, value) => property.SetBoundsInt(value),
                setAndApply: (property, value) => property.SetBoundsIntAndApply(value),
                setAndApplyWithoutUndo: (property, value) => property.SetBoundsIntAndApplyWithoutUndo(value),
                aliasSet: (property, value) => property.SetValue(value),
                aliasAndApply: (property, value) => property.SetValueAndApply(value),
                aliasAndApplyWithoutUndo: (property, value) => property.SetValueAndApplyWithoutUndo(value));
        }

        [Test]
        public void ColorSetters()
        {
            Verify(nameof(SetterTarget.Color), value: new Color(0.1f, 0.2f, 0.3f, 0.4f), read: target => target.Color,
                set: (property, value) => property.SetColor(value),
                setAndApply: (property, value) => property.SetColorAndApply(value),
                setAndApplyWithoutUndo: (property, value) => property.SetColorAndApplyWithoutUndo(value),
                aliasSet: (property, value) => property.SetValue(value),
                aliasAndApply: (property, value) => property.SetValueAndApply(value),
                aliasAndApplyWithoutUndo: (property, value) => property.SetValueAndApplyWithoutUndo(value));
        }

        [Test]
        public void GradientSetters()
        {
            var gradient = new Gradient { mode = GradientMode.Fixed };
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.red, 0f), new GradientColorKey(Color.blue, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.5f, 1f) });

            Verify(nameof(SetterTarget.Gradient), value: gradient, read: target => CopyOf(target.Gradient),
                set: (property, value) => property.SetGradient(value),
                setAndApply: (property, value) => property.SetGradientAndApply(value),
                setAndApplyWithoutUndo: (property, value) => property.SetGradientAndApplyWithoutUndo(value),
                aliasSet: (property, value) => property.SetValue(value),
                aliasAndApply: (property, value) => property.SetValueAndApply(value),
                aliasAndApplyWithoutUndo: (property, value) => property.SetValueAndApplyWithoutUndo(value),
                comparer: new FuncComparer<Gradient>((left, right) =>
                    left.mode == right.mode &&
                    left.colorKeys.SequenceEqual(right.colorKeys) &&
                    left.alphaKeys.SequenceEqual(right.alphaKeys)));
        }

        [Test]
        public void Hash128Setters()
        {
            Verify(nameof(SetterTarget.Hash), value: Hash128.Compute("aspid"), read: target => target.Hash,
                set: (property, value) => property.SetHash128(value),
                setAndApply: (property, value) => property.SetHash128AndApply(value),
                setAndApplyWithoutUndo: (property, value) => property.SetHash128AndApplyWithoutUndo(value),
                aliasSet: (property, value) => property.SetValue(value),
                aliasAndApply: (property, value) => property.SetValueAndApply(value),
                aliasAndApplyWithoutUndo: (property, value) => property.SetValueAndApplyWithoutUndo(value));
        }

        [Test]
        public void Vector4Setters()
        {
            Verify(nameof(SetterTarget.Vector4), value: new Vector4(1, 2, 3, 4), read: target => target.Vector4,
                set: (property, value) => property.SetVector4(value),
                setAndApply: (property, value) => property.SetVector4AndApply(value),
                setAndApplyWithoutUndo: (property, value) => property.SetVector4AndApplyWithoutUndo(value),
                aliasSet: (property, value) => property.SetValue(value),
                aliasAndApply: (property, value) => property.SetValueAndApply(value),
                aliasAndApplyWithoutUndo: (property, value) => property.SetValueAndApplyWithoutUndo(value));
        }

        [Test]
        public void Vector3Setters()
        {
            Verify(nameof(SetterTarget.Vector3), value: new Vector3(1, 2, 3), read: target => target.Vector3,
                set: (property, value) => property.SetVector3(value),
                setAndApply: (property, value) => property.SetVector3AndApply(value),
                setAndApplyWithoutUndo: (property, value) => property.SetVector3AndApplyWithoutUndo(value),
                aliasSet: (property, value) => property.SetValue(value),
                aliasAndApply: (property, value) => property.SetValueAndApply(value),
                aliasAndApplyWithoutUndo: (property, value) => property.SetValueAndApplyWithoutUndo(value));
        }

        [Test]
        public void Vector3IntSetters()
        {
            Verify(nameof(SetterTarget.Vector3Int), value: new Vector3Int(1, 2, 3), read: target => target.Vector3Int,
                set: (property, value) => property.SetVector3Int(value),
                setAndApply: (property, value) => property.SetVector3IntAndApply(value),
                setAndApplyWithoutUndo: (property, value) => property.SetVector3IntAndApplyWithoutUndo(value),
                aliasSet: (property, value) => property.SetValue(value),
                aliasAndApply: (property, value) => property.SetValueAndApply(value),
                aliasAndApplyWithoutUndo: (property, value) => property.SetValueAndApplyWithoutUndo(value));
        }

        [Test]
        public void Vector2Setters()
        {
            Verify(nameof(SetterTarget.Vector2), value: new Vector2(1, 2), read: target => target.Vector2,
                set: (property, value) => property.SetVector2(value),
                setAndApply: (property, value) => property.SetVector2AndApply(value),
                setAndApplyWithoutUndo: (property, value) => property.SetVector2AndApplyWithoutUndo(value),
                aliasSet: (property, value) => property.SetValue(value),
                aliasAndApply: (property, value) => property.SetValueAndApply(value),
                aliasAndApplyWithoutUndo: (property, value) => property.SetValueAndApplyWithoutUndo(value));
        }

        [Test]
        public void Vector2IntSetters()
        {
            Verify(nameof(SetterTarget.Vector2Int), value: new Vector2Int(1, 2), read: target => target.Vector2Int,
                set: (property, value) => property.SetVector2Int(value),
                setAndApply: (property, value) => property.SetVector2IntAndApply(value),
                setAndApplyWithoutUndo: (property, value) => property.SetVector2IntAndApplyWithoutUndo(value),
                aliasSet: (property, value) => property.SetValue(value),
                aliasAndApply: (property, value) => property.SetValueAndApply(value),
                aliasAndApplyWithoutUndo: (property, value) => property.SetValueAndApplyWithoutUndo(value));
        }

        [Test]
        public void QuaternionSetters()
        {
            Verify(nameof(SetterTarget.Rotation), value: Quaternion.Euler(10, 20, 30), read: target => target.Rotation,
                set: (property, value) => property.SetQuaternion(value),
                setAndApply: (property, value) => property.SetQuaternionAndApply(value),
                setAndApplyWithoutUndo: (property, value) => property.SetQuaternionAndApplyWithoutUndo(value),
                aliasSet: (property, value) => property.SetValue(value),
                aliasAndApply: (property, value) => property.SetValueAndApply(value),
                aliasAndApplyWithoutUndo: (property, value) => property.SetValueAndApplyWithoutUndo(value));
        }

        [Test]
        public void StringSetters()
        {
            Verify(nameof(SetterTarget.Text), value: "Fireball", read: target => target.Text,
                set: (property, value) => property.SetString(value),
                setAndApply: (property, value) => property.SetStringAndApply(value),
                setAndApplyWithoutUndo: (property, value) => property.SetStringAndApplyWithoutUndo(value),
                aliasSet: (property, value) => property.SetValue(value),
                aliasAndApply: (property, value) => property.SetValueAndApply(value),
                aliasAndApplyWithoutUndo: (property, value) => property.SetValueAndApplyWithoutUndo(value));
        }

        [Test]
        public void AnimationCurveSetters()
        {
            var curve = AnimationCurve.Linear(0f, 1f, 2f, 5f);

            Verify(nameof(SetterTarget.Curve), value: curve, read: target => CopyOf(target.Curve),
                set: (property, value) => property.SetAnimationCurve(value),
                setAndApply: (property, value) => property.SetAnimationCurveAndApply(value),
                setAndApplyWithoutUndo: (property, value) => property.SetAnimationCurveAndApplyWithoutUndo(value),
                aliasSet: (property, value) => property.SetValue(value),
                aliasAndApply: (property, value) => property.SetValueAndApply(value),
                aliasAndApplyWithoutUndo: (property, value) => property.SetValueAndApplyWithoutUndo(value),
                comparer: new FuncComparer<AnimationCurve>((left, right) => left.keys.SequenceEqual(right.keys)));
        }

        [Test]
        public void EnumSetters()
        {
            Verify(nameof(SetterTarget.Rarity), value: SetterRarity.Rare, read: target => target.Rarity,
                set: (property, value) => property.SetEnum(value),
                setAndApply: (property, value) => property.SetEnumAndApply(value),
                setAndApplyWithoutUndo: (property, value) => property.SetEnumAndApplyWithoutUndo(value),
                aliasSet: (property, value) => property.SetValue(value),
                aliasAndApply: (property, value) => property.SetValueAndApply(value),
                aliasAndApplyWithoutUndo: (property, value) => property.SetValueAndApplyWithoutUndo(value));
        }

        [Test]
        public void EnumFlagSetters()
        {
            Verify(nameof(SetterTarget.Directions), value: (int)(SetterDirections.Up | SetterDirections.Left),
                read: target => (int)target.Directions,
                set: (property, value) => property.SetEnumFlag(value),
                setAndApply: (property, value) => property.SetEnumFlagAndApply(value),
                setAndApplyWithoutUndo: (property, value) => property.SetEnumFlagAndApplyWithoutUndo(value));
        }

        [Test]
        public void EnumIndexSetters()
        {
            // Index 1 of SetterRarity is Rare: the entries are listed by value, and Rare = 20 comes before Epic = 50.
            Verify(nameof(SetterTarget.Rarity), value: SetterRarity.Rare, read: target => target.Rarity,
                set: (property, _) => property.SetEnumIndex(1),
                setAndApply: (property, _) => property.SetEnumIndexAndApply(1),
                setAndApplyWithoutUndo: (property, _) => property.SetEnumIndexAndApplyWithoutUndo(1));
        }

        [Test]
        public void ObjectReferenceSetters()
        {
            var reference = ScriptableObject.CreateInstance<SetterTarget>();
            try
            {
                Verify(nameof(SetterTarget.Reference), value: (Object)reference, read: target => target.Reference,
                    set: (property, value) => property.SetObjectReference(value),
                    setAndApply: (property, value) => property.SetObjectReferenceAndApply(value),
                    setAndApplyWithoutUndo: (property, value) => property.SetObjectReferenceAndApplyWithoutUndo(value),
                    comparer: new FuncComparer<Object>((left, right) => left == right));
            }
            finally
            {
                Object.DestroyImmediate(reference);
            }
        }

        [Test]
        public void ManagedReferenceSetters()
        {
            Verify(nameof(SetterTarget.Managed), value: new ManagedValue { Number = 7 }, read: target => target.Managed,
                set: (property, value) => property.SetManagedReference(value),
                setAndApply: (property, value) => property.SetManagedReferenceAndApply(value),
                setAndApplyWithoutUndo: (property, value) => property.SetManagedReferenceAndApplyWithoutUndo(value),
                comparer: new FuncComparer<ManagedValue>(ReferenceEquals));
        }

        [Test]
        public void BoxedSetters()
        {
            Verify(nameof(SetterTarget.Count), value: (object)42, read: target => target.Count,
                set: (property, value) => property.SetBoxed(value),
                setAndApply: (property, value) => property.SetBoxedAndApply(value),
                setAndApplyWithoutUndo: (property, value) => property.SetBoxedAndApplyWithoutUndo(value));
        }

        [Test]
        public void ArrayAndApply_AppliesSizeAndPreservesDefaultIncrements()
        {
            var items = _serializedObject.FindProperty(nameof(SetterTarget.Items));
            Assert.AreSame(items, items.SetArraySizeAndApply(3));
            Assert.AreEqual(3, _target.Items.Length);
            Assert.AreSame(items, items.AddArraySizeAndApply());
            Assert.AreEqual(4, _target.Items.Length);
            Assert.AreSame(items, items.RemoveArraySizeAndApply(2));
            Assert.AreEqual(2, _target.Items.Length);
            items.RemoveArraySizeAndApply();
            Assert.AreEqual(1, _target.Items.Length);
            Assert.IsFalse(_serializedObject.hasModifiedProperties);
        }

        [Test]
        public void ArraySize_PendingSettersLeaveTheChangeUnapplied()
        {
            var items = _serializedObject.FindProperty(nameof(SetterTarget.Items));

            Assert.AreSame(items, items.SetArraySize(4));
            Assert.AreSame(items, items.AddArraySize(2));
            Assert.AreSame(items, items.RemoveArraySize(1));
            Assert.AreEqual(0, _target.Items.Length);
            Assert.IsTrue(_serializedObject.hasModifiedProperties);

            _serializedObject.ApplyModifiedProperties();
            Assert.AreEqual(5, _target.Items.Length);
        }

        [Test]
        public void EnumIndex_TakesAnIndexAndEnum_TakesTheValue()
        {
            var rarity = _serializedObject.FindProperty(nameof(SetterTarget.Rarity));

            rarity.SetEnumIndexAndApply(1);
            Assert.AreEqual(SetterRarity.Rare, _target.Rarity, "Index 1 is the second entry by value.");

            rarity.SetEnumAndApply(SetterRarity.Epic);
            Assert.AreEqual(SetterRarity.Epic, _target.Rarity, "SetEnum takes the enum value, not an index.");
            Assert.AreEqual(2, rarity.Update().enumValueIndex);

            rarity.SetEnumAndApply(SetterRarity.Common);
            Assert.AreEqual(SetterRarity.Common, _target.Rarity);
        }

        [Test]
        public void Enum_WritesValueOfEveryUnderlyingType()
        {
            _serializedObject.FindProperty(nameof(SetterTarget.Directions))
                .SetEnumAndApply(SetterDirections.Down | SetterDirections.Right);
            _serializedObject.FindProperty(nameof(SetterTarget.SmallRarity)).SetValueAndApply(SetterSmallRarity.Big);

            Assert.AreEqual(SetterDirections.Down | SetterDirections.Right, _target.Directions);
            Assert.AreEqual(SetterSmallRarity.Big, _target.SmallRarity);
        }

        [Test]
        public void ExposedReference_WithContext_KeepsTheNameOnlyAfterApply()
        {
            var table = ScriptableObject.CreateInstance<ExposedTable>();
            var reference = ScriptableObject.CreateInstance<SetterTarget>();
            try
            {
                using var contextual = new SerializedObject(new Object[] { _target }, table);
                var property = contextual.FindProperty(nameof(SetterTarget.Exposed));

                Assert.AreSame(property, property.SetExposedReference(reference));
                Assert.IsTrue(contextual.hasModifiedProperties, "The new exposed name is a pending change.");
                Assert.AreSame(reference, property.exposedReferenceValue);

                property.ApplyModifiedProperties().Update();

                Assert.AreSame(reference, property.exposedReferenceValue);
                Assert.AreSame(reference, _target.Exposed.Resolve(table));
            }
            finally
            {
                Object.DestroyImmediate(table);
                Object.DestroyImmediate(reference);
            }
        }

        [Test]
        public void ExposedReference_WithContext_UpdateDiscardsThePendingName()
        {
            var table = ScriptableObject.CreateInstance<ExposedTable>();
            var reference = ScriptableObject.CreateInstance<SetterTarget>();
            try
            {
                using var contextual = new SerializedObject(new Object[] { _target }, table);
                var property = contextual.FindProperty(nameof(SetterTarget.Exposed));

                property.SetExposedReference(reference).Update();

                Assert.IsNull(property.exposedReferenceValue);
                Assert.IsNull(_target.Exposed.Resolve(table));
            }
            finally
            {
                Object.DestroyImmediate(table);
                Object.DestroyImmediate(reference);
            }
        }

        [Test]
        public void ExposedReference_WithoutContext_AppliesTheWriteItself()
        {
            var reference = ScriptableObject.CreateInstance<SetterTarget>();
            try
            {
                var property = _serializedObject.FindProperty(nameof(SetterTarget.Exposed));

                Assert.AreSame(property, property.SetExposedReference(reference));

                Assert.IsFalse(_serializedObject.hasModifiedProperties);
                Assert.AreSame(reference, _target.Exposed.defaultValue);
            }
            finally
            {
                Object.DestroyImmediate(reference);
            }
        }

#if UNITY_6000_2_OR_NEWER
        [Test]
        public void EntityIdSetters()
        {
            var reference = ScriptableObject.CreateInstance<SetterTarget>();
            try
            {
                Verify(nameof(SetterTarget.Identifier), value: reference.GetEntityId(), read: target => target.Identifier,
                    set: (property, value) => property.SetEntityId(value),
                    setAndApply: (property, value) => property.SetEntityIdAndApply(value),
                    setAndApplyWithoutUndo: (property, value) => property.SetEntityIdAndApplyWithoutUndo(value),
                    aliasSet: (property, value) => property.SetValue(value),
                    aliasAndApply: (property, value) => property.SetValueAndApply(value),
                    aliasAndApplyWithoutUndo: (property, value) => property.SetValueAndApplyWithoutUndo(value));
            }
            finally
            {
                Object.DestroyImmediate(reference);
            }
        }
#endif

#if UNITY_6000_4_OR_NEWER
        [Test]
        public void GuidSetters()
        {
            Verify(nameof(SetterTarget.Guid), value: new GUID("0123456789abcdef0123456789abcdef"), read: target => target.Guid,
                set: (property, value) => property.SetGuid(value),
                setAndApply: (property, value) => property.SetGuidAndApply(value),
                setAndApplyWithoutUndo: (property, value) => property.SetGuidAndApplyWithoutUndo(value),
                aliasSet: (property, value) => property.SetValue(value),
                aliasAndApply: (property, value) => property.SetValueAndApply(value),
                aliasAndApplyWithoutUndo: (property, value) => property.SetValueAndApplyWithoutUndo(value));
        }
#endif

        private delegate SerializedProperty Setter<in TValue>(SerializedProperty property, TValue value);

        // Runs a setter family on a fresh target per call and checks what each variant promises:
        // a plain setter leaves the write pending, AndApply applies it with Undo, AndApplyWithoutUndo applies it without.
        private void Verify<TValue>(
            string field,
            TValue value,
            Func<SetterTarget, TValue> read,
            Setter<TValue> set,
            Setter<TValue> setAndApply,
            Setter<TValue> setAndApplyWithoutUndo,
            Setter<TValue> aliasSet = null,
            Setter<TValue> aliasAndApply = null,
            Setter<TValue> aliasAndApplyWithoutUndo = null,
            IEqualityComparer<TValue> comparer = null)
        {
            comparer ??= EqualityComparer<TValue>.Default;

            Run(field, value, read, comparer, set, Mode.Pending, "Set");
            Run(field, value, read, comparer, setAndApply, Mode.Applied, "SetAndApply");
            Run(field, value, read, comparer, setAndApplyWithoutUndo, Mode.AppliedWithoutUndo, "SetAndApplyWithoutUndo");

            if (aliasSet is not null) Run(field, value, read, comparer, aliasSet, Mode.Pending, "SetValue");
            if (aliasAndApply is not null) Run(field, value, read, comparer, aliasAndApply, Mode.Applied, "SetValueAndApply");
            if (aliasAndApplyWithoutUndo is not null)
                Run(field, value, read, comparer, aliasAndApplyWithoutUndo, Mode.AppliedWithoutUndo, "SetValueAndApplyWithoutUndo");
        }

        private enum Mode { Pending, Applied, AppliedWithoutUndo }

        private static void Run<TValue>(
            string field,
            TValue value,
            Func<SetterTarget, TValue> read,
            IEqualityComparer<TValue> comparer,
            Setter<TValue> setter,
            Mode mode,
            string label)
        {
            Undo.IncrementCurrentGroup();
            var undoGroup = Undo.GetCurrentGroup();
            var target = ScriptableObject.CreateInstance<SetterTarget>();
            try
            {
                using var serializedObject = new SerializedObject(target);
                var property = serializedObject.FindProperty(field);
                var initial = read(target);
                Assert.IsFalse(comparer.Equals(initial, value), $"{label}: the test value must differ from the default.");

                Assert.AreSame(property, setter(property, value), $"{label} must return the same property.");

                if (mode is Mode.Pending)
                {
                    Assert.IsTrue(serializedObject.hasModifiedProperties, $"{label} must leave the write pending.");
                    Assert.IsTrue(comparer.Equals(initial, read(target)), $"{label} must not write the target before Apply.");
                    serializedObject.ApplyModifiedPropertiesWithoutUndo();
                }

                Assert.IsFalse(serializedObject.hasModifiedProperties, $"{label} must leave no pending write.");
                Assert.IsTrue(comparer.Equals(value, read(target)), $"{label}: the target holds {read(target)}, expected {value}.");

                Undo.FlushUndoRecordObjects();
                Undo.RevertAllDownToGroup(undoGroup);

                var afterUndo = read(target);
                if (mode is Mode.Applied)
                    Assert.IsTrue(comparer.Equals(initial, afterUndo), $"{label} must record Undo.");
                else
                    Assert.IsTrue(comparer.Equals(value, afterUndo), $"{label} must not record Undo.");
            }
            finally
            {
                Undo.ClearUndo(target);
                Object.DestroyImmediate(target);
            }
        }

        private static Gradient CopyOf(Gradient gradient)
        {
            var copy = new Gradient { mode = gradient.mode };
            copy.SetKeys(gradient.colorKeys, gradient.alphaKeys);
            return copy;
        }

        private static AnimationCurve CopyOf(AnimationCurve curve) =>
            new(curve.keys);

        // The first type argument is the property type; the generic enum setters add an enum type after it.
        private static MethodInfo Close(MethodInfo method) =>
            method.MakeGenericMethod(method.GetGenericArguments()
                .Select((_, index) => index == 0 ? typeof(SerializedProperty) : typeof(SetterRarity))
                .ToArray());

        private sealed class FuncComparer<T> : IEqualityComparer<T>
        {
            private readonly Func<T, T, bool> _equals;

            public FuncComparer(Func<T, T, bool> equals) =>
                _equals = equals;

            public bool Equals(T left, T right) =>
                _equals(left, right);

            public int GetHashCode(T value) =>
                value?.GetHashCode() ?? 0;
        }

        private enum SetterRarity { Common = 0, Epic = 50, Rare = 20 }

        [Flags]
        private enum SetterDirections { None = 0, Up = 1, Down = 2, Left = 4, Right = 8 }

        private enum SetterSmallRarity : byte { Small = 1, Big = 5 }

        private sealed class ExposedTable : ScriptableObject, IExposedPropertyTable
        {
            private readonly Dictionary<PropertyName, Object> _references = new();

            public void SetReferenceValue(PropertyName id, Object value) =>
                _references[id] = value;

            public Object GetReferenceValue(PropertyName id, out bool idValid)
            {
                idValid = _references.TryGetValue(id, out var value);
                return value;
            }

            public void ClearReferenceValue(PropertyName id) =>
                _references.Remove(id);
        }

        private sealed class SetterTarget : ScriptableObject
        {
            public int Count = 0;
            public float Weight = 0;
            public int[] Items = Array.Empty<int>();
            public Object Reference = null;
            [SerializeReference] public ManagedValue Managed = null;

            public uint Unsigned = 0;
            public long Long = 0;
            public ulong Ulong = 0;
            public double Precise = 0;
            public bool Enabled = false;
            public byte Byte = 0;
            public sbyte Sbyte = 0;
            public short Short = 0;
            public ushort Ushort = 0;
            public char Letter = '\0';
            public Rect Rect = default;
            public RectInt RectInt = default;
            public Bounds Bounds = default;
            public BoundsInt BoundsInt = default;
            public Color Color = default;
            public Gradient Gradient = new();
            public Hash128 Hash = default;
            public Vector4 Vector4 = default;
            public Vector3 Vector3 = default;
            public Vector3Int Vector3Int = default;
            public Vector2 Vector2 = default;
            public Vector2Int Vector2Int = default;
            public Quaternion Rotation = Quaternion.identity;
            public string Text = "";
            public AnimationCurve Curve = new();
            public SetterRarity Rarity = SetterRarity.Common;
            public SetterDirections Directions = SetterDirections.None;
            public SetterSmallRarity SmallRarity = SetterSmallRarity.Small;
            public ExposedReference<SetterTarget> Exposed = default;
#if UNITY_6000_2_OR_NEWER
            public EntityId Identifier = default;
#endif
#if UNITY_6000_4_OR_NEWER
            public GUID Guid = default;
#endif
        }

        [Serializable]
        private sealed class ManagedValue
        {
            public int Number;
        }
    }
}
