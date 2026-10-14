using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Aspid.FastTools.UIElements;
using System.Collections.Generic;
using Aspid.FastTools.UIElements.Editors.Internal;
using Object = UnityEngine.Object;

using Aspid.FastTools.SerializeReferences.Editors;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Editors
{
    internal sealed class TabWindow : EditorWindow
    {
        private const string RootClass = "aspid-fasttools-tab-window";
        private const string BackgroundClass = RootClass + "__background";
        private const string ToolbarClass = RootClass + "__toolbar";
        private const string ToolbarButtonClass = RootClass + "__toolbar-button";
        private const string ToolbarButtonActiveClass = ToolbarButtonClass + "--active";
        private const string ToolbarButtonSquareClass = ToolbarButtonClass + "--square";
        private const string ToolbarButtonCompactClass = ToolbarButtonClass + "--compact";
        private const string TabTitleClass = RootClass + "__tab-title";
        private const string TabUnderlineClass = RootClass + "__tab-underline";
        private const string TabHintClass = RootClass + "__tab-hint";
        private const string TabIconClass = RootClass + "__tab-icon";
        private const string TabIconHomeClass = TabIconClass + "--home";
        private const string TabIconSettingsClass = TabIconClass + "--settings";
        private const string ContainerClass = RootClass + "__container";

        private const string WindowStyleSheetPath = "UI/Windows/Aspid-FastTools-TabWindow";

        private const string WindowIconPath = "Icons/aspid_icon_window_tab_green_64x64";

        // The margin-left of the tab hint in the window stylesheet: a hidden hint has no layout to read it from.
        private const float TabHintGap = 8f;

        private static readonly Vector2 _minWindowSize = new(480f, 360f);

        // One row per tab, in toolbar order; the toolbar, the footer and the shortcuts read it. A new tab adds a TabType
        // value, a row here, a case in CreateView and a [Shortcut] method in TabWindowShortcuts.
        internal static readonly TabDefinition[] Tabs =
        {
            new(TabType.Welcome, title: null, TabIconHomeClass,
                TabWindowShortcuts.HomeId, TabWindowShortcuts.HomeKey),
            new(TabType.AssetReference, "Asset References", iconClass: null,
                TabWindowShortcuts.AssetReferencesId, TabWindowShortcuts.AssetReferencesKey),
            new(TabType.ProjectReferences, "Project References", iconClass: null,
                TabWindowShortcuts.ProjectReferencesId, TabWindowShortcuts.ProjectReferencesKey),
            new(TabType.Settings, title: null, TabIconSettingsClass,
                TabWindowShortcuts.SettingsId, TabWindowShortcuts.SettingsKey,
                status: StatusStyle.Type.Info, hasAdjustableRows: true),
        };

        [Tooltip("The asset the References tabs open when the window is rebuilt.")]
        [SerializeField] private Object _pendingTarget;

        // Serialized so the tab survives a domain reload and a Play Mode entry; an auto-property is not serialized.
        [SerializeField] private TabType _currentTabType;

        // A view is built on the first visit to its tab and then kept, so its scroll position, expanded rows, repair
        // summaries and keyboard focus survive a visit to another tab; closing the window or a domain reload drops it.
        private readonly Dictionary<TabType, VisualElement> _views = new();

        // The canvas status each tab reported last, applied again when the tab is shown.
        private readonly Dictionary<TabType, StatusStyle.Type> _statuses = new();

        private readonly Dictionary<TabType, Button> _buttons = new();

        private AspidAnimatedDotsBackground _background;
        private AspidWindowFooter _footer;
        private VisualElement _container;

        internal TabType CurrentTabType => _currentTabType;

        #region Open Methods
        [MenuItem("Tools/Aspid 🐍/FastTools/Welcome", priority = 0)]
        public static void OpenWelcome()
        {
            var window = Open();
            window.SwitchMode(TabType.Welcome);

            WelcomeWindowStartup.MarkSeen();
        }

        [MenuItem("Tools/Aspid 🐍/FastTools/Asset References", priority = 20)]
        [MenuItem("Assets/Aspid 🐍/FastTools/Asset References")]
        public static void OpenAssetReferences() =>
            OpenAssetReferences(Selection.activeObject);

        [MenuItem("Assets/Aspid 🐍/FastTools/Asset References", validate = true)]
        private static bool ValidateOpenAssetReferences() =>
            CanInspect(Selection.activeObject);

        public static void OpenAssetReferences(Object target)
        {
            var window = Open();

            window._pendingTarget = ResolvePendingTarget(window._pendingTarget, target);
            window.SwitchMode(TabType.AssetReference);
        }

        // An empty selection or a plain scene object keeps the asset the tab already shows.
        internal static Object ResolvePendingTarget(Object current, Object requested) =>
            CanInspect(requested) ? requested : current;

        // Asset References maps a prefab, ScriptableObject or scene, also one in an embedded package, or the source
        // prefab of a prefab instance. A folder, a texture or a script has no managed references to show.
        internal static bool CanInspect(Object target)
        {
            if (!target) return false;

            return HasScanExtension(AssetDatabase.GetAssetPath(target))
                   || SerializeReferenceHelpers.TryGetSourcePrefabPath(target, out _);
        }

        private static bool HasScanExtension(string path)
        {
            if (string.IsNullOrEmpty(path)) return false;

            foreach (var extension in SerializeReferenceYaml.ScanExtensions)
            {
                if (path.EndsWith(extension, StringComparison.OrdinalIgnoreCase)) return true;
            }

            return false;
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

            _views.Clear();
            _buttons.Clear();

            // The window class goes first: it makes the theme sheets pick the light palette on the light skin.
            var root = rootVisualElement;
            root.AddClass(AspidStyles.WindowClass)
                .AddAspidThemeStyleSheets()
                .AddStyleSheetFromResources(WindowStyleSheetPath)
                .AddClass(RootClass);

            _background = new AspidAnimatedDotsBackground()
                .AddClass(BackgroundClass)
                .SetPickingMode(PickingMode.Ignore);

            var toolbar = new VisualElement().AddClass(ToolbarClass);
            foreach (var tab in Tabs)
            {
                var button = tab.Title is null ? SquareTabButton(tab) : TextTabButton(tab);
                _buttons[tab.Type] = button;
                toolbar.AddChild(button);
            }

            toolbar.RegisterCallback<GeometryChangedEvent>(_ => UpdateCompactTabs());

            _container = new VisualElement().AddClass(ContainerClass);

            _footer = new AspidWindowFooter();

            root.AddChild(_background)
                .AddChild(toolbar)
                .AddChild(_container)
                .AddChild(_footer);

            SwitchMode(CurrentTabType);
        }

        private Button TextTabButton(TabDefinition tab)
        {
            var hint = TabWindowShortcuts.HintFor(tab.Type);

            var button = new Button(() => SwitchMode(tab.Type)) { tooltip = hint };
            button.AddClass(ToolbarButtonClass);

            // The title is a child, not the button text, so the title and the hint center together as one group.
            button.AddChild(new Label(tab.Title)
                    .AddClass(TabTitleClass)
                    .SetPickingMode(PickingMode.Ignore))
                .AddChild(new Label(hint)
                    .AddClass(TabHintClass)
                    .SetPickingMode(PickingMode.Ignore))
                .AddChild(BuildUnderline());

            return button;
        }

        private Button SquareTabButton(TabDefinition tab)
        {
            var button = new Button(() => SwitchMode(tab.Type)) { tooltip = TabWindowShortcuts.HintFor(tab.Type) };
            button.AddClass(ToolbarButtonClass).AddClass(ToolbarButtonSquareClass);

            button.AddChild(new VisualElement()
                    .AddClass(TabIconClass)
                    .AddClass(tab.IconClass)
                    .SetPickingMode(PickingMode.Ignore))
                .AddChild(BuildUnderline());

            return button;
        }

        // A child background-color repaints reliably; changing the tab border color can wait until resize.
        private static VisualElement BuildUnderline() =>
            new VisualElement()
                .AddClass(TabUnderlineClass)
                .SetPickingMode(PickingMode.Ignore);

        // A text tab drops its shortcut hint once the title and the hint no longer fit side by side. The text tabs share
        // the toolbar width equally, so hiding the hint does not change the width this check reads.
        private void UpdateCompactTabs()
        {
            foreach (var button in _buttons.Values)
            {
                var title = button.Q<Label>(className: TabTitleClass);
                var hint = button.Q<Label>(className: TabHintClass);
                if (title is null || hint is null) continue;

                var needed = MeasureText(title) + TabHintGap + MeasureText(hint);
                button.EnableInClassList(ToolbarButtonCompactClass, button.contentRect.width < needed);
            }
        }

        private static float MeasureText(TextElement element) =>
            element.MeasureTextSize(element.text, 0f, VisualElement.MeasureMode.Undefined, 0f, VisualElement.MeasureMode.Undefined).x;

        internal void SwitchMode(TabType tabType)
        {
            _currentTabType = tabType;
            if (_container is null) return; // CreateGUI applies the selected tab once the container exists.

            foreach (var pair in _views)
                pair.Value.style.display = pair.Key == tabType ? DisplayStyle.Flex : DisplayStyle.None;

            if (_views.TryGetValue(tabType, out var view))
            {
                RefreshView(view);

                // The view takes the keyboard focus when it attaches, which happens once; a later visit gives it back.
                // A view hidden until this frame cannot take the focus before the style pass.
                view.schedule.Execute(view.Focus);
            }
            else
            {
                view = CreateView(tabType);
                _views[tabType] = view;
                _container.AddChild(view);

                if (view is SerializeReferenceProjectView project) project.Initialize();
            }

            _background?.SetStatus(GetStatus(tabType));
            _footer?.SetKeysHint(KeysHintFor(tabType));
            UpdateToolbar();
        }

        private VisualElement CreateView(TabType tabType) => tabType switch
        {
            TabType.Welcome => new WelcomeView(),
            TabType.AssetReference => new SerializeReferenceGraphView(
                _pendingTarget,
                status => ReportStatus(TabType.AssetReference, status),
                target => _pendingTarget = target),
            TabType.Settings => new SettingsView(),
            _ => new SerializeReferenceProjectView
            {
                OnInspectAsset = InspectAsset,
                OnCanvasStatus = status => ReportStatus(TabType.ProjectReferences, status),
            },
        };

        // A kept view still shows what it showed when the user left it, but the asset or the project may have changed.
        private void RefreshView(VisualElement view)
        {
            switch (view)
            {
                case SerializeReferenceGraphView graph: graph.Refresh(_pendingTarget); break;
                case SerializeReferenceProjectView project: project.Refresh(); break;
            }
        }

        internal static TabDefinition DefinitionOf(TabType tabType)
        {
            foreach (var tab in Tabs)
            {
                if (tab.Type == tabType) return tab;
            }

            return Tabs[0];
        }

        // Only a tab whose rows the left and right arrows step names those keys.
        internal static string KeysHintFor(TabType tabType) =>
            DefinitionOf(tabType).HasAdjustableRows ? AspidWindowFooter.AdjustableKeysHint : AspidWindowFooter.KeysHint;

        private StatusStyle.Type GetStatus(TabType tabType) =>
            _statuses.TryGetValue(tabType, out var status) ? status : DefinitionOf(tabType).Status;

        private void ReportStatus(TabType tabType, StatusStyle.Type status)
        {
            _statuses[tabType] = status;
            if (tabType == _currentTabType) _background?.SetStatus(status);
        }

        private void InspectAsset(Object target)
        {
            _pendingTarget = target;
            SwitchMode(TabType.AssetReference);
        }

        private void UpdateToolbar()
        {
            foreach (var pair in _buttons)
                pair.Value.EnableInClassList(ToolbarButtonActiveClass, pair.Key == _currentTabType);
        }

        internal readonly struct TabDefinition
        {
            internal readonly TabType Type;

            // Null for a square tab, which shows IconClass instead.
            internal readonly string Title;
            internal readonly string IconClass;

            internal readonly string ShortcutId;
            internal readonly KeyCode Key;

            // The canvas status of the tab until its view reports one.
            internal readonly StatusStyle.Type Status;

            // The left and right arrows step the rows of the tab, so the footer names them.
            internal readonly bool HasAdjustableRows;

            internal TabDefinition(
                TabType type,
                string title,
                string iconClass,
                string shortcutId,
                KeyCode key,
                StatusStyle.Type status = StatusStyle.Type.None,
                bool hasAdjustableRows = false)
            {
                Type = type;
                Title = title;
                IconClass = iconClass;
                ShortcutId = shortcutId;
                Key = key;
                Status = status;
                HasAdjustableRows = hasAdjustableRows;
            }
        }
    }
}
