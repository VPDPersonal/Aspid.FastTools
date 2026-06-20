using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.ShortcutManagement;
using Aspid.FastTools.Editors;
using Aspid.FastTools.UIElements;
using Aspid.FastTools.UIElements.Editors.Internal;
using Object = UnityEngine.Object;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    /// <summary>
    /// The single managed-reference workbench. Two modes share one window: <b>Asset References</b> maps a saved asset's
    /// whole reference graph and repairs entries inline, and <b>Project References</b> sweeps the project for missing
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
        private const string TabHintClass = RootClass + "__tab-hint";
        private const string TabIconClass = RootClass + "__tab-icon";
        private const string TabIconHomeClass = TabIconClass + "--home";
        private const string TabIconSettingsClass = TabIconClass + "--settings";
        private const string ContainerClass = RootClass + "__container";

        private const string WindowStyleSheetPath = "UI/SerializeReferences/Aspid-FastTools-SerializeReference-Window";

        // ShortcutManager ids for the tab-switch bindings. They surface under this category in Edit > Shortcuts and are
        // user-rebindable; the visible tab badges read the live binding back from these ids (see BindingLabel).
        private const string ShortcutCategory = "Aspid FastTools/Managed References/";
        private const string HomeShortcutId = ShortcutCategory + "Home";
        private const string InspectShortcutId = ShortcutCategory + "Asset References";
        private const string ProjectShortcutId = ShortcutCategory + "Project References";
        private const string SettingsShortcutId = ShortcutCategory + "Settings";

        private AspidAnimatedDotsBackground _background;
        private VisualElement _container;
        private Button _homeButton;
        private Button _inspectButton;
        private Button _projectButton;
        private Button _settingsButton;
        private Mode _mode = Mode.Inspect;
        private Object _pendingTarget;

        // One-shot flag: the breakage-notification deep-link wants the project scanned immediately even from a cold
        // index, whereas a plain Project References tab click is warmth-gated inside the view. Consumed in SwitchMode.
        private bool _forceProjectScan;

        /// <summary>Opens the window on the Welcome home tab — the menu entry and the first-run auto-show.</summary>
        [MenuItem("Tools/Aspid 🐍/FastTools/Welcome", priority = 0)]
        public static void OpenWelcome()
        {
            var window = Reveal();
            window.SwitchMode(Mode.Welcome);
            WelcomeWindowStartup.MarkSeen();
        }

        // The priority gap (0 → 20 → 40) is wider than Unity's 10-step separator threshold, so the menu renders
        // Welcome / [Asset + Project References] / Settings as three separated groups.
        [MenuItem("Tools/Aspid 🐍/FastTools/Asset References", priority = 20)]
        private static void OpenMenu() => Open(Selection.activeObject);

        /// <summary>Opens the window in Inspect mode on <paramref name="target"/> (the deep-link for per-asset repair).</summary>
        public static void Open(Object target)
        {
            var window = Reveal();
            window._pendingTarget = target;
            window.SwitchMode(Mode.Inspect);
        }

        /// <summary>Opens the window on the Project References tab (no auto-scan — the idle Scan panel shows first).</summary>
        [MenuItem("Tools/Aspid 🐍/FastTools/Project References", priority = 21)]
        private static void OpenProject() => Reveal().SwitchMode(Mode.Project);

        /// <summary>Opens the window straight into a project audit (the breakage-notification deep-link).</summary>
        public static void OpenProjectScan()
        {
            var window = Reveal();
            window._forceProjectScan = true;
            window.SwitchMode(Mode.Project);
        }

        /// <summary>Opens the window on the Settings tab.</summary>
        [MenuItem("Tools/Aspid 🐍/FastTools/Settings", priority = 40)]
        private static void OpenSettings() => Reveal().SwitchMode(Mode.Settings);

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

            _homeButton = SquareTabButton(Mode.Welcome, TabIconHomeClass, BindingLabel(HomeShortcutId, 1));
            _inspectButton = ModeButton("Asset References", Mode.Inspect, BindingLabel(InspectShortcutId, 2));
            _projectButton = ModeButton("Project References", Mode.Project, BindingLabel(ProjectShortcutId, 3));
            _settingsButton = SquareTabButton(Mode.Settings, TabIconSettingsClass, BindingLabel(SettingsShortcutId, 0));

            var toolbar = new VisualElement().AddClass(ToolbarClass);
            toolbar.AddChild(_homeButton)
                .AddChild(_inspectButton)
                .AddChild(_projectButton)
                .AddChild(_settingsButton);

            _container = new VisualElement().AddClass(ContainerClass);
            _container.style.flexGrow = 1;

            // The version/GitHub footer is owned by the window, not any single tab, so it stays pinned to the bottom
            // across every mode. _container (flex-grow:1) takes the remaining height and pushes the footer down; the
            // footer is transparent, so the shared dotted canvas reads continuously behind it.
            root.AddChild(_background)
                .AddChild(toolbar)
                .AddChild(_container)
                .AddChild(new AspidWindowFooter());

            SwitchMode(_mode);
        }

        private Button ModeButton(string label, Mode mode, string hint)
        {
            var button = new Button(() => SwitchMode(mode)) { text = label, tooltip = hint };
            button.AddClass(ToolbarButtonClass);

            // The tab's switch shortcut, shown as a small keyboard-cap badge pinned to the right of the centred label so
            // the binding is discoverable on the tab itself. Absolutely positioned (like the underline below), so it
            // floats over the button without disturbing its centred text.
            button.AddChild(new Label(hint)
                .AddClass(TabHintClass)
                .SetPickingMode(PickingMode.Ignore));

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
        private Button SquareTabButton(Mode mode, string iconModifierClass, string hint)
        {
            var button = new Button(() => SwitchMode(mode)) { tooltip = hint };
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

        // Tab-switch hotkeys, registered with the ShortcutManager against this window as the context. A context shortcut
        // is dispatched at the editor level whenever the window is focused — unlike a panel KeyDownEvent, which only
        // fires while some element *inside* the window holds keyboard focus, so it would go silent the moment the user
        // clicked empty chrome. This way the bindings always work as long as the window is focused. Alt+digit is used
        // because Unity reserves every primary-modifier digit combo globally (Cmd/Ctrl+digit, +Shift, +Alt — see the
        // tab badges); Alt alone is otherwise only bound inside Scene View / Shader Graph, which are different contexts.
        // The bindings appear under "Aspid FastTools/Managed References" in Edit > Shortcuts and are user-rebindable.
        [Shortcut(HomeShortcutId, typeof(SerializeReferenceWindow), KeyCode.Alpha1, ShortcutModifiers.Alt)]
        private static void OnHomeShortcut(ShortcutArguments args) => SwitchFrom(args, Mode.Welcome);

        [Shortcut(InspectShortcutId, typeof(SerializeReferenceWindow), KeyCode.Alpha2, ShortcutModifiers.Alt)]
        private static void OnInspectShortcut(ShortcutArguments args) => SwitchFrom(args, Mode.Inspect);

        [Shortcut(ProjectShortcutId, typeof(SerializeReferenceWindow), KeyCode.Alpha3, ShortcutModifiers.Alt)]
        private static void OnProjectShortcut(ShortcutArguments args) => SwitchFrom(args, Mode.Project);

        [Shortcut(SettingsShortcutId, typeof(SerializeReferenceWindow), KeyCode.Alpha0, ShortcutModifiers.Alt)]
        private static void OnSettingsShortcut(ShortcutArguments args) => SwitchFrom(args, Mode.Settings);

        // The shortcut's context is the focused window instance, handed in via ShortcutArguments.
        private static void SwitchFrom(ShortcutArguments args, Mode mode)
        {
            if (args.context is SerializeReferenceWindow window)
                window.SwitchMode(mode);
        }

        // The badge / tooltip text for a tab: the shortcut's live binding read straight from the ShortcutManager, so it
        // tracks any rebind the user makes in Edit > Shortcuts and renders the real per-platform glyph (⌥2 on macOS,
        // Alt+2 elsewhere — the same string the Shortcuts window shows). Falls back to the default ⌥/Alt hint if the id
        // isn't registered yet (e.g. first import before discovery) or its binding has been cleared.
        private static string BindingLabel(string shortcutId, int number)
        {
            try
            {
                var binding = ShortcutManager.instance.GetShortcutBinding(shortcutId).ToString();
                if (!string.IsNullOrEmpty(binding)) return binding;
            }
            catch (System.Exception)
            {
                // ShortcutManager not ready / unknown id — fall through to the static default below.
            }

            return ShortcutHint(number);
        }

        // The default tab-switch hint, used as BindingLabel's fallback: the ⌥ Option glyph on macOS (its own "icon"),
        // spelled-out Alt+ elsewhere. Mirrors the [Shortcut] defaults above (ShortcutModifiers.Alt + the digit).
        private static string ShortcutHint(int number) =>
            (Application.platform == RuntimePlatform.OSXEditor ? "⌥" : "Alt+") + number;

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

                // A plain tab switch never auto-scans (Initialize just shows the idle Scan panel — no scan freeze on
                // large projects); only the breakage-notification deep-link forces the scan, since the user opened it
                // specifically to see the breakage.
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
