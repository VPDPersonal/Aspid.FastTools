using UnityEditor;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors.Internal
{
    /// <summary>
    /// A <see cref="VisualElement"/> that renders a styled dividing line.
    /// Supports theme, status, and size customisation, all of which can be driven by USS
    /// custom properties or set explicitly in code.
    /// </summary>
    [UxmlElement(libraryPath = "Aspid/FastTools")]
    public sealed partial class AspidDividingLine : VisualElement
    {
        /// <summary>
        /// USS class for the <see cref="DividingLineSize.Thin"/> variant.
        /// </summary>
        public const string ThinClass = "aspid-fasttools-thin-line";

        /// <summary>
        /// USS class for the <see cref="DividingLineSize.Medium"/> variant.
        /// </summary>
        public const string MediumClass = "aspid-fasttools-medium-line";

        /// <summary>
        /// USS class for the <see cref="DividingLineSize.Bold"/> variant.
        /// </summary>
        public const string BoldClass = "aspid-fasttools-bold-line";

        /// <summary>
        /// USS class for the <see cref="DividingLineDirection.Horizontal"/> orientation.
        /// </summary>
        public const string HorizontalClass = "aspid-fasttools-horizontal-line";

        /// <summary>
        /// USS class for the <see cref="DividingLineDirection.Vertical"/> orientation.
        /// </summary>
        public const string VerticalClass = "aspid-fasttools-vertical-line";

        /// <summary>
        /// USS class applied when the editor runs at low DPI (<c>pixelsPerPoint &lt; 2</c>),
        /// so thin strokes stay visible.
        /// </summary>
        public const string LowDpiClass = "aspid-fasttools-low-dpi";
        
        /// <summary>
        /// Custom USS property for overriding the line size via USS.
        /// </summary>
        public static readonly CustomStyleProperty<string> SizeStyleProperty = new("--aspid-fasttools-metrics-line_size");

        /// <summary>
        /// Custom USS property for overriding the line orientation via USS.
        /// </summary>
        public static readonly CustomStyleProperty<string> DirectionStyleProperty = new("--aspid-fasttools-prop-line_direction");

        private const string StyleSheetPath = "UI/Components/Aspid-FastTools-AspidDividingLine";
        
        private StyleOverride<ThemeStyle> _theme;
        private StyleOverride<StatusStyle> _status;
        private StyleOverride<DividingLineSize> _size;
        private StyleOverride<DividingLineDirection> _direction;

        /// <summary>
        /// Gets or sets the visual theme of the line.
        /// </summary>
        [UxmlAttribute]
        public ThemeStyle Theme
        {
            get => _theme;
            set => _theme.Set(value);
        }

        /// <summary>
        /// Gets or sets the status color accent of the line.
        /// </summary>
        [UxmlAttribute]
        public StatusStyle Status
        {
            get => _status;
            set => _status.Set(value);
        }

        /// <summary>
        /// Gets or sets the thickness of the line.
        /// </summary>
        [UxmlAttribute]
        public DividingLineSize Size
        {
            get => _size;
            set => _size.Set(value);
        }

        /// <summary>
        /// Gets or sets the orientation of the line.
        /// </summary>
        [UxmlAttribute]
        public DividingLineDirection Direction
        {
            get => _direction;
            set => _direction.Set(value);
        }

        /// <summary>
        /// Creates an <see cref="AspidDividingLine"/> using <see cref="DividingLinePreset.Default"/>.
        /// </summary>
        public AspidDividingLine()
            : this(DividingLinePreset.Default) { }

        /// <summary>
        /// Creates an <see cref="AspidDividingLine"/> with the given preset.
        /// </summary>
        /// <param name="preset">The configuration preset to apply.</param>
        public AspidDividingLine(DividingLinePreset preset)
        {
            this.AddStyleSheetsFromResource(StyleSheetPath);
            
            if (EditorGUIUtility.pixelsPerPoint < 2f)
                this.AddClass(LowDpiClass);

            _theme = new StyleOverride<ThemeStyle>(preset.Theme, (oldValue, newValue) =>
            {
                this.RemoveClass(oldValue.ToUss())
                    .AddClass(newValue.ToUss());
            });

            _status = new StyleOverride<StatusStyle>(preset.Status, (oldValue, newValue) =>
            {
                this.RemoveClass(oldValue.ToUss())
                    .AddClass(newValue.ToUss());
            });

            _size = new StyleOverride<DividingLineSize>(preset.Size, (oldValue, newValue) =>
            {
                this.RemoveClass(oldValue.ToUss())
                    .AddClass(newValue.ToUss());
            });

            _direction = new StyleOverride<DividingLineDirection>(preset.Direction, (oldValue, newValue) =>
            {
                this.RemoveClass(oldValue.ToUss())
                    .AddClass(newValue.ToUss());
            });

            RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);
        }

        private void OnCustomStyleResolved(CustomStyleResolvedEvent evt)
        {
            if (evt.customStyle.TryGetByEnum<ThemeStyle>(StyleClasses.Theme.Property, out var themeValue))
                _theme.SetDefault(themeValue);

            if (evt.customStyle.TryGetByEnum<StatusStyle>(StyleClasses.Status.Property, out var statusValue))
                _status.SetDefault(statusValue);

            if (evt.customStyle.TryGetByEnum<DividingLineSize>(SizeStyleProperty, out var sizeValue))
                _size.SetDefault(sizeValue);

            if (evt.customStyle.TryGetByEnum<DividingLineDirection>(DirectionStyleProperty, out var directionValue))
                _direction.SetDefault(directionValue);
        }
    }
} 
