using System;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Aspid.FastTools.Types;
using Aspid.FastTools.Editors;
using Aspid.FastTools.UIElements;
using System.Collections.Generic;
using Aspid.FastTools.Types.Editors;
using UnityEditor.SceneManagement;
using System.Text.RegularExpressions;
using Aspid.FastTools.UIElements.Editors.Internal;
using Object = UnityEngine.Object;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    /// <summary>
    /// Asset-level visualiser for <c>[SerializeReference]</c> managed-reference graphs. For each serialized object
    /// document in the asset it draws the reference tree — field-pointer roots, their nested children, shared
    /// (aliased) references and orphaned payloads — straight from the YAML, so it surfaces references at any nesting
    /// depth and the orphans the Inspector cannot navigate to. Every reference card is an inline type dropdown: the
    /// same embedded picker the Repair window uses, anchored under the clicked card, where picking a type assigns /
    /// re-points the reference and <c>&lt;None&gt;</c> clears it. Healthy and empty (unassigned) slots are edited
    /// through Unity's live serialization (so the <c>RefIds</c> entry is created or removed exactly as the Inspector
    /// would); a missing reference — which Unity cannot reassign through the API — is re-pointed / cleared by rewriting
    /// the YAML in place, keeping its orphaned payload. Orphaned payloads no field reaches carry a <c>Clear</c> action.
    /// </summary>
    internal sealed class SerializeReferenceGraphView : VisualElement
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

        private const string DocumentClass = RootClass + "__document";
        private const string DocumentHeaderClass = RootClass + "__document-header";
        private const string DocumentHeaderIssuesClass = DocumentHeaderClass + "--issues";
        private const string DocumentHeaderRowClass = RootClass + "__document-header-row";
        private const string DocumentTitleClass = RootClass + "__document-title";
        private const string DocumentCountClass = RootClass + "__document-count";
        private const string DocumentBodyClass = RootClass + "__document-body";

        private const string NodeClass = RootClass + "__node";
        private const string NodeOrphanClass = NodeClass + "--orphan";
        private const string NodeBackEdgeClass = NodeClass + "--back-edge";
        private const string NodeEmptyClass = NodeClass + "--empty";
        private const string NodePickingClass = NodeClass + "--picking";
        private const string NodeBandClass = RootClass + "__node-band";
        private const string NodeBandMissingClass = NodeBandClass + "--missing";
        private const string NodeBandMigrateClass = NodeBandClass + "--migrate";
        private const string NodeBandRowClass = RootClass + "__node-band-row";
        private const string NodeMigrateClass = RootClass + "__node-migrate";
        private const string NodeSuggestClass = RootClass + "__node-suggest";
        private const string NodeHeaderClass = RootClass + "__node-header";
        private const string NodeFooterClass = RootClass + "__node-footer";
        private const string NodeRootLabelClass = RootClass + "__node-root-label";
        private const string NodeTypeClass = RootClass + "__node-type";
        private const string NodeRidClass = RootClass + "__node-rid";
        private const string NodeBadgesClass = RootClass + "__node-badges";

        private const string BadgeClass = RootClass + "__badge";
        private const string BadgeMissingClass = BadgeClass + "--missing";
        private const string BadgeSharedClass = BadgeClass + "--shared";

        private const string ChipClass = RootClass + "__chip";
        private const string ClearOrphanClass = RootClass + "__clear-orphan";
        private const string OpenSourceClass = RootClass + "__open-source";
        private const string OrphanGroupClass = RootClass + "__orphan-group";
        private const string OrphanGroupHeaderClass = RootClass + "__orphan-group-header";
        private const string PickerClass = RootClass + "__picker";
        private const string PickerAttachedClass = PickerClass + "--attached";

        // Every reference card is an inline dropdown, its band carrying a verb + collapse chevron pinned right of the
        // value label: "Fix Missing ▼" on a broken card, "Change ▼" on a healthy one (re-point its type or clear it),
        // "Assign ▼" on an empty slot (give the unset field a type). All flip the chevron in place while their picker is
        // open, so the collapse toggle (TogglePicker / ClosePicker) swaps the glyph alone and never needs the label.
        private const string FixCollapsedText = "Fix Missing  ▼";
        private const string ChangeCollapsedText = "Change  ▼";
        private const string AssignCollapsedText = "Assign  ▼";

        // A pending-migration card is not missing (Unity migrates it in memory; only the file is stale), so its band
        // drops the "Missing" word — the picker behind it stays the manual escape hatch beside the Migrate action.
        private const string MigrateFixCollapsedText = "Fix  ▼";
        private const char BandChevronCollapsed = '▼';
        private const char BandChevronExpanded = '▲';

        // An unassigned [SerializeReference] slot's placeholder label — single-sourced from the picker's own "<None>"
        // option, so an empty slot reads the same way in the graph as the cleared field does in the Inspector.
        private const string EmptySlotText = TypeSelectorHelpers.NoneOption;

        // The document header's collapse chevron: ▼ while the body is shown, ▶ once collapsed.
        private const string DocumentChevronExpanded = "▼";
        private const string DocumentChevronCollapsed = "▶";

        // Reports this view's state-tone to the host window, which owns the shared dotted canvas behind every mode.
        private readonly Action<Color> _onCanvasTone;

        // Reports a target change back to the host window so its cached target follows an in-view pick. The host rebuilds
        // this view from that cached target on every tab switch, so without this an asset picked here (or the Open Source
        // Prefab retarget) would be dropped the next time the user returns to this tab.
        private readonly Action<Object> _onTargetChanged;

        private Object _target;
        private ObjectField _assetField;
        private AspidGradientButton _rescanButton;
        private VisualElement _empty;
        private VisualElement _overview;
        private AspidLabel _overviewTitle;
        private Label _overviewHint;
        private ScrollView _scroll;
        private VisualElement _list;

        private VisualElement _openPicker;
        private AspidGradientButton _openPickerRow;
        private VisualElement _openPickerCard;

        // Per-asset constraint map cache, mirroring SerializeReferenceProjectView: BuildConstraintMap does a
        // LoadAllAssetsAtPath + full SerializedObject walk, so without this each Fix-Missing picker open would re-scan
        // the whole asset. Keyed by asset path (the graph maps one asset at a time, but a multi-document asset still
        // shares one map across its cards). Cleared on every Rescan / apply so a fix's rewritten YAML is re-read.
        private readonly Dictionary<string, Dictionary<(long fileId, long rid), Type>> _constraintCache = new(StringComparer.Ordinal);

        public SerializeReferenceGraphView(Object target, Action<Color> onCanvasTone, Action<Object> onTargetChanged = null)
        {
            _target = target;
            _onCanvasTone = onCanvasTone;
            _onTargetChanged = onTargetChanged;

            var root = this;
            style.flexGrow = 1;
            root.AddAspidThemeStyleSheets()
                .AddStyleSheetsFromResource(StyleSheetPath)
                .AddClass(RootClass);

            // The asset-picker card mirrors the Project References "Find missing references" panel: a title and a one-line
            // description of what the tab does, then a single control fusing the asset field with the Rescan action —
            // so the two tabs' top panels read as one family (same translucent card, same title + description rhythm).
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

            // The field is hosted *inside* the Rescan button (below), so its own clicks — opening the object picker,
            // pinging, dragging an asset in — must not bubble to the button's Clickable and re-run Rescan on every
            // interaction. Swallow the press at the field boundary; only clicks on the surrounding "Rescan" area reach
            // the button. The field has already handled the event by the time it bubbles here, so its behaviour is intact.
            _assetField.RegisterCallback<PointerDownEvent>(evt => evt.StopPropagation());

            // Rescan fuses with the field rather than stacking below it: the "Rescan" label leads at the row's left and
            // the ObjectField rides as the button's trailing content, filling the row (FillWithTrailingContent yields the
            // free space to it) — so picking an asset and re-reading it share one control. The label stays visible always,
            // so the action reads the same whether or not an asset is set.
            _rescanButton = new AspidGradientButton("Rescan", _ => Rescan())
                .AddClass(RescanClass);
            _rescanButton.AddTrailingContent(_assetField);
            _rescanButton.FillWithTrailingContent();

            var card = new AspidBox(AspidBoxPreset.Default.SetTheme(ThemeStyle.Type.Darkness))
                .AddClass(CardClass)
                .AddChild(cardTitle)
                .AddChild(cardDescription)
                .AddChild(_rescanButton);

            // The empty state (no asset / no managed references) shares one centred hero below the card — a large
            // dimmed info icon, a headline and a dimmed explanation; a successful scan swaps it for the per-document
            // tree list inside a scroll view.
            _empty = new VisualElement().AddClass(EmptyClass);

            // Scan overview, mirroring the Project References results header: a status headline over a dim one-line
            // breakdown, sitting between the asset card and the document scroll. Amber "N missing references" when the
            // graph carries broken / orphaned references, green "No missing references" when every type resolves — the
            // same status read the dotted canvas wears. Hidden in the empty / prefab-instance states (see HideOverview /
            // ShowResults); its label / line status flip between Warning and Success per scan (see ShowOverview).
            _overviewTitle = new AspidLabel(string.Empty, AspidLabelPreset.Default
                    .SetLabelStatus(StatusStyle.Type.Warning)
                    .SetLabelSize(AspidLabelSizeStyle.Type.H4)
                    .SetLineTheme(ThemeStyle.Type.Dark)
                    .SetLineStatus(StatusStyle.Type.Warning))
                .AddClass(OverviewTitleClass);

            _overviewHint = new Label(string.Empty).AddClass(OverviewHintClass);

            _overview = new VisualElement()
                .AddClass(OverviewClass)
                .AddClass(OverviewHiddenClass)
                .AddChild(_overviewTitle)
                .AddChild(_overviewHint);

            _list = new VisualElement().AddClass(ListClass);

            // One scroll spans the whole view between the window's tabs and footer: the asset card, the empty hero, the
            // scan overview and the per-document tree all live inside it, so the card and overview scroll away with the
            // document list rather than staying pinned above a separately-scrolling list.
            var content = new VisualElement()
                .AddClass(ContentClass)
                .AddChild(card)
                .AddChild(_empty)
                .AddChild(_overview)
                .AddChild(_list);

            _scroll = new ScrollView().AddClass(ScrollClass);
            _scroll.AddChild(content);

            root.AddChild(_scroll);

            Rescan();
        }

        private void SetTarget(Object target)
        {
            _target = target;
            // Mirror the pick back to the host window so its cached target follows. A tab switch rebuilds this view from
            // that cached target, so without this an in-view pick (or the Open Source Prefab retarget) would be dropped
            // the next time the user returns to this tab. The host just stores it (no rebuild), so this never re-enters.
            _onTargetChanged?.Invoke(target);
            // Open() retargets an already-open window, so the field must follow the new target — without notifying,
            // or the change callback would trigger a second scan.
            _assetField?.SetValueWithoutNotify(target);
            if (_list is not null) Rescan();
        }

        private void Rescan(List<ReferenceGraphDocument> prebuilt = null)
        {
            if (_list is null) return;

            ClosePicker();
            // Drop the per-asset constraint maps so a rescan after a fix / clear re-reads the rewritten YAML rather than
            // a stale map. Every apply path (ApplyFix / ClearReference / ApplyLive / ClearOrphan) funnels through here.
            _constraintCache.Clear();
            _list.Clear();

            var assetPath = _target ? AssetDatabase.GetAssetPath(_target) : null;
            if (string.IsNullOrEmpty(assetPath))
            {
                // A nested prefab instance keeps its managed-reference data in the source prefab, not the host, so offer
                // to retarget the graph onto that source where the RefIds actually live.
                if (SerializeReferenceHelpers.TryGetSourcePrefabPath(_target, out var sourcePath))
                {
                    ShowResults();
                    _onCanvasTone?.Invoke(SerializeReferenceCanvasStyle.Info);

                    var info = new AspidHelpBox(AspidHelpBoxPreset.Default.SetMessageType(HelpBoxMessageType.Info));
                    info.Message = "This is a prefab instance — its managed references live in the source prefab.";
                    _list.AddChild(info);

                    _list.AddChild(new AspidGradientButton("Open Source Prefab",
                            _ => SetTarget(AssetDatabase.LoadAssetAtPath<Object>(sourcePath)))
                        .AddClass(OpenSourceClass));
                    return;
                }

                ShowEmpty(
                    "No asset selected",
                    "Select a saved asset (a prefab or ScriptableObject) to map its managed-reference graph.");
                return;
            }

            var documents = prebuilt ?? SerializeReferenceGraphScanner.Build(assetPath);
            if (documents.Count == 0)
            {
                ShowEmpty(
                    "No managed references",
                    "This asset has no [SerializeReference] managed references to map.");
                return;
            }

            ShowResults();

            // Tally the whole asset's graph as the documents are built, so the overview header can headline the
            // missing / orphaned count (amber) or a clean bill of health (green) — the same status-wash language the
            // Project References view uses, and the same read the dotted canvas wears below. Empty (unassigned) slots are
            // tallied separately: they are not broken, so they never tip the headline / canvas to amber — they are only
            // surfaced in the dim hint so a cleared field stays noticeable.
            var total = 0;
            var missing = 0;
            var orphans = 0;
            var empties = 0;
            var migrations = 0;

            // The per-document header only earns its place when there is more than one document to tell apart (a
            // multi-component prefab); a single document drops it (see BuildDocument).
            var showHeaders = documents.Count > 1;
            foreach (var document in documents)
            {
                _list.AddChild(BuildDocument(assetPath, document, showHeaders));

                total += document.Nodes.Count;
                var (broken, documentMigrations) = CountUnresolved(assetPath, document);
                missing += broken + documentMigrations;
                migrations += documentMigrations;
                orphans += document.Orphans.Count;
                empties += CountEmptySlots(document);
            }

            ShowOverview(total, missing, orphans, empties, migrations);

            // Pending migrations are not breakages — a graph whose only annotations are migrations reads info-blue,
            // matching the Project References group card; anything actually missing / orphaned keeps the amber wash.
            _onCanvasTone?.Invoke(missing - migrations > 0 || orphans > 0
                ? SerializeReferenceCanvasStyle.Warning
                : migrations > 0
                    ? SerializeReferenceCanvasStyle.Info
                    : SerializeReferenceCanvasStyle.Success);
        }

        // The ranked Smart Fix for a missing node: the stored identity scored against the node's field constraint,
        // keyed by the payload's field names — through the shared per-(path, fileId, rid) cache, so a rescan and the
        // inline drawer reuse one computation. Best-effort: a parse miss just means no suggestion row.
        private bool TryGetNodeSuggestion(string assetPath, long fileId, long rid, ManagedTypeName storedType,
            out SerializeReferenceRepairSuggestions.RepairCandidate suggestion)
        {
            suggestion = default;

            try
            {
                var fieldNames = SerializeReferenceYamlEditor.GetReferenceFieldNames(assetPath, fileId, rid);
                var constraint = ResolveConstraint(assetPath, fileId, rid) ?? typeof(object);

                var ranked = SerializeReferenceRepairSuggestions.GetCached(assetPath, fileId, rid,
                    () => SerializeReferenceRepairSuggestions.Rank(storedType, fieldNames, constraint));
                if (ranked.Count == 0) return false;

                suggestion = ranked[0];
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // A missing node whose stored type is claimed by exactly one [MovedFrom] target that fits the field's declared
        // type reads as a pending migration — the Project References group gate, applied per node. An unrecoverable
        // constraint behaves like the unconstrained picker and lets the migration through.
        private bool IsPendingMigration(string assetPath, long fileId, long rid, ManagedTypeName storedType, out Type target)
        {
            if (!SerializeReferenceMovedFromResolver.TryResolve(storedType, out target)) return false;

            var constraint = ResolveConstraint(assetPath, fileId, rid);
            return constraint is null || constraint == typeof(object) || constraint.IsAssignableFrom(target);
        }

        // How many unassigned [SerializeReference] slots the document holds — empty field roots plus empty nested edges
        // (each points at the null sentinel). Used only for the overview hint; empty slots are not "issues".
        private static int CountEmptySlots(ReferenceGraphDocument document)
        {
            var count = 0;

            foreach (var root in document.Roots)
                if (root.IsEmpty) count++;

            foreach (var pair in document.Edges)
            foreach (var edge in pair.Value)
                if (edge.IsEmpty) count++;

            return count;
        }

        // Splits a document's unresolved nodes into genuinely broken ones and pending [MovedFrom] migrations. An
        // orphaned rid always counts as broken — nothing loads an orphan, so the "Unity migrates it in memory"
        // argument does not apply to it (it also keeps the Migrate row out of the warning-tinted Orphaned group).
        private (int broken, int migrations) CountUnresolved(string assetPath, ReferenceGraphDocument document)
        {
            var broken = 0;
            var migrations = 0;

            foreach (var node in document.Nodes)
            {
                if (node.Resolves || node.StoredType.IsEmpty) continue;

                // An unresolved orphan is one entity, already counted (and amber-glowed) by the orphan tallies —
                // adding it to "missing" too would double-count it in the overview headline and hints.
                if (document.Orphans.Contains(node.Rid)) continue;

                if (IsPendingMigration(assetPath, document.FileId, node.Rid, node.StoredType, out _))
                    migrations++;
                else
                    broken++;
            }

            return (broken, migrations);
        }

        // A root is "missing" when the node it points at has an unresolved (unloadable) stored type — the same
        // predicate the MISSING badge and amber tint use. Drives the missing-first root ordering in BuildDocument.
        private static bool RootIsMissing(ReferenceGraphDocument document, long rid)
        {
            var node = document.FindNode(rid);
            return node is { Resolves: false } && !node.Value.StoredType.IsEmpty;
        }

        // Both empty states reuse one hero (mirroring the Project References view): a large dimmed info icon, a headline
        // and a dimmed explanation, centred in the space below the asset card.
        private void ShowEmpty(string title, string message)
        {
            HideOverview();
            _list.AddClass(ListHiddenClass);
            _empty.RemoveClass(EmptyHiddenClass);
            _empty.Clear();
            _onCanvasTone?.Invoke(SerializeReferenceCanvasStyle.Info);

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
            // The generic "swap the hero for the scroll" call. The overview header is left hidden here; only the
            // document-graph path (Rescan) re-shows it via ShowOverview, so the prefab-instance branch — which reuses
            // ShowResults for its info box — keeps the missing-reference headline suppressed.
            HideOverview();
            _empty.AddClass(EmptyHiddenClass);
            _list.RemoveClass(ListHiddenClass);
        }

        // The scan overview above the document scroll, mirroring the Project References results header. Headlines the
        // real missing count first, then orphaned rids, then pending [MovedFrom] migrations, falling back to a green
        // all-clear; the label and its divider flip between Warning (amber), Info (blue) and Success (green) in place,
        // and the dim hint spells out the full breakdown.
        private void ShowOverview(int total, int missing, int orphans, int empties, int migrations)
        {
            // Only genuinely missing / orphaned references are "issues" that tip the headline and divider to amber;
            // pending migrations are stale files, not breakages (info), and empty slots are unassigned, not broken.
            var broken = missing - migrations;
            var status = broken > 0 || orphans > 0
                ? StatusStyle.Type.Warning
                : migrations > 0
                    ? StatusStyle.Type.Info
                    : StatusStyle.Type.Success;

            _overviewTitle.Text = broken > 0
                ? (broken == 1 ? "1 missing reference" : $"{broken} missing references")
                : orphans > 0
                    ? (orphans == 1 ? "1 orphaned reference" : $"{orphans} orphaned references")
                    : migrations > 0
                        ? (migrations == 1 ? "1 pending migration" : $"{migrations} pending migrations")
                        : "No missing references";

            _overviewTitle.LabelStatus = status;
            _overviewTitle.LineStatus = status;

            _overviewHint.text = BuildOverviewHint(total, missing, orphans, empties, migrations);
            _overview.RemoveClass(OverviewHiddenClass);
        }

        // The overview's dim subtitle: the mapped reference count, annotated with the missing / orphaned / unassigned
        // tallies and a one-line cue toward the matching inline action — or a clean bill of health when nothing is
        // broken (an unassigned-only graph still reads clean, with the empty count appended as a quiet note).
        private static string BuildOverviewHint(int total, int missing, int orphans, int empties, int migrations)
        {
            var references = total == 1 ? "1 managed reference" : $"{total} managed references";
            var emptyNote = empties == 0
                ? string.Empty
                : empties == 1 ? " · 1 unassigned field" : $" · {empties} unassigned fields";

            if (missing == 0 && orphans == 0)
                return $"{references} mapped{emptyNote} — every [SerializeReference] type resolves.";

            var broken = missing - migrations;

            var parts = new List<string>(4);
            if (broken > 0) parts.Add(broken == 1 ? "1 missing type" : $"{broken} missing types");
            if (migrations > 0)
                parts.Add(migrations == 1 ? "1 pending [MovedFrom] migration" : $"{migrations} pending [MovedFrom] migrations");
            if (orphans > 0) parts.Add(orphans == 1 ? "1 orphaned rid" : $"{orphans} orphaned rids");
            if (empties > 0) parts.Add(empties == 1 ? "1 unassigned field" : $"{empties} unassigned fields");

            var action = broken > 0
                ? "Fix a missing type inline from its card."
                : migrations > 0
                    ? "Migrate a renamed type from its card — the Inspector already loads it; only the file is stale."
                    : "Clear an orphaned rid from its card.";

            return $"{references} mapped · {string.Join(" · ", parts)}. {action}";
        }

        private void HideOverview() => _overview?.AddClass(OverviewHiddenClass);

        // One serialized object document: a clickable header band (styled like the Project References group header — a
        // gradient row carrying the component / ScriptableObject name, a reference count and a collapse chevron) over a
        // collapsible body that holds each root's reference subtree as a flat stack of separate node cards (nesting read
        // from each card's field path, not indentation) plus a trailing "Orphaned" group for any rids no root reaches.
        // The header is dropped when the asset has a single document (see showHeader) — there it would only restate the
        // ObjectField above it.
        private VisualElement BuildDocument(string assetPath, ReferenceGraphDocument document, bool showHeader)
        {
            // Pending migrations are not issues — a document whose only findings are migrations keeps the calm
            // header, matching the info-toned overview; orphans and genuinely broken nodes still glow amber.
            var (broken, migrations) = CountUnresolved(assetPath, document);
            var hasIssues = document.Orphans.Count > 0 || broken > 0;

            // The collapsible body — every node card and the orphan group live here so the header chevron can hide
            // them in one toggle.
            var body = new VisualElement().AddClass(DocumentBodyClass);

            // Surface the roots that need attention first: a root whose own stored type is missing renders above the
            // healthy ones. Two passes over the asset's field order make this a stable partition — each group keeps
            // its source order and the list does not reshuffle between rescans. Empty (unassigned) roots are not
            // missing, so they fall to the second pass and render in field order as quiet "<None>" leaves.
            foreach (var root in document.Roots)
            {
                if (root.IsEmpty || !RootIsMissing(document, root.Rid)) continue;
                var visited = new HashSet<long>();
                AppendNode(body, assetPath, document, root.Rid, root.Label, visited);
            }

            foreach (var root in document.Roots)
            {
                if (root.IsEmpty)
                {
                    body.AddChild(BuildEmptySlotCard(assetPath, document.FileId, root.Label));
                    continue;
                }

                if (RootIsMissing(document, root.Rid)) continue;
                var visited = new HashSet<long>();
                AppendNode(body, assetPath, document, root.Rid, root.Label, visited);
            }

            var orphans = BuildOrphanGroup(assetPath, document);
            if (orphans is not null) body.AddChild(orphans);

            // Single-document asset: no header band. The asset is already named in the ObjectField above, so a type ·
            // name title and its collapse chevron would only restate it. The body renders on its own, always expanded.
            if (!showHeader)
                return new VisualElement().AddClass(DocumentClass).AddChild(body);

            // The header band: a gradient button (transparent fill, amber hover glow when the document has issues) whose
            // Text is the collapse chevron and whose leading content is the title + reference count. The self-reference
            // lets the click handler flip its own chevron alongside toggling the body.
            AspidGradientButton header = null;
            var collapsed = false;
            header = new AspidGradientButton(DocumentChevronExpanded, _ =>
            {
                collapsed = !collapsed;
                body.style.display = collapsed ? DisplayStyle.None : DisplayStyle.Flex;
                header.Text = collapsed ? DocumentChevronCollapsed : DocumentChevronExpanded;
            }).AddClass(DocumentHeaderClass);
            if (hasIssues) header.AddClass(DocumentHeaderIssuesClass);
            header.tooltip = $"fileId {document.FileId}";

            // Title + dim count, docked into the band to the left of the chevron and ignored for picking so clicks fall
            // through to the band's own handler.
            header.AddLeadingContent(new VisualElement()
                .AddClass(DocumentHeaderRowClass)
                .SetPickingMode(PickingMode.Ignore)
                .AddChild(new Label(document.TypeName)
                    .AddClass(DocumentTitleClass)
                    .SetPickingMode(PickingMode.Ignore))
                .AddChild(new Label(BuildDocumentCountText(document, broken, migrations))
                    .AddClass(DocumentCountClass)
                    .SetPickingMode(PickingMode.Ignore)));

            return new VisualElement()
                .AddClass(DocumentClass)
                .AddChild(header)
                .AddChild(body);
        }

        // The header's dim subtitle: total managed-reference count, annotated with the missing / migration / orphaned
        // tallies when present — the same "N · M" two-part shape the Project References group header uses. A pending
        // [MovedFrom] migration is named as such so the header never contradicts the overview's "0 missing".
        private static string BuildDocumentCountText(ReferenceGraphDocument document, int broken, int migrations)
        {
            var total = document.Nodes.Count;
            var orphans = document.Orphans.Count;

            var text = total == 1 ? "1 reference" : $"{total} references";
            if (broken > 0) text += $" · {broken} missing";
            if (migrations > 0) text += migrations == 1 ? " · 1 migration" : $" · {migrations} migrations";
            if (orphans > 0) text += orphans == 1 ? " · 1 orphaned" : $" · {orphans} orphaned";
            return text;
        }

        // Appends a node's card and then, recursively, its children's cards as flat siblings — a flat, scannable stack
        // rather than indented nested boxes. Nesting is read entirely from the field path, not the layout: the full field
        // path is threaded down so each child joins its own field path (relative to the parent's data block) onto the
        // parent's path, and a nested reference shows where it lives from the document root (e.g.
        // "_primaryWeapon._chargeEffect"). An empty child slot renders as an "<None>" leaf and never recurses. The visited
        // set makes the walk cycle-safe: a rid already on the current path renders as a back-edge leaf ("↩ rid N")
        // instead of recursing forever.
        private void AppendNode(VisualElement container, string assetPath, ReferenceGraphDocument document, long rid, string pathLabel, HashSet<long> visited)
        {
            if (!visited.Add(rid))
            {
                container.AddChild(BuildBackEdgeCard(rid));
                return;
            }

            var node = document.FindNode(rid);
            container.AddChild(BuildNodeCard(assetPath, document, node, rid, pathLabel, isOrphan: false));

            foreach (var edge in document.ChildrenOf(rid))
            {
                var childPath = CombinePath(pathLabel, edge.Label);
                if (edge.IsEmpty)
                    container.AddChild(BuildEmptySlotCard(assetPath, document.FileId, childPath));
                else
                    AppendNode(container, assetPath, document, edge.Rid, childPath, visited);
            }

            // Leaving the recursion: drop the rid so a sibling subtree may legitimately reference it again (shared),
            // while a back-edge on the current path is still caught above.
            visited.Remove(rid);
        }

        // Joins a parent reference's full field path with a child field's path relative to the parent's data block, so a
        // nested reference shows the route from the document root down (e.g. "_primaryWeapon" + "_chargeEffect" =>
        // "_primaryWeapon._chargeEffect"). Either side may be empty — a root whose label could not be recovered, or a
        // child whose own relative path could not be built — in which case the non-empty side is used alone.
        private static string CombinePath(string parent, string child)
        {
            if (string.IsNullOrEmpty(child)) return parent;
            if (string.IsNullOrEmpty(parent)) return child;
            return $"{parent}.{child}";
        }

        // A node card laid out over two lines, mirroring the Project References group header so a broken node reads the
        // same way in both views. Top band: the stored type as an Aspid status label (an amber pill when the type is
        // missing / orphaned, a quiet light label otherwise) with the MISSING / SHARED badges beside it and a collapse
        // chevron docked right — the whole band is a dropdown toggling the inline picker. A missing card's band carries
        // the amber "Fix Missing ▼" cue and edits through the YAML; a healthy card's band is a plain "▼" dropdown that
        // edits through the live serialization API. An orphan keeps a static band (no field points at it) plus a footer
        // Clear. Bottom line: the dim field path the reference sits under, then the rid (an orphan adds its Clear here).
        // Cards are not indented — the field path alone carries the nesting, so the stack stays a flat, scannable column.
        private VisualElement BuildNodeCard(string assetPath, ReferenceGraphDocument document, ReferenceGraphNode? node, long rid, string pathLabel, bool isOrphan)
        {
            var missing = node is { Resolves: false } && !node.Value.StoredType.IsEmpty;

            // An authoritative [MovedFrom] rename is a pending migration, not a breakage: Unity loads the reference
            // fine — only this file still stores the old name. The card mirrors the Project References migration
            // group: info pill, a neutral "Fix ▼" band (the manual escape hatch) and a one-click Migrate row below.
            // Never for an orphan — nothing loads an orphan, so the in-memory migration argument does not hold and
            // its card stays on the plain missing path inside the warning-tinted Orphaned group.
            Type migrationTarget = null;
            var isMigration = missing && !isOrphan &&
                IsPendingMigration(assetPath, document.FileId, rid, node.Value.StoredType, out migrationTarget);

            var card = new AspidBox(AspidBoxPreset.Default.SetTheme(ThemeStyle.Type.Darkness))
                .AddClass(NodeClass);
            if (isOrphan) card.AddClass(NodeOrphanClass);

            // Top band — type identity + status badges on the left, the Fix action docked right.

            // The type name drives its own colour through an Aspid status label: a missing / orphaned type wears the
            // same amber pill the Project References group header uses, a pending migration the same info pill; a
            // healthy type stays a quiet light label.
            var typePreset = AspidLabelPreset.Default
                .SetLabelSize(AspidLabelSizeStyle.Type.H5)
                .SetLineSize(AspidDividingLineSizeStyle.Type.None);
            typePreset = isMigration
                ? typePreset.SetLabelStatus(StatusStyle.Type.Info)
                : missing || isOrphan
                    ? typePreset.SetLabelStatus(StatusStyle.Type.Warning)
                    : typePreset.SetLabelTheme(ThemeStyle.Type.Lightness);

            var typeLabel = new AspidLabel(node?.ShortName ?? $"rid {rid}", typePreset)
                .AddClass(NodeTypeClass)
                .SetPickingMode(PickingMode.Ignore);
            if (node is not null && !node.Value.StoredType.IsEmpty)
                typeLabel.tooltip = node.Value.FullName;

            // Badges sit beside the type. The missing state is carried by the "Fix Missing" band action and the amber
            // type pill, so no MISSING badge here; only SHARED (an alias cue, orthogonal to missing-ness) remains.
            var badges = new VisualElement()
                .AddClass(NodeBadgesClass)
                .SetPickingMode(PickingMode.Ignore);

            if (document.Shared.Contains(rid))
            {
                var shared = new Label("SHARED").AddClass(BadgeClass).AddClass(BadgeSharedClass);
                var chip = new VisualElement().AddClass(ChipClass);
                chip.style.backgroundColor = SerializeReferenceRidColor.ForRid(rid);
                shared.AddChild(chip);

                badges.AddChild(shared);
            }

            var bandRow = new VisualElement()
                .AddClass(NodeBandRowClass)
                .AddChild(typeLabel)
                .AddChild(badges);
            bandRow.pickingMode = PickingMode.Ignore;

            if (missing)
            {
                // The band itself is the Fix dropdown: leading content (type + badges) hugs the left, the "Fix ▼" label
                // pins right, and clicking anywhere on the band toggles the inline picker — the Project References group-
                // header interaction. The self-referencing capture lets the handler anchor the picker below the card;
                // the captured file id targets the rewrite at exactly this document's rid (rids collide across docs).
                // A missing reference cannot be reassigned through the serialization API, so its edit goes through the
                // YAML (keeping the orphaned payload).
                var fileId = document.FileId;
                AspidGradientButton band = null;
                band = new AspidGradientButton(isMigration ? MigrateFixCollapsedText : FixCollapsedText,
                        _ => OpenMissingPicker(assetPath, fileId, rid, band))
                    .AddClass(NodeBandClass)
                    .AddClass(isMigration ? NodeBandMigrateClass : NodeBandMissingClass);
                band.AddLeadingContent(bandRow);
                card.AddChild(band);

                if (isMigration)
                {
                    // One click bakes the rename into this document's entry through the same YAML rewrite a picker
                    // pick performs — no confirm, matching the picker's own apply. The picker above stays available
                    // for re-pointing at a different type.
                    var migrate = new AspidGradientButton($"Migrate  →  {migrationTarget.Name}",
                            _ => ApplyFix(assetPath, fileId, rid, migrationTarget.AssemblyQualifiedName))
                        .AddClass(NodeMigrateClass);
                    migrate.tooltip =
                        $"This entry resolves to {migrationTarget.FullName} via its declared [MovedFrom] — Unity " +
                        "already migrates it in memory when the asset loads. Migrating rewrites the stored type " +
                        "name in the file so it matches the code.";
                    card.AddChild(migrate);
                }
                else if (TryGetNodeSuggestion(assetPath, document.FileId, rid, node.Value.StoredType, out var suggestion))
                {
                    // The same Smart Fix quick-apply the inline notice and the Project References card surface — the
                    // graph was the one missing-type surface without it. Safe to hand straight to ApplyFix: Rank's
                    // pool is constraint-filtered, so the suggestion is always a type the picker itself would offer.
                    var suggest = new AspidGradientButton(SerializeReferenceHelpers.GetSuggestionLabel(suggestion),
                            _ => ApplyFix(assetPath, fileId, rid, suggestion.Type.AssemblyQualifiedName))
                        .AddClass(NodeSuggestClass);
                    suggest.tooltip = SerializeReferenceHelpers.GetSuggestionDetail(suggestion);
                    card.AddChild(suggest);
                }
            }
            else if (!isOrphan)
            {
                // A healthy assigned reference is a dropdown too: clicking the band opens the same selector so its type
                // can be changed or the reference reset to <None>. The edit goes through the live serialization API
                // (keyed by the field path), so Unity rewrites — or, on <None>, removes — the RefIds entry exactly as
                // the Inspector would. The dim "Change ▼" label sits right of the type pill, reading as a value dropdown.
                var fileId = document.FileId;
                var graphPath = pathLabel;
                AspidGradientButton band = null;
                band = new AspidGradientButton(ChangeCollapsedText, _ => OpenLivePicker(assetPath, fileId, graphPath, band))
                    .AddClass(NodeBandClass);
                band.AddLeadingContent(bandRow);
                card.AddChild(band);
            }
            else
            {
                // An orphan has no field pointing at it, so there is no live property to edit — its band stays static
                // and the footer Clear (below) drops the dangling entry.
                card.AddChild(bandRow);
            }

            // Bottom line — the dim field path, the rid, and (for an orphan) the Clear action. Healthy and empty slots
            // are cleared through their band's picker (pick <None>), so they carry no separate button here.
            var meta = new VisualElement().AddClass(NodeFooterClass);

            if (!string.IsNullOrEmpty(pathLabel))
            {
                meta.AddChild(new Label($"{pathLabel}:")
                    .AddClass(NodeRootLabelClass)
                    .SetPickingMode(PickingMode.Ignore));
            }

            meta.AddChild(new Label($"rid {rid}")
                .AddClass(NodeRidClass)
                .SetPickingMode(PickingMode.Ignore));

            if (isOrphan)
            {
                // Drop a dangling RefIds entry no field points at. File edit, so it is confirmed and not undoable.
                var fileId = document.FileId;
                meta.AddChild(new AspidGradientButton("Clear", _ => ClearOrphan(assetPath, fileId, rid))
                    .AddClass(ClearOrphanClass));
            }

            card.AddChild(meta);

            return card;
        }

        // A back-edge to a rid already on the current render path — a single dim, italic line (no footer) so cycles
        // terminate visibly.
        private static VisualElement BuildBackEdgeCard(long rid)
        {
            var card = new AspidBox(AspidBoxPreset.Default.SetTheme(ThemeStyle.Type.Darkness))
                .AddClass(NodeClass)
                .AddClass(NodeBackEdgeClass);

            card.AddChild(new VisualElement()
                .AddClass(NodeHeaderClass)
                .AddChild(new Label($"↩ rid {rid}")
                    .AddClass(NodeTypeClass)
                    .SetPickingMode(PickingMode.Ignore)));

            return card;
        }

        // An unassigned [SerializeReference] slot — a field whose pointer is the null sentinel (rid -2). Rendered as a
        // quiet, dim leaf so a cleared or never-assigned reference is visible in the graph (you can see the field is
        // unset) instead of silently dropping out. Laid out like a node card — a top "<None>" label over the dim field
        // path — with no badges and no recursion, but its band is still a dropdown: clicking it opens the selector so a
        // type can be assigned to the empty field straight from the graph (through the live serialization API, so Unity
        // creates the RefIds entry). A slot whose field path could not be recovered stays static (nothing to target).
        // Not indented — its field path carries the nesting, like every other card.
        private VisualElement BuildEmptySlotCard(string assetPath, long fileId, string pathLabel)
        {
            var card = new AspidBox(AspidBoxPreset.Default.SetTheme(ThemeStyle.Type.Darkness))
                .AddClass(NodeClass)
                .AddClass(NodeEmptyClass);

            // The placeholder type label — a quiet dim "<None>". A plain Label (like a back-edge leaf), so the --empty
            // USS rule tints it dim italic rather than an AspidLabel painting its own status colour.
            var typeLabel = new Label(EmptySlotText)
                .AddClass(NodeTypeClass)
                .SetPickingMode(PickingMode.Ignore);

            var bandRow = new VisualElement()
                .AddClass(NodeBandRowClass)
                .AddChild(typeLabel);
            bandRow.pickingMode = PickingMode.Ignore;

            if (string.IsNullOrEmpty(pathLabel))
            {
                // No recoverable field path to target — leave the slot a static "<None>" leaf.
                card.AddChild(bandRow);
            }
            else
            {
                // The band is a dropdown: its picker assigns a type to the empty field through the live serialization
                // API. <None> is a no-op here (the slot is already unset). The dim "Assign ▼" sits right of the "<None>".
                var graphPath = pathLabel;
                AspidGradientButton band = null;
                band = new AspidGradientButton(AssignCollapsedText, _ => OpenLivePicker(assetPath, fileId, graphPath, band))
                    .AddClass(NodeBandClass);
                band.AddLeadingContent(bandRow);
                card.AddChild(band);
            }

            // Bottom line — the dim field path, then an "unassigned" note where a live node shows its rid.
            var meta = new VisualElement().AddClass(NodeFooterClass);

            if (!string.IsNullOrEmpty(pathLabel))
            {
                meta.AddChild(new Label($"{pathLabel}:")
                    .AddClass(NodeRootLabelClass)
                    .SetPickingMode(PickingMode.Ignore));
            }

            meta.AddChild(new Label("unassigned")
                .AddClass(NodeRidClass)
                .SetPickingMode(PickingMode.Ignore));

            card.AddChild(meta);

            return card;
        }

        // Trailing warning-tinted group listing rids no root reaches — leftover payloads from deleted fields or broken
        // parents. Each orphan is a node card (so a missing orphan is still fixable — the inline picker anchors below
        // the card) whose footer carries a Clear action to drop the dangling entry, without recursion into children.
        private VisualElement BuildOrphanGroup(string assetPath, ReferenceGraphDocument document)
        {
            if (document.Orphans.Count == 0) return null;

            var group = new AspidBox(AspidBoxPreset.Default.SetTheme(ThemeStyle.Type.Darkness))
                .AddClass(OrphanGroupClass);

            group.AddChild(new AspidLabel("Orphaned", AspidLabelPreset.Default
                    .SetLabelStatus(StatusStyle.Type.Warning)
                    .SetLabelSize(AspidLabelSizeStyle.Type.H5)
                    .SetLineSize(AspidDividingLineSizeStyle.Type.None))
                .AddClass(OrphanGroupHeaderClass));

            foreach (var node in document.Nodes)
            {
                if (!document.Orphans.Contains(node.Rid)) continue;
                group.AddChild(BuildNodeCard(assetPath, document, node, node.Rid, pathLabel: null, isOrphan: true));
            }

            return group;
        }

        // Removes a single orphaned RefIds entry from the saved asset. Re-derives the orphan set fresh (the on-screen
        // graph may be stale) and only removes a rid that is still genuinely orphaned, then reimports and rescans.
        private void ClearOrphan(string assetPath, long fileId, long rid)
        {
            if (BlockedByOpenCopy(assetPath)) return;

            if (!EditorUtility.DisplayDialog(
                    "Drop Orphaned Entry",
                    $"Remove the orphaned managed-reference entry (rid {rid}) from\n{assetPath}?\n\n" +
                    "This edits the asset file directly and cannot be undone.",
                    "Remove", "Cancel"))
                return;

            // Guard against a stale graph: confirm the rid is still an orphan against a fresh scan before deleting.
            var fresh = SerializeReferenceGraphScanner.Build(assetPath);
            var stillOrphan = false;
            foreach (var document in fresh)
                if (document.FileId == fileId && document.Orphans.Contains(rid)) { stillOrphan = true; break; }

            if (!stillOrphan)
            {
                // The on-screen graph was stale (the rid is no longer an orphan); re-render from the scan we just built
                // instead of reading the unchanged file a second time.
                Rescan(fresh);
                return;
            }

            if (!SerializeReferenceYamlEditor.TryRemoveEntry(assetPath, fileId, rid)) return;

            // The forced import lets the index invalidator patch this one asset surgically — a full ClearCache here
            // would dump the whole warm index and put Project References back on its modal first-scan.
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            SerializeReferenceRepairSuggestions.ClearCache();
            Rescan();
        }

        // Missing card: opens the YAML fix picker. Constrained to the rid's declared field type (recovered from the
        // asset's managed-reference fields) so a repair cannot pick an incompatible type that would null on import; an
        // unresolvable field type falls back to an unconstrained picker. A pick rewrites the stored type in place
        // (keeping the broken payload) or, on <None>, nulls the reference — neither needs the live object, since Unity
        // cannot reassign a missing reference through the serialization API.
        private void OpenMissingPicker(string assetPath, long fileId, long rid, AspidGradientButton anchor) =>
            TogglePicker(anchor, ResolveConstraint(assetPath, fileId, rid),
                currentAqn: null, // a missing entry has no current value — nothing (not even <None>) wears the check
                assemblyQualifiedName => ApplyFix(assetPath, fileId, rid, assemblyQualifiedName));

        // Healthy / empty card: opens the live picker. The constraint and the pre-navigated current type are read from
        // the live property at the field path; a pick assigns / re-points the reference and <None> clears it, all
        // through Unity's own managed-reference serialization (see ApplyLive). A field the API cannot reach (e.g. an
        // unrecoverable path) opens an unconstrained picker and surfaces the failure on apply.
        private void OpenLivePicker(string assetPath, long fileId, string graphPath, AspidGradientButton anchor)
        {
            Type constraint = typeof(object);
            var currentAqn = string.Empty;

            if (TryResolveLiveProperty(assetPath, fileId, graphPath, out var serializedObject, out var property))
                using (serializedObject)
                {
                    constraint = SerializeReferenceHelpers.GetFieldType(property);
                    currentAqn = property.managedReferenceValue?.GetType().AssemblyQualifiedName ?? string.Empty;
                }

            TogglePicker(anchor, constraint, currentAqn,
                assemblyQualifiedName => ApplyLive(assetPath, fileId, graphPath, assemblyQualifiedName));
        }

        // The picker expands inline as an accordion welded into the clicked card, directly under its band — the same
        // selector view the Project References group picker hosts, attached the same way (the card frames it). One panel
        // at a time; the band's chevron flips ▼→▲ while open and clicking it again collapses it. Generic over the source
        // of truth: the caller supplies the candidate constraint, the type to pre-navigate to, and what a pick does
        // (a YAML fix for a missing card, a live edit for a healthy / empty one).
        private void TogglePicker(AspidGradientButton anchor, Type constraint, string currentAqn, Action<string> onSelected)
        {
            var wasOpen = _openPickerRow == anchor;
            ClosePicker();
            if (wasOpen) return;

            var baseType = constraint ?? typeof(object);

            var view = new TypeSelectorView(
                filter: new TypeSelectorFilter
                {
                    Types = new[] { baseType },
                    Predicate = SerializeReferenceHelpers.IsAssignableManagedReference,
                    AdditionalTypes = baseType == typeof(object) ? null : GenericTypeResolver.GetAssignableGenericDefinitions(baseType),
                    ArgumentFilter = SerializeReferenceHelpers.IsValidGenericArgument,
                },
                currentAqn: currentAqn, // null (no current-value concept) and "" (holds <None>) both pass through as-is
                onSelected: onSelected,
                onDismiss: ClosePicker);

            _openPicker = new AspidBox(AspidBoxPreset.Default.SetTheme(ThemeStyle.Type.Darkness))
                .AddClass(PickerClass)
                .AddChild(view);

            _openPickerRow = anchor;
            // Flip the band's collapse chevron to ▲ in place — glyph only, so the same swap works for the missing card's
            // "Fix Missing ▼" and the plain "▼" healthy / empty bands.
            if (anchor is not null) anchor.Text = anchor.Text.Replace(BandChevronCollapsed, BandChevronExpanded);

            // The band is a direct child of the card; drop the picker right below it inside the card — reading as a
            // dropdown welded under the band, with the bottom meta line shifting beneath it — mirroring the Project
            // Audit group picker. The ?? fallback keeps a sane target if the band is ever hosted outside a card.
            // On a migration or suggestion card the one-click row sits right under the band as its visual pair — the
            // picker slots in after it, so expanding the escape hatch never wedges itself between the two.
            var card = anchor?.parent;
            var container = card ?? _list;
            var insertAt = container.IndexOf(anchor) + 1;
            if (insertAt < container.childCount &&
                (container[insertAt].ClassListContains(NodeMigrateClass) ||
                 container[insertAt].ClassListContains(NodeSuggestClass)))
                insertAt++;
            container.InsertChild(insertAt, _openPicker);

            // The whole card becomes the active surface: it lights an accent frame (see __node--picking) and the
            // selector sheds its own box (see __picker--attached), so band, selector and meta line read as one active
            // card rather than a button stacked over a separate dropdown — exactly the Project References group behaviour.
            if (card is not null)
            {
                _openPickerCard = card;
                _openPickerCard.AddClass(NodePickingClass);
                _openPicker.AddClass(PickerAttachedClass);
            }

            view.FocusPicker();
        }

        private void ClosePicker()
        {
            _openPicker?.RemoveFromHierarchy();
            // Restore the band's resting chevron (▲→▼) in place, so the label — "Fix Missing" or the lone glyph — is
            // preserved across the swap.
            if (_openPickerRow is not null)
                _openPickerRow.Text = _openPickerRow.Text.Replace(BandChevronExpanded, BandChevronCollapsed);
            _openPickerCard?.RemoveClass(NodePickingClass);

            _openPicker = null;
            _openPickerRow = null;
            _openPickerCard = null;
        }

        private void ApplyFix(string assetPath, long fileId, long rid, string assemblyQualifiedName)
        {
            if (BlockedByOpenCopy(assetPath)) return;

            // <None> in the picker emits an empty name: the user wants the reference cleared, not re-pointed at a type —
            // so null it out (dropping the broken payload) instead of treating it as a no-op. Mirrors the Project
            // References group picker, where <None> clears the group. (Before this, an empty name fell through to the
            // null-type guard below and silently did nothing, so picking <None> on a missing card cleared nothing.)
            if (string.IsNullOrEmpty(assemblyQualifiedName))
            {
                ClearReference(assetPath, fileId, rid);
                return;
            }

            var type = Type.GetType(assemblyQualifiedName, throwOnError: false);
            if (type is null) return;

            // The missing entry lives in exactly one document — the one whose Fix button was clicked. Rewrite only that
            // document, identified by its captured file id: a rid is unique within a document but can collide across
            // documents, so looping the asset's documents could rewrite a healthy reference that merely shares the rid.
            if (!SerializeReferenceYamlEditor.TryRewriteType(assetPath, fileId, rid, ManagedTypeName.FromType(type)))
                return;

            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            SerializeReferenceRepairSuggestions.ClearCache();
            Rescan();
        }

        // Resets a missing managed reference to <None>: nulls every field / array-element pointer to the rid (to Unity's
        // null sentinel -2), drops the now-orphaned RefIds entry and adds the null sentinel entry when one is needed —
        // exactly what Unity writes for a cleared [SerializeReference] field. Reached by picking <None> in a missing
        // card's Fix picker (a missing reference cannot be cleared through the serialization API, so it is done in the
        // YAML). Confirmed and not undoable: the reference's broken payload is discarded, mirroring ClearOrphan. A shared
        // (aliased) reference nulls every field that points at it, so the confirm dialog names that count up front (the
        // rid is unrecoverable, so a per-slot detach isn't offered here). The captured file id targets exactly this
        // document's rid (rids collide across documents). Healthy / empty slots clear through the live path (see ApplyLive).
        private void ClearReference(string assetPath, long fileId, long rid)
        {
            if (BlockedByOpenCopy(assetPath)) return;

            // Name how many fields the clear will null so an aliased reference doesn't silently take down siblings the
            // user didn't realize shared the rid. A non-positive count means the pointers couldn't be located — fall
            // back to the unnumbered wording rather than print "0 fields".
            var fieldCount = SerializeReferenceYamlEditor.CountPointersTo(assetPath, fileId, rid);
            var pointerLine = fieldCount switch
            {
                1 => "This nulls the 1 field pointing at it",
                > 1 => $"This reference is aliased across {fieldCount} fields — clearing it nulls every one of them",
                _ => "This nulls every field pointing at it",
            };

            if (!EditorUtility.DisplayDialog(
                    "Clear Reference",
                    $"Reset this managed reference (rid {rid}) to <None> in\n{assetPath}?\n\n" +
                    $"{pointerLine} and discards its stored data. It edits the asset file directly and cannot be undone.",
                    "Clear", "Cancel"))
                return;

            if (!SerializeReferenceYamlEditor.TryNullReference(assetPath, fileId, rid)) return;

            // Surgical index patch via the import invalidator, not a full ClearCache (see ClearOrphan).
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            SerializeReferenceRepairSuggestions.ClearCache();
            Rescan();
        }

        // Assigns / re-points / clears a healthy or empty slot through Unity's live serialization, keyed by the field
        // path. Because the change goes through SerializedProperty.managedReferenceValue, Unity creates the RefIds entry
        // for a freshly assigned type, rewrites it for a re-point (carrying over matching fields), or removes it (and any
        // nested children) on <None> — exactly as the Inspector would, with none of the YAML hand-editing the missing /
        // orphan paths need. The asset is saved to disk so the disk-read graph reflects the edit on rescan. A path the
        // serialization API cannot reach (an orphan, a scene, or a field under a missing parent) is reported and skipped.
        private void ApplyLive(string assetPath, long fileId, string graphPath, string assemblyQualifiedName)
        {
            var type = string.IsNullOrEmpty(assemblyQualifiedName)
                ? null
                : Type.GetType(assemblyQualifiedName, throwOnError: false);

            // A non-empty name that fails to load is an unresolved pick, not a clear — leave the slot untouched rather
            // than silently nulling it.
            if (!string.IsNullOrEmpty(assemblyQualifiedName) && type is null) return;

            if (!TryResolveLiveProperty(assetPath, fileId, graphPath, out var serializedObject, out var property))
            {
                EditorUtility.DisplayDialog(
                    "Edit Reference",
                    "This slot cannot be edited here — its field is not reachable through the serialization API " +
                    "(it may be an orphan, live in a scene, or sit under a missing parent). Edit it in the Inspector " +
                    "or repair its parent first.",
                    "OK");
                return;
            }

            using (serializedObject)
            {
                var previous = property.managedReferenceValue;
                // type == null clears to <None>; a concrete type carries over the previous value's matching fields.
                property.SetManagedReferenceAndApply(SerializeReferenceHelpers.CreateInstancePreservingData(type, previous));
                property.isExpanded = type is not null;

                var target = serializedObject.targetObject;
                EditorUtility.SetDirty(target);
                PersistEdit(assetPath, target);
            }

            // PersistEdit's save triggers the import that lets the index invalidator patch this asset surgically —
            // no full ClearCache (see ClearOrphan).
            SerializeReferenceRepairSuggestions.ClearCache();
            SerializeReferenceYamlProbeCache.ClearCache();
            Rescan();
        }

        // A file rewrite is only safe when the asset is not loaded as a scene and not open in Prefab Mode — the open
        // in-memory copy would win on its next save, silently clobbering the fix (or resurrecting a cleared payload).
        // Same writable test the Project References bulk apply runs (IsEntryWritable); the graph blocks with a dialog
        // instead of splitting to an in-memory path, keeping this view's edits all-YAML — the drawer already covers
        // the open-asset repair story.
        private static bool BlockedByOpenCopy(string assetPath)
        {
            var openInScene = UnityEngine.SceneManagement.SceneManager.GetSceneByPath(assetPath).isLoaded;
            var stagePath = PrefabStageUtility.GetCurrentPrefabStage()?.assetPath;
            var openInPrefabMode = !string.IsNullOrEmpty(stagePath) &&
                                   string.Equals(stagePath, assetPath, StringComparison.Ordinal);

            if (!openInScene && !openInPrefabMode) return false;

            EditorUtility.DisplayDialog(
                "Asset References",
                "This asset is open " + (openInPrefabMode ? "in Prefab Mode" : "as a loaded scene") +
                " — a file rewrite would be overwritten by its next save.\n\n" +
                "Close it and rescan, or repair the field directly in the Inspector.",
                "OK");
            return true;
        }

        // Flushes a live edit to disk so the disk-read graph reflects it on the next rescan. A ScriptableObject (.asset)
        // persists through SaveAssetIfDirty; a prefab asset's component edit does not reliably flush through the generic
        // asset-dirty path (the prefab pipeline owns its serialization), so it goes through SavePrefabAsset on the
        // prefab's in-memory root — the same cached object graph the edit was applied to — which the docs define as
        // "save the version currently loaded in memory back to disk".
        private static void PersistEdit(string assetPath, Object target)
        {
            var prefabRoot = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (prefabRoot != null) PrefabUtility.SavePrefabAsset(prefabRoot);
            else AssetDatabase.SaveAssetIfDirty(target);
        }

        // Resolves the live SerializedObject document anchored at fileId and the managed-reference property at graphPath,
        // so a healthy / empty slot can be edited through Unity's serialization rather than a hand-rolled YAML rewrite.
        // graphPath is the graph's field path ("_alternates[2]", "_weapon._chargeEffect"); list indices are expanded to
        // Unity's ".Array.data[i]" form. The caller disposes the returned SerializedObject. Returns false for a path the
        // API cannot reach (an empty path, a scene asset, or a field nested under a missing/null parent).
        private static bool TryResolveLiveProperty(string assetPath, long fileId, string graphPath,
            out SerializedObject serializedObject, out SerializedProperty property)
        {
            serializedObject = null;
            property = null;

            if (string.IsNullOrEmpty(graphPath)) return false;
            // Scenes are not loadable through LoadAllAssetsAtPath (see SerializeReferenceHelpers.IsScene).
            if (SerializeReferenceHelpers.IsScene(assetPath)) return false;

            var propertyPath = ToSerializedPropertyPath(graphPath);

            foreach (var obj in AssetDatabase.LoadAllAssetsAtPath(assetPath))
            {
                if (obj == null) continue;
                if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(obj, out _, out var id) || id != fileId) continue;

                var serialized = new SerializedObject(obj);
                var found = serialized.FindProperty(propertyPath);
                if (found is { propertyType: SerializedPropertyType.ManagedReference })
                {
                    serializedObject = serialized;
                    property = found;
                    return true;
                }

                // The document matched but the path did not resolve to a managed reference — no other document shares
                // this file id, so bail rather than scan on.
                serialized.Dispose();
                return false;
            }

            return false;
        }

        // Expands the graph's field path into a Unity SerializedProperty path: a list / array index "name[i]" becomes
        // "name.Array.data[i]" (managed-reference and plain nested fields keep their dotted names). The inverse of the
        // ".Array.data" stripping SerializeReferenceYamlEditor does when it normalises a property path.
        private static string ToSerializedPropertyPath(string graphPath) =>
            Regex.Replace(graphPath, @"\[(\d+)\]", ".Array.data[$1]");

        // Recovers the declared field type backing rid so the Fix picker is constrained the same way the Repair window
        // constrains its own. The asset's whole managed-reference constraint map (keyed by document file id + rid) is
        // built once, cached per asset path (so K missing cards on one asset share a single scan rather than re-scanning
        // per picker open) and looked up by the owning document's exact (fileId, rid) key, so a rid that collides across
        // documents resolves to this document's field type rather than another's. The cache is dropped on every Rescan /
        // apply so a fix's rewritten YAML is re-read. Returns null (unconstrained) when no field points at the rid (an
        // orphaned payload) or the field type is unresolvable.
        private Type ResolveConstraint(string assetPath, long fileId, long rid)
        {
            if (!_constraintCache.TryGetValue(assetPath, out var map))
            {
                map = SerializeReferenceHelpers.BuildConstraintMap(assetPath);
                _constraintCache[assetPath] = map;
            }

            return map.TryGetValue((fileId, rid), out var constraint) ? constraint : null;
        }

        // Future work: a "Make unique" action on a SHARED node — cloning the aliased reference so the two fields no
        // longer affect each other (mirrors SerializeReferenceHelpers.MakeReferenceUnique, which operates on a live
        // SerializedProperty). Beyond that, the cards are editable in place: missing references via the YAML Fix, healthy
        // and empty slots via the live picker (assign / re-point / clear to <None>).
    }
}
