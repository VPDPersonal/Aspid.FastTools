using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UIElements;
using Aspid.FastTools.UIElements;
using UnityEditor.PackageManager.UI;
using Aspid.FastTools.UIElements.Editors.Internal;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Editors
{
    /// <summary>
    /// The Welcome panel as a reusable element: hero, samples list, the logo asset-store link and the cursor toast.
    /// Hosted as the leftmost "home" tab of the Managed References window (see SerializeReferenceWindow).
    /// The UI is cloned from the same UXML the standalone window used; only the toast positioning is
    /// retargeted from the window root to this view (the view sits below the tab strip).
    /// </summary>
    internal sealed class WelcomeView : VisualElement
    {
        private const long ToastVisibleDurationMs = 2500;
        private const float ToastEdgeMargin = 8f;
        private const float ToastCursorOffset = 16f;

        private const string PackageName = "tech.aspid.fasttools";
        private const string PackageRootPath = "Assets/Aspid/FastTools";

        private const string SamplesPath = PackageRootPath + "/Samples";
        private const string AssetStoreUrl = "https://assetstore.unity.com/packages/slug/365584";
        private const string GitHubUrl = "https://github.com/VPDPersonal/Aspid.FastTools";
        private const string DocumentationUrl = GitHubUrl + "/blob/main/Aspid.FastTools/Packages/tech.aspid.fasttools/Documentation/EN/README.md";

        private const string DocsLinkName = "welcome-link-docs";
        private const string GitHubLinkName = "welcome-link-github";
        private const string StoreLinkName = "welcome-link-store";

        private const string UxmlResourcePath = "UI/Windows/Welcome/Aspid-FastTools-Welcome";

        private const string LogoName = "welcome-logo";
        private const string ToastName = "welcome-toast";
        private const string ScrollName = "welcome-scroll";
        private const string SamplesListName = "welcome-samples-list";
        private const string ToastVisibleClass = "aspid-fasttools-welcome__toast--visible";

        private const string NavTargetClass = "aspid-fasttools-welcome__nav-target";

        private const string SampleCardClass = "aspid-fasttools-welcome__sample";
        private const string SampleHeaderHoverClass = "aspid-fasttools-welcome__sample--header-hover";
        private const string SampleHeaderRowClass = "aspid-fasttools-welcome__sample-header-row";
        private const string SampleHeaderClass = "aspid-fasttools-welcome__sample-header";
        private const string SampleHeaderRemoveClass = "aspid-fasttools-welcome__sample-header--remove";
        private const string SampleInfoClass = "aspid-fasttools-welcome__sample-info";
        private const string SampleTitleClass = "aspid-fasttools-welcome__sample-title";
        private const string SampleStateDotClass = "aspid-fasttools-welcome__sample-state-dot";
        private const string SampleStateDotImportedClass = "aspid-fasttools-welcome__sample-state-dot--imported";
        private const string SampleDividerClass = "aspid-fasttools-welcome__sample-divider";
        private const string SampleSweepClass = "aspid-fasttools-welcome__sample-sweep";
        private const string SampleSweepRemoveClass = "aspid-fasttools-welcome__sample-sweep--remove";
        private const string SampleDescriptionClass = "aspid-fasttools-welcome__sample-description";

        private Label _toast;
        private ScrollView _scroll;
        private VisualElement _samplesList;
        private IVisualElementScheduledItem _toastShow;
        private IVisualElementScheduledItem _toastHide;

        // Keyboard navigation: one flat focus ring over the sample cards' header buttons in list order (hero
        // links stay mouse-only). -1 means nothing is highlighted.
        private readonly List<(VisualElement Element, Action Activate)> _navTargets = new();
        private int _navIndex = -1;

        public WelcomeView()
        {
            style.flexGrow = 1;

            var tree = Resources.Load<VisualTreeAsset>(UxmlResourcePath);
            if (tree == null)
            {
                Debug.LogError($"WelcomeView: failed to load UXML at Resources/{UxmlResourcePath}.uxml");
                return;
            }

            tree.CloneTree(this);

            _toast = this.Q<Label>(ToastName);
            if (_toast != null)
            {
                // Reparent to the view root so the absolute coordinates we set in ShowToast are view-relative
                // (welcome-content has padding that would offset positioning).
                _toast.RemoveFromHierarchy();
                Add(_toast);
                _toast.SetPickingMode(PickingMode.Ignore);
            }

            _scroll = this.Q<ScrollView>(ScrollName);

            PopulateSamples(this);
            SetUpLogoLink(this);
            SetUpHeroLinks(this);

            // Arrow-key navigation: the view holds keyboard focus (grabbed on attach) so key events reach
            // OnNavKeyDown even before anything is highlighted; events from focused descendants bubble here too.
            focusable = true;
            RegisterCallback<KeyDownEvent>(OnNavKeyDown);
            RegisterCallback<AttachToPanelEvent>(_ => schedule.Execute(() => Focus()));
        }

        // ---------------------------------------------------------------------------------------------------------
        // Keyboard navigation
        // ---------------------------------------------------------------------------------------------------------

        private void OnNavKeyDown(KeyDownEvent evt)
        {
            // FunctionKey rides along with arrows on some platforms; any real modifier means the key is not ours.
            if ((evt.modifiers & ~EventModifiers.FunctionKey) != 0) return;

            switch (evt.keyCode)
            {
                case KeyCode.DownArrow:
                    MoveNavFocus(+1);
                    evt.StopPropagation();
                    break;

                case KeyCode.UpArrow:
                    MoveNavFocus(-1);
                    evt.StopPropagation();
                    break;

                case KeyCode.Return or KeyCode.KeypadEnter when _navIndex >= 0:
                    _navTargets[_navIndex].Activate();
                    evt.StopPropagation();
                    break;

                case KeyCode.Escape when _navIndex >= 0:
                    ClearNavFocus();
                    evt.StopPropagation();
                    break;
            }
        }

        // First press (nothing highlighted) lands on the first sample whichever arrow is hit; after that the
        // ring clamps at both ends instead of wrapping.
        private void MoveNavFocus(int delta)
        {
            if (_navTargets.Count == 0) return;
            SetNavFocus(_navIndex < 0 ? 0 : Mathf.Clamp(_navIndex + delta, 0, _navTargets.Count - 1));
        }

        private void SetNavFocus(int index, bool scrollTo = true)
        {
            if (_navIndex == index) return;

            ClearNavFocus();
            _navIndex = index;

            var element = _navTargets[index].Element;
            // Sample headers paint their hover in code (accent overlay + tinted labels), so keyboard focus
            // drives that same programmatic hover instead of a USS focused class.
            if (element is AspidGradientButton button) button.Highlighted = true;
            SetHeaderSweep(element, on: true);
            if (scrollTo) _scroll?.ScrollTo(element);
        }

        private void ClearNavFocus()
        {
            if (_navIndex >= 0 && _navIndex < _navTargets.Count)
            {
                var element = _navTargets[_navIndex].Element;
                if (element is AspidGradientButton button) button.Highlighted = false;
                SetHeaderSweep(element, on: false);
            }

            _navIndex = -1;
        }

        // A focused sample header also lights its card's divider sweep — the same card-level hover mirror the
        // mouse path drives in CreateSampleCard — so keyboard focus and mouse hover render identically.
        private static void SetHeaderSweep(VisualElement element, bool on)
        {
            if (!element.ClassListContains(SampleHeaderClass)) return;

            for (var ancestor = element.parent; ancestor is not null; ancestor = ancestor.parent)
            {
                if (!ancestor.ClassListContains(SampleCardClass)) continue;
                ancestor.EnableInClassList(SampleHeaderHoverClass, on);
                return;
            }
        }

        // The samples list is rebuilt after every Import/Remove, so the ring is rebuilt with it: sample headers
        // in list order. Hero links are deliberately not in the ring — they are mouse-only decoration.
        private void ResetNavTargets()
        {
            ClearNavFocus();
            _navTargets.Clear();
        }

        private void RegisterNavTarget(VisualElement element, Action activate)
        {
            element.AddToClassList(NavTargetClass);
            _navTargets.Add((element, activate));
        }

        private static void SetUpLogoLink(VisualElement root)
        {
            var logo = root.Q<AspidAnimatedLogo>(LogoName);
            logo?.AddManipulator(new Clickable(() => Application.OpenURL(AssetStoreUrl)));
        }

        private static void SetUpHeroLinks(VisualElement root)
        {
            SetUpLink(root, DocsLinkName, DocumentationUrl);
            SetUpLink(root, GitHubLinkName, GitHubUrl);
            SetUpLink(root, StoreLinkName, AssetStoreUrl);
        }

        private static void SetUpLink(VisualElement root, string name, string url)
        {
            var link = root.Q<Label>(name);
            link?.AddManipulator(new Clickable(() => Application.OpenURL(url)));
        }

        private void ShowToast(string message, Vector2 mousePosition)
        {
            if (_toast == null) return;

            _toast.text = message;

            // The view sits below the tab strip, so the event's panel-space cursor position must be converted to
            // view-local coordinates before it drives the toast's absolute top/left.
            var local = this.WorldToLocal(mousePosition);

            // Tentative position; clamping happens after the toast resolves its size on the
            // next layout pass (see _toastShow callback below).
            _toast.style.top = local.y + ToastCursorOffset;
            _toast.style.left = local.x;

            // Defer the visible class so the opacity:0 baseline is committed first; otherwise
            // Unity batches the position update with the class change and the fade-in snaps.
            _toastShow?.Pause();
            _toastShow = _toast.schedule.Execute(() =>
            {
                ClampToastWithinPanel(local);
                _toast.AddClass(ToastVisibleClass);
            }).StartingIn(16);

            _toastHide?.Pause();
            _toastHide = _toast.schedule.Execute(HideToast).StartingIn(ToastVisibleDurationMs);
        }

        private void ClampToastWithinPanel(Vector2 local)
        {
            if (_toast == null) return;

            var panelWidth = layout.width;
            var panelHeight = layout.height;
            var toastWidth = _toast.layout.width;
            var toastHeight = _toast.layout.height;

            if (float.IsNaN(toastWidth) || float.IsNaN(toastHeight)) return;
            if (toastWidth <= 0f || toastHeight <= 0f) return;

            var left = local.x;
            if (left + toastWidth + ToastEdgeMargin > panelWidth)
                left = panelWidth - toastWidth - ToastEdgeMargin;
            if (left < ToastEdgeMargin)
                left = ToastEdgeMargin;

            var top = local.y + ToastCursorOffset;
            if (top + toastHeight + ToastEdgeMargin > panelHeight)
                top = local.y - toastHeight - ToastEdgeMargin;
            if (top < ToastEdgeMargin)
                top = ToastEdgeMargin;

            _toast.style.left = left;
            _toast.style.top = top;
        }

        private void HideToast() =>
            _toast?.RemoveClass(ToastVisibleClass);

        private void PopulateSamples(VisualElement root)
        {
            _samplesList = root.Q<VisualElement>(SamplesListName);
            RebuildSamplesList();
        }

        private void RebuildSamplesList()
        {
            if (_samplesList is null) return;

            // An Import/Remove triggered from the keyboard rebuilds the very ring it came from — put the
            // highlight back on the same slot (clamped: Remove may shrink nothing, but stay safe) so the
            // keyboard flow continues from where it acted.
            var keepIndex = _navIndex;

            _samplesList.Clear();
            ResetNavTargets();

            var package = PackageInfo.FindForPackageName(PackageName);
            if (package is not null) AddUpmSamples(package);
            else if (AssetDatabase.IsValidFolder(SamplesPath)) AddLocalSamples();

            if (keepIndex >= 0 && _navTargets.Count > 0)
                SetNavFocus(Mathf.Min(keepIndex, _navTargets.Count - 1), scrollTo: false);
        }

        private void AddUpmSamples(PackageInfo package)
        {
            foreach (var sample in Sample.FindByPackage(package.name, package.version))
                _samplesList.Add(CreateUpmSampleCard(sample));
        }

        private VisualElement CreateUpmSampleCard(Sample sample)
        {
            var displayName = sample.displayName;
            var description = sample.description;
            var captured = sample;

            if (sample.isImported)
            {
                // An imported sample's one action is taking the copy back out — deletion is confirmed and the
                // sample stays reimportable right after, so the direct verb replaces a Reimport/Remove menu.
                return CreateSampleCard(displayName, description, "Remove",
                    pointer => RemoveSample(captured, displayName, pointer),
                    imported: true);
            }

            return CreateSampleCard(displayName, description, "Import", imported: false, onClick: pointer =>
            {
                if (!captured.Import(Sample.ImportOptions.HideImportWindow))
                {
                    ShowToast($"Failed to import “{displayName}”", pointer);
                    return;
                }

                AssetDatabase.Refresh();
                ShowToast($"“{displayName}” imported into Assets/Samples", pointer);
                RebuildSamplesList();
            });
        }

        /// <summary>
        /// Builds a sample card in the References group-card idiom: a glass box whose whole header row is one flat
        /// clickable button (the display name on the left, the <paramref name="actionText"/> verb pinned to the
        /// right, an accent glow on hover), with the package.json description (when present) wrapping below.
        /// A state dot ahead of the title carries <paramref name="imported"/>: brand-blue while the sample is
        /// not imported yet (the unread-marker idiom), green once it is; an imported card's destructive verb
        /// also hovers in the error tone. <see langword="null"/> drops the dot — the state doesn't apply
        /// (local, non-UPM samples). <paramref name="onClick"/> receives the panel-space anchor for the result
        /// toast: the cursor on a mouse click, the header's center on a keyboard Enter.
        /// </summary>
        private VisualElement CreateSampleCard(
            string displayName,
            string description,
            string actionText,
            Action<Vector2> onClick,
            bool? imported = null)
        {
            var card = new AspidBox(AspidBoxPreset.Default.SetTheme(ThemeStyle.Type.Darkness))
                .AddClass(SampleCardClass);

            var action = new AspidGradientButton(actionText, evt => onClick(GetMousePosition(evt)))
                .AddClass(SampleHeaderClass);
            RegisterNavTarget(action, () => onClick(action.worldBound.center));
            if (imported == true)
                action.AddClass(SampleHeaderRemoveClass);

            // The sweep sits outside the button (riding the divider below), so USS :hover can't reach it —
            // the button mirrors its hover onto a card modifier the sweep rule listens to instead.
            action.RegisterCallback<MouseEnterEvent>(_ => card.AddToClassList(SampleHeaderHoverClass));
            action.RegisterCallback<MouseLeaveEvent>(_ => card.RemoveFromClassList(SampleHeaderHoverClass));

            var info = new VisualElement()
                .AddClass(SampleInfoClass)
                .SetPickingMode(PickingMode.Ignore);

            if (imported.HasValue)
            {
                // Kept pickable (no click handler — presses bubble through to the header button) so its
                // tooltip can explain the state.
                var dot = new VisualElement().AddClass(SampleStateDotClass);

                if (imported.Value)
                    dot.AddClass(SampleStateDotImportedClass);

                dot.tooltip = imported.Value ? "Imported" : "Not imported yet";
                info.AddChild(dot);
            }

            info.AddChild(new Label(displayName)
                .AddClass(SampleTitleClass)
                .SetPickingMode(PickingMode.Ignore));

            action.AddLeadingContent(info);

            var header = new VisualElement()
                .AddClass(SampleHeaderRowClass)
                .AddChild(action);

            card.AddChild(header);

            if (!string.IsNullOrEmpty(description))
            {
                card.AddChild(new AspidDividingLine(AspidDividingLinePreset.Default
                        .SetTheme(ThemeStyle.Type.Light)
                        .SetSize(AspidDividingLineSizeStyle.Type.Thin))
                    .AddClass(SampleDividerClass));

                // The accent sweep riding the divider: a hairline that scales in from the left while the header
                // button is hovered (via the --header-hover card modifier). Red on an imported (Remove) card,
                // brand green otherwise.
                var sweep = new VisualElement().AddClass(SampleSweepClass);
                if (imported == true)
                    sweep.AddClass(SampleSweepRemoveClass);
                card.AddChild(sweep);

                card.AddChild(new Label(description)
                    .AddClass(SampleDescriptionClass));
            }

            return card;
        }

        private void RemoveSample(Sample sample, string displayName, Vector2 pointer)
        {
            var target = ToProjectRelativePath(sample.importPath);

            var confirmed = EditorUtility.DisplayDialog(
                $"Remove “{displayName}”",
                $"This deletes “{target}” from the project, discarding any local changes to the copy. Continue?",
                "Remove",
                "Cancel");

            if (!confirmed) return;

            if (!AssetDatabase.DeleteAsset(target))
            {
                ShowToast($"Failed to remove “{displayName}”", pointer);
                return;
            }

            AssetDatabase.Refresh();
            ShowToast($"“{displayName}” removed from Assets/Samples", pointer);
            RebuildSamplesList();
        }

        private void AddLocalSamples()
        {
            foreach (var subfolder in AssetDatabase.GetSubFolders(SamplesPath))
            {
                var fileName = Path.GetFileName(subfolder);
                if (string.IsNullOrEmpty(fileName)) continue;

                _samplesList.Add(CreateSampleCard(fileName, null, "Show", pointer =>
                {
                    PingAsset(subfolder);
                    ShowToast($"“{fileName}” selected in the Project window", pointer);
                }));
            }
        }

        private static Vector2 GetMousePosition(EventBase evt) => evt switch
        {
            IPointerEvent pointer => new Vector2(pointer.position.x, pointer.position.y),
            IMouseEvent mouse => mouse.mousePosition,
            _ => Vector2.zero,
        };

        private static void PingAsset(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath)) return;

            var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
            if (asset is null) return;

            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
        }

        private static string ToProjectRelativePath(string path)
        {
            if (string.IsNullOrEmpty(path)) return path;

            var normalized = path.Replace('\\', '/');
            if (normalized.StartsWith("Assets/", StringComparison.Ordinal) || normalized == "Assets")
                return normalized;

            var dataPath = Application.dataPath.Replace('\\', '/');
            if (!dataPath.EndsWith("/Assets", StringComparison.Ordinal)) return normalized;

            var projectRoot = dataPath.Substring(0, dataPath.Length - "Assets".Length);
            return normalized.StartsWith(projectRoot, StringComparison.OrdinalIgnoreCase)
                ? normalized.Substring(projectRoot.Length)
                : normalized;
        }
    }
}
