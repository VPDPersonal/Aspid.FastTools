using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Aspid.FastTools.UIElements.Editors.Internal.Tests;

namespace Aspid.FastTools.UIElements.Tests
{
    [TestFixture]
    internal sealed class INotifyValueChangedExtensionsTests
    {
        private TestPanel _panel;
        private int _calls;

        [SetUp]
        public void SetUp()
        {
            _calls = 0;
            _panel = new TestPanel();
        }

        [TearDown]
        public void TearDown() => _panel.Dispose();

        private void CountCall(ChangeEvent<int> _) => _calls++;

        [UnityTest]
        public IEnumerator SetValue_Notify_RaisesEventInPanelWhenValueDiffers()
        {
            var field = new IntegerField().AddValueChanged(CountCall);
            _panel.Root.Add(field);
            yield return null;

            field.SetValue(5);

            Assert.AreEqual(5, field.value);
            Assert.AreEqual(1, _calls);
        }

        [Test]
        public void SetValue_Notify_RaisesNoEventOutsidePanel()
        {
            var field = new IntegerField().AddValueChanged(CountCall);

            field.SetValue(5);

            Assert.AreEqual(5, field.value);
            Assert.AreEqual(0, _calls);
        }

        [UnityTest]
        public IEnumerator SetValue_Notify_RaisesNoEventWhenValueIsEqual()
        {
            var field = new IntegerField().SetValue(5, notify: false);
            _panel.Root.Add(field);
            yield return null;
            field.AddValueChanged(CountCall);

            field.SetValue(5);

            Assert.AreEqual(0, _calls);
        }

        [UnityTest]
        public IEnumerator SetValue_NotifyFalse_RaisesNoEventInPanel()
        {
            var field = new IntegerField().AddValueChanged(CountCall);
            _panel.Root.Add(field);
            yield return null;

            field.SetValue(5, notify: false);

            Assert.AreEqual(5, field.value);
            Assert.AreEqual(0, _calls);
        }

        [UnityTest]
        public IEnumerator RemoveValueChanged_RemovesMethodGroup()
        {
            var field = new IntegerField();
            _panel.Root.Add(field);
            yield return null;

            field.AddValueChanged(CountCall).RemoveValueChanged(CountCall).SetValue(5);

            Assert.AreEqual(0, _calls);
        }

        [UnityTest]
        public IEnumerator RemoveValueChanged_KeepsInlineLambda()
        {
            var field = new IntegerField();
            _panel.Root.Add(field);
            yield return null;

            field.AddValueChanged(_ => _calls++).RemoveValueChanged(_ => _calls++).SetValue(5);

            Assert.AreEqual(1, _calls);
        }
    }
}
