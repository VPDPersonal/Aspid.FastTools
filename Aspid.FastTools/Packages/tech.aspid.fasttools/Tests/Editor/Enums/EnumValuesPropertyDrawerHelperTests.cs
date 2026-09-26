using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using NUnit.Framework;
using UnityEngine.UIElements;
using System.Collections.Generic;
using Aspid.FastTools.Enums.Editors;

namespace Aspid.FastTools.Enums.Tests
{
    // Default is an alias: it shares Medium's value, and ToString() names only one of them.
    internal enum Quality
    {
        Low,
        Medium,
        High,
        Default = Medium,
    }

    [Flags]
    internal enum WideUnsignedFlags : ulong
    {
        None = 0,
        Low = 1,
        Top = 1UL << 63,
    }

    /// <summary>
    /// Coverage for <see cref="EnumValuesPropertyDrawerHelper"/> and the UI Toolkit drawers: drawing a row never
    /// rewrites its key, the header shows the given label, Populate Missing Enum Members compares numeric values
    /// and copies collection values, and 64-bit flags are toggled without truncation.
    /// </summary>
    [TestFixture]
    internal sealed class EnumValuesPropertyDrawerHelperTests
    {
        private sealed class Host : ScriptableObject
        {
            [SerializeField] private EnumValues<int> _ints = new();
            [SerializeField] private EnumValues<int[]> _arrays = new();
            [SerializeField] private EnumValues<List<string>> _lists = new();
        }

        private Host _host;
        private SerializedObject _serializedObject;

        [SetUp]
        public void SetUp()
        {
            _host = ScriptableObject.CreateInstance<Host>();
            _serializedObject = new SerializedObject(_host);
        }

        [TearDown]
        public void TearDown()
        {
            _serializedObject.Dispose();
            UnityEngine.Object.DestroyImmediate(_host);
        }

        [Test]
        public void EnumTypeChange_KeepsKeysAndRestoresThemOnSwitchBack()
        {
            SetEnumType("_ints", typeof(Season));
            AddEntry("_ints", nameof(Season.Summer));
            AddEntry("_ints", nameof(Season.Autumn));

            SetEnumType("_ints", typeof(Sides));
            DrawRowsWithoutWrites("_ints");

            CollectionAssert.AreEqual(new[] { "Summer", "Autumn" }, GetKeys("_ints"));
            Assert.AreEqual("<Missing Summer>", GetCaption("_ints", 0));

            SetEnumType("_ints", typeof(Season));
            DrawRowsWithoutWrites("_ints");

            CollectionAssert.AreEqual(new[] { "Summer", "Autumn" }, GetKeys("_ints"));
            Assert.AreEqual(nameof(Season.Summer), GetCaption("_ints", 0));
        }

        [Test]
        public void AliasKey_IsNotRewrittenToTheCanonicalName()
        {
            SetEnumType("_ints", typeof(Quality));
            AddEntry("_ints", nameof(Quality.Default));

            DrawRowsWithoutWrites("_ints");

            CollectionAssert.AreEqual(new[] { nameof(Quality.Default) }, GetKeys("_ints"));
        }

        [Test]
        public void Header_ShowsTheGivenLabel()
        {
            SetEnumType("_ints", typeof(Season));

            var root = EnumValuesUIToolkitPropertyDrawer.Draw(
                _serializedObject.FindProperty("_ints"), "Damage colors", isTyped: false);

            Assert.AreEqual("Damage colors", root.Q(className: "aspid-fasttools-enum-values__header").Q<Label>().text);
        }

        [Test]
        public void GetKeyCaption_DescribesUnresolvedAndEmptyKeys()
        {
            Assert.AreEqual("<None>", EnumValuesPropertyDrawerHelper.GetKeyCaption(string.Empty, null));
            Assert.AreEqual("<Missing Frozen>", EnumValuesPropertyDrawerHelper.GetKeyCaption("Frozen", null));
            Assert.AreEqual("None", EnumValuesPropertyDrawerHelper.GetKeyCaption("None", Sides.None));
        }

        [Test]
        public void Populate_AliasEnum_AddsEachValueOnceAndThenHasNothingMissing()
        {
            SetEnumType("_ints", typeof(Quality));

            Populate("_ints");

            var keys = GetKeys("_ints");
            Assert.AreEqual(3, keys.Length);
            CollectionAssert.AreEquivalent(
                new[] { Quality.Low, Quality.Medium, Quality.High },
                keys.Select(key => Enum.Parse(typeof(Quality), key)).Cast<Quality>());

            Assert.IsFalse(HasMissing("_ints"));

            Populate("_ints");
            Assert.AreEqual(3, GetKeys("_ints").Length);
        }

        [Test]
        public void Populate_AliasKeyAlreadyPresent_DoesNotAddItsMember()
        {
            SetEnumType("_ints", typeof(Quality));
            AddEntry("_ints", nameof(Quality.Default));

            Populate("_ints");

            var keys = GetKeys("_ints");
            Assert.AreEqual(3, keys.Length);
            Assert.AreEqual(1, keys.Count(key => (Quality)Enum.Parse(typeof(Quality), key) == Quality.Medium));
        }

        [Test]
        public void Populate_Flags_AddsDeclaredMembersIncludingCombinations()
        {
            SetEnumType("_ints", typeof(Sides));

            Populate("_ints");

            CollectionAssert.AreEquivalent(new[] { "None", "Left", "Right", "Both" }, GetKeys("_ints"));
        }

        [Test]
        public void Populate_ArrayValue_CopiesDefaultValueIntoEveryRow()
        {
            SetEnumType("_arrays", typeof(Season));

            var defaultValue = _serializedObject.FindProperty("_arrays._defaultValue");
            defaultValue.arraySize = 2;
            defaultValue.GetArrayElementAtIndex(0).intValue = 3;
            defaultValue.GetArrayElementAtIndex(1).intValue = 5;
            _serializedObject.ApplyModifiedPropertiesWithoutUndo();

            Populate("_arrays");

            var values = _serializedObject.FindProperty("_arrays._values");
            Assert.AreEqual(4, values.arraySize);

            for (var i = 0; i < values.arraySize; i++)
            {
                var value = values.GetArrayElementAtIndex(i).FindPropertyRelative("_value");
                Assert.AreEqual(2, value.arraySize);
                Assert.AreEqual(3, value.GetArrayElementAtIndex(0).intValue);
                Assert.AreEqual(5, value.GetArrayElementAtIndex(1).intValue);
            }
        }

        [Test]
        public void Populate_ListValue_CopiesDefaultValueIntoEveryRow()
        {
            SetEnumType("_lists", typeof(Sides));

            var defaultValue = _serializedObject.FindProperty("_lists._defaultValue");
            defaultValue.arraySize = 1;
            defaultValue.GetArrayElementAtIndex(0).stringValue = "hit";
            _serializedObject.ApplyModifiedPropertiesWithoutUndo();

            Populate("_lists");

            var values = _serializedObject.FindProperty("_lists._values");
            Assert.AreEqual(4, values.arraySize);
            Assert.IsFalse(_serializedObject.hasModifiedProperties);

            for (var i = 0; i < values.arraySize; i++)
            {
                var value = values.GetArrayElementAtIndex(i).FindPropertyRelative("_value");
                Assert.AreEqual(1, value.arraySize);
                Assert.AreEqual("hit", value.GetArrayElementAtIndex(0).stringValue);
            }
        }

        [Test]
        public void IsWideFlags_OnlyForFlagsWithA64BitUnderlyingType()
        {
            Assert.IsTrue(EnumValuesPropertyDrawerHelper.IsWideFlags(typeof(BigFlags)));
            Assert.IsTrue(EnumValuesPropertyDrawerHelper.IsWideFlags(typeof(WideUnsignedFlags)));
            Assert.IsFalse(EnumValuesPropertyDrawerHelper.IsWideFlags(typeof(Sides)));
            Assert.IsFalse(EnumValuesPropertyDrawerHelper.IsWideFlags(typeof(UnsignedValues)));
        }

        [Test]
        public void ToggleFlag_LongFlags_KeepsHighBits()
        {
            var high = EnumValuesPropertyDrawerHelper.ToggleFlag(BigFlags.None, BigFlags.High);
            var all = EnumValuesPropertyDrawerHelper.ToggleFlag(high, BigFlags.Top);
            var top = EnumValuesPropertyDrawerHelper.ToggleFlag(all, BigFlags.High);

            Assert.AreEqual(BigFlags.High, high);
            Assert.AreEqual(BigFlags.All, all);
            Assert.AreEqual(BigFlags.Top, top);
        }

        [Test]
        public void ToggleFlag_ULongFlags_KeepsTopBit()
        {
            var value = EnumValuesPropertyDrawerHelper.ToggleFlag(WideUnsignedFlags.Low, WideUnsignedFlags.Top);

            Assert.AreEqual(WideUnsignedFlags.Low | WideUnsignedFlags.Top, value);
            Assert.AreEqual(WideUnsignedFlags.Low | WideUnsignedFlags.Top, Enum.Parse(typeof(WideUnsignedFlags), value.ToString()));
        }

        private void SetEnumType(string field, Type enumType)
        {
            _serializedObject.FindProperty($"{field}._enumType").stringValue = enumType.AssemblyQualifiedName;
            _serializedObject.ApplyModifiedPropertiesWithoutUndo();

            EnumValuesPropertyDrawerHelper.SyncEntryEnumTypes(
                _serializedObject.FindProperty($"{field}._values"),
                _serializedObject.FindProperty($"{field}._enumType"));
        }

        private void AddEntry(string field, string key)
        {
            var values = _serializedObject.FindProperty($"{field}._values");
            values.arraySize++;

            var element = values.GetArrayElementAtIndex(values.arraySize - 1);
            element.FindPropertyRelative("_key").stringValue = key;
            element.FindPropertyRelative("_enumType").stringValue =
                _serializedObject.FindProperty($"{field}._enumType").stringValue;

            _serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        // Builds the UI Toolkit row of every entry, which resolves the key as the Inspector does, and checks
        // that nothing was written to the asset.
        private void DrawRowsWithoutWrites(string field)
        {
            var dirtyCount = EditorUtility.GetDirtyCount(_host);
            var values = _serializedObject.FindProperty($"{field}._values");

            for (var i = 0; i < values.arraySize; i++)
                EnumValueUIToolkitPropertyDrawer.Draw(values.GetArrayElementAtIndex(i));

            Assert.IsFalse(_serializedObject.hasModifiedProperties);
            Assert.AreEqual(dirtyCount, EditorUtility.GetDirtyCount(_host));
        }

        private string GetCaption(string field, int index)
        {
            var element = _serializedObject.FindProperty($"{field}._values").GetArrayElementAtIndex(index);
            var key = element.FindPropertyRelative("_key").stringValue;
            var enumType = EnumValuesPropertyDrawerHelper.GetEnumType(element.FindPropertyRelative("_enumType"));

            return EnumValuesPropertyDrawerHelper.GetKeyCaption(
                key,
                EnumValuesPropertyDrawerHelper.ParseKey(key, enumType));
        }

        private string[] GetKeys(string field)
        {
            _serializedObject.Update();
            var values = _serializedObject.FindProperty($"{field}._values");

            return Enumerable.Range(0, values.arraySize)
                .Select(i => values.GetArrayElementAtIndex(i).FindPropertyRelative("_key").stringValue)
                .ToArray();
        }

        private void Populate(string field) => EnumValuesPropertyDrawerHelper.PopulateMissing(
            _serializedObject.FindProperty($"{field}._values"),
            _serializedObject.FindProperty($"{field}._enumType"),
            _serializedObject.FindProperty($"{field}._defaultValue"));

        private bool HasMissing(string field) => EnumValuesPropertyDrawerHelper.HasMissingMembers(
            _serializedObject.FindProperty($"{field}._values"),
            _serializedObject.FindProperty($"{field}._enumType"));
    }
}
