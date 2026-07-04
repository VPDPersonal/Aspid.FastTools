using System;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Aspid.FastTools.Editors;
using Aspid.FastTools.UIElements;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using Aspid.FastTools.Types.Editors;
using System.Text.RegularExpressions;
using Aspid.FastTools.UIElements.Editors.Internal;
using System.Linq;
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
        private const string BadgeSharedClass = BadgeClass + "--shared";

        private const string ChipClass = RootClass + "__chip";
        private const string ClearOrphanClass = RootClass + "__clear-orphan";
        private const string OrphanGroupClass = RootClass + "__orphan-group";
        private const string OrphanGroupHeaderClass = RootClass + "__orphan-group-header";
        private const string PickerClass = RootClass + "__picker";
        private const string PickerAttachedClass = PickerClass + "--attached";

        // Band verb + collapse chevron; TogglePicker / ClosePicker swap the chevron glyph alone, never the label.
        private const string FixCollapsedText = "Fix Missing  ▼";
        private const string ChangeCollapsedText = "Change  ▼";
        private const string AssignCollapsedText = "Assign  ▼";

        // A pending-migration card is not missing (Unity migrates it in memory; only the file is stale), so no "Missing".
        private const string MigrateFixCollapsedText = "Fix  ▼";
        private const char BandChevronCollapsed = '▼';
        private const char BandChevronExpanded = '▲';

        // Single-sourced from the picker's "<None>" option so an empty slot reads like a cleared field in the Inspector.
        private const string EmptySlotText = TypeSelectorHelpers.NoneOption;

        private const string DocumentChevronExpanded = "▼";
        private const string DocumentChevronCollapsed = "▶";

        // Reports this view's state-tone to the host window, which owns the shared dotted canvas behind every mode.
        private readonly Action<Color> _onCanvasTone;

        // Reports a target change to the host window: it rebuilds this view from its cached target on every tab switch,
        // so without this an in-view pick would be dropped on the next return to this tab.
        private readonly Action<Object> _onTargetChanged;

        private Object _target;
        private readonly ObjectField _assetField;
        private readonly VisualElement _empty;
        private readonly VisualElement _overview;
        private readonly AspidLabel _overviewTitle;
        private readonly Label _overviewHint;
        private readonly VisualElement _list;

        private VisualElement _openPicker;
        private AspidGradientButton _openPickerRow;
        private VisualElement _openPickerCard;

        // Per-asset constraint map cache: BuildConstraintMap does a LoadAllAssetsAtPath + full SerializedObject walk,
        // so each Fix-Missing picker open must not re-scan. Cleared on every Rescan / apply so rewritten YAML is re-read.
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

            // The field is hosted inside the Rescan button: swallow its presses so opening the object picker or
            // dragging an asset in doesn't bubble to the button's Clickable and re-run Rescan.
            _assetField.RegisterCallback<PointerDownEvent>(evt => evt.StopPropagation());

            var rescanButton = new AspidGradientButton("Rescan", _ => Rescan())
                .AddClass(RescanClass);
            rescanButton.AddTrailingContent(_assetField);
            rescanButton.FillWithTrailingContent();

            var card = new AspidBox(AspidBoxPreset.Default.SetTheme(ThemeStyle.Type.Darkness))
                .AddClass(CardClass)
                .AddChild(cardTitle)
                .AddChild(cardDescription)
                .AddChild(rescanButton);

            _empty = new VisualElement().AddClass(EmptyClass);

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

            // One scroll spans the whole view, so the card and overview scroll away with the document list rather than
            // staying pinned above a separately-scrolling list.
            var content = new VisualElement()
                .AddClass(ContentClass)
                .AddChild(card)
                .AddChild(_empty)
                .AddChild(_overview)
                .AddChild(_list);

            var scroll = new ScrollView().AddClass(ScrollClass);
            scroll.AddChild(content);

            root.AddChild(scroll);

            Rescan();
        }

        private void SetTarget(Object target)
        {
            _target = target;
            // Mirror the pick back to the host so its cached target follows; the host just stores it (no rebuild),
            // so this never re-enters.
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
            // Drop the constraint maps so a rescan after a fix / clear re-reads the rewritten YAML, not a stale map.
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
                        _ => SetTarget(AssetDatabase.LoadAssetAtPath<Object>(sourcePath))));
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

            // Empty (unassigned) slots are tallied separately: they are not broken, so they never tip the
            // headline / canvas to amber — they only surface in the dim hint.
            var total = 0;
            var missing = 0;
            var orphans = 0;
            var empties = 0;
            var migrations = 0;

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

        // Ranked Smart Fix for a missing node, via the shared per-(path, fileId, rid) cache so a rescan and the
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
        // type reads as a pending migration. An unrecoverable constraint lets the migration through.
        private bool IsPendingMigration(string assetPath, long fileId, long rid, ManagedTypeName storedType, out Type target)
        {
            if (!SerializeReferenceMovedFromResolver.TryResolve(storedType, out target)) return false;

            var constraint = ResolveConstraint(assetPath, fileId, rid);
            return constraint is null || constraint == typeof(object) || constraint.IsAssignableFrom(target);
        }

        // Used only for the overview hint; empty slots are not "issues".
        private static int CountEmptySlots(ReferenceGraphDocument document)
        {
            var count = document.Roots.Count(root => root.IsEmpty);

            foreach (var pair in document.Edges)
            {
                count += pair.Value.Count(edge => edge.IsEmpty);
            }

            return count;
        }

        // Splits a document's unresolved nodes into genuinely broken ones and pending [MovedFrom] migrations. An
        // orphaned rid always counts as broken — nothing loads an orphan, so in-memory migration does not apply.
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

        // The same missing-predicate the amber tint uses; drives the missing-first root ordering in BuildDocument.
        private static bool RootIsMissing(ReferenceGraphDocument document, long rid)
        {
            var node = document.FindNode(rid);
            return node is { Resolves: false } && !node.Value.StoredType.IsEmpty;
        }

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
            // The overview stays hidden here; only the document-graph path (Rescan) re-shows it, so the
            // prefab-instance branch that reuses ShowResults keeps the missing-reference headline suppressed.
            HideOverview();
            _empty.AddClass(EmptyHiddenClass);
            _list.RemoveClass(ListHiddenClass);
        }

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
                ? broken == 1 ? "1 missing reference" : $"{broken} missing references"
                : orphans > 0
                    ? orphans == 1 ? "1 orphaned reference" : $"{orphans} orphaned references"
                    : migrations > 0
                        ? migrations == 1 ? "1 pending migration" : $"{migrations} pending migrations"
                        : "No missing references";

            _overviewTitle.LabelStatus = status;
            _overviewTitle.LineStatus = status;

            _overviewHint.text = BuildOverviewHint(total, missing, orphans, empties, migrations);
            _overview.RemoveClass(OverviewHiddenClass);
        }

        private static string BuildOverviewHint(int total, int missing, int orphans, int empties, int migrations)
        {
            var references = total == 1 ? "1 managed reference" : $"{total} managed references";
            var emptyNote = empties switch
            {
                0 => string.Empty,
                1 => " · 1 unassigned field",
                _ => $" · {empties} unassigned fields"
            };

            if (missing == 0 && orphans == 0)
                return $"{references} mapped{emptyNote} — every [SerializeReference] type resolves.";

            var broken = missing - migrations;

            var parts = new List<string>(4);
            if (broken > 0) parts.Add(broken == 1 ? "1 missing type" : $"{broken} missing types");
            if (migrations > 0) parts.Add(migrations == 1 ? "1 pending [MovedFrom] migration" : $"{migrations} pending [MovedFrom] migrations");
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

        // One serialized object document: a collapsible header band over a flat stack of node cards (nesting is read
        // from each card's field path, not indentation) plus a trailing "Orphaned" group. The header is dropped for a
        // single-document asset — there it would only restate the ObjectField above it.
        private VisualElement BuildDocument(string assetPath, ReferenceGraphDocument document, bool showHeader)
        {
            // Pending migrations are not issues — a document whose only findings are migrations keeps the calm
            // header, matching the info-toned overview; orphans and genuinely broken nodes still glow amber.
            var (broken, migrations) = CountUnresolved(assetPath, document);
            var hasIssues = document.Orphans.Count > 0 || broken > 0;

            var body = new VisualElement().AddClass(DocumentBodyClass);

            // Missing roots render first. Two passes over the asset's field order keep the partition stable between
            // rescans; empty (unassigned) roots are not missing, so they fall to the second pass.
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

            // Single-document asset: no header band — the ObjectField above already names it. Always expanded.
            if (!showHeader)
                return new VisualElement().AddClass(DocumentClass).AddChild(body);

            // The self-reference lets the click handler flip its own chevron alongside toggling the body.
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

            // Ignored for picking so clicks fall through to the band's own handler.
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

        // A pending [MovedFrom] migration is named as such so the header never contradicts the overview's "0 missing".
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

        // Appends a node's card and, recursively, its children's as flat siblings — nesting is carried by the threaded
        // field path, not the layout. The visited set makes the walk cycle-safe: a rid already on the current path
        // renders as a back-edge leaf instead of recursing forever.
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

        private static string CombinePath(string parent, string child)
        {
            if (string.IsNullOrEmpty(child)) return parent;
            return string.IsNullOrEmpty(parent) ? child : $"{parent}.{child}";
        }

        // A node card whose band is an inline dropdown: a missing card edits through the YAML, a healthy one through
        // the live serialization API, an orphan keeps a static band plus a footer Clear. Cards are not indented —
        // the field path alone carries the nesting.
        private VisualElement BuildNodeCard(string assetPath, ReferenceGraphDocument document, ReferenceGraphNode? node, long rid, string pathLabel, bool isOrphan)
        {
            var missing = node is { Resolves: false } && !node.Value.StoredType.IsEmpty;

            // An authoritative [MovedFrom] rename is a pending migration, not a breakage: Unity loads the reference
            // fine — only this file still stores the old name. Never for an orphan — nothing loads an orphan, so the
            // in-memory migration argument does not hold.
            Type migrationTarget = null;
            var isMigration = missing && !isOrphan &&
                IsPendingMigration(assetPath, document.FileId, rid, node.Value.StoredType, out migrationTarget);

            var card = new AspidBox(AspidBoxPreset.Default.SetTheme(ThemeStyle.Type.Darkness))
                .AddClass(NodeClass);

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

            // No MISSING badge — the band action and amber type pill already carry it; only SHARED remains.
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
                // The captured file id targets the rewrite at exactly this document's rid (rids collide across docs).
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
                    // The same YAML rewrite a picker pick performs — no confirm, matching the picker's own apply.
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
                    // Safe to hand straight to ApplyFix: Rank's pool is constraint-filtered, so the suggestion is
                    // always a type the picker itself would offer.
                    var suggest = new AspidGradientButton(SerializeReferenceHelpers.GetSuggestionLabel(suggestion),
                            _ => ApplyFix(assetPath, fileId, rid, suggestion.Type.AssemblyQualifiedName))
                        .AddClass(NodeSuggestClass);
                    suggest.tooltip = SerializeReferenceHelpers.GetSuggestionDetail(suggestion);
                    card.AddChild(suggest);
                }
            }
            else if (!isOrphan)
            {
                // A healthy reference edits through the live serialization API (keyed by the field path), so Unity
                // rewrites — or, on <None>, removes — the RefIds entry exactly as the Inspector would.
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

            // Healthy and empty slots are cleared through their band's picker (<None>), so no separate button here.
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

        // An unassigned [SerializeReference] slot — a field whose pointer is the null sentinel (rid -2). Its band is
        // still a dropdown assigning a type through the live serialization API; a slot whose field path could not be
        // recovered stays static (nothing to target).
        private VisualElement BuildEmptySlotCard(string assetPath, long fileId, string pathLabel)
        {
            var card = new AspidBox(AspidBoxPreset.Default.SetTheme(ThemeStyle.Type.Darkness))
                .AddClass(NodeClass)
                .AddClass(NodeEmptyClass);

            // A plain Label so the --empty USS rule tints it, rather than an AspidLabel painting its own status colour.
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
                // <None> is a no-op here — the slot is already unset.
                var graphPath = pathLabel;
                AspidGradientButton band = null;
                band = new AspidGradientButton(AssignCollapsedText, _ => OpenLivePicker(assetPath, fileId, graphPath, band))
                    .AddClass(NodeBandClass);
                band.AddLeadingContent(bandRow);
                card.AddChild(band);
            }

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

        // Warning-tinted group for rids no root reaches. Each orphan is a full node card (so a missing orphan is still
        // fixable inline) with a footer Clear, without recursion into children.
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

        // Missing card: opens the YAML fix picker, constrained to the rid's declared field type so a repair cannot
        // pick an incompatible type that would null on import; an unresolvable field type falls back to unconstrained.
        private void OpenMissingPicker(string assetPath, long fileId, long rid, AspidGradientButton anchor) =>
            TogglePicker(anchor, ResolveConstraint(assetPath, fileId, rid),
                currentAqn: null, // a missing entry has no current value — nothing (not even <None>) wears the check
                assemblyQualifiedName => ApplyFix(assetPath, fileId, rid, assemblyQualifiedName));

        // Healthy / empty card: constraint and current type are read from the live property at the field path. A field
        // the API cannot reach opens an unconstrained picker and surfaces the failure on apply.
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

        // The picker expands inline under the clicked card's band, one panel at a time. Generic over the source of
        // truth: the caller supplies the constraint, the type to pre-navigate to, and what a pick does.
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
            // Glyph-only swap (▼→▲) so the same replace works for every band label.
            if (anchor is not null) anchor.Text = anchor.Text.Replace(BandChevronCollapsed, BandChevronExpanded);

            // Drop the picker right below the band inside the card (the ?? fallback keeps a sane target if the band is
            // ever hosted outside a card). On a migration / suggestion card the one-click row stays welded under the
            // band, so the picker slots in after it.
            var card = anchor?.parent;
            var container = card ?? _list;
            var insertAt = container.IndexOf(anchor) + 1;
            if (insertAt < container.childCount &&
                (container[insertAt].ClassListContains(NodeMigrateClass) ||
                 container[insertAt].ClassListContains(NodeSuggestClass)))
                insertAt++;
            container.InsertChild(insertAt, _openPicker);

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
            // Glyph-only swap (▲→▼) so the band's label is preserved.
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

            // <None> emits an empty name: clear the reference (dropping the broken payload) rather than letting it
            // fall through to the null-type guard below as a silent no-op.
            if (string.IsNullOrEmpty(assemblyQualifiedName))
            {
                ClearReference(assetPath, fileId, rid);
                return;
            }

            var type = Type.GetType(assemblyQualifiedName, throwOnError: false);
            if (type is null) return;

            // Rewrite only the captured file id's document: a rid is unique within a document but can collide across
            // documents, so looping the asset's documents could rewrite a healthy reference that shares the rid.
            if (!SerializeReferenceYamlEditor.TryRewriteType(assetPath, fileId, rid, ManagedTypeName.FromType(type)))
                return;

            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            SerializeReferenceRepairSuggestions.ClearCache();
            Rescan();
        }

        // Resets a missing reference to <None> in the YAML (a missing reference cannot be cleared through the
        // serialization API): nulls every pointer to Unity's null sentinel (-2) and drops the RefIds entry — exactly
        // what Unity writes for a cleared field. Confirmed and not undoable; the broken payload is discarded.
        private void ClearReference(string assetPath, long fileId, long rid)
        {
            if (BlockedByOpenCopy(assetPath)) return;

            // Name how many fields the clear will null so an aliased reference doesn't silently take down siblings.
            // A non-positive count means the pointers couldn't be located — use the unnumbered wording, not "0 fields".
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

        // Edits a healthy / empty slot through SerializedProperty.managedReferenceValue, so Unity creates / rewrites /
        // removes the RefIds entry exactly as the Inspector would. The asset is saved to disk so the disk-read graph
        // reflects the edit on rescan; a path the API cannot reach is reported and skipped.
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
        // in-memory copy would win on its next save, silently clobbering the fix. Same test as IsEntryWritable.
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

        // A prefab component edit does not reliably flush through the generic asset-dirty path (the prefab pipeline
        // owns its serialization), so prefabs save via SavePrefabAsset on the in-memory root; anything else via
        // SaveAssetIfDirty.
        private static void PersistEdit(string assetPath, Object target)
        {
            var prefabRoot = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (prefabRoot != null) PrefabUtility.SavePrefabAsset(prefabRoot);
            else AssetDatabase.SaveAssetIfDirty(target);
        }

        // Resolves the live document at fileId and the managed-reference property at graphPath (list indices expanded
        // to Unity's ".Array.data[i]" form). The caller disposes the returned SerializedObject; returns false for a
        // path the API cannot reach (an empty path, a scene asset, or a field under a missing/null parent).
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

        // "name[i]" becomes Unity's "name.Array.data[i]" — the inverse of the ".Array.data" stripping
        // SerializeReferenceYamlEditor does when it normalises a property path.
        private static string ToSerializedPropertyPath(string graphPath) =>
            Regex.Replace(graphPath, @"\[(\d+)\]", ".Array.data[$1]");

        // Recovers the declared field type backing rid through the per-asset constraint-map cache (one scan shared by
        // every picker open), keyed by exact (fileId, rid) since rids collide across documents. Returns null
        // (unconstrained) for an orphaned payload or an unresolvable field type.
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
        // longer affect each other (mirrors SerializeReferenceHelpers.MakeReferenceUnique).
    }
}
