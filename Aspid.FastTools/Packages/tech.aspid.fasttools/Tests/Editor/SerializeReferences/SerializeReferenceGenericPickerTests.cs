using System;
using UnityEditor;
using UnityEngine;
using NUnit.Framework;
using System.Reflection;
using UnityEngine.UIElements;
using System.Collections.Generic;
using Aspid.FastTools.Types.Editors;

namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    internal interface IGenericPickerEffect { }

    internal interface IGenericPickerEffect<T> : IGenericPickerEffect { }

#pragma warning disable CS0169
    [Serializable]
    internal sealed class GenericPickerEffect<T> : IGenericPickerEffect<T>
    {
        [SerializeField] private T _value;
    }

    // The attribute pins T but nothing pins TScale, so the picker has to ask for both.
    [Serializable]
    internal sealed class GenericPickerScaledEffect<T, TScale> : IGenericPickerEffect<T>
    {
        [SerializeField] private T _value;
        [SerializeField] private TScale _scale;
    }

    [Serializable]
    internal sealed class GenericPickerContainer<T> : IGenericPickerEffect
    {
        [SerializeField] private T _value;
    }

    [Serializable]
    internal struct GenericPickerOptional<T>
    {
        [SerializeField] private T _value;
    }

    internal interface IGenericPickerBox<T> { }

    [Serializable]
    internal sealed class GenericPickerBox<T> : IGenericPickerBox<T>
    {
        [SerializeField] private T _value;
    }

    // No [Serializable]: Unity drops a field of this type, so it must not end up as a generic argument.
    internal sealed class GenericPickerUnserializableBox<T> : IGenericPickerBox<T>
    {
        private T _value;
    }

    // The constraint pins the argument of a nested box, so the argument page offers boxes already closed.
    [Serializable]
    internal sealed class GenericPickerBoxContainer<TBox> : IGenericPickerEffect
        where TBox : IGenericPickerBox<int>
    {
        [SerializeField] private TBox _box;
    }
#pragma warning restore CS0169

    /// <summary>
    /// The generic flow of the <c>[SerializeReference]</c> type picker, driven without a window: an argument page
    /// offers only generics Unity can serialize, a generic struct can be an argument, and a generic closed on the root
    /// page fits the <c>[TypeSelector]</c> types.
    /// </summary>
    [TestFixture]
    internal sealed class SerializeReferenceGenericPickerTests
    {
        private static readonly MethodInfo ActivateNode = typeof(TypeSelectorView)
            .GetMethod("ActivateNode", BindingFlags.Instance | BindingFlags.NonPublic);

        private static readonly Type FieldType = typeof(IGenericPickerEffect);

        private string _recentsJson;

        [SetUp]
        public void SetUp() =>
            _recentsJson = EditorPrefs.GetString(TypeSelectorPreferences.RecentsKey, string.Empty);

        // Picking records a recent type; the developer's own list must survive the tests.
        [TearDown]
        public void TearDown()
        {
            TypeSelectorPreferences.ClearRecents();

            if (!string.IsNullOrEmpty(_recentsJson))
                EditorPrefs.SetString(TypeSelectorPreferences.RecentsKey, _recentsJson);
        }

        [Test]
        public void GetAssignableGenericDefinitions_ValueTypesOnRequest_OffersAGenericStruct()
        {
            var classes = GenericTypeResolver.GetAssignableGenericDefinitions(typeof(object), null);
            var arguments = GenericTypeResolver.GetAssignableGenericDefinitions(typeof(object), null,
                includeValueTypes: true);

            CollectionAssert.DoesNotContain(classes, typeof(GenericPickerOptional<>),
                "A [SerializeReference] value cannot be a struct.");
            CollectionAssert.Contains(arguments, typeof(GenericPickerOptional<>),
                "A generic argument can be a struct.");
            CollectionAssert.DoesNotContain(arguments, typeof(Span<>),
                "A ref struct cannot be a generic argument.");
        }

        [Test]
        public void IsValidGenericArgument_FrameworkGenericStruct_IsRejected()
        {
            Assert.IsFalse(SerializeReferenceHelpers.IsValidGenericArgument(typeof(int?)));
            Assert.IsFalse(SerializeReferenceHelpers.IsValidGenericArgument(typeof(KeyValuePair<string, int>)));
            Assert.IsFalse(SerializeReferenceHelpers.IsValidGenericArgument(typeof(ValueTuple<int, int>)),
                "These carry [Serializable], but Unity does not serialize them.");
            Assert.IsTrue(SerializeReferenceHelpers.IsValidGenericArgument(typeof(GenericPickerOptional<int>)));
        }

        [Test]
        public void ArgumentPage_GenericStruct_ClosesTheArgument()
        {
            var picker = Open(narrowingTypes: null);

            picker.Pick(typeof(GenericPickerContainer<>));
            picker.Pick(typeof(GenericPickerOptional<>));
            picker.Pick(typeof(int));

            Assert.IsNull(picker.Error);
            Assert.AreEqual(typeof(GenericPickerContainer<GenericPickerOptional<int>>).AssemblyQualifiedName,
                picker.Selected);
        }

        [Test]
        public void ArgumentPage_GenericUnityDoesNotSerialize_IsNotOffered()
        {
            var picker = Open(narrowingTypes: null);

            picker.Pick(typeof(GenericPickerContainer<>));

            Assert.IsFalse(picker.Offers(typeof(GenericPickerUnserializableBox<>)), "It has no [Serializable].");
            Assert.IsFalse(picker.Offers(typeof(Nullable<>)), "Unity does not serialize Nullable<T>.");
            Assert.IsFalse(picker.Offers(typeof(KeyValuePair<,>)), "Unity does not serialize KeyValuePair<,>.");
        }

        [Test]
        public void ArgumentPage_InferredNestedGeneric_PassesTheArgumentFilter()
        {
            var picker = Open(narrowingTypes: null);

            picker.Pick(typeof(GenericPickerBoxContainer<>));

            Assert.IsFalse(picker.Offers(typeof(GenericPickerUnserializableBox<int>)),
                "Unity would drop the value of a type without [Serializable].");

            picker.Pick(typeof(GenericPickerBox<int>));

            Assert.AreEqual(typeof(GenericPickerBoxContainer<GenericPickerBox<int>>).AssemblyQualifiedName,
                picker.Selected);
        }

        // The last line of defence for a filter that lists definitions it cannot judge up front.
        [Test]
        public void ArgumentPage_ClosedGenericTheFilterRejects_IsNotStored()
        {
            var filter = SerializeReferenceHelpers.BuildPickerFilter(FieldType, baseTypes: null,
                excludeEditorOnly: false);
            filter.GenericDefinitionFilter = null;

            var picker = Open(filter);

            picker.Pick(typeof(GenericPickerContainer<>));
            picker.Pick(typeof(GenericPickerUnserializableBox<>));
            picker.Pick(typeof(int));

            Assert.IsNull(picker.Selected, "Unity would drop the value of a type without [Serializable].");
            StringAssert.Contains("GenericPickerUnserializableBox<Int32>", picker.Error);
        }

        [Test]
        public void RootPage_ArgumentPinnedByTheAttribute_IsInferred()
        {
            var picker = Open(narrowingTypes: new[] { typeof(IGenericPickerEffect<float>) });

            picker.Pick(typeof(GenericPickerEffect<>));

            Assert.AreEqual(typeof(GenericPickerEffect<float>).AssemblyQualifiedName, picker.Selected);
        }

        [Test]
        public void RootPage_ClosedTypeOutsideTheAttribute_IsRejected()
        {
            var picker = Open(narrowingTypes: new[] { typeof(IGenericPickerEffect<float>) });

            picker.Pick(typeof(GenericPickerScaledEffect<,>));
            picker.Pick(typeof(int));
            picker.Pick(typeof(int));

            Assert.IsNull(picker.Selected, "The attribute allows only IGenericPickerEffect<float>.");
            StringAssert.Contains("IGenericPickerEffect<Single>", picker.Error);
        }

        [Test]
        public void RootPage_UnboundAttributeType_AcceptsAnyClosing()
        {
            var picker = Open(narrowingTypes: new[] { typeof(IGenericPickerEffect<>) });

            picker.Pick(typeof(GenericPickerEffect<>));
            picker.Pick(typeof(int));

            Assert.IsNull(picker.Error);
            Assert.AreEqual(typeof(GenericPickerEffect<int>).AssemblyQualifiedName, picker.Selected,
                "typeof(IGenericPickerEffect<>) admits every IGenericPickerEffect<T>.");
        }

        // The filter the [SerializeReference] drawers build for a field of IGenericPickerEffect.
        private static Picker Open(Type[] narrowingTypes) =>
            Open(SerializeReferenceHelpers.BuildPickerFilter(FieldType, narrowingTypes, excludeEditorOnly: false));

        private static Picker Open(TypeSelectorFilter filter)
        {
            var picker = new Picker();

            picker.View = new TypeSelectorView(
                filter: filter,
                currentAqn: null,
                onSelected: assemblyQualifiedName => picker.Selected = assemblyQualifiedName);

            return picker;
        }

        private sealed class Picker
        {
            public TypeSelectorView View;
            public string Selected;

            public string Error
            {
                get
                {
                    var label = View.Q<Label>("type-selector-error");
                    return label.style.display == DisplayStyle.Flex ? label.text : null;
                }
            }

            // Picks the row of the type on the current page, as a click would.
            public void Pick(Type type)
            {
                var node = FindOnPage(type);

                Assert.IsNotNull(node, $"The current page must offer {type.Name}.");
                ActivateNode.Invoke(View, new object[] { node });
            }

            public bool Offers(Type type) =>
                FindOnPage(type) is not null;

            private TreeNode FindOnPage(Type type) =>
                Find((IEnumerable<TreeNode>)View.Q<ListView>().itemsSource, type.AssemblyQualifiedName);

            private static TreeNode Find(IEnumerable<TreeNode> nodes, string assemblyQualifiedName)
            {
                foreach (var node in nodes)
                {
                    if (node.AssemblyQualifiedName == assemblyQualifiedName) return node;

                    var child = Find(node.Children, assemblyQualifiedName);
                    if (child is not null) return child;
                }

                return null;
            }
        }
    }
}
