using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using NUnit.Framework;
using Aspid.FastTools.Enums;
using UnityEngine.TestTools.Constraints;
using Is = UnityEngine.TestTools.Constraints.Is;

namespace Aspid.FastTools.Enums.Tests
{
    internal enum Season
    {
        Winter,
        Spring,
        Summer,
        Autumn,
    }

    [Flags]
    internal enum Sides
    {
        None = 0,
        Left = 1,
        Right = 2,
        Both = Left | Right,
    }

    // Exercises the 64-bit branch of the typed variant's numeric key conversion.
    [Flags]
    internal enum BigFlags : long
    {
        None = 0,
        High = 1L << 40,
        Top = 1L << 62,
        All = High | Top,
    }

    // Exercises sign extension in the typed variant's numeric key conversion.
    internal enum SignedValues : short
    {
        Negative = -5,
        Positive = 7,
    }

    /// <summary>
    /// Coverage for <see cref="EnumValues{TValue}"/> and <see cref="EnumValues{TEnum,TValue}"/>:
    /// lookup semantics (regular + <c>[Flags]</c>), the typed variant's auto-stamped
    /// <c>_enumType</c>, and the serialized-layout compatibility between the two variants.
    /// All data is written through <see cref="SerializedObject"/> so the real
    /// serialize/deserialize path (including <see cref="ISerializationCallbackReceiver"/>) runs.
    /// </summary>
    [TestFixture]
    internal sealed class EnumValuesTests
    {
        private sealed class Host : ScriptableObject
        {
            [SerializeField] private EnumValues<int> _untyped = new();
            [SerializeField] private EnumValues<Season, int> _seasons = new();
            [SerializeField] private EnumValues<Sides, int> _sides = new();
            [SerializeField] private EnumValues<BigFlags, int> _bigFlags = new();
            [SerializeField] private EnumValues<SignedValues, int> _signed = new();

            public EnumValues<int> Untyped => _untyped;

            public EnumValues<Season, int> Seasons => _seasons;

            public EnumValues<Sides, int> Sides => _sides;

            public EnumValues<BigFlags, int> BigFlags => _bigFlags;

            public EnumValues<SignedValues, int> Signed => _signed;
        }

        private Host _host;

        [SetUp]
        public void SetUp() => _host = ScriptableObject.CreateInstance<Host>();

        [TearDown]
        public void TearDown() => UnityEngine.Object.DestroyImmediate(_host);

        private void AddEntry(string field, string key, int value, string enumType = null)
        {
            var serializedObject = new SerializedObject(_host);

            if (enumType is not null)
                serializedObject.FindProperty($"{field}._enumType").stringValue = enumType;

            var values = serializedObject.FindProperty($"{field}._values");
            values.arraySize++;

            var element = values.GetArrayElementAtIndex(values.arraySize - 1);
            element.FindPropertyRelative("_key").stringValue = key;
            element.FindPropertyRelative("_value").intValue = value;

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private void SetDefaultValue(string field, int value)
        {
            var serializedObject = new SerializedObject(_host);
            serializedObject.FindProperty($"{field}._defaultValue").intValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        [Test]
        public void Typed_GetValue_ReturnsMappedValue()
        {
            AddEntry("_seasons", nameof(Season.Summer), 42);

            Assert.AreEqual(42, _host.Seasons.GetValue(Season.Summer));
        }

        [Test]
        public void Typed_GetValue_ReturnsDefaultWhenNoEntryMatches()
        {
            SetDefaultValue("_seasons", -1);
            AddEntry("_seasons", nameof(Season.Summer), 42);

            Assert.AreEqual(-1, _host.Seasons.GetValue(Season.Winter));
        }

        [Test]
        public void Typed_GetValue_DoesNotDependOnEnumTypeMirror()
        {
            // The typed variant must resolve TEnum from the generic argument alone — the
            // serialized _enumType mirror is editor-only sugar for the drawers. Clear it
            // (undoing the stamp AddEntry's serialize pass wrote) and the lookup must still work.
            AddEntry("_seasons", nameof(Season.Autumn), 7);

            var serializedObject = new SerializedObject(_host);
            serializedObject.FindProperty("_seasons._enumType").stringValue = string.Empty;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();

            Assert.AreEqual(7, _host.Seasons.GetValue(Season.Autumn));
        }

        [Test]
        public void Typed_Flags_ExactMatchWinsOverContainment()
        {
            AddEntry("_sides", nameof(Sides.Left), 1);
            AddEntry("_sides", nameof(Sides.Both), 3);

            Assert.AreEqual(3, _host.Sides.GetValue(Sides.Both));
        }

        [Test]
        public void Typed_Flags_ContainmentMatchesWhenNoExactEntry()
        {
            AddEntry("_sides", nameof(Sides.Left), 1);

            Assert.AreEqual(1, _host.Sides.GetValue(Sides.Both));
        }

        [Test]
        public void Typed_Flags_ZeroOnlyMatchesZero()
        {
            SetDefaultValue("_sides", -1);
            AddEntry("_sides", nameof(Sides.None), 100);

            Assert.AreEqual(100, _host.Sides.GetValue(Sides.None));
            Assert.AreEqual(-1, _host.Sides.GetValue(Sides.Left));
        }

        [Test]
        public void Typed_Equals_UsesFlagsSemantics()
        {
            Assert.IsTrue(_host.Sides.Equals(Sides.Both, Sides.Left));
            Assert.IsFalse(_host.Sides.Equals(Sides.Left, Sides.Both));
            Assert.IsFalse(_host.Sides.Equals(Sides.Left, Sides.None));
        }

        [Test]
        public void Typed_Enumerator_YieldsTypedConfiguredEntries()
        {
            AddEntry("_seasons", nameof(Season.Winter), 1);
            AddEntry("_seasons", nameof(Season.Spring), 2);

            var entries = _host.Seasons.ToArray();

            Assert.AreEqual(2, entries.Length);
            Assert.AreEqual(Season.Winter, entries[0].Key);
            Assert.AreEqual(1, entries[0].Value);
            Assert.AreEqual(Season.Spring, entries[1].Key);
            Assert.AreEqual(2, entries[1].Value);
        }

        [Test]
        public void Typed_Foreach_YieldsEntriesAndDoesNotAllocate()
        {
            AddEntry("_seasons", nameof(Season.Winter), 1);
            AddEntry("_seasons", nameof(Season.Spring), 2);

            var sum = 0;

            // Warm-up: the first pass lazily resolves the keys (which allocates) — the
            // steady-state foreach below must bind to the struct enumerator and stay clean.
            foreach (var entry in _host.Seasons)
                sum += entry.Value;

            Assert.AreEqual(3, sum);

            Assert.That(() =>
            {
                foreach (var entry in _host.Seasons)
                    sum += entry.Value;
            }, Is.Not.AllocatingGCMemory());

            Assert.AreEqual(6, sum);
        }

        [Test]
        public void Untyped_Foreach_YieldsEntriesAndDoesNotAllocate()
        {
            AddEntry("_untyped", nameof(Season.Winter), 1, typeof(Season).AssemblyQualifiedName);
            AddEntry("_untyped", nameof(Season.Spring), 2);

            var sum = 0;

            foreach (var entry in _host.Untyped)
                sum += entry.Value;

            Assert.AreEqual(3, sum);

            Assert.That(() =>
            {
                foreach (var entry in _host.Untyped)
                    sum += entry.Value;
            }, Is.Not.AllocatingGCMemory());

            Assert.AreEqual(6, sum);
        }

        [Test]
        public void Typed_OnBeforeSerialize_StampsEnumType()
        {
            // Creating a SerializedObject forces a serialize pass, which runs OnBeforeSerialize.
            var serializedObject = new SerializedObject(_host);

            Assert.AreEqual(
                typeof(Season).AssemblyQualifiedName,
                serializedObject.FindProperty("_seasons._enumType").stringValue);
        }

        [Test]
        public void Typed_LayoutMatchesUntypedFieldPaths()
        {
            // Serialized-layout compatibility contract: both variants expose the same
            // _enumType / _defaultValue / _values(_key, _value) property paths, so switching
            // a field between them migrates existing data.
            var serializedObject = new SerializedObject(_host);

            foreach (var field in new[] { "_untyped", "_seasons" })
            {
                Assert.IsNotNull(serializedObject.FindProperty($"{field}._enumType"), field);
                Assert.IsNotNull(serializedObject.FindProperty($"{field}._defaultValue"), field);
                Assert.IsNotNull(serializedObject.FindProperty($"{field}._values"), field);
            }
        }

        [Test]
        public void Typed_Flags_LongUnderlyingType_HighBitsSurviveLookup()
        {
            SetDefaultValue("_bigFlags", -1);
            AddEntry("_bigFlags", nameof(BigFlags.High), 1);
            AddEntry("_bigFlags", nameof(BigFlags.All), 3);

            Assert.AreEqual(3, _host.BigFlags.GetValue(BigFlags.All));
            Assert.AreEqual(1, _host.BigFlags.GetValue(BigFlags.High));
            Assert.AreEqual(-1, _host.BigFlags.GetValue(BigFlags.Top));
        }

        [Test]
        public void Typed_NegativeUnderlyingValue_MatchesItsEntry()
        {
            SetDefaultValue("_signed", -1);
            AddEntry("_signed", nameof(SignedValues.Negative), 5);

            Assert.AreEqual(5, _host.Signed.GetValue(SignedValues.Negative));
            Assert.AreEqual(-1, _host.Signed.GetValue(SignedValues.Positive));
        }

        [Test]
        public void Untyped_GetValue_ReturnsMappedValue()
        {
            AddEntry("_untyped", nameof(Season.Summer), 42, typeof(Season).AssemblyQualifiedName);

            Assert.AreEqual(42, _host.Untyped.GetValue(Season.Summer));
        }

        [Test]
        public void Untyped_GetValue_ReturnsDefaultWhenNoEntryMatches()
        {
            SetDefaultValue("_untyped", -1);
            AddEntry("_untyped", nameof(Season.Summer), 42, typeof(Season).AssemblyQualifiedName);

            Assert.AreEqual(-1, _host.Untyped.GetValue(Season.Winter));
        }

        [Test]
        public void Untyped_GetValue_DifferentEnumType_NeverMatchesNumericCollision()
        {
            SetDefaultValue("_untyped", -1);
            // Season.Winter and Sides.None share the numeric value 0 — the type guard must
            // keep a lookup with the wrong enum type from matching it.
            AddEntry("_untyped", nameof(Season.Winter), 42, typeof(Season).AssemblyQualifiedName);

            Assert.AreEqual(42, _host.Untyped.GetValue(Season.Winter));
            Assert.AreEqual(-1, _host.Untyped.GetValue(Sides.None));
        }

        [Test]
        public void Untyped_GetValue_NullLookup_ReturnsDefault()
        {
            SetDefaultValue("_untyped", -1);
            AddEntry("_untyped", nameof(Season.Winter), 42, typeof(Season).AssemblyQualifiedName);

            Assert.AreEqual(-1, _host.Untyped.GetValue(null));
        }
    }
}
