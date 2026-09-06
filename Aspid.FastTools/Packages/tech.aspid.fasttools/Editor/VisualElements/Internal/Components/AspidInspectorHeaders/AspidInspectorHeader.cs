using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Aspid.FastTools.Editors;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors.Internal
{
    // A VisualElement that renders an Inspector section header consisting of a script icon (with an optional open-
    // script command), a primary label, an optional subtext label, and an animated color gradient that appears on icon
    // hover when a status is set.
    [UxmlElement(libraryPath = "Aspid/FastTools")]
    internal sealed partial class AspidInspectorHeader : VisualElement
    {
        private const string StyleSheetPath = "UI/Components/Aspid-FastTools-AspidInspectorHeader";
        private const string ContainerClass = "aspid-fasttools-inspector-header__container";
        private const string IconClass = "aspid-fasttools-inspector-header__icon";
        private const string TextContainerClass = "aspid-fasttools-inspector-header__text-container";
        private const string TextClass = "aspid-fasttools-inspector-header__text";
        private const string SubtextClass = "aspid-fasttools-inspector-header__subtext";

        private readonly AspidBox _container;
        private readonly AspidLabel _textElement;
        private readonly AspidLabel _subtextElement;
        private readonly AspidHoverGradientOverlay _overlay;
        private readonly StatusStyle _status;

        private Object _obj;
        private MonoScript _script;

        [UxmlAttribute]
        public string Text
        {
            get => _textElement.Text;
            set => _textElement.Text = value;
        }

        [UxmlAttribute]
        public string Subtext
        {
            get => _subtextElement.Text;
            set => _subtextElement.Text = value;
        }

        [UxmlAttribute]
        public StatusStyle.Type Status
        {
            get => _status.Value;
            set => _status.SetValue(value);
        }

        public Object Obj
        {
            get => _obj;
            set
            {
                _obj = value;
                _script = value switch
                {
                    MonoBehaviour mono => MonoScript.FromMonoBehaviour(mono),
                    ScriptableObject scriptable => MonoScript.FromScriptableObject(scriptable),
                    _ => null
                };
            }
        }

        public AspidInspectorHeader()
            : this(AspidInspectorHeaderPreset.Default, obj: null) { }

        public AspidInspectorHeader(AspidInspectorHeaderPreset preset)
            : this(preset, obj: null) { }

        public AspidInspectorHeader(Object obj)
            : this(AspidInspectorHeaderPreset.Default.SetText(obj.GetScriptName()), obj) { }

        public AspidInspectorHeader(Component component)
            : this(AspidInspectorHeaderPreset.Default.SetText(component.GetScriptNameWithIndex()), component) { }

        public AspidInspectorHeader(string label, Object obj)
            : this(AspidInspectorHeaderPreset.Default.SetText(label), obj) { }

        public AspidInspectorHeader(AspidInspectorHeaderPreset preset, Object obj)
        {
            this.AddStyleSheetsFromResource(StyleSheetPath);

            _textElement = new AspidLabel(preset.Text).AddClass(TextClass);
            _subtextElement = new AspidLabel(preset.Subtext).AddClass(SubtextClass);

            Obj = obj;

            var iconElement = new Image()
                .AddClass(IconClass);

            var doubleClick = new DoubleClickTracker();
            iconElement.RegisterCallback<MouseUpEvent>(evt =>
            {
                if (evt.button != (int)MouseButton.LeftMouse) return;
                if (doubleClick.Detect() && _script) AssetDatabase.OpenAsset(_script);
            });

            iconElement.RegisterCallback<MouseEnterEvent>(OnIconMouseEnter);
            iconElement.RegisterCallback<MouseLeaveEvent>(OnIconMouseLeave);

            _overlay = new AspidHoverGradientOverlay();
            _ = new AspidInspectorHeaderGradientStyle(this, _overlay);

            var textContainer = new VisualElement()
                .AddClass(TextContainerClass)
                .AddChild(_textElement)
                .AddChild(_subtextElement);

            _container = new AspidBox(AspidBoxPreset.Default.SetTheme(ThemeStyle.Type.Dark));
            _container.AddClass(ContainerClass)
                .AddChild(_overlay)
                .AddChild(iconElement)
                .AddChild(textContainer);

            _status = new StatusStyle(this, preset.Status);

            this.AddChild(_container);
        }

        private void OnIconMouseEnter(MouseEnterEvent _)
        {
            if (Status is StatusStyle.Type.None) return;

            _overlay.SetTarget(1f);
            _container.Status = Status;
            _textElement.LabelStatus = Status;
            _subtextElement.LabelStatus = Status;
            _container.Theme = ThemeStyle.Type.Darkness;
        }

        private void OnIconMouseLeave(MouseLeaveEvent _)
        {
            if (Status is StatusStyle.Type.None) return;

            _overlay.SetTarget(0f);
            _container.Theme = ThemeStyle.Type.Dark;
            _container.Status = StatusStyle.Type.None;
            _textElement.LabelStatus = StatusStyle.Type.None;
            _subtextElement.LabelStatus = StatusStyle.Type.None;
        }
    }
}
