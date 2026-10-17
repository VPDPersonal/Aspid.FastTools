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
        private const string RescanIconClass = RootClass + "__rescan-icon";
        private const string NoticeActionClass = RootClass + "__notice-action";
        private const string NoticeClass = RootClass + "__notice";
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
        private readonly VisualElement _legend;
        private readonly VisualElement _list;
        private readonly ScrollView _scroll;

        private readonly NavRing _ring;

        private readonly AuditPickerHost _picker;

        private readonly SerializeReferenceConstraintCache _constraints = new();

        private static readonly SerializeReferenceAuditUI.LegendClasses _legendClassSet =
            new(LegendItemClass, LegendDotClass, LegendDotInfoClass, LegendTextClass);

        private IReadOnlyList<GateViolation> _requiredViolations = Array.Empty<GateViolation>();
        private IReadOnlyList<TypeNameField> _typeNames = Array.Empty<TypeNameField>();

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

            // The icon and the accent label mark Rescan as the action; without them it read as the field's label.
            _rescanButton = new AspidGradientButton("Rescan", _ => Rescan())
                .AddClass(RescanClass);
            _rescanButton.AddLeadingContent(SerializeReferenceAuditUI.BuildRescanIcon(RescanIconClass));
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

            _legend = new VisualElement()
                .AddClass(LegendClass)
                .AddClass(LegendHiddenClass)
                .AddChild(SerializeReferenceAuditUI.BuildLegendItem("Broken — pick a replacement", info: false, _legendClassSet))
                .AddChild(SerializeReferenceAuditUI.BuildLegendItem("Renamed — one-click migrate", info: true, _legendClassSet));

            _overview = new VisualElement()
                .AddClass(OverviewClass)
                .AddClass(OverviewHiddenClass)
                .AddChild(_overviewTitle)
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
            ReferenceEmptyStateLayout.PinIcon(_empty, _scroll);

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

        // The window keeps this view between visits to its tab: a new target maps that asset, the same one is mapped
        // again, since a repair elsewhere may have changed it.
        internal void Refresh(Object target)
        {
            if (target != _target) SetTarget(target);
            else Rescan();
        }

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
            _typeNames = Array.Empty<TypeNameField>();

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

            // The map reads the saved file and the required scan the loaded objects, so unsaved edits would mix both.
            if (SerializeReferenceOpenCopyGuard.HasUnsavedChanges(assetPath))
            {
                ShowUnsavedChanges(assetPath);
                return;
            }

            var documents = prebuilt ?? SerializeReferenceGraphScanner.Build(assetPath);

            // Required string fields may exist without a RefIds block, so scan them before the empty-graph return.
            _requiredViolations = SerializeReferenceGateScanner.ScanAssetRequiredFields(assetPath);
            _typeNames = SerializeReferenceGraphAnalysis.CollectTypeNameFields(assetPath);

            if (documents.Count == 0 && _requiredViolations.Count == 0 && _typeNames.Count == 0)
            {
                // A variant or a scene may hold its managed references only as prefab instance overrides, which have
                // no RefIds graph to map.
                if (SerializeReferenceYamlEditor.FindPrefabOverrideReferences(assetPath).Count > 0)
                {
                    ShowEmpty(
                        "Only prefab overrides",
                        "This asset sets [SerializeReference] types only through prefab instance overrides, which this " +
                        "view does not map. Project References lists the missing ones.");
                    return;
                }

                if (SerializeReferenceGateScanner.TryGetUnreadFormat(assetPath, EditorSettings.serializationMode, out var unreadFormat))
                {
                    ShowEmpty("Not scanned", SerializeReferenceGraphSummary.BuildUnreadAssetMessage(unreadFormat));
                    return;
                }

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

            // Drawn like the repair summaries of Project References: a tinted box with a title and a small button in its
            // corner.
            var info = new AspidHelpBox(AspidHelpBoxPreset.Default.SetMessageType(HelpBoxMessageType.Info))
                .SetTitle("Prefab instance")
                .SetMessage("Its managed references live in the source prefab.")
                .AddClass(NoticeClass);
            _list.AddChild(info);

            void OpenSource() => SetTarget(AssetDatabase.LoadAssetAtPath<Object>(sourcePath));

            var openSource = new Button(OpenSource) { text = "Open Source Prefab" }
                .AddClass(AspidStyles.ActionButtonClass)
                .AddClass(NoticeActionClass);
            RegisterNavTarget(openSource, OpenSource);
            info.AddChild(openSource);
            return true;
        }

        private void ShowUnsavedChanges(string assetPath)
        {
            ShowResults();
            _onCanvasStatus?.Invoke(StatusStyle.Type.Info);

            var info = new AspidHelpBox(AspidHelpBoxPreset.Default.SetMessageType(HelpBoxMessageType.Info))
                .SetTitle("Unsaved changes")
                .SetMessage("This view maps the saved file. Save the asset to map its current fields.")
                .AddClass(NoticeClass);
            _list.AddChild(info);

            void Save()
            {
                SerializeReferenceOpenCopyGuard.Save(assetPath);
                Rescan();
            }

            var save = new Button(Save) { text = "Save Asset" }
                .AddClass(AspidStyles.ActionButtonClass)
                .AddClass(NoticeActionClass);
            RegisterNavTarget(save, Save);
            info.AddChild(save);
        }

        private void RenderDocuments(string assetPath, List<ReferenceGraphDocument> documents)
        {
            var missing = 0;
            var orphans = 0;
            var migrations = 0;

            // The card budget is shared, so a scene with many documents is bounded as a whole.
            var plans = SerializeReferenceGraphPlan.BuildAll(documents, SerializeReferenceGraphPlan.MaxCards);
            var emptySlotPaths = SerializeReferenceGraphAnalysis.CollectEmptySlotPaths(plans);

            var hidden = 0;
            var hiddenEmpties = 0;
            var drawn = plans.Sum(plan => plan.Cards.Count);

            var ungraphedRequired = _requiredViolations
                .Where(violation => !emptySlotPaths.Contains((violation.FileId, violation.FieldPath)))
                .ToList();

            var required = _requiredViolations.Count;

            var requiredCards = SerializeReferenceGraphAnalysis.SelectRequiredCards(
                ungraphedRequired, SerializeReferenceGraphPlan.MaxCards - drawn, out var hiddenRequired);

            var unplacedRequired = new List<GateViolation>(requiredCards);
            var unplacedTypeNames = new List<TypeNameField>(_typeNames);

            // Every object with a name gets its heading, even the only one: the heading says whose fields the cards
            // are. A scene object is read as text and has only its YAML class id, so it is headed only among several.
            var several = documents.Count > 1;
            foreach (var plan in plans)
            {
                var document = plan.Document;
                var documentRequired = requiredCards.Where(violation => violation.FileId == document.FileId).ToList();
                var documentTypeNames = _typeNames.Where(field => field.FileId == document.FileId).ToList();

                // A document that got no card would be a header over nothing, so only the notice speaks for it.
                if (plan.Cards.Count > 0 || plan.Hidden == 0 || documentRequired.Count > 0 || documentTypeNames.Count > 0)
                {
                    _list.AddChild(BuildDocument(assetPath, plan, documentRequired, documentTypeNames,
                        showHeader: several || document.Source != null));
                    unplacedRequired.RemoveAll(violation => violation.FileId == document.FileId);
                    unplacedTypeNames.RemoveAll(field => field.FileId == document.FileId);
                }

                hidden += plan.Hidden;
                hiddenEmpties += plan.HiddenEmpty;

                var (broken, documentMigrations) = SerializeReferenceGraphAnalysis.CountUnresolved(assetPath, document, _constraints);
                missing += broken + documentMigrations;
                migrations += documentMigrations;
                orphans += document.Orphans.Count;
            }

            // A required field whose slot the plans left out is already in the hidden count, so it is taken out of it
            // once. The plans keep no paths past the budget, so this counts by number.
            var hiddenSlots = Math.Min(hiddenRequired, hiddenEmpties);
            hidden += hiddenRequired - hiddenSlots;

            var status = SerializeReferenceAuditUI.ResolveStatus(missing - migrations, orphans, required, migrations);
            ShowOverview(status, missing, orphans, migrations, required);

            AddFieldCards(_list, assetPath, unplacedTypeNames, unplacedRequired, qualify: true);

            if (hidden > 0) _list.AddChild(BuildHiddenNotice(hidden));

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

        // The title counts what needs attention and the section headings count the rest, so the overview is the title
        // alone, with how to act on it in its tooltip.
        private void ShowOverview(StatusStyle.Type status, int missing, int orphans, int migrations, int required)
        {
            var broken = missing - migrations;

            _overviewTitle.Text = SerializeReferenceGraphSummary.BuildOverviewTitle(broken, orphans, migrations, required);
            _overviewTitle.LabelStatus = status;
            _overviewTitle.LineStatus = status;

            _overviewTitle.tooltip = SerializeReferenceGraphSummary.BuildOverviewGuide(missing, orphans, migrations, required);

            var hasAmber = broken > 0 || orphans > 0 || required > 0;
            _legend.EnableInClassList(LegendHiddenClass, migrations == 0 || !hasAmber);

            _overview.RemoveClass(OverviewHiddenClass);
        }

        private void HideOverview() => _overview?.AddClass(OverviewHiddenClass);

    }
}
