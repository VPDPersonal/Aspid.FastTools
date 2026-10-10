using System;
using System.Linq;
using UnityEngine;
using NUnit.Framework;
using System.Reflection;
using UnityEngine.UIElements;
using System.Collections.Generic;
using Aspid.FastTools.UIElements.Editors.Internal.Tests;

namespace Aspid.FastTools.UIElements.Tests
{
    /// <summary>
    /// The value overloads are one template repeated for every value type. Real fields anchor the behavior,
    /// and a sweep over every overload with a probe element catches a flipped <c>notify</c> flag or an
    /// <c>Add</c> that unregisters in a single overload.
    /// </summary>
    [TestFixture]
    internal sealed class INotifyValueChangedExtensionsTests
    {
        // Compiled only when com.unity.mathematics is installed, so it is looked up by name.
        private const string MathExtensionsName =
            "Aspid.FastTools.UIElements.INotifyValueChangedMathExtensions, Aspid.FastTools.VisualElements.Math";

        // The lookup finds 36 overloads without com.unity.mathematics, which a CI project does not install,
        // and about 100 with it.
        private static readonly int MinOverloads = Type.GetType(MathExtensionsName) is null ? 30 : 50;

        #region Anchors
        [Test]
        public void SetValue_Notify_RaisesChangeEvent()
        {
            using var panel = new TestPanel();
            var field = new IntegerField();
            panel.Root.Add(field);
            var calls = 0;
            field.RegisterValueChangedCallback(_ => calls++);

            field.SetValue(5);

            Assert.AreEqual(5, field.value);
            Assert.AreEqual(1, calls);
        }

        [Test]
        public void SetValue_WithoutNotify_DoesNotRaiseChangeEvent()
        {
            using var panel = new TestPanel();
            var field = new IntegerField();
            panel.Root.Add(field);
            var calls = 0;
            field.RegisterValueChangedCallback(_ => calls++);

            field.SetValue(5, notify: false);

            Assert.AreEqual(5, field.value);
            Assert.AreEqual(0, calls);
        }

        [Test]
        public void SetValue_ReturnsFieldForChaining()
        {
            var source = new TextField();

            TextField field = source.SetValue("a", notify: false).SetValue("b", notify: false);

            Assert.AreSame(source, field);
            Assert.AreEqual("b", field.value);
        }

        [Test]
        public void SetValue_GenericOverload_UsesExplicitTypeArguments()
        {
            using var panel = new TestPanel();
            var field = new Vector3Field();
            panel.Root.Add(field);
            var calls = 0;
            field.RegisterValueChangedCallback(_ => calls++);

            field.SetValue<Vector3Field, Vector3>(Vector3.one, notify: false);
            Assert.AreEqual(Vector3.one, field.value);
            Assert.AreEqual(0, calls);

            field.SetValue<Vector3Field, Vector3>(Vector3.up);
            Assert.AreEqual(Vector3.up, field.value);
            Assert.AreEqual(1, calls);
        }

        [Test]
        public void AddValueChanged_RaisesCallback_AndRemoveValueChanged_StopsIt()
        {
            using var panel = new TestPanel();
            var field = new IntegerField();
            panel.Root.Add(field);
            var calls = 0;
            void OnChanged(ChangeEvent<int> _) => calls++;

            field.AddValueChanged(OnChanged);
            field.SetValue(1);
            field.RemoveValueChanged(OnChanged);
            field.SetValue(2);

            Assert.AreEqual(1, calls);
        }
        #endregion

        #region Sweeps
        [Test]
        public void SetValue_EveryOverload_NotifiesOnlyWhenAsked()
        {
            var failures = new List<string>();
            var checkedOverloads = 0;

            foreach (var (method, valueType) in GetOverloads("SetValue", valueParameter => valueParameter))
            {
                var probe = CreateProbe(valueType);
                var invoke = method.MakeGenericMethod(probe.GetType());
                var value = valueType.IsValueType ? Activator.CreateInstance(valueType) : null;
                var name = $"SetValue({valueType.Name})";
                checkedOverloads++;

                var returned = invoke.Invoke(null, BindingFlags.OptionalParamBinding, null, new[] { probe, value, Type.Missing }, null);
                if (!ReferenceEquals(returned, probe)) failures.Add($"{name} must return the element");
                if (!IsCounted((IProbe)probe, notified: 1, silent: 0)) failures.Add($"{name} must notify by default");

                invoke.Invoke(null, new[] { probe, value, false });
                if (!IsCounted((IProbe)probe, notified: 1, silent: 1)) failures.Add($"{name} with notify: false must not notify");

                invoke.Invoke(null, new[] { probe, value, true });
                if (!IsCounted((IProbe)probe, notified: 2, silent: 1)) failures.Add($"{name} with notify: true must notify");
            }

            Assert.Greater(checkedOverloads, MinOverloads, "The reflection lookup must find the SetValue overloads.");
            Assert.IsEmpty(failures, string.Join(Environment.NewLine, failures));
        }

        [Test]
        public void ValueChanged_EveryOverload_RegistersAndUnregistersTheCallback()
        {
            using var panel = new TestPanel();
            var failures = new List<string>();
            var checkedOverloads = 0;

            var removers = GetOverloads("RemoveValueChanged", callbackParameter => GetChangeEventValueType(callbackParameter))
                .ToDictionary(overload => overload.ValueType, overload => overload.Method);

            foreach (var (adder, valueType) in GetOverloads("AddValueChanged", callbackParameter => GetChangeEventValueType(callbackParameter)))
            {
                var name = $"ValueChanged({valueType.Name})";
                var probe = CreateProbe(valueType);
                var counter = (ICounter)Activator.CreateInstance(typeof(Counter<>).MakeGenericType(valueType));
                var callback = Delegate.CreateDelegate(
                    typeof(EventCallback<>).MakeGenericType(typeof(ChangeEvent<>).MakeGenericType(valueType)),
                    counter,
                    counter.GetType().GetMethod(nameof(Counter<int>.OnChanged)));
                panel.Root.Add((VisualElement)probe);
                checkedOverloads++;

                if (!removers.TryGetValue(valueType, out var remover))
                {
                    failures.Add($"{name} has no RemoveValueChanged overload");
                    continue;
                }

                var value = valueType.IsValueType ? Activator.CreateInstance(valueType) : null;
                var add = adder.MakeGenericMethod(probe.GetType());
                var remove = remover.MakeGenericMethod(probe.GetType());

                if (!ReferenceEquals(add.Invoke(null, new object[] { probe, callback }), probe)) failures.Add($"{name} Add must return the element");
                probe.Value = value;
                if (counter.Count != 1) failures.Add($"{name} Add must register the callback, it ran {counter.Count} times");

                if (!ReferenceEquals(remove.Invoke(null, new object[] { probe, callback }), probe)) failures.Add($"{name} Remove must return the element");
                probe.Value = value;
                if (counter.Count != 1) failures.Add($"{name} Remove must unregister the callback, it ran {counter.Count} times");
            }

            Assert.Greater(checkedOverloads, MinOverloads, "The reflection lookup must find the AddValueChanged overloads.");
            Assert.IsEmpty(failures, string.Join(Environment.NewLine, failures));
        }

        // Overloads of the extension classes that take the value as the second parameter and a single type argument.
        private static IEnumerable<(MethodInfo Method, Type ValueType)> GetOverloads(string name, Func<Type, Type> getValueType)
        {
            var extensions = new List<Type> { typeof(INotifyValueChangedExtensions) };

            var math = Type.GetType(MathExtensionsName);
            if (math is not null) extensions.Add(math);

            return extensions
                .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.Static))
                .Where(method => method.Name == name && method.GetGenericArguments().Length == 1)
                .Select(method => (method, getValueType(method.GetParameters()[1].ParameterType)));
        }

        private static Type GetChangeEventValueType(Type callbackType) =>
            callbackType.GetGenericArguments()[0].GetGenericArguments()[0];

        private static IProbe CreateProbe(Type valueType) =>
            (IProbe)Activator.CreateInstance(typeof(ValueProbe<>).MakeGenericType(valueType));

        private static bool IsCounted(IProbe probe, int notified, int silent) =>
            probe.NotifiedSets == notified && probe.SilentSets == silent;
        #endregion

        #region Probes
        private interface IProbe
        {
            object Value { set; }

            int SilentSets { get; }

            int NotifiedSets { get; }
        }

        private interface ICounter
        {
            int Count { get; }
        }

        private sealed class Counter<TValue> : ICounter
        {
            public int Count { get; private set; }

            public void OnChanged(ChangeEvent<TValue> evt) => Count++;
        }

        // Counts the two ways of setting a value and raises the change event the way a field does.
        private sealed class ValueProbe<TValue> : VisualElement, INotifyValueChanged<TValue>, IProbe
        {
            private TValue _value;

            public int SilentSets { get; private set; }

            public int NotifiedSets { get; private set; }

            object IProbe.Value
            {
                set => this.value = (TValue)value;
            }

            public TValue value
            {
                get => _value;
                set
                {
                    NotifiedSets++;

                    using var evt = ChangeEvent<TValue>.GetPooled(_value, value);
                    evt.target = this;
                    _value = value;
                    SendEvent(evt);
                }
            }

            public void SetValueWithoutNotify(TValue newValue)
            {
                SilentSets++;
                _value = newValue;
            }
        }
        #endregion
    }
}
