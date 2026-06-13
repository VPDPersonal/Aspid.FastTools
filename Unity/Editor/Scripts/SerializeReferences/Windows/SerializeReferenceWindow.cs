using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
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
            Inspect,
            Project,
        }

        private const string StyleSheetPath = "UI/SerializeReferences/Aspid-FastTools-SerializeReference-Window";

        private const string RootClass = "aspid-fasttools-serialize-reference-window";
        private const string ToolbarClass = RootClass + "__toolbar";
        private const string TabClass = RootClass + "__tab";
        private const string TabActiveClass = TabClass + "--active";
        private const string TabLabelClass = RootClass + "__tab-label";
        private const string ContainerClass = RootClass + "__container";

        private VisualElement _container;
        private VisualElement _inspectTab;
        private VisualElement _projectTab;
        private Mode _mode = Mode.Inspect;
        private Object _pendingTarget;

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
            window.SwitchMode(Mode.Project);
        }

        private static SerializeReferenceWindow Reveal()
        {
            var window = GetWindow<SerializeReferenceWindow>();
            window.titleContent = new GUIContent("Managed References");
            window.minSize = new Vector2(480f, 360f);
            window.Show();
            return window;
        }

        private void CreateGUI()
        {
            var root = rootVisualElement;
            root.AddStyleSheetsFromResource(AspidStyles.DefaultStyleSheet)
                .AddStyleSheetsFromResource(StyleSheetPath)
                .AddClass(RootClass);

            _inspectTab = BuildTab("Inspect Asset", Mode.Inspect);
            _projectTab = BuildTab("Project Audit", Mode.Project);

            var toolbar = new VisualElement()
                .AddClass(ToolbarClass)
                .AddChild(_inspectTab)
                .AddChild(_projectTab);

            _container = new VisualElement().AddClass(ContainerClass);
            _container.style.flexGrow = 1;

            root.AddChild(toolbar).AddChild(_container);

            SwitchMode(_mode);
        }

        private VisualElement BuildTab(string label, Mode mode)
        {
            var tab = new VisualElement().AddClass(TabClass);
            tab.AddChild(new Label(label).AddClass(TabLabelClass).SetPickingMode(PickingMode.Ignore));
            tab.AddManipulator(new Clickable(() => SwitchMode(mode)));
            return tab;
        }

        private void SwitchMode(Mode mode)
        {
            _mode = mode;
            if (_container is null) return; // Open() ran before CreateGUI; CreateGUI re-invokes SwitchMode(_mode).

            _container.Clear();

            if (mode == Mode.Inspect)
            {
                _container.AddChild(new SerializeReferenceGraphView(_pendingTarget));
            }
            else
            {
                var project = new SerializeReferenceProjectView { OnInspectAsset = InspectAsset };
                _container.AddChild(project);
                project.ScanProject();
            }

            UpdateToolbar();
        }

        // Cross-link: jumping from a project-audit result to that asset's full graph.
        private void InspectAsset(Object target)
        {
            _pendingTarget = target;
            SwitchMode(Mode.Inspect);
        }

        private void UpdateToolbar()
        {
            _inspectTab?.EnableInClassList(TabActiveClass, _mode == Mode.Inspect);
            _projectTab?.EnableInClassList(TabActiveClass, _mode == Mode.Project);
        }
    }
}
