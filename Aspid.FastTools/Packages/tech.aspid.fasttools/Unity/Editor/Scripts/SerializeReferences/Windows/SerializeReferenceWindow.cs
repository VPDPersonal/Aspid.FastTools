using System;
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
        // Declaration order mirrors the toolbar left-to-right (Home → Asset References → Project References → Settings);
        // the Ctrl+Tab cycle relies on it, stepping through the values numerically with wrap-around.
        private enum Mode
        {
            Welcome,
            Inspect,
            Project,
            Settings,
        }

        // Derived, not hardcoded: a fifth tab added to Mode would otherwise compile cleanly while Ctrl+Tab silently
        // wrapped early and never reached it.
        private static readonly int ModeCount = Enum.GetValues(typeof(Mode)).Length;

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

        // The Aspid brand mark shown beside the window title; padded variant so it doesn't dominate the tab.
        private const string WindowIconPath = "Icons/aspid_icon_window_tab_green_1022x1011";

        // ShortcutManager ids for the tab-switch bindings. They surface under this category in Edit > Shortcuts and are
        // user-rebindable; the visible tab badges read the live binding back from these ids (see BindingLabel).
        private const string ShortcutCategory = "Aspid FastTools/Managed References/";
        private const string HomeShortcutId = ShortcutCategory + "Home";
        private const string InspectShortcutId = ShortcutCategory + "Asset References";
        private const string ProjectShortcutId = ShortcutCategory + "Project References";
        private const string SettingsShortcutId = ShortcutCategory + "Settings";
        private const string NextTabShortcutId = ShortcutCategory + "Next Tab";
        private const string PreviousTabShortcutId = ShortcutCategory + "Previous Tab";

        private AspidAnimatedDotsBackground _background;
        private VisualElement _container;
        private Button _homeButton;
        private Button _inspectButton;
        private Button _projectButton;
        private Button _settingsButton;
        // Serialized so the active tab and the inspected asset survive a domain reload — EditorWindow persists
        // [SerializeField] state across assembly reloads; a plain field would reset to its initializer.
        [SerializeField] private Mode _mode = Mode.Inspect;
        [SerializeField] private Object _pendingTarget;

        // One-shot flag: the breakage-notification deep-link wants the project scanned immediately even from a cold
        // index, whereas a plain Project References tab click is warmth-gated inside the view. Consumed in SwitchMode.
        private bool _forceProjectScan;

        /// <summary>
        /// Opens the window on the Welcome home tab — the menu entry and the first-run auto-show.
        /// </summary>
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

        /// <summary>
        /// Opens the window in Inspect mode on <paramref name="target"/> (the deep-link for per-asset repair).
        /// </summary>
        public static void Open(Object target)
        {
            var window = Reveal();
            window._pendingTarget = target;
            window.SwitchMode(Mode.Inspect);
        }

        /// <summary>
        /// Opens the window on the Project References tab (no auto-scan — the idle Scan panel shows first).
        /// </summary>
        [MenuItem("Tools/Aspid 🐍/FastTools/Project References", priority = 21)]
        private static void OpenProject() => Reveal().SwitchMode(Mode.Project);

        /// <summary>
        /// Opens the window straight into a project audit (the breakage-notification deep-link).
        /// </summary>
        public static void OpenProjectScan()
        {
            var window = Reveal();
            window._forceProjectScan = true;
            window.SwitchMode(Mode.Project);
        }

        /// <summary>
        /// Opens the window on the Settings tab. Also the deep-link target of the type selector's footer gear.
        /// </summary>
        [MenuItem("Tools/Aspid 🐍/FastTools/Settings", priority = 40)]
        public static void OpenSettings() => Reveal().SwitchMode(Mode.Settings);

        private static SerializeReferenceWindow Reveal()
        {
            var window = GetWindow<SerializeReferenceWindow>();
            window.titleContent = new GUIContent("Aspid FastTools", Resources.Load<Texture2D>(WindowIconPath));
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

            // One dotted canvas, owned by the window, fills it behind everything; its tint follows the active view's
            // state via the SetCanvasTone callback handed to each view.
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

            // The footer is owned by the window, not any single tab, so it stays pinned to the bottom across every
            // mode; _container (flex-grow:1) pushes it down.
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

            // Shortcut badge, absolutely positioned so it floats over the button without disturbing the centred label.
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

        // The edge tabs (home / settings) are square and icon-only: the USS --square modifier overrides the flex
        // sizing, the inner __tab-icon modifier supplies the glyph. Same underline bar as the mode tabs.
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

        // Registered against this window as the context: a context shortcut fires whenever the window is focused,
        // unlike a panel KeyDownEvent, which goes silent when focus sits on empty chrome. Alt+digit because Unity
        // reserves every primary-modifier digit combo globally; user-rebindable in Edit > Shortcuts.
        [Shortcut(HomeShortcutId, typeof(SerializeReferenceWindow), KeyCode.Alpha1, ShortcutModifiers.Alt)]
        private static void OnHomeShortcut(ShortcutArguments args) => SwitchFrom(args, Mode.Welcome);

        [Shortcut(InspectShortcutId, typeof(SerializeReferenceWindow), KeyCode.Alpha2, ShortcutModifiers.Alt)]
        private static void OnInspectShortcut(ShortcutArguments args) => SwitchFrom(args, Mode.Inspect);

        [Shortcut(ProjectShortcutId, typeof(SerializeReferenceWindow), KeyCode.Alpha3, ShortcutModifiers.Alt)]
        private static void OnProjectShortcut(ShortcutArguments args) => SwitchFrom(args, Mode.Project);

        [Shortcut(SettingsShortcutId, typeof(SerializeReferenceWindow), KeyCode.Alpha0, ShortcutModifiers.Alt)]
        private static void OnSettingsShortcut(ShortcutArguments args) => SwitchFrom(args, Mode.Settings);

        // Browser-style cyclic tab switching. ShortcutModifiers.Control is deliberate on BOTH platforms — Action would
        // map to ⌘ on macOS, and Cmd+Tab is reserved by the OS for the application switcher. If the ShortcutManager
        // ever refuses KeyCode.Tab, the fallback is a TrickleDown KeyDownEvent on rootVisualElement (KeyDown caveat above).
        [Shortcut(NextTabShortcutId, typeof(SerializeReferenceWindow), KeyCode.Tab, ShortcutModifiers.Control)]
        private static void OnNextTabShortcut(ShortcutArguments args) => CycleFrom(args, +1);

        [Shortcut(PreviousTabShortcutId, typeof(SerializeReferenceWindow), KeyCode.Tab, ShortcutModifiers.Control | ShortcutModifiers.Shift)]
        private static void OnPreviousTabShortcut(ShortcutArguments args) => CycleFrom(args, -1);

        private static void SwitchFrom(ShortcutArguments args, Mode mode)
        {
            if (args.context is SerializeReferenceWindow window)
                window.SwitchMode(mode);
        }

        private static void CycleFrom(ShortcutArguments args, int step)
        {
            if (args.context is not SerializeReferenceWindow window) return;

            var next = (Mode)(((int)window._mode + step + ModeCount) % ModeCount);
            window.SwitchMode(next);
        }

        // The tab's badge / tooltip: the live binding read from the ShortcutManager, so it tracks user rebinds and
        // renders the real per-platform glyph. Falls back to the static default when the id isn't registered yet or
        // its binding has been cleared.
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

        // BindingLabel's fallback, mirroring the [Shortcut] defaults: the ⌥ glyph on macOS, spelled-out Alt+ elsewhere.
        private static string ShortcutHint(int number) =>
            (Application.platform == RuntimePlatform.OSXEditor ? "⌥" : "Alt+") + number;

        private void SwitchMode(Mode mode)
        {
            _mode = mode;
            if (_container is null) return; // Open() ran before CreateGUI; CreateGUI re-invokes SwitchMode(_mode).

            _container.Clear();

            if (mode == Mode.Welcome)
            {
                // Welcome carries no single status; restore the default signal gradient a prior view's tone flattened.
                _background?.SetSignalGradient();
                _container.AddChild(new WelcomeView());
            }
            else if (mode == Mode.Inspect)
            {
                // Track the in-view pick back onto _pendingTarget so a tab switch rebuilds the view on the asset the user
                // actually has open, not the one Inspect first opened on.
                _container.AddChild(new SerializeReferenceGraphView(_pendingTarget, SetCanvasTone, target => _pendingTarget = target));
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

                // A plain tab switch never auto-scans (no scan freeze on large projects); only the
                // breakage-notification deep-link forces the scan.
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
