using UnityEngine;
using NUnit.Framework;
using Unity.Properties;
using UnityEngine.UIElements;
using Aspid.FastTools.UIElements.Editors.Internal.Tests;

namespace Aspid.FastTools.UIElements.Tests
{
    /// <summary>
    /// Anchors for the <see cref="VisualElement"/> property setters, the <see cref="Focusable"/> helpers
    /// and the manipulator helpers.
    /// </summary>
    [TestFixture]
    internal sealed class VisualElementExtensionsTests
    {
        #region Properties
        [Test]
        public void Setters_ReturnElementAndSetProperties()
        {
            var userData = new object();
            var dataSource = new object();
            var source = new VisualElement();

            VisualElement element = source
                .SetName("root")
                .SetVisible(false)
                .SetTooltip("Hint")
                .SetUserData(userData)
                .SetDataSource(dataSource)
                .SetViewDataKey("key")
                .SetDataSourceType(typeof(string))
                .SetUsageHints(UsageHints.DynamicTransform)
                .SetPickingMode(PickingMode.Ignore)
                .SetDisablePlayModeTint(true)
                .SetDataSourcePath(new PropertyPath("a.b"))
                .SetLanguageDirection(LanguageDirection.RTL);

            Assert.AreSame(source, element);
            Assert.AreEqual("root", element.name);
            Assert.IsFalse(element.visible);
            Assert.AreEqual("Hint", element.tooltip);
            Assert.AreSame(userData, element.userData);
            Assert.AreSame(dataSource, element.dataSource);
            Assert.AreEqual("key", element.viewDataKey);
            Assert.AreEqual(typeof(string), element.dataSourceType);
            Assert.AreEqual(UsageHints.DynamicTransform, element.usageHints);
            Assert.AreEqual(PickingMode.Ignore, element.pickingMode);
            Assert.IsTrue(element.disablePlayModeTint);
            Assert.AreEqual(new PropertyPath("a.b"), element.dataSourcePath);
            Assert.AreEqual(LanguageDirection.RTL, element.languageDirection);
        }

        [Test]
        public void SetEnabledSelf_DisablesElementAndReturnsIt()
        {
            var source = new VisualElement();

            var element = source.SetEnabledSelf(false);

            Assert.AreSame(source, element);
            Assert.IsFalse(element.enabledSelf);
        }
        #endregion

        #region Focusable
        [Test]
        public void Focusable_SetterProperties_ReturnElementAndSetProperties()
        {
            var source = new VisualElement();

            VisualElement element = source
                .SetTabIndex(3)
                .SetFocusable(true)
                .SetDelegatesFocus(true);

            Assert.AreSame(source, element);
            Assert.AreEqual(3, element.tabIndex);
            Assert.IsTrue(element.focusable);
            Assert.IsTrue(element.delegatesFocus);
        }

        [Test]
        public void Focusable_FocusSelfAndBlurSelf_MoveFocus()
        {
            using var panel = new TestPanel();
            var button = new Button();
            panel.Root.Add(button);

            Assert.AreSame(button, button.FocusSelf());
            Assert.IsTrue(button.IsFocused());

            Assert.AreSame(button, button.BlurSelf());
            Assert.IsFalse(button.IsFocused());
        }

        [Test]
        public void Focusable_IsFocused_OutsideAPanel_IsFalse()
        {
            Assert.IsFalse(new Button().IsFocused());
        }
        #endregion

        #region Manipulators
        [Test]
        public void AddManipulatorSelf_AttachesManipulator()
        {
            var element = new VisualElement();
            var manipulator = new Clickable(() => { });

            var result = element.AddManipulatorSelf(manipulator);

            Assert.AreSame(element, result);
            Assert.AreSame(element, manipulator.target);
        }

        [Test]
        public void RemoveManipulatorSelf_DetachesManipulator()
        {
            var element = new VisualElement();
            var manipulator = new Clickable(() => { });
            element.AddManipulatorSelf(manipulator);

            var result = element.RemoveManipulatorSelf(manipulator);

            Assert.AreSame(element, result);
            Assert.IsNull(manipulator.target);
        }

        [Test]
        public void AddClickable_Action_AttachesClickable()
        {
            var element = new VisualElement();

            var result = element.AddClickable(() => { }, out var clickable);

            Assert.AreSame(element, result);
            Assert.AreSame(element, clickable.target);
        }

        [Test]
        public void AddClickable_EventHandler_AttachesClickable()
        {
            var element = new VisualElement();

            var result = element.AddClickable(_ => { }, out var clickable);

            Assert.AreSame(element, result);
            Assert.AreSame(element, clickable.target);
        }

        [Test]
        public void Press_WithoutClickable_DoesNotCapturePointer()
        {
            using var panel = new TestPanel();
            var element = new VisualElement();
            panel.Root.Add(element);

            Assert.IsFalse(Press(element));
        }

        [Test]
        public void AddClickable_Action_AttachesManipulator()
        {
            using var panel = new TestPanel();
            var element = new VisualElement().AddClickable(() => { });
            panel.Root.Add(element);

            Assert.IsTrue(Press(element));
        }

        [Test]
        public void AddClickable_EventHandler_AttachesManipulator()
        {
            using var panel = new TestPanel();
            var element = new VisualElement().AddClickable(_ => { });
            panel.Root.Add(element);

            Assert.IsTrue(Press(element));
        }

        [Test]
        public void AddClickable_DelayAndInterval_AttachesRepeatingManipulator()
        {
            using var panel = new TestPanel();
            var element = new VisualElement().AddClickable(() => { }, delay: 100, interval: 50);
            panel.Root.Add(element);

            Assert.IsTrue(Press(element));
        }

        // Clickable captures the pointer when the left button goes down, so a capture shows that it is attached.
        private static bool Press(VisualElement element)
        {
            using var evt = PointerDownEvent.GetPooled(new Event { type = EventType.MouseDown, button = 0 });
            evt.target = element;
            element.SendEvent(evt);

            var captured = element.HasPointerCapture(PointerId.mousePointerId);
            element.ReleasePointer(PointerId.mousePointerId);
            return captured;
        }

        [Test]
        public void AddClickable_DelayAndInterval_AttachesManipulator()
        {
            var element = new VisualElement();

            var result = element.AddClickable(() => { }, delay: 100, interval: 50, out var clickable);

            Assert.AreSame(element, result);
            Assert.AreSame(element, clickable.target);
        }

        [Test]
        public void AddKeyboardNavigationManipulator_AttachesManipulator()
        {
            var element = new VisualElement();

            var result = element.AddKeyboardNavigationManipulator((_, _) => { }, out var manipulator);

            Assert.AreSame(element, result);
            Assert.AreSame(element, manipulator.target);
        }

        [Test]
        public void AddContextualMenuManipulator_AttachesManipulator()
        {
            var element = new VisualElement();

            var result = element.AddContextualMenuManipulator(_ => { }, out var manipulator);

            Assert.AreSame(element, result);
            Assert.AreSame(element, manipulator.target);
        }
        #endregion
    }
}
