using System;
using System.IO;
using UnityEditor;
using UnityEngine;
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

        private const string UxmlResourcePath = "UI/Windows/Welcome/Aspid-FastTools-Welcome";

        private const string LogoName = "welcome-logo";
        private const string ToastName = "welcome-toast";
        private const string SamplesListName = "welcome-samples-list";
        private const string ToastVisibleClass = "aspid-fasttools-welcome__toast--visible";

        private const string SampleCardClass = "aspid-fasttools-welcome__sample";
        private const string SampleBodyClass = "aspid-fasttools-welcome__sample-body";
        private const string SampleHeaderClass = "aspid-fasttools-welcome__sample-header";
        private const string SampleTitleClass = "aspid-fasttools-welcome__sample-title";
        private const string SampleActionClass = "aspid-fasttools-welcome__sample-action";
        private const string SampleDividerClass = "aspid-fasttools-welcome__sample-divider";
        private const string SampleDescriptionClass = "aspid-fasttools-welcome__sample-description";

        private Label _toast;
        private VisualElement _samplesList;
        private IVisualElementScheduledItem _toastShow;
        private IVisualElementScheduledItem _toastHide;

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

            // The host window already paints one shared dotted canvas behind every tab; drop the Welcome UXML's
            // own background so the tabs read over a single continuous canvas instead of a doubled-up layer.
            this.Q<AspidAnimatedDotsBackground>()?.RemoveFromHierarchy();

            _toast = this.Q<Label>(ToastName);
            if (_toast != null)
            {
                // Reparent to the view root so the absolute coordinates we set in ShowToast are view-relative
                // (welcome-content has padding that would offset positioning).
                _toast.RemoveFromHierarchy();
                Add(_toast);
                _toast.SetPickingMode(PickingMode.Ignore);
            }

            PopulateSamples(this);
            SetUpLogoLink(this);
        }

        private static void SetUpLogoLink(VisualElement root)
        {
            var logo = root.Q<AspidAnimatedLogo>(LogoName);
            logo?.AddManipulator(new Clickable(() => Application.OpenURL(AssetStoreUrl)));
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
            _samplesList.Clear();

            var package = PackageInfo.FindForPackageName(PackageName);
            if (package is not null)
            {
                AddUpmSamples(package);
                return;
            }

            if (AssetDatabase.IsValidFolder(SamplesPath))
                AddLocalSamples();
        }

        private void AddUpmSamples(PackageInfo package)
        {
            foreach (var sample in Sample.FindByPackage(package.name, package.version))
                _samplesList.Add(CreateUpmSampleButton(sample));
        }

        private AspidGradientButton CreateUpmSampleButton(Sample sample)
        {
            var displayName = sample.displayName;
            var description = sample.description;
            var captured = sample;

            if (sample.isImported)
            {
                // The sample already lives under Assets — re-importing overwrites it, so mirror the Package
                // Manager and confirm before clobbering any local edits the user may have made to the copy.
                return CreateSampleCard(displayName, description, "Reimport  ▼", evt =>
                {
                    var pointer = GetMousePosition(evt);
                    var target = ToProjectRelativePath(captured.importPath);

                    var confirmed = EditorUtility.DisplayDialog(
                        $"Reimport “{displayName}”",
                        $"This overwrites “{target}”, discarding any local changes to the sample. Continue?",
                        "Reimport",
                        "Cancel");

                    if (!confirmed) return;

                    if (!captured.Import(Sample.ImportOptions.OverridePreviousImports | Sample.ImportOptions.HideImportWindow))
                    {
                        ShowToast($"Failed to reimport “{displayName}”", pointer);
                        return;
                    }

                    AssetDatabase.Refresh();
                    ShowToast($"“{displayName}” reimported into Assets/Samples", pointer);
                    RebuildSamplesList();
                });
            }

            return CreateSampleCard(displayName, description, "Import  ▼", evt =>
            {
                var pointer = GetMousePosition(evt);

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
        /// Builds a two-line sample row: the display name on the first line, the package.json description (when
        /// present) wrapping below it, and the <paramref name="trailingText"/> action pinned to the right edge.
        /// The name + description live in a flex-grow leading column so the trailing action stays right-aligned;
        /// the <see cref="SampleCardClass"/> USS rule relaxes the gradient button's fixed single-line height.
        /// </summary>
        private AspidGradientButton CreateSampleCard(string displayName, string description, string actionText, Action<EventBase> onClick)
        {
            var card = new AspidGradientButton(string.Empty, onClick)
                .AddClass(SampleCardClass);

            var body = new VisualElement()
                .AddClass(SampleBodyClass)
                .SetPickingMode(PickingMode.Ignore);

            var header = new VisualElement()
                .AddClass(SampleHeaderClass)
                .SetPickingMode(PickingMode.Ignore);

            var title = new Label(displayName)
                .AddClass(SampleTitleClass)
                .SetPickingMode(PickingMode.Ignore);
            header.Add(title);

            Label action = null;
            if (!string.IsNullOrEmpty(actionText))
            {
                action = new Label(actionText)
                    .AddClass(SampleActionClass)
                    .SetPickingMode(PickingMode.Ignore);
                header.Add(action);
            }

            body.Add(header);

            if (!string.IsNullOrEmpty(description))
            {
                body.Add(new AspidDividingLine(AspidDividingLinePreset.Default
                        .SetTheme(ThemeStyle.Type.Light)
                        .SetSize(AspidDividingLineSizeStyle.Type.Thin))
                    .AddClass(SampleDividerClass)
                    .SetPickingMode(PickingMode.Ignore));

                body.Add(new Label(description)
                    .AddClass(SampleDescriptionClass)
                    .SetPickingMode(PickingMode.Ignore));
            }

            card.AddLeadingContent(body);
            card.FillWithLeadingContent();

            // The action/title now live in the leading content instead of the button's own labels, so the
            // button's built-in hover recolor no longer reaches them — mirror it here against the accent color.
            card.RegisterCallback<MouseEnterEvent>(_ =>
            {
                title.style.color = card.Accent;
                if (action is not null) action.style.color = card.Accent;
            });
            card.RegisterCallback<MouseLeaveEvent>(_ =>
            {
                title.style.color = StyleKeyword.Null;
                if (action is not null) action.style.color = StyleKeyword.Null;
            });

            return card;
        }

        private void AddLocalSamples()
        {
            foreach (var subfolder in AssetDatabase.GetSubFolders(SamplesPath))
            {
                var fileName = Path.GetFileName(subfolder);
                if (string.IsNullOrEmpty(fileName)) continue;

                _samplesList.Add(CreateSampleCard(fileName, null, string.Empty, evt =>
                {
                    PingAsset(subfolder);
                    ShowToast($"“{fileName}” selected in the Project window", GetMousePosition(evt));
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
