using System;
using UnityEditor;
using UnityEngine;
using NUnit.Framework;
using Aspid.FastTools.Enums.Editors;

namespace Aspid.FastTools.Enums.Tests
{
    /// <summary>
    /// Coverage for the IMGUI layout of <see cref="EnumValues{TValue}"/>: a row is as tall as its value field,
    /// and the card header leaves room for the notice of the enum type field.
    /// </summary>
    [TestFixture]
    internal sealed class EnumValuesIMGUIPropertyDrawerTests
    {
        private const float Tolerance = 0.01f;

        private sealed class Host : ScriptableObject
        {
            [SerializeField] private EnumValues<int> _ints = new();
            [SerializeField] private EnumValues<Rect> _rects = new();
            [SerializeField] private EnumValues<Season, int> _typed = new();
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
        public void Row_ScalarValue_IsOneLine()
        {
            SetEnumType("_ints", typeof(Season));
            AddEntry("_ints", nameof(Season.Summer));

            Assert.AreEqual(EditorGUIUtility.singleLineHeight, GetRowHeight("_ints"), Tolerance);
        }

        [Test]
        public void Row_RectValue_TakesTheHeightOfTheField()
        {
            SetEnumType("_rects", typeof(Season));
            AddEntry("_rects", nameof(Season.Summer));

            var value = GetEntry("_rects").FindPropertyRelative("_value");
            var fieldHeight = EditorGUI.GetPropertyHeight(value, GUIContent.none, includeChildren: false);

            Assert.Greater(fieldHeight, EditorGUIUtility.singleLineHeight);
            Assert.AreEqual(fieldHeight, GetRowHeight("_rects"), Tolerance);
        }

        [Test]
        public void Card_RequiredTypeIsNotSet_ReservesTheNoticeRow()
        {
            var noticeHeight = EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight;

            SetEnumType("_ints", typeof(Season));
            var withType = GetCardHeight("_ints");

            SetEnumType("_ints", enumType: null);

            Assert.AreEqual(noticeHeight, GetCardHeight("_ints") - withType, Tolerance);
        }

        [Test]
        public void Card_MissingType_ReservesTheNoticeRow()
        {
            var noticeHeight = EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight;

            SetEnumType("_ints", typeof(Season));
            var withType = GetCardHeight("_ints");

            _serializedObject.FindProperty("_ints._enumType").stringValue = "Old.Namespace.Season, Missing.Assembly";
            _serializedObject.ApplyModifiedPropertiesWithoutUndo();

            Assert.AreEqual(noticeHeight, GetCardHeight("_ints") - withType, Tolerance);
        }

        [Test]
        public void Card_TypedVariant_NeverReservesTheNoticeRow()
        {
            var withType = GetCardHeight("_typed");

            _serializedObject.FindProperty("_typed._enumType").stringValue = string.Empty;
            _serializedObject.ApplyModifiedPropertiesWithoutUndo();

            Assert.AreEqual(withType, GetCardHeight("_typed"), Tolerance);
        }

        private float GetRowHeight(string field) =>
            EnumValueIMGUIPropertyDrawer.GetHeight(GetEntry(field));

        private float GetCardHeight(string field) =>
            EnumValuesIMGUIPropertyDrawer.GetHeight(_serializedObject.FindProperty(field));

        private SerializedProperty GetEntry(string field) =>
            _serializedObject.FindProperty($"{field}._values").GetArrayElementAtIndex(0);

        private void SetEnumType(string field, Type enumType)
        {
            _serializedObject.FindProperty($"{field}._enumType").stringValue = enumType?.AssemblyQualifiedName ?? string.Empty;
            _serializedObject.ApplyModifiedPropertiesWithoutUndo();
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
    }
}
