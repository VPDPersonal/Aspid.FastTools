using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Aspid.FastTools.Editors;
using Aspid.FastTools.UIElements;
using Aspid.FastTools.UIElements.Editors.Internal;
using Object = UnityEngine.Object;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    /// <summary>
    /// The single managed-reference workbench. Two modes share one window: <b>Inspect Asset</b> maps a saved asset's
    /// whole reference graph and repairs entries inline, and <b>Project Audit</b> sweeps the project for missing
    /// references and bulk-fixes them grouped by broken type. The per-asset repair list of the old Repair window is
    /// subsumed by the richer Inspect graph; the project sweep keeps its grouped bulk-fix flow.
    /// </summary>
    internal sealed class SerializeReferenceWindow : EditorWindow
    {
        private enum Mode
        {
            Welcome,
            Inspect,
            Project,
            Settings,
        }

        private const string RootClass = "aspid-fasttools-serialize-reference-window";
        private const string BackgroundClass = RootClass + "__background";
        private const string ToolbarClass = RootClass + "__toolbar";
        private const string ToolbarButtonClass = RootClass + "__toolbar-button";
        private const string ToolbarButtonActiveClass = ToolbarButtonClass + "--active";
        private const string ToolbarButtonSquareClass = ToolbarButtonClass + "--square";
        private const string TabUnderlineClass = RootClass + "__tab-underline";
        private const string TabIconClass = RootClass + "__tab-icon";
        private const string TabIconHomeClass = TabIconClass + "--home";
        private const string TabIconSettingsClass = TabIconClass + "--settings";
        private const string ContainerClass = RootClass + "__container";

        private const string WindowStyleSheetPath = "UI/SerializeReferences/Aspid-FastTools-SerializeReference-Window";

        private AspidAnimatedDotsBackground _background;
        private VisualElement _container;
        private Button _homeButton;
        private Button _inspectButton;
        private Button _projectButton;
        private Button _settingsButton;
        private Mode _mode = Mode.Inspect;
        private Object _pendingTarget;

        // One-shot flag: the breakage-notification deep-link wants the project scanned immediately even from a cold
        // index, whereas a plain Project Audit tab click is warmth-gated inside the view. Consumed in SwitchMode.
        private bool _forceProjectScan;

        [MenuItem("Tools/Aspid 🐍/Managed References FastTools", priority = 21)]
        private static void OpenMenu() => Open(Selection.activeObject);

        /// <summary>Opens the window in Inspect mode on <paramref name="target"/> (the deep-link for per-asset repair).</summary>
        public static void Open(Object target)
        {
            var window = Reveal();
            window._pendingTarget = target;
            window.SwitchMode(Mode.Inspect);
        }

        /// <summary>Opens the window straight into a project audit (the breakage-notification deep-link).</summary>
        public static void OpenProjectScan()
        {
            var window = Reveal();
            window._forceProjectScan = true;
            window.SwitchMode(Mode.Project);
        }

        /// <summary>Opens the window on the Welcome home tab — the menu entry and the first-run auto-show.</summary>
        [MenuItem("Tools/Aspid 🐍/Welcome FastTools", priority = 0)]
        public static void OpenWelcome()
        {
            var window = Reveal();
            window.SwitchMode(Mode.Welcome);
            WelcomeWindowStartup.MarkSeen();
        }

        private static SerializeReferenceWindow Reveal()
        {
            var window = GetWindow<SerializeReferenceWindow>();
            window.titleContent = new GUIContent("Aspid FastTools");
            window.minSize = new Vector2(480f, 360f);
            window.Show();
            return window;
        }

        private void CreateGUI()
        {
            var root = rootVisualElement;
            root.AddAspidThemeStyleSheets()
                .AddStyleSheetsFromResource(WindowStyleSheetPath)
                .AddClass(RootClass);

            // One dotted canvas, owned by the window, absolutely fills it behind everything; the transparent toolbar
            // and mode content float over it so the dots read continuously. Its tint follows the active view's state,
            // routed through the SetCanvasTone callback handed to each view.
            _background = new AspidAnimatedDotsBackground()
                .AddClass(BackgroundClass)
                .SetPickingMode(PickingMode.Ignore);

            _homeButton = SquareTabButton(Mode.Welcome, TabIconHomeClass);
            _inspectButton = ModeButton("Inspect Asset", Mode.Inspect);
            _projectButton = ModeButton("Project Audit", Mode.Project);
            _settingsButton = SquareTabButton(Mode.Settings, TabIconSettingsClass);

            var toolbar = new VisualElement().AddClass(ToolbarClass);
            toolbar.AddChild(_homeButton)
                .AddChild(_inspectButton)
                .AddChild(_projectButton)
                .AddChild(_settingsButton);

            _container = new VisualElement().AddClass(ContainerClass);
            _container.style.flexGrow = 1;

            root.AddChild(_background).AddChild(toolbar).AddChild(_container);

            SwitchMode(_mode);
        }

        private Button ModeButton(string label, Mode mode)
        {
            var button = new Button(() => SwitchMode(mode)) { text = label };
            button.AddClass(ToolbarButtonClass);

            // The active underline is a child bar, not a border-bottom — flipping a child's background-color via the
            // parent's --active class repaints reliably (a border-color flip only showed up after a window resize).
            button.AddChild(new VisualElement()
                .AddClass(TabUnderlineClass)
                .SetPickingMode(PickingMode.Ignore));

            return button;
        }

        // The edge tabs (home on the left, settings on the right) are square and icon-only, not flex label tabs. The
        // USS --square modifier overrides the flex sizing into a square; the inner __tab-icon (plus an --home/--settings
        // glyph modifier supplying the background-image) carries the tint. Each keeps the same bottom underline bar as
        // the mode tabs (grey baseline, green when active) for consistent feedback.
        private Button SquareTabButton(Mode mode, string iconModifierClass)
        {
            var button = new Button(() => SwitchMode(mode));
            button.AddClass(ToolbarButtonClass).AddClass(ToolbarButtonSquareClass);

            button.AddChild(new VisualElement()
                .AddClass(TabIconClass)
                .AddClass(iconModifierClass)
                .SetPickingMode(PickingMode.Ignore));

            button.AddChild(new VisualElement()
                .AddClass(TabUnderlineClass)
                .SetPickingMode(PickingMode.Ignore));

            return button;
        }

        private void SwitchMode(Mode mode)
        {
            _mode = mode;
            if (_container is null) return; // Open() ran before CreateGUI; CreateGUI re-invokes SwitchMode(_mode).

            _container.Clear();

            if (mode == Mode.Welcome)
            {
                // Welcome carries no single status; restore the component's default green→amber→red "traffic light"
                // gradient on the shared canvas (a prior Inspect/Project toned every blob one colour, flattening it).
                _background?.SetSignalGradient();
                _container.AddChild(new WelcomeView());
            }
            else if (mode == Mode.Inspect)
            {
                _container.AddChild(new SerializeReferenceGraphView(_pendingTarget, SetCanvasTone));
            }
            else if (mode == Mode.Settings)
            {
                // Settings carries no status either; the calm idle tone keeps the canvas neutral here.
                SetCanvasTone(SerializeReferenceCanvasStyle.Info);
                _container.AddChild(new SettingsView());
            }
            else
            {
                var project = new SerializeReferenceProjectView
                {
                    OnInspectAsset = InspectAsset,
                    OnCanvasTone = SetCanvasTone,
                };
                _container.AddChild(project);

                // A plain tab switch is warmth-gated inside Initialize (no cold-scan freeze on large projects); the
                // breakage-notification deep-link forces the scan, since the user opened it to see the breakage.
                if (_forceProjectScan)
                {
                    _forceProjectScan = false;
                    project.ScanProject();
                }
                else
                {
                    project.Initialize();
                }
            }

            UpdateToolbar();
        }

        // The active view reports its state-tone here; the window owns the shared dotted canvas and applies it.
        private void SetCanvasTone(Color tone) => _background?.SetTone(tone);

        // Cross-link: jumping from a project-audit result to that asset's full graph.
        private void InspectAsset(Object target)
        {
            _pendingTarget = target;
            SwitchMode(Mode.Inspect);
        }

        private void UpdateToolbar()
        {
            _homeButton?.EnableInClassList(ToolbarButtonActiveClass, _mode == Mode.Welcome);
            _inspectButton?.EnableInClassList(ToolbarButtonActiveClass, _mode == Mode.Inspect);
            _projectButton?.EnableInClassList(ToolbarButtonActiveClass, _mode == Mode.Project);
            _settingsButton?.EnableInClassList(ToolbarButtonActiveClass, _mode == Mode.Settings);
        }
    }
}
