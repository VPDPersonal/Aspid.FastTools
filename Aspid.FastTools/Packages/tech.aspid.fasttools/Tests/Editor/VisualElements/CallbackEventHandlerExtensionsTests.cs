using NUnit.Framework;
using System.Collections;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Aspid.FastTools.UIElements.Editors.Internal.Tests;

namespace Aspid.FastTools.UIElements.Tests
{
    [TestFixture]
    internal sealed class CallbackEventHandlerExtensionsTests
    {
        private TestPanel _panel;

        [SetUp]
        public void SetUp() => _panel = new TestPanel();

        [TearDown]
        public void TearDown() => _panel.Dispose();

        [UnityTest]
        public IEnumerator RegisterCallbackSelf_InvokesCallbackAndKeepsElementType()
        {
            var calls = 0;
            var button = new Button();
            _panel.Root.Add(button);
            yield return null;

            var result = button
                .RegisterCallbackSelf((MouseEnterEvent _) => calls++)
                .SetTooltip("Apply");
            Send<MouseEnterEvent>(button);

            Assert.AreSame(button, result);
            Assert.AreEqual(1, calls);
        }

        [UnityTest]
        public IEnumerator RegisterCallbackSelf_WithExplicitTypeArguments_InvokesMethodGroup()
        {
            var calls = 0;
            var button = new Button();
            _panel.Root.Add(button);
            yield return null;

            void OnEnter(MouseEnterEvent evt) => calls++;
            button.RegisterCallbackSelf<Button, MouseEnterEvent>(OnEnter);
            Send<MouseEnterEvent>(button);

            Assert.AreEqual(1, calls);
        }

        [UnityTest]
        public IEnumerator UnregisterCallbackSelf_StopsCallback()
        {
            var calls = 0;
            var button = new Button();
            _panel.Root.Add(button);
            yield return null;

            EventCallback<MouseEnterEvent> callback = _ => calls++;
            var result = button
                .RegisterCallbackSelf(callback)
                .UnregisterCallbackSelf(callback);
            Send<MouseEnterEvent>(button);

            Assert.AreSame(button, result);
            Assert.AreEqual(0, calls);
        }

        [UnityTest]
        public IEnumerator RegisterCallbackSelf_UserArgs_PassesArgsToCallback()
        {
            var received = 0;
            var button = new Button();
            _panel.Root.Add(button);
            yield return null;

            EventCallback<MouseEnterEvent, int> callback = (_, args) => received = args;
            var result = button.RegisterCallbackSelf(callback, 7);
            Send<MouseEnterEvent>(button);

            Assert.AreSame(button, result);
            Assert.AreEqual(7, received);
        }

        [UnityTest]
        public IEnumerator UnregisterCallbackSelf_UserArgs_StopsCallback()
        {
            var calls = 0;
            var button = new Button();
            _panel.Root.Add(button);
            yield return null;

            EventCallback<MouseEnterEvent, int> callback = (_, _) => calls++;
            var result = button
                .RegisterCallbackSelf(callback, 7)
                .UnregisterCallbackSelf(callback);
            Send<MouseEnterEvent>(button);

            Assert.AreSame(button, result);
            Assert.AreEqual(0, calls);
        }

        [UnityTest]
        public IEnumerator RegisterCallbackSelf_UseTrickleDown_ChoosesPhase()
        {
            var parent = new VisualElement();
            var child = new VisualElement();
            parent.Add(child);
            _panel.Root.Add(parent);
            yield return null;

            var trickle = PropagationPhase.None;
            var bubble = PropagationPhase.None;
            parent
                .RegisterCallbackSelf((ClickEvent evt) => trickle = evt.propagationPhase, TrickleDown.TrickleDown)
                .RegisterCallbackSelf((ClickEvent evt) => bubble = evt.propagationPhase);
            Send<ClickEvent>(child);

            Assert.AreEqual(PropagationPhase.TrickleDown, trickle);
            Assert.AreEqual(PropagationPhase.BubbleUp, bubble);
        }

        private static void Send<TEvent>(VisualElement target)
            where TEvent : EventBase<TEvent>, new()
        {
            using var evt = EventBase<TEvent>.GetPooled();
            evt.target = target;
            target.SendEvent(evt);
        }
    }
}
