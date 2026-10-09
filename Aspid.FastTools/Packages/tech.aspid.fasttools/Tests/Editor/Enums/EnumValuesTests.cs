using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using NUnit.Framework;
using System.Threading;
using Aspid.FastTools.Types;
using UnityEngine.TestTools;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;
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

    // Exercises the ulong branch of the numeric key conversion (top bit set —
    // the unchecked ulong→long reinterpretation must stay consistent on both sides).
    internal enum UnsignedValues : ulong
    {
        Zero = 0,
        Top = 1UL << 63,
    }

    /// <summary>
    /// Coverage for <see cref="EnumValues{TValue}"/> and <see cref="EnumValues{TEnum,TValue}"/>:
    /// lookup semantics (regular + <c>[Flags]</c>), the typed variant's auto-stamped
    /// <c>_enumType</c>, the serialized-layout compatibility between the two variants, and the
    /// degrade paths (unconfigured/unresolvable/non-enum enum type, unparseable entry keys).
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
            [SerializeField] private EnumValues<UnsignedValues, int> _unsigned = new();

            public EnumValues<int> Untyped => _untyped;

            public EnumValues<Season, int> Seasons => _seasons;

            public EnumValues<Sides, int> Sides => _sides;

            public EnumValues<BigFlags, int> BigFlags => _bigFlags;

            public EnumValues<SignedValues, int> Signed => _signed;

            public EnumValues<UnsignedValues, int> Unsigned => _unsigned;
        }

        private Host _host;

        [SetUp]
        public void SetUp() =>
            _host = ScriptableObject.CreateInstance<Host>();

        [TearDown]
        public void TearDown() =>
            UnityEngine.Object.DestroyImmediate(_host);

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

        private void SetEnumType(string field, string enumType)
        {
            var serializedObject = new SerializedObject(_host);
            serializedObject.FindProperty($"{field}._enumType").stringValue = enumType;
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
        public void Typed_ULongUnderlyingType_TopBitSurvivesLookup()
        {
            SetDefaultValue("_unsigned", -1);
            AddEntry("_unsigned", nameof(UnsignedValues.Top), 5);

            Assert.AreEqual(5, _host.Unsigned.GetValue(UnsignedValues.Top));
            Assert.AreEqual(-1, _host.Unsigned.GetValue(UnsignedValues.Zero));
        }

        [Test]
        public void Typed_Flags_ContainmentPrefersFirstSerializedEntry()
        {
            // Documented tie-break: with no exact entry, the first entry in serialized
            // order whose bits are contained in the lookup value wins.
            AddEntry("_sides", nameof(Sides.Right), 20);
            AddEntry("_sides", nameof(Sides.Left), 10);

            Assert.AreEqual(20, _host.Sides.GetValue(Sides.Both));
        }

        [Test]
        public void Typed_Equals_RegularEnum_UsesExactEquality()
        {
            Assert.IsTrue(_host.Seasons.Equals(Season.Winter, Season.Winter));
            Assert.IsFalse(_host.Seasons.Equals(Season.Winter, Season.Spring));
        }

        [Test]
        public void Typed_TryGetValue_ReportsWhetherAnEntryMatched()
        {
            SetDefaultValue("_seasons", -1);
            AddEntry("_seasons", nameof(Season.Summer), 42);

            Assert.IsTrue(_host.Seasons.TryGetValue(Season.Summer, out var found));
            Assert.AreEqual(42, found);

            Assert.IsFalse(_host.Seasons.TryGetValue(Season.Winter, out var missing));
            Assert.AreEqual(-1, missing, "A miss yields the default value, as GetValue does.");
        }

        [Test]
        public void Typed_TryGetValue_EntryEqualToDefault_StillMatches()
        {
            SetDefaultValue("_seasons", 5);
            AddEntry("_seasons", nameof(Season.Winter), 5);

            Assert.IsTrue(_host.Seasons.TryGetValue(Season.Winter, out var value));
            Assert.AreEqual(5, value);
        }

        [Test]
        public void Typed_TryGetValue_Flags_UsesFlagsSemantics()
        {
            SetDefaultValue("_sides", -1);
            AddEntry("_sides", nameof(Sides.Left), 1);
            AddEntry("_sides", nameof(Sides.None), 100);

            Assert.IsTrue(_host.Sides.TryGetValue(Sides.Both, out var contained));
            Assert.AreEqual(1, contained, "Containment matches when there is no exact entry.");

            Assert.IsTrue(_host.Sides.TryGetValue(Sides.None, out var zero));
            Assert.AreEqual(100, zero);

            Assert.IsFalse(_host.Sides.TryGetValue(Sides.Right, out var missing));
            Assert.AreEqual(-1, missing);
        }

        [Test]
        public void Typed_Count_CountsOnlyResolvedEntries()
        {
            AddEntry("_seasons", nameof(Season.Winter), 1);
            AddEntry("_seasons", nameof(Season.Winter), 2);
            AddEntry("_seasons", "Bogus", 99);

            LogAssert.Expect(LogType.Error, new Regex("Couldn't parse key 'Bogus'"));

            Assert.AreEqual(2, _host.Seasons.Count, "Duplicate keys count, an unresolved key does not.");
            Assert.AreEqual(_host.Seasons.Count, _host.Seasons.ToArray().Length);
        }

        [Test]
        public void Typed_Count_EmptyTable_IsZero() =>
            Assert.AreEqual(0, _host.Seasons.Count);

        [Test]
        public void Typed_UnparseableKey_LogsErrorAndEntryNeverMatches()
        {
            SetDefaultValue("_seasons", -1);
            AddEntry("_seasons", nameof(Season.Winter), 1);
            AddEntry("_seasons", "Bogus", 99);

            LogAssert.Expect(LogType.Error, new Regex("Couldn't parse key 'Bogus'"));

            Assert.AreEqual(1, _host.Seasons.GetValue(Season.Winter));
            Assert.AreEqual(-1, _host.Seasons.GetValue(Season.Summer));

            // The unresolved entry must be skipped by the enumerator too.
            var entries = _host.Seasons.ToArray();

            Assert.AreEqual(1, entries.Length);
            Assert.AreEqual(Season.Winter, entries[0].Key);
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

        [Test]
        public void Untyped_GenericGetValue_ReturnsMappedValueWithoutBoxing()
        {
            SetDefaultValue("_untyped", -1);
            AddEntry("_untyped", nameof(Season.Summer), 42, typeof(Season).AssemblyQualifiedName);

            // Warm-up: the first access resolves the keys, which allocates.
            Assert.AreEqual(42, _host.Untyped.GetValue(Season.Summer));
            Assert.AreEqual(-1, _host.Untyped.GetValue(Season.Winter));

            var untyped = _host.Untyped;

            // A typed argument binds to GetValue<TEnum>, so the key is not boxed.
            Assert.That(() =>
            {
                untyped.GetValue(Season.Summer);
                untyped.GetValue(Season.Winter);
            }, Is.Not.AllocatingGCMemory());
        }

        [Test]
        public void Untyped_GenericGetValue_DifferentEnumType_ReturnsDefault()
        {
            SetDefaultValue("_untyped", -1);
            AddEntry("_untyped", nameof(Season.Winter), 42, typeof(Season).AssemblyQualifiedName);

            // Season.Winter and Sides.None share the numeric value 0.
            Assert.AreEqual(-1, _host.Untyped.GetValue(Sides.None));
        }

        [Test]
        public void Untyped_TryGetValue_ReportsWhetherAnEntryMatched()
        {
            SetDefaultValue("_untyped", -1);
            AddEntry("_untyped", nameof(Season.Summer), 42, typeof(Season).AssemblyQualifiedName);

            Assert.IsTrue(_host.Untyped.TryGetValue((Enum)Season.Summer, out var found));
            Assert.AreEqual(42, found);

            Assert.IsFalse(_host.Untyped.TryGetValue((Enum)Season.Winter, out var missing));
            Assert.AreEqual(-1, missing, "A miss yields the default value, as GetValue does.");
        }

        [Test]
        public void Untyped_TryGetValue_NullOrForeignEnum_DoesNotMatch()
        {
            SetDefaultValue("_untyped", -1);
            AddEntry("_untyped", nameof(Season.Winter), 42, typeof(Season).AssemblyQualifiedName);

            Assert.IsFalse(_host.Untyped.TryGetValue(null, out var nullKey));
            Assert.AreEqual(-1, nullKey);

            // Season.Winter and Sides.None share the numeric value 0.
            Assert.IsFalse(_host.Untyped.TryGetValue((Enum)Sides.None, out var foreignKey));
            Assert.AreEqual(-1, foreignKey);
        }

        [Test]
        public void Untyped_GenericTryGetValue_MatchesOnlyTheConfiguredEnum()
        {
            SetDefaultValue("_untyped", -1);
            AddEntry("_untyped", nameof(Season.Winter), 42, typeof(Season).AssemblyQualifiedName);

            Assert.IsTrue(_host.Untyped.TryGetValue(Season.Winter, out var found));
            Assert.AreEqual(42, found);

            Assert.IsFalse(_host.Untyped.TryGetValue(Season.Spring, out var missing));
            Assert.AreEqual(-1, missing);

            Assert.IsFalse(_host.Untyped.TryGetValue(Sides.None, out var foreign));
            Assert.AreEqual(-1, foreign);
        }

        [Test]
        public void Untyped_Count_CountsOnlyResolvedEntries()
        {
            AddEntry("_untyped", nameof(Season.Winter), 1, typeof(Season).AssemblyQualifiedName);
            AddEntry("_untyped", nameof(Season.Spring), 2);
            AddEntry("_untyped", "Bogus", 99);

            LogAssert.Expect(LogType.Error, new Regex("Couldn't parse key 'Bogus'"));

            Assert.AreEqual(2, _host.Untyped.Count);
            Assert.AreEqual(_host.Untyped.Count, _host.Untyped.ToArray().Length);
        }

        [Test]
        public void Untyped_Count_NoEnumTypeConfigured_IsZero()
        {
            AddEntry("_untyped", nameof(Season.Winter), 1);

            LogAssert.Expect(LogType.Warning, new Regex("No enum type configured"));
            Assert.AreEqual(0, _host.Untyped.Count);
        }

        [Test]
        public void Untyped_EnumPicker_DoesNotOfferTheAbstractSystemEnum()
        {
            var field = typeof(EnumValues<int>).GetField("_enumType",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

            var attribute = (TypeSelectorAttribute)Attribute.GetCustomAttribute(field, typeof(TypeSelectorAttribute));
            var baseTypes = attribute.AssemblyQualifiedNames.Select(name => Type.GetType(name)).ToArray();

            var offered = Aspid.FastTools.Types.Editors.TypeInfo
                .GetAllTypeInfos(baseTypes, attribute.Allow)
                .Select(info => info.AssemblyQualifiedName)
                .ToArray();

            Assert.IsTrue(attribute.Required);
            CollectionAssert.Contains(offered, typeof(Season).AssemblyQualifiedName);
            CollectionAssert.DoesNotContain(offered, typeof(Enum).AssemblyQualifiedName,
                "Picking System.Enum fills the field, so Required stays quiet, but no member can ever be keyed.");
        }

        [Test]
        public void InitializedFlag_IsVolatile_SoThreadsSeeTheDataBeforeIt()
        {
            foreach (var type in new[] { typeof(EnumValues<int>), typeof(EnumValues<Season, int>) })
            {
                var field = type.GetField("_isInitialized",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

                CollectionAssert.Contains(field.GetRequiredCustomModifiers(), typeof(IsVolatile), type.Name);
            }
        }

        [Test]
        public void FirstAccessFromSeveralThreads_EveryThreadSeesTheConfiguredValue()
        {
            SetDefaultValue("_seasons", -1);
            AddEntry("_seasons", nameof(Season.Summer), 42);
            SetDefaultValue("_untyped", -1);
            AddEntry("_untyped", nameof(Season.Summer), 42, typeof(Season).AssemblyQualifiedName);

            var typed = _host.Seasons;
            var untyped = _host.Untyped;
            var results = new int[16];

            using var start = new ManualResetEventSlim();
            var threads = Enumerable.Range(0, results.Length / 2).SelectMany(i => new[]
            {
                new Thread(() => { start.Wait(); results[i * 2] = typed.GetValue(Season.Summer); }),
                new Thread(() => { start.Wait(); results[i * 2 + 1] = untyped.GetValue(Season.Summer); }),
            }).ToArray();

            foreach (var thread in threads)
                thread.Start();

            start.Set();

            foreach (var thread in threads)
                thread.Join();

            CollectionAssert.AreEqual(Enumerable.Repeat(42, results.Length), results);
        }

        [Test]
        public void Untyped_NoEnumTypeConfigured_WarnsAndReturnsDefault()
        {
            SetDefaultValue("_untyped", -1);
            AddEntry("_untyped", nameof(Season.Winter), 42);

            LogAssert.Expect(LogType.Warning, new Regex("No enum type configured"));
            Assert.AreEqual(-1, _host.Untyped.GetValue(Season.Winter));
        }

        [Test]
        public void Untyped_UnresolvableEnumType_LogsErrorAndReturnsDefault()
        {
            SetDefaultValue("_untyped", -1);
            AddEntry("_untyped", nameof(Season.Winter), 42, "Not.A.Real.Type, Fake.Assembly");

            LogAssert.Expect(LogType.Error, new Regex("Couldn't resolve enum type"));
            Assert.AreEqual(-1, _host.Untyped.GetValue(Season.Winter));
        }

        [Test]
        public void Untyped_NonEnumType_LogsErrorAndReturnsDefault()
        {
            // A type that resolves but is not an enum (e.g. an enum refactored into a
            // class/struct with the same name) must degrade like an unresolvable one
            // instead of throwing from Enum.TryParse on every lookup.
            SetDefaultValue("_untyped", -1);
            AddEntry("_untyped", nameof(Season.Winter), 42, typeof(string).AssemblyQualifiedName);

            LogAssert.Expect(LogType.Error, new Regex("is not an enum"));
            Assert.AreEqual(-1, _host.Untyped.GetValue(Season.Winter));
        }

        [Test]
        public void Untyped_EnumTypeClearedAfterResolution_DegradesAndYieldsNothing()
        {
            SetDefaultValue("_untyped", -1);
            AddEntry("_untyped", nameof(Season.Winter), 1, typeof(Season).AssemblyQualifiedName);

            // Resolve the keys once, then clear the type — degrading must reset the
            // previously resolved keys instead of letting them keep matching lookups
            // and being yielded by the enumerator.
            Assert.AreEqual(1, _host.Untyped.GetValue(Season.Winter));
            SetEnumType("_untyped", string.Empty);

            LogAssert.Expect(LogType.Warning, new Regex("No enum type configured"));

            Assert.AreEqual(-1, _host.Untyped.GetValue(Season.Winter));
            Assert.AreEqual(0, _host.Untyped.ToArray().Length);
        }

        [Test]
        public void Untyped_UnparseableKey_LogsErrorAndEntryNeverMatches()
        {
            SetDefaultValue("_untyped", -1);
            AddEntry("_untyped", nameof(Season.Winter), 1, typeof(Season).AssemblyQualifiedName);
            AddEntry("_untyped", "Bogus", 99);

            LogAssert.Expect(LogType.Error, new Regex("Couldn't parse key 'Bogus'"));

            Assert.AreEqual(1, _host.Untyped.GetValue(Season.Winter));
            Assert.AreEqual(-1, _host.Untyped.GetValue(Season.Summer));

            // The unresolved entry must be skipped by the enumerator too.
            var entries = _host.Untyped.ToArray();

            Assert.AreEqual(1, entries.Length);
            Assert.AreEqual(Season.Winter, entries[0].Key);
        }

        [Test]
        public void Untyped_Equals_NullArgument_ReturnsFalse()
        {
            SetEnumType("_untyped", typeof(Season).AssemblyQualifiedName);

            Assert.IsFalse(_host.Untyped.Equals(null, Season.Winter));
            Assert.IsFalse(_host.Untyped.Equals(Season.Winter, null));
            Assert.IsFalse(_host.Untyped.Equals(null, null));
        }

        [Test]
        public void Untyped_Equals_DifferentEnumTypes_ReturnsFalse()
        {
            SetEnumType("_untyped", typeof(Season).AssemblyQualifiedName);

            // Season.Winter and Sides.None share the numeric value 0 — never equal anyway.
            Assert.IsFalse(_host.Untyped.Equals(Season.Winter, Sides.None));
        }

        [Test]
        public void Untyped_Equals_TypeOtherThanConfigured_ReturnsFalse()
        {
            SetEnumType("_untyped", typeof(Season).AssemblyQualifiedName);

            // Same rule as GetValue: identical values of a foreign type are not equal either.
            Assert.IsFalse(_host.Untyped.Equals(Sides.Left, Sides.Left));
        }

        [Test]
        public void Untyped_Equals_RegularEnum_UsesExactEquality()
        {
            SetEnumType("_untyped", typeof(Season).AssemblyQualifiedName);

            Assert.IsTrue(_host.Untyped.Equals(Season.Winter, Season.Winter));
            Assert.IsFalse(_host.Untyped.Equals(Season.Winter, Season.Spring));
        }

        [Test]
        public void Untyped_Equals_FlagsEnum_UsesFlagsSemantics()
        {
            // The flags semantics come from the configured enum type, not the arguments.
            SetEnumType("_untyped", typeof(Sides).AssemblyQualifiedName);

            Assert.IsTrue(_host.Untyped.Equals(Sides.Both, Sides.Left));
            Assert.IsFalse(_host.Untyped.Equals(Sides.Left, Sides.Both));
            Assert.IsFalse(_host.Untyped.Equals(Sides.Left, Sides.None));
        }

        [Test]
        public void Untyped_Flags_LongUnderlyingType_HighBitsSurviveLookup()
        {
            // Exercises the boxed EnumInfo.ToInt64 Int64 branch (the typed variant
            // goes through the separate EnumInfo<TEnum> converter).
            SetDefaultValue("_untyped", -1);
            AddEntry("_untyped", nameof(BigFlags.High), 1, typeof(BigFlags).AssemblyQualifiedName);
            AddEntry("_untyped", nameof(BigFlags.All), 3);

            Assert.AreEqual(3, _host.Untyped.GetValue(BigFlags.All));
            Assert.AreEqual(1, _host.Untyped.GetValue(BigFlags.High));
            Assert.AreEqual(-1, _host.Untyped.GetValue(BigFlags.Top));
        }

        [Test]
        public void Untyped_NegativeUnderlyingValue_MatchesItsEntry()
        {
            SetDefaultValue("_untyped", -1);
            AddEntry("_untyped", nameof(SignedValues.Negative), 5, typeof(SignedValues).AssemblyQualifiedName);

            Assert.AreEqual(5, _host.Untyped.GetValue(SignedValues.Negative));
            Assert.AreEqual(-1, _host.Untyped.GetValue(SignedValues.Positive));
        }

        [Test]
        public void Untyped_ULongUnderlyingType_TopBitSurvivesLookup()
        {
            SetDefaultValue("_untyped", -1);
            AddEntry("_untyped", nameof(UnsignedValues.Top), 5, typeof(UnsignedValues).AssemblyQualifiedName);

            Assert.AreEqual(5, _host.Untyped.GetValue(UnsignedValues.Top));
            Assert.AreEqual(-1, _host.Untyped.GetValue(UnsignedValues.Zero));
        }
    }
}
