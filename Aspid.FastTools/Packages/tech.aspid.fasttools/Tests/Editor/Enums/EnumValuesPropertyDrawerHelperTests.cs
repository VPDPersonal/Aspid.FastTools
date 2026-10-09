using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using NUnit.Framework;
using System.Collections;
using UnityEngine.TestTools;
using UnityEditor.UIElements;
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

    [Flags]
    internal enum TopBitFlags : uint
    {
        None = 0,
        Low = 1,
        Top = 1u << 31,
    }

    [Flags]
    internal enum NarrowUnsignedFlags : uint
    {
        None = 0,
        Low = 1,
        High = 1u << 30,
    }

    // Unity's enum fields leave out Axe, so a key on it reads blank there.
    internal enum LegacyWeapon
    {
        Sword,
        [Obsolete] Axe,
        Bow,
    }

    [Flags]
    internal enum LegacyFlags
    {
        None = 0,
        Fire = 1,
        [Obsolete] Ice = 2,
    }

    // Old and New share a value: New has no attribute, so the value stays visible in Unity's enum fields.
    internal enum RenamedMode
    {
        None = 0,
        [Obsolete] Old = 1,
        New = 1,
    }

    /// <summary>
    /// Coverage for <see cref="EnumValuesPropertyDrawerHelper"/> and the UI Toolkit drawers: drawing never writes
    /// to the asset, a row takes its enum from the table that owns it, the header shows the given label,
    /// Populate Missing Enum Members compares numeric values, copies collection values, skips obsolete members
    /// and stays off for several selected objects, a key on an obsolete member is named, and flags beyond
    /// a signed 32-bit mask are toggled without truncation.
    /// </summary>
    [TestFixture]
    internal sealed class EnumValuesPropertyDrawerHelperTests
    {
        private sealed class Host : ScriptableObject
        {
            [SerializeField] private EnumValues<int> _ints = new();
            [SerializeField] private EnumValues<int[]> _arrays = new();
            [SerializeField] private EnumValues<List<string>> _lists = new();
            [SerializeField] private EnumValues<EnumValues<int>> _nested = new();
        }

        private sealed class TypedFields
        {
            public readonly EnumValues<Season, int> Single = new();
            public readonly List<EnumValues<Season, int>> List = new();
            public readonly EnumValues<Season, int>[] Array = System.Array.Empty<EnumValues<Season, int>>();

            public readonly EnumValues<int> Untyped = new();
            public readonly EnumValues<int>[] UntypedArray = System.Array.Empty<EnumValues<int>>();
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

        [UnityTest]
        public IEnumerator KeyChangedElsewhere_UpdatesTheKeyDropdown()
        {
            SetEnumType("_ints", typeof(Season));
            AddEntry("_ints", nameof(Season.Summer));

            var window = ScriptableObject.CreateInstance<EditorWindow>();

            try
            {
                window.ShowUtility();

                var row = EnumValueUIToolkitPropertyDrawer.Draw(
                    _serializedObject.FindProperty("_ints._values").GetArrayElementAtIndex(0));

                window.rootVisualElement.Add(row);
                yield return null;

                var keyField = row.Q<EnumField>();
                Assert.AreEqual(Season.Summer, keyField.value);

                // Stands in for Undo, Revert or Paste: the key changes without going through the row.
                using (var other = new SerializedObject(_host))
                {
                    other.FindProperty("_ints._values.Array.data[0]._key").stringValue = nameof(Season.Winter);
                    other.ApplyModifiedPropertiesWithoutUndo();
                }

                var deadline = EditorApplication.timeSinceStartup + 3;
                while (!Equals(keyField.value, Season.Winter) && EditorApplication.timeSinceStartup < deadline)
                    yield return null;

                Assert.AreEqual(Season.Winter, keyField.value);
            }
            finally
            {
                if (window) window.Close();
            }
        }

        [Test]
        public void WideFlagsKey_ShowsTheMenuFieldWithoutWriting()
        {
            SetEnumType("_ints", typeof(BigFlags));
            AddEntry("_ints", nameof(BigFlags.High));

            var row = DrawRowsWithoutWrites("_ints")[0];

            var menuField = row.Q<BaseField<string>>(className: EnumField.ussClassName);
            Assert.AreEqual(DisplayStyle.Flex, menuField.style.display.value);
            Assert.AreEqual(nameof(BigFlags.High), menuField.Q<TextElement>(className: EnumField.textUssClassName).text);
            Assert.AreEqual(DisplayStyle.None, row.Q<EnumFlagsField>().style.display.value);
            CollectionAssert.AreEqual(new[] { nameof(BigFlags.High) }, GetKeys("_ints"));
        }

        [Test]
        public void Header_ShowsTheGivenLabel()
        {
            SetEnumType("_ints", typeof(Season));

            var root = EnumValuesUIToolkitPropertyDrawer.Draw(
                _serializedObject.FindProperty("_ints"), "Damage colors", isTyped: false);

            Assert.AreEqual("Damage colors", root.Q(className: "aspid-fasttools-enum-values__header").Q<Label>().text);
        }

        [UnityTest]
        public IEnumerator PropertyField_HeaderShowsItsLabelOrTheDisplayName()
        {
            SetEnumType("_ints", typeof(Season));

            var window = ScriptableObject.CreateInstance<EditorWindow>();

            try
            {
                window.ShowUtility();

                var property = _serializedObject.FindProperty("_ints");
                var labeled = new PropertyField(property, "Damage colors");
                var unlabeled = new PropertyField(property);

                window.rootVisualElement.Add(labeled);
                window.rootVisualElement.Add(unlabeled);
                labeled.Bind(_serializedObject);
                unlabeled.Bind(_serializedObject);

                var deadline = EditorApplication.timeSinceStartup + 3;
                while ((GetHeader(labeled) is null || GetHeader(unlabeled) is null)
                    && EditorApplication.timeSinceStartup < deadline)
                    yield return null;

                Assert.AreEqual("Damage colors", GetHeader(labeled));
                Assert.AreEqual(property.displayName, GetHeader(unlabeled));
            }
            finally
            {
                if (window) window.Close();
            }

            static string GetHeader(VisualElement field) =>
                field.Q(className: "aspid-fasttools-enum-values__header")?.Q<Label>()?.text;
        }

        [Test]
        public void Row_StoresNoEnumTypeOfItsOwn()
        {
            SetEnumType("_ints", typeof(Season));
            AddEntry("_ints", nameof(Season.Summer));

            var row = _serializedObject.FindProperty("_ints._values").GetArrayElementAtIndex(0);

            Assert.IsNull(row.FindPropertyRelative("_enumType"));
        }

        [Test]
        public void FindEnumTypeProperty_ReturnsTheEnumTypeOfTheTableThatOwnsTheRow()
        {
            SetEnumType("_ints", typeof(Season));
            SetEnumType("_lists", typeof(Sides));
            AddEntry("_ints", nameof(Season.Summer));
            AddEntry("_lists", nameof(Sides.Left));

            Assert.AreEqual("_ints._enumType", FindEnumType("_ints._values.Array.data[0]").propertyPath);
            Assert.AreEqual("_lists._enumType", FindEnumType("_lists._values.Array.data[0]").propertyPath);
        }

        [Test]
        public void FindEnumTypeProperty_RowInsideAnotherRow_ReturnsTheInnerTable()
        {
            SetEnumType("_nested", typeof(Season));
            AddEntry("_nested", nameof(Season.Summer));

            var inner = "_nested._values.Array.data[0]._value";
            _serializedObject.FindProperty($"{inner}._enumType").stringValue = typeof(Sides).AssemblyQualifiedName;
            _serializedObject.FindProperty($"{inner}._values").arraySize = 2;
            _serializedObject.ApplyModifiedPropertiesWithoutUndo();

            Assert.AreEqual(
                $"{inner}._enumType",
                FindEnumType($"{inner}._values.Array.data[1]").propertyPath);

            Assert.AreEqual(
                typeof(Sides),
                EnumValuesPropertyDrawerHelper.GetEnumType(FindEnumType($"{inner}._values.Array.data[1]")));
        }

        [Test]
        public void Draw_TableWithRows_WritesNothingToTheAsset()
        {
            SetEnumType("_ints", typeof(Season));
            AddEntry("_ints", nameof(Season.Summer));
            AddEntry("_ints", nameof(Season.Autumn));

            var dirtyCount = EditorUtility.GetDirtyCount(_host);

            var root = EnumValuesUIToolkitPropertyDrawer.Draw(
                _serializedObject.FindProperty("_ints"), "Ints", isTyped: false);

            Assert.IsNotNull(root);
            Assert.IsFalse(_serializedObject.hasModifiedProperties);
            Assert.AreEqual(dirtyCount, EditorUtility.GetDirtyCount(_host));
        }

        [Test]
        public void RowKey_FollowsTheEnumOfTheTable()
        {
            SetEnumType("_ints", typeof(Sides));
            AddEntry("_ints", nameof(Sides.Right));

            var row = DrawRowsWithoutWrites("_ints")[0];

            Assert.AreEqual(Sides.Right, row.Q<EnumFlagsField>().value);
            Assert.AreEqual(DisplayStyle.Flex, row.Q<EnumFlagsField>().style.display.value);
        }

        [Test]
        public void Populate_SeveralObjectsSelected_IsDisabledAndAddsNothing()
        {
            var other = ScriptableObject.CreateInstance<Host>();

            try
            {
                using var both = new SerializedObject(new UnityEngine.Object[] { _host, other });
                using var single = new SerializedObject(other);
                both.FindProperty("_ints._enumType").stringValue = typeof(Season).AssemblyQualifiedName;
                both.ApplyModifiedPropertiesWithoutUndo();

                var values = both.FindProperty("_ints._values");
                var enumType = both.FindProperty("_ints._enumType");
                var defaultValue = both.FindProperty("_ints._defaultValue");

                Assert.IsTrue(both.isEditingMultipleObjects);
                Assert.IsTrue(EnumValuesPropertyDrawerHelper.HasMissingMembers(values, enumType));
                Assert.IsFalse(EnumValuesPropertyDrawerHelper.CanPopulate(values, enumType));

                EnumValuesPropertyDrawerHelper.PopulateMissing(values, enumType, defaultValue);

                Assert.AreEqual(0, values.arraySize);
                Assert.IsFalse(both.hasModifiedProperties);
                Assert.AreEqual(0, single.FindProperty("_ints._values").arraySize);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(other);
            }
        }

        [Test]
        public void Populate_OneObjectSelected_IsEnabledWhileMembersAreMissing()
        {
            SetEnumType("_ints", typeof(Season));
            Assert.IsTrue(CanPopulate("_ints"));

            Populate("_ints");
            Assert.IsFalse(CanPopulate("_ints"));
        }

        [Test]
        public void Populate_ObsoleteMember_IsNotAdded()
        {
            SetEnumType("_ints", typeof(LegacyWeapon));

            Populate("_ints");

            CollectionAssert.AreEquivalent(new[] { "Sword", "Bow" }, GetKeys("_ints"));
            Assert.IsFalse(HasMissing("_ints"));
        }

        [Test]
        public void Populate_ObsoleteMemberWithALiveAlias_AddsTheValueOnce()
        {
            SetEnumType("_ints", typeof(RenamedMode));

            Populate("_ints");

            Assert.AreEqual(2, GetKeys("_ints").Length);
            Assert.IsFalse(HasMissing("_ints"));
        }

        [Test]
        public void ObsoleteKey_ShowsItsNameInTheMenuFieldWithoutWriting()
        {
            SetEnumType("_ints", typeof(LegacyWeapon));
            AddEntry("_ints", "Axe");

            var row = DrawRowsWithoutWrites("_ints")[0];

            var menuField = row.Q<BaseField<string>>(className: EnumField.ussClassName);
            Assert.AreEqual(DisplayStyle.Flex, menuField.style.display.value);
            Assert.AreEqual("Axe", menuField.Q<TextElement>(className: EnumField.textUssClassName).text);
            Assert.AreEqual(DisplayStyle.None, row.Q<EnumField>().style.display.value);
            CollectionAssert.AreEqual(new[] { "Axe" }, GetKeys("_ints"));
        }

        [Test]
        public void LiveKey_NextToObsoleteMembers_KeepsTheEnumField()
        {
            SetEnumType("_ints", typeof(LegacyWeapon));
            AddEntry("_ints", nameof(LegacyWeapon.Bow));

            var row = DrawRowsWithoutWrites("_ints")[0];

            Assert.AreEqual(DisplayStyle.Flex, row.Q<EnumField>().style.display.value);
            Assert.AreEqual(LegacyWeapon.Bow, row.Q<EnumField>().value);
        }

        [TestCase(typeof(LegacyWeapon), "Axe", true)]
        [TestCase(typeof(LegacyWeapon), "Sword", false)]
        [TestCase(typeof(LegacyWeapon), "Bow", false)]
        [TestCase(typeof(RenamedMode), "Old", false)]
        [TestCase(typeof(RenamedMode), "None", false)]
        [TestCase(typeof(LegacyFlags), "Ice", true)]
        [TestCase(typeof(LegacyFlags), "Fire, Ice", true)]
        [TestCase(typeof(LegacyFlags), "Fire", false)]
        [TestCase(typeof(LegacyFlags), "None", false)]
        [TestCase(typeof(Season), "Summer", false)]
        public void HasObsoleteMember_IsTrueOnlyWhenAValueHasNoLiveName(Type enumType, string key, bool expected)
        {
            var value = (Enum)Enum.Parse(enumType, key);

            Assert.AreEqual(expected, EnumValuesPropertyDrawerHelper.HasObsoleteMember(enumType, value));
        }

        [TestCase(nameof(TypedFields.Single), true)]
        [TestCase(nameof(TypedFields.List), true)]
        [TestCase(nameof(TypedFields.Array), true)]
        [TestCase(nameof(TypedFields.Untyped), false)]
        [TestCase(nameof(TypedFields.UntypedArray), false)]
        public void IsTypedVariant_LooksThroughArraysAndLists(string field, bool expected)
        {
            var fieldInfo = typeof(TypedFields).GetField(field);

            Assert.AreEqual(expected, EnumValuesPropertyDrawer.IsTypedVariant(fieldInfo));
        }

        [Test]
        public void IsTypedVariant_WithoutAField_IsFalse() =>
            Assert.IsFalse(EnumValuesPropertyDrawer.IsTypedVariant(null));

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
        public void IsWideFlags_ForFlagsBeyondASigned32BitMask()
        {
            Assert.IsTrue(EnumValuesPropertyDrawerHelper.IsWideFlags(typeof(BigFlags)));
            Assert.IsTrue(EnumValuesPropertyDrawerHelper.IsWideFlags(typeof(WideUnsignedFlags)));
            Assert.IsTrue(EnumValuesPropertyDrawerHelper.IsWideFlags(typeof(TopBitFlags)));
            Assert.IsFalse(EnumValuesPropertyDrawerHelper.IsWideFlags(typeof(NarrowUnsignedFlags)));
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

        [Test]
        public void ToggleFlag_UIntFlags_KeepsBit31()
        {
            var value = EnumValuesPropertyDrawerHelper.ToggleFlag(TopBitFlags.Low, TopBitFlags.Top);

            Assert.AreEqual(TopBitFlags.Low | TopBitFlags.Top, value);
            Assert.AreEqual(TopBitFlags.Low | TopBitFlags.Top, Enum.Parse(typeof(TopBitFlags), value.ToString()));
        }

        private void SetEnumType(string field, Type enumType)
        {
            _serializedObject.FindProperty($"{field}._enumType").stringValue = enumType.AssemblyQualifiedName;
            _serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private void AddEntry(string field, string key)
        {
            var values = _serializedObject.FindProperty($"{field}._values");
            values.arraySize++;

            var element = values.GetArrayElementAtIndex(values.arraySize - 1);
            element.FindPropertyRelative("_key").stringValue = key;

            _serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        // Builds the UI Toolkit row of every entry, which resolves the key as the Inspector does, and checks
        // that nothing was written to the asset.
        private VisualElement[] DrawRowsWithoutWrites(string field)
        {
            var dirtyCount = EditorUtility.GetDirtyCount(_host);
            var values = _serializedObject.FindProperty($"{field}._values");

            var rows = Enumerable.Range(0, values.arraySize)
                .Select(i => EnumValueUIToolkitPropertyDrawer.Draw(values.GetArrayElementAtIndex(i)))
                .ToArray();

            Assert.IsFalse(_serializedObject.hasModifiedProperties);
            Assert.AreEqual(dirtyCount, EditorUtility.GetDirtyCount(_host));

            return rows;
        }

        private string GetCaption(string field, int index)
        {
            var element = _serializedObject.FindProperty($"{field}._values").GetArrayElementAtIndex(index);
            var key = element.FindPropertyRelative("_key").stringValue;
            var enumType = EnumValuesPropertyDrawerHelper.GetEnumType(
                EnumValuesPropertyDrawerHelper.FindEnumTypeProperty(element));

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

        private SerializedProperty FindEnumType(string rowPath) =>
            EnumValuesPropertyDrawerHelper.FindEnumTypeProperty(_serializedObject.FindProperty(rowPath));

        private bool CanPopulate(string field) => EnumValuesPropertyDrawerHelper.CanPopulate(
            _serializedObject.FindProperty($"{field}._values"),
            _serializedObject.FindProperty($"{field}._enumType"));

        private bool HasMissing(string field) => EnumValuesPropertyDrawerHelper.HasMissingMembers(
            _serializedObject.FindProperty($"{field}._values"),
            _serializedObject.FindProperty($"{field}._enumType"));
    }
}
