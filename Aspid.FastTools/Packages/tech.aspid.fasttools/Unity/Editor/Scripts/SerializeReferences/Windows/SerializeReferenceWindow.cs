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

        private const string RootClass = "aspid-fasttools-serialize-reference-window";
        private const string ToolbarClass = RootClass + "__toolbar";
        private const string ToolbarButtonClass = RootClass + "__toolbar-button";
        private const string ContainerClass = RootClass + "__container";

        private const float InactiveOpacity = 0.45f;

        private VisualElement _container;
        private AspidGradientButton _inspectButton;
        private AspidGradientButton _projectButton;
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
            root.AddStyleSheetsFromResource(AspidStyles.DefaultStyleSheet).AddClass(RootClass);

            _inspectButton = ModeButton("Inspect Asset", Mode.Inspect);
            _projectButton = ModeButton("Project Audit", Mode.Project);

            var toolbar = new VisualElement().AddClass(ToolbarClass);
            toolbar.style.flexDirection = FlexDirection.Row;
            toolbar.style.flexShrink = 0;
            toolbar.AddChild(_inspectButton).AddChild(_projectButton);

            _container = new VisualElement().AddClass(ContainerClass);
            _container.style.flexGrow = 1;

            root.AddChild(toolbar).AddChild(_container);

            SwitchMode(_mode);
        }

        private AspidGradientButton ModeButton(string label, Mode mode)
        {
            var button = new AspidGradientButton(label, _ => SwitchMode(mode)).AddClass(ToolbarButtonClass);
            button.style.flexGrow = 1;
            return button;
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
            if (_inspectButton != null) _inspectButton.style.opacity = _mode == Mode.Inspect ? 1f : InactiveOpacity;
            if (_projectButton != null) _projectButton.style.opacity = _mode == Mode.Project ? 1f : InactiveOpacity;
        }
    }
}
