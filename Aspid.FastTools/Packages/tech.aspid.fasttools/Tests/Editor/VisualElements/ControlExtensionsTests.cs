using UnityEngine;
using NUnit.Framework;
using UnityEngine.UIElements;
using Aspid.FastTools.UIElements.Editors.Internal.Tests;

namespace Aspid.FastTools.UIElements.Tests
{
    /// <summary>
    /// Anchors for the single-control extension families: one test per family is enough to catch a setter
    /// that writes the wrong property or forgets to return the element.
    /// </summary>
    [TestFixture]
    internal sealed class ControlExtensionsTests
    {
        private TestPanel _panel;

        [SetUp]
        public void SetUp() => _panel = new TestPanel();

        [TearDown]
        public void TearDown() => _panel.Dispose();

        #region Button
        [Test]
        public void Button_AddClicked_RaisesCallback()
        {
            var calls = 0;
            var button = AddToPanel(new Button().AddClicked(() => calls++));

            Click(button);

            Assert.AreEqual(1, calls);
        }

        [Test]
        public void Button_RemoveClicked_StopsCallback()
        {
            var calls = 0;
            void OnClicked() => calls++;
            var button = AddToPanel(new Button().AddClicked(OnClicked));

            button.RemoveClicked(OnClicked);
            Click(button);

            Assert.AreEqual(0, calls);
        }

        [Test]
        public void Button_SetClickable_Action_ReplacesPreviousCallbacks()
        {
            var first = 0;
            var second = 0;
            var button = AddToPanel(new Button().AddClicked(() => first++));

            button.SetClickable(() => second++);
            Click(button);

            Assert.AreEqual(0, first);
            Assert.AreEqual(1, second);
        }

        [Test]
        public void Button_SetClickable_Clickable_AssignsTheInstance()
        {
            var clickable = new Clickable(() => { });

            var button = new Button().SetClickable(clickable);

            Assert.AreSame(clickable, button.clickable);
        }

        [Test]
        public void Button_SetIconImage_SetsBackground()
        {
            var texture = new Texture2D(2, 2);

            var button = new Button().SetIconImage(Background.FromTexture2D(texture));

            Assert.AreSame(texture, button.iconImage.texture);

            Object.DestroyImmediate(texture);
        }

        // A submit event is what Enter and the gamepad button send, and Button turns it into a click.
        private static void Click(Button button)
        {
            using var evt = NavigationSubmitEvent.GetPooled();
            evt.target = button;
            button.SendEvent(evt);
        }

        private T AddToPanel<T>(T element)
            where T : VisualElement
        {
            _panel.Root.Add(element);
            return element;
        }
        #endregion

        #region Text controls
        [Test]
        public void Foldout_Setters_ReturnFoldoutAndSetProperties()
        {
            var source = new Foldout();

            Foldout foldout = source.SetText("Section").SetToggleOnLabelClick(false);

            Assert.AreSame(source, foldout);
            Assert.AreEqual("Section", foldout.text);
            Assert.IsFalse(foldout.toggleOnLabelClick);
            Assert.IsTrue(foldout.SetToggleOnLabelClick(true).toggleOnLabelClick);
        }

        [Test]
        public void HelpBox_Setters_ReturnHelpBoxAndSetProperties()
        {
            var source = new HelpBox();

            HelpBox helpBox = source.SetText("Careful").SetMessageType(HelpBoxMessageType.Warning);

            Assert.AreSame(source, helpBox);
            Assert.AreEqual("Careful", helpBox.text);
            Assert.AreEqual(HelpBoxMessageType.Warning, helpBox.messageType);
        }

        [Test]
        public void BaseBoolField_Setters_ReturnToggleAndSetProperties()
        {
            var source = new Toggle();

            Toggle toggle = source.SetText("Enabled").SetLabel("Option").SetToggleOnLabelClick(false);

            Assert.AreSame(source, toggle);
            Assert.AreEqual("Enabled", toggle.text);
            Assert.AreEqual("Option", toggle.label);
            Assert.IsFalse(toggle.toggleOnLabelClick);
            Assert.IsTrue(toggle.SetToggleOnLabelClick(true).toggleOnLabelClick);
        }

        [Test]
        public void BaseField_SetLabel_KeepsFieldTypeForNumericField()
        {
            FloatField field = new FloatField().SetLabel("Speed");

            Assert.AreEqual("Speed", field.label);
        }

        [Test]
        public void BaseField_SetLabel_UsesExplicitTypeArgumentsForCustomValueType()
        {
            var field = new Vector3Field().SetLabel<Vector3Field, Vector3>("Offset");

            Assert.AreEqual("Offset", field.label);
        }

        [Test]
        public void TextElement_Setters_ReturnLabelAndSetProperties()
        {
            var source = new Label();

            Label label = source
                .SetEnableRichText(false)
                .SetEmojiFallbackSupport(false)
                .SetParseEscapeSequences(true)
                .SetDisplayTooltipWhenElided(false);

            Assert.AreSame(source, label);
            Assert.IsFalse(label.enableRichText);
            Assert.IsFalse(label.emojiFallbackSupport);
            Assert.IsTrue(label.parseEscapeSequences);
            Assert.IsFalse(label.displayTooltipWhenElided);
        }
        #endregion

        #region Value controls
        [Test]
        public void EnumField_Initialize_SetsValueAndReturnsField()
        {
            var source = new EnumField();

            EnumField field = source.Initialize(HelpBoxMessageType.Error);

            Assert.AreSame(source, field);
            Assert.AreEqual(HelpBoxMessageType.Error, field.value);
        }

        [Test]
        public void IMixedValueSupport_SetShowMixedValue_SetsProperty()
        {
            var source = new Toggle();

            Toggle toggle = source.SetShowMixedValue(true);

            Assert.AreSame(source, toggle);
            Assert.IsTrue(toggle.showMixedValue);
        }

        [Test]
        public void ProgressBar_Setters_ReturnBarAndSetProperties()
        {
            var source = new ProgressBar();

            ProgressBar bar = source
                .SetTitle("Loading")
                .SetLowValue(10f)
                .SetHighValue(200f)
                .SetValue(150f);

            Assert.AreSame(source, bar);
            Assert.AreEqual("Loading", bar.title);
            Assert.AreEqual(10f, bar.lowValue);
            Assert.AreEqual(200f, bar.highValue);
            Assert.AreEqual(150f, bar.value);
        }
        #endregion
    }
}
