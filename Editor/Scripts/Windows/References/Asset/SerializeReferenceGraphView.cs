using System;
using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Aspid.FastTools.UIElements;
using System.Collections.Generic;
using Aspid.FastTools.UIElements.Editors.Internal;
using Object = UnityEngine.Object;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    internal sealed partial class SerializeReferenceGraphView : VisualElement
    {
        private const string StyleSheetPath = "UI/SerializeReferences/Aspid-FastTools-ReferenceGraph";

        private const string RootClass = "aspid-fasttools-reference-graph";
        private const string ContentClass = RootClass + "__content";
        private const string CardClass = RootClass + "__card";
        private const string CardTitleClass = RootClass + "__card-title";
        private const string CardDescriptionClass = RootClass + "__card-description";
        private const string AssetClass = RootClass + "__asset";
        private const string RescanClass = RootClass + "__rescan";
        private const string EmptyClass = RootClass + "__empty";
        private const string EmptyHiddenClass = EmptyClass + "--hidden";
        private const string EmptyIconClass = RootClass + "__empty-icon";
        private const string EmptyIconInfoClass = EmptyIconClass + "--info";
        private const string EmptyTitleClass = RootClass + "__empty-title";
        private const string EmptyMessageClass = RootClass + "__empty-message";
        private const string ScrollClass = RootClass + "__scroll";
        private const string ListClass = RootClass + "__list";
        private const string ListHiddenClass = ListClass + "--hidden";

        private const string OverviewClass = RootClass + "__overview";
        private const string OverviewHiddenClass = OverviewClass + "--hidden";
        private const string OverviewTitleClass = RootClass + "__overview-title";
        private const string OverviewHintClass = RootClass + "__overview-hint";

        private const string LegendClass = RootClass + "__legend";
        private const string LegendHiddenClass = LegendClass + "--hidden";
        private const string LegendItemClass = RootClass + "__legend-item";
        private const string LegendDotClass = RootClass + "__legend-dot";
        private const string LegendDotInfoClass = LegendDotClass + "--info";
        private const string LegendTextClass = RootClass + "__legend-text";

        private const string NavTargetClass = RootClass + "__nav-target";
        private const string NavTargetFocusedClass = NavTargetClass + "--focused";

        private readonly Action<StatusStyle.Type> _onCanvasStatus;

        private readonly Action<Object> _onTargetChanged;

        private Object _target;
        private readonly ObjectField _assetField;
        private readonly AspidGradientButton _rescanButton;
        private readonly VisualElement _empty;
        private readonly VisualElement _overview;
        private readonly AspidLabel _overviewTitle;
        private readonly Label _overviewHint;
        private readonly VisualElement _legend;
        private readonly VisualElement _list;
        private readonly ScrollView _scroll;

        private readonly NavRing _ring;

        private readonly AuditPickerHost _picker;

        private readonly SerializeReferenceConstraintCache _constraints = new();

        private static readonly SerializeReferenceAuditUI.LegendClasses _legendClassSet =
            new(LegendItemClass, LegendDotClass, LegendDotInfoClass, LegendTextClass);

        private IReadOnlyList<GateViolation> _requiredViolations = Array.Empty<GateViolation>();

        public SerializeReferenceGraphView(Object target, Action<StatusStyle.Type> onCanvasStatus, Action<Object> onTargetChanged = null)
        {
            _target = target;
            _onCanvasStatus = onCanvasStatus;
            _onTargetChanged = onTargetChanged;

            var root = this;
            style.flexGrow = 1;
            root.AddAspidThemeStyleSheets()
                .AddStyleSheetFromResources(StyleSheetPath)
                .AddClass(RootClass);

            var cardTitle = new AspidLabel("Inspect asset", AspidLabelPreset.Default
                    .SetLabelTheme(ThemeStyle.Type.Lightness)
                    .SetLabelSize(AspidLabelSizeStyle.Type.H5)
                    .SetLineSize(AspidDividingLineSizeStyle.Type.None))
                .AddClass(CardTitleClass);

            var cardDescription = new Label(
                    "Map a saved asset's [SerializeReference] graph and repair missing types inline.")
                .AddClass(CardDescriptionClass);

            _assetField = new ObjectField
            {
                objectType = typeof(Object),
                allowSceneObjects = false,
                value = _target,
            };
            _assetField.AddClass(AssetClass);
            _assetField.RegisterValueChangedCallback(evt => SetTarget(evt.newValue));

            // Stop the nested object field from also activating the surrounding Rescan button.
            _assetField.RegisterCallback<PointerDownEvent>(evt => evt.StopPropagation());

            _rescanButton = new AspidGradientButton("Rescan", _ => Rescan())
                .AddClass(RescanClass);
            _rescanButton.AddTrailingContent(_assetField);
            _rescanButton.FillWithTrailingContent();

            var card = new AspidBox(AspidBoxPreset.Default.SetTheme(ThemeStyle.Type.Darkness))
                .AddClass(CardClass)
                .AddChild(cardTitle)
                .AddChild(cardDescription)
                .AddChild(_rescanButton);

            _empty = new VisualElement().AddClass(EmptyClass);

            _overviewTitle = new AspidLabel(string.Empty, AspidLabelPreset.Default
                    .SetLabelStatus(StatusStyle.Type.Warning)
                    .SetLabelSize(AspidLabelSizeStyle.Type.H4)
                    .SetLineTheme(ThemeStyle.Type.Dark)
                    .SetLineStatus(StatusStyle.Type.Warning))
                .AddClass(OverviewTitleClass);

            _overviewHint = new Label(string.Empty).AddClass(OverviewHintClass);

            _legend = new VisualElement()
                .AddClass(LegendClass)
                .AddClass(LegendHiddenClass)
                .AddChild(SerializeReferenceAuditUI.BuildLegendItem("Broken — pick a replacement", info: false, _legendClassSet))
                .AddChild(SerializeReferenceAuditUI.BuildLegendItem("Renamed — one-click migrate", info: true, _legendClassSet));

            _overview = new VisualElement()
                .AddClass(OverviewClass)
                .AddClass(OverviewHiddenClass)
                .AddChild(_overviewTitle)
                .AddChild(_overviewHint)
                .AddChild(_legend);

            _list = new VisualElement().AddClass(ListClass);

            var content = new VisualElement()
                .AddClass(ContentClass)
                .AddChild(card)
                .AddChild(_empty)
                .AddChild(_overview)
                .AddChild(_list);

            _scroll = new ScrollView().AddClass(ScrollClass);
            _scroll.AddChild(content);

            root.AddChild(_scroll);

            _picker = new AuditPickerHost(this, _list, _pickerClassSet);

            _ring = new NavRing(
                host: this,
                navTargetClass: NavTargetClass,
                focusedClass: NavTargetFocusedClass,
                scrollTo: element => _scroll.ScrollTo(element),
                isSuspended: () => _picker.IsOpen);

            Rescan();
        }

        private void ResetNavTargets()
        {
            _ring.Clear(keepFocusedElement: true);
            RegisterNavTarget(_rescanButton, () => Rescan());
        }

        private void RegisterNavTarget(VisualElement element, Action activate) => _ring.Register(element, activate);

        private void RegisterNavBand(AspidGradientButton band, VisualElement card, Action activate) =>
            _ring.RegisterHeader(band, card, NodeHeaderHoverClass, activate);

        private void SetTarget(Object target)
        {
            _target = target;
            _onTargetChanged?.Invoke(target);
            _assetField?.SetValueWithoutNotify(target);
            if (_list is not null) Rescan();
        }

        private void Rescan(List<ReferenceGraphDocument> prebuilt = null)
        {
            if (_list is null) return;

            _picker.Close();
            _constraints.Clear();
            _list.Clear();
            ResetNavTargets();
            _requiredViolations = Array.Empty<GateViolation>();

            var assetPath = _target ? AssetDatabase.GetAssetPath(_target) : null;
            if (string.IsNullOrEmpty(assetPath))
            {
                if (!TryOfferSourcePrefab())
                {
                    ShowEmpty(
                        "No asset selected",
                        "Select a saved asset (a prefab or ScriptableObject) to map its managed-reference graph.");
                }

                return;
            }

            var documents = prebuilt ?? SerializeReferenceGraphScanner.Build(assetPath);

            // Required string fields may exist without a RefIds block, so scan them before the empty-graph return.
            _requiredViolations = SerializeReferenceGateScanner.ScanAssetRequiredFields(assetPath);

            if (documents.Count == 0 && _requiredViolations.Count == 0)
            {
                ShowEmpty(
                    "No managed references",
                    "This asset has no [SerializeReference] managed references to map.");
                return;
            }

            ShowResults();
            RenderDocuments(assetPath, documents);
        }

        private bool TryOfferSourcePrefab()
        {
            if (!SerializeReferenceHelpers.TryGetSourcePrefabPath(_target, out var sourcePath)) return false;

            ShowResults();
            _onCanvasStatus?.Invoke(StatusStyle.Type.Info);

            var info = new AspidHelpBox(AspidHelpBoxPreset.Default.SetMessageType(HelpBoxMessageType.Info))
                .SetMessage("This is a prefab instance — its managed references live in the source prefab.");
            _list.AddChild(info);

            void OpenSource() => SetTarget(AssetDatabase.LoadAssetAtPath<Object>(sourcePath));

            var openSource = new AspidGradientButton("Open Source Prefab", _ => OpenSource());
            RegisterNavTarget(openSource, OpenSource);
            _list.AddChild(openSource);
            return true;
        }

        private void RenderDocuments(string assetPath, List<ReferenceGraphDocument> documents)
        {
            var total = 0;
            var missing = 0;
            var orphans = 0;
            var empties = 0;
            var migrations = 0;

            var emptySlotPaths = SerializeReferenceGraphAnalysis.CollectEmptySlotPaths(documents);

            var showHeaders = documents.Count > 1;
            foreach (var document in documents)
            {
                _list.AddChild(BuildDocument(assetPath, document, showHeaders));

                total += document.Nodes.Count;
                var (broken, documentMigrations) = SerializeReferenceGraphAnalysis.CountUnresolved(assetPath, document, _constraints);
                missing += broken + documentMigrations;
                migrations += documentMigrations;
                orphans += document.Orphans.Count;
                empties += SerializeReferenceGraphAnalysis.CountEmptySlots(document);
            }

            var ungraphedRequired = _requiredViolations
                .Where(violation => !emptySlotPaths.Contains((violation.FileId, violation.FieldPath)))
                .ToList();

            // Count required empty fields only as violations, avoiding duplicate unassigned-field counts.
            var required = _requiredViolations.Count;
            var graphedRequired = required - ungraphedRequired.Count;

            var status = SerializeReferenceAuditUI.ResolveStatus(missing - migrations, orphans, required, migrations);
            ShowOverview(status, total, missing, orphans, Math.Max(0, empties - graphedRequired), migrations, required);

            if (ungraphedRequired.Count > 0)
            {
                var labels = new ViolationFieldLabels();
                foreach (var violation in ungraphedRequired)
                    _list.AddChild(BuildRequiredOnlyCard(violation, labels));
            }

            _onCanvasStatus?.Invoke(status);
        }

        private void ShowEmpty(string title, string message)
        {
            HideOverview();
            _list.AddClass(ListHiddenClass);
            _empty.RemoveClass(EmptyHiddenClass);
            _empty.Clear();
            _onCanvasStatus?.Invoke(StatusStyle.Type.Info);

            var icon = new VisualElement()
                .AddClass(EmptyIconClass)
                .AddClass(EmptyIconInfoClass);

            _empty.AddChild(icon)
                .AddChild(new AspidLabel(title, AspidLabelPreset.Default
                        .SetLabelTheme(ThemeStyle.Type.Lightness)
                        .SetLabelSize(AspidLabelSizeStyle.Type.H3)
                        .SetLineSize(AspidDividingLineSizeStyle.Type.None))
                    .AddClass(EmptyTitleClass))
                .AddChild(new Label(message).AddClass(EmptyMessageClass));
        }

        private void ShowResults()
        {
            HideOverview();
            _empty.AddClass(EmptyHiddenClass);
            _list.RemoveClass(ListHiddenClass);
        }

        private void ShowOverview(StatusStyle.Type status, int total, int missing, int orphans, int empties, int migrations, int required)
        {
            var broken = missing - migrations;

            _overviewTitle.Text = SerializeReferenceGraphSummary.BuildOverviewTitle(broken, orphans, migrations, required);
            _overviewTitle.LabelStatus = status;
            _overviewTitle.LineStatus = status;

            _overviewHint.text = SerializeReferenceGraphSummary.BuildOverviewHint(total, missing, orphans, empties, migrations, required);

            var hasAmber = broken > 0 || orphans > 0 || required > 0;
            _legend.EnableInClassList(LegendHiddenClass, migrations == 0 || !hasAmber);

            _overview.RemoveClass(OverviewHiddenClass);
        }

        private void HideOverview() => _overview?.AddClass(OverviewHiddenClass);

    }
}
