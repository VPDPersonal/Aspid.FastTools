using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Aspid.FastTools.UIElements;
using Aspid.FastTools.UIElements.Editors.Internal;
using Object = UnityEngine.Object;

using Aspid.FastTools.SerializeReferences.Editors;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Editors
{
    internal sealed class TabWindow : EditorWindow
    {
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

        private const string WindowIconPath = "Icons/aspid_icon_window_tab_green_1022x1011";

        private static readonly Vector2 _minWindowSize = new(480f, 360f);

        [Tooltip("The asset the References tabs open when the window is rebuilt.")]
        [SerializeField] private Object _pendingTarget;

        private AspidAnimatedDotsBackground _background;
        private VisualElement _container;
        private Button _homeButton;
        private Button _inspectButton;
        private Button _projectButton;
        private Button _settingsButton;

        private bool _forceProjectScan;

        internal TabType CurrentTabType { get; private set; }

        #region Open Methods
        [MenuItem("Tools/Aspid 🐍/FastTools/Welcome", priority = 0)]
        public static void OpenWelcome()
        {
            var window = Open();
            window.SwitchMode(TabType.Welcome);

            WelcomeWindowStartup.MarkSeen();
        }

        [MenuItem("Tools/Aspid 🐍/FastTools/Asset References", priority = 20)]
        public static void OpenAssetReferences() =>
            OpenAssetReferences(Selection.activeObject);

        public static void OpenAssetReferences(Object target)
        {
            var window = Open();

            window._pendingTarget = target;
            window.SwitchMode(TabType.AssetReference);
        }

        [MenuItem("Tools/Aspid 🐍/FastTools/Project References", priority = 21)]
        public static void OpenProjectReferences() =>
            Open().SwitchMode(TabType.ProjectReferences);

        [MenuItem("Tools/Aspid 🐍/FastTools/Settings", priority = 40)]
        public static void OpenSettings() =>
            Open().SwitchMode(TabType.Settings);

        private static TabWindow Open()
        {
            var window = GetWindow<TabWindow>();
            window.Show();

            return window;
        }
        #endregion

        private void CreateGUI()
        {
            minSize = _minWindowSize;
            titleContent = new GUIContent("Aspid FastTools", Resources.Load<Texture2D>(WindowIconPath));

            var root = rootVisualElement;
            root.AddAspidThemeStyleSheets()
                .AddStyleSheetFromResources(WindowStyleSheetPath)
                .AddClass(RootClass);

            _background = new AspidAnimatedDotsBackground()
                .AddClass(BackgroundClass)
                .SetPickingMode(PickingMode.Ignore);

            _homeButton = SquareTabButton(TabType.Welcome, TabIconHomeClass);
            _inspectButton = ModeButton("Asset References", TabType.AssetReference);
            _projectButton = ModeButton("Project References", TabType.ProjectReferences);
            _settingsButton = SquareTabButton(TabType.Settings, TabIconSettingsClass);

            var toolbar = new VisualElement().AddClass(ToolbarClass);
            toolbar.AddChild(_homeButton)
                .AddChild(_inspectButton)
                .AddChild(_projectButton)
                .AddChild(_settingsButton);

            _container = new VisualElement().AddClass(ContainerClass);
            _container.style.flexGrow = 1;

            root.AddChild(_background)
                .AddChild(toolbar)
                .AddChild(_container)
                .AddChild(new AspidWindowFooter());

            SwitchMode(CurrentTabType);
        }

        private Button ModeButton(string label, TabType tabType)
        {
            var hint = TabWindowShortcuts.HintFor(tabType);

            var button = new Button(() => SwitchMode(tabType)) { text = label, tooltip = hint };
            button.AddClass(ToolbarButtonClass);

            button.AddChild(new Label(hint)
                .AddClass(TabHintClass)
                .SetPickingMode(PickingMode.Ignore));

            // A child background-color repaints reliably; changing the tab border color can wait until resize.
            button.AddChild(new VisualElement()
                .AddClass(TabUnderlineClass)
                .SetPickingMode(PickingMode.Ignore));

            return button;
        }

        private Button SquareTabButton(TabType tabType, string iconModifierClass)
        {
            var button = new Button(() => SwitchMode(tabType)) { tooltip = TabWindowShortcuts.HintFor(tabType) };
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

        internal void SwitchMode(TabType tabType)
        {
            CurrentTabType = tabType;
            if (_container is null) return; // CreateGUI applies the selected tab once the container exists.

            _container.Clear();

            if (tabType == TabType.Welcome)
            {
                SetCanvasStatus(StatusStyle.Type.None);
                _container.AddChild(new WelcomeView());
            }
            else if (tabType == TabType.AssetReference)
            {
                _container.AddChild(new SerializeReferenceGraphView(_pendingTarget, SetCanvasStatus, target => _pendingTarget = target));
            }
            else if (tabType == TabType.Settings)
            {
                SetCanvasStatus(StatusStyle.Type.Info);
                _container.AddChild(new SettingsView());
            }
            else
            {
                var project = new SerializeReferenceProjectView
                {
                    OnInspectAsset = InspectAsset,
                    OnCanvasStatus = SetCanvasStatus,
                };
                _container.AddChild(project);

                // Only a breakage deep-link may force a blocking project scan on a cold index.
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

        private void SetCanvasStatus(StatusStyle.Type status) =>
            _background?.SetStatus(status);

        private void InspectAsset(Object target)
        {
            _pendingTarget = target;
            SwitchMode(TabType.AssetReference);
        }

        private void UpdateToolbar()
        {
            _homeButton?.EnableInClassList(ToolbarButtonActiveClass, CurrentTabType == TabType.Welcome);
            _inspectButton?.EnableInClassList(ToolbarButtonActiveClass, CurrentTabType == TabType.AssetReference);
            _projectButton?.EnableInClassList(ToolbarButtonActiveClass, CurrentTabType == TabType.ProjectReferences);
            _settingsButton?.EnableInClassList(ToolbarButtonActiveClass, CurrentTabType == TabType.Settings);
        }
    }
}
