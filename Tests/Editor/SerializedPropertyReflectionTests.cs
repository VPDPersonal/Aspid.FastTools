using System;
using UnityEditor;
using UnityEngine;
using NUnit.Framework;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace Aspid.FastTools.Editors.Tests
{
    internal class ReflectionBookBase : ScriptableObject
    {
        [SerializeField] private int _level = 3;

        public int Level => _level;
    }

    internal sealed class ReflectionBook : ReflectionBookBase
    {
        public int[] Levels = { 1, 2 };
        public List<ReflectionAbility> Abilities = new();
        [SerializeReference] public IReflectionEffect Effect;
    }

    [Serializable]
    internal sealed class ReflectionAbility
    {
        public string Name;
    }

    internal interface IReflectionEffect { }

    [Serializable]
    internal sealed class ReflectionBurnEffect : IReflectionEffect
    {
        public float Damage;
    }

    // The field type and owner table of the SerializedProperty Extensions page, row by row.
    internal sealed class SerializedPropertyReflectionTests
    {
        private ReflectionBook _book;
        private SerializedObject _serializedObject;

        [SetUp]
        public void SetUp()
        {
            _book = ScriptableObject.CreateInstance<ReflectionBook>();
            _book.Abilities.Add(new ReflectionAbility { Name = "Fireball" });
            _book.Abilities.Add(new ReflectionAbility { Name = "Frostbolt" });
            _book.Effect = new ReflectionBurnEffect { Damage = 5 };
            _serializedObject = new SerializedObject(_book);
        }

        [TearDown]
        public void TearDown()
        {
            _serializedObject.Dispose();
            Object.DestroyImmediate(_book);
        }

        private SerializedProperty Abilities => _serializedObject.FindProperty(nameof(ReflectionBook.Abilities));

        private SerializedProperty Effect => _serializedObject.FindProperty(nameof(ReflectionBook.Effect));

        [Test]
        public void GetPropertyType_ReturnsFieldTypeOrElementType()
        {
            var ability = Abilities.GetArrayElementAtIndex(0);

            Assert.AreEqual(typeof(List<ReflectionAbility>), Abilities.GetPropertyType());
            Assert.AreEqual(typeof(ReflectionAbility), ability.GetPropertyType());
            Assert.AreEqual(typeof(string), ability.FindPropertyRelative(nameof(ReflectionAbility.Name)).GetPropertyType());
            Assert.AreEqual(typeof(int), _serializedObject.FindProperty(nameof(ReflectionBook.Levels)).GetArrayElementAtIndex(1).GetPropertyType());
            Assert.AreEqual(typeof(IReflectionEffect), Effect.GetPropertyType());
            Assert.AreEqual(typeof(float), Effect.FindPropertyRelative(nameof(ReflectionBurnEffect.Damage)).GetPropertyType());
        }

        [Test]
        public void GetFieldInfo_ResolvesCollectionNestedAndManagedReferenceFields()
        {
            var abilitiesField = typeof(ReflectionBook).GetField(nameof(ReflectionBook.Abilities));
            var ability = Abilities.GetArrayElementAtIndex(0);

            Assert.AreEqual(abilitiesField, Abilities.GetFieldInfo());
            Assert.AreEqual(abilitiesField, ability.GetFieldInfo());
            Assert.AreEqual(typeof(ReflectionAbility).GetField(nameof(ReflectionAbility.Name)),
                ability.FindPropertyRelative(nameof(ReflectionAbility.Name)).GetFieldInfo());
            Assert.AreEqual(typeof(ReflectionBurnEffect).GetField(nameof(ReflectionBurnEffect.Damage)),
                Effect.FindPropertyRelative(nameof(ReflectionBurnEffect.Damage)).GetFieldInfo());
        }

        [Test]
        public void GetFieldInfo_FindsPrivateFieldOfBaseClass()
        {
            var field = _serializedObject.FindProperty("_level").GetFieldInfo();

            Assert.IsNotNull(field);
            Assert.AreEqual(typeof(ReflectionBookBase), field.DeclaringType);
        }

        [Test]
        public void GetDeclaringInstance_ReturnsOwnerOfTheField()
        {
            var ability = Abilities.GetArrayElementAtIndex(0);

            Assert.AreSame(_book, Abilities.GetDeclaringInstance());
            Assert.AreSame(_book, ability.GetDeclaringInstance());
            Assert.AreSame(_book.Abilities[0], ability.FindPropertyRelative(nameof(ReflectionAbility.Name)).GetDeclaringInstance());
            Assert.AreSame(_book.Effect, Effect.FindPropertyRelative(nameof(ReflectionBurnEffect.Damage)).GetDeclaringInstance());
        }

        [Test]
        public void Resolution_ReturnsNullForIndexOutsideTheList()
        {
            var name = Abilities.GetArrayElementAtIndex(1).FindPropertyRelative(nameof(ReflectionAbility.Name));
            _book.Abilities.RemoveAt(1);

            Assert.IsNull(name.GetDeclaringInstance());
            Assert.IsNull(name.GetFieldInfo());
            Assert.IsNull(name.GetPropertyType());
        }

        [Test]
        public void Resolution_ReturnsNullForNullReferenceOnThePath()
        {
            var damage = Effect.FindPropertyRelative(nameof(ReflectionBurnEffect.Damage));
            _book.Effect = null;

            Assert.IsNull(damage.GetDeclaringInstance());
            Assert.IsNull(damage.GetFieldInfo());
            Assert.IsNull(damage.GetPropertyType());
        }
    }
}
