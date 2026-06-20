using System;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Aspid.FastTools.Types;
using Aspid.FastTools.UIElements;
using System.Collections.Generic;
using Aspid.FastTools.Types.Editors;
using Aspid.FastTools.UIElements.Editors.Internal;
using Object = UnityEngine.Object;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    /// <summary>
    /// Asset-level visualiser for <c>[SerializeReference]</c> managed-reference graphs. For each serialized object
    /// document in the asset it draws the reference tree — field-pointer roots, their nested children, shared
    /// (aliased) references and orphaned payloads — straight from the YAML, so it surfaces references at any nesting
    /// depth and the orphans the Inspector cannot navigate to. The tree is read-only except that a missing reference
    /// can be re-pointed in place through the same embedded type picker the Repair window uses.
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
        private const string NodeBandRowClass = RootClass + "__node-band-row";
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

        private const string FixCollapsedText = "Fix Missing  ▼";
        private const string FixExpandedText = "Fix Missing  ▲";

        // An unassigned [SerializeReference] slot's placeholder label — single-sourced from the picker's own "<None>"
        // option, so an empty slot reads the same way in the graph as the cleared field does in the Inspector.
        private const string EmptySlotText = TypeSelectorHelpers.NoneOption;

        // The document header's collapse chevron: ▼ while the body is shown, ▶ once collapsed.
        private const string DocumentChevronExpanded = "▼";
        private const string DocumentChevronCollapsed = "▶";

        // Indentation step (px) applied per tree depth so nested cards read as a hierarchy.
        private const float IndentStep = 16f;

        // Reports this view's state-tone to the host window, which owns the shared dotted canvas behind every mode.
        private readonly Action<Color> _onCanvasTone;

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

        public SerializeReferenceGraphView(Object target, Action<Color> onCanvasTone)
        {
            _target = target;
            _onCanvasTone = onCanvasTone;

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
            // Open() retargets an already-open window, so the field must follow the new target — without notifying,
            // or the change callback would trigger a second scan.
            _assetField?.SetValueWithoutNotify(target);
            if (_list is not null) Rescan();
        }

        private void Rescan()
        {
            if (_list is null) return;

            ClosePicker();
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

            var documents = SerializeReferenceGraphScanner.Build(assetPath);
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

            // The per-document header only earns its place when there is more than one document to tell apart (a
            // multi-component prefab); a single document drops it (see BuildDocument).
            var showHeaders = documents.Count > 1;
            foreach (var document in documents)
            {
                _list.AddChild(BuildDocument(assetPath, document, showHeaders));

                total += document.Nodes.Count;
                foreach (var node in document.Nodes)
                    if (!node.Resolves && !node.StoredType.IsEmpty) missing++;
                orphans += document.Orphans.Count;
                empties += CountEmptySlots(document);
            }

            ShowOverview(total, missing, orphans, empties);

            _onCanvasTone?.Invoke(missing > 0 || orphans > 0
                ? SerializeReferenceCanvasStyle.Warning
                : SerializeReferenceCanvasStyle.Success);
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

        // A document has issues when it carries any orphaned rid or any unresolved (missing-type) node.
        private static bool DocumentHasIssues(ReferenceGraphDocument document)
        {
            if (document.Orphans.Count > 0) return true;

            foreach (var node in document.Nodes)
                if (!node.Resolves && !node.StoredType.IsEmpty) return true;

            return false;
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
        // missing count first, then orphaned rids, falling back to a green all-clear; the label and its divider flip
        // between Warning (amber) and Success (green) in place, and the dim hint spells out the full breakdown.
        private void ShowOverview(int total, int missing, int orphans, int empties)
        {
            // Only missing / orphaned references are "issues" that tip the headline and divider to amber; empty slots
            // are unassigned, not broken, so a graph whose only annotation is empty slots still reads green.
            var status = missing > 0 || orphans > 0
                ? StatusStyle.Type.Warning
                : StatusStyle.Type.Success;

            _overviewTitle.Text = missing > 0
                ? (missing == 1 ? "1 missing reference" : $"{missing} missing references")
                : orphans > 0
                    ? (orphans == 1 ? "1 orphaned reference" : $"{orphans} orphaned references")
                    : "No missing references";

            _overviewTitle.LabelStatus = status;
            _overviewTitle.LineStatus = status;

            _overviewHint.text = BuildOverviewHint(total, missing, orphans, empties);
            _overview.RemoveClass(OverviewHiddenClass);
        }

        // The overview's dim subtitle: the mapped reference count, annotated with the missing / orphaned / unassigned
        // tallies and a one-line cue toward the matching inline action — or a clean bill of health when nothing is
        // broken (an unassigned-only graph still reads clean, with the empty count appended as a quiet note).
        private static string BuildOverviewHint(int total, int missing, int orphans, int empties)
        {
            var references = total == 1 ? "1 managed reference" : $"{total} managed references";
            var emptyNote = empties == 0
                ? string.Empty
                : empties == 1 ? " · 1 unassigned field" : $" · {empties} unassigned fields";

            if (missing == 0 && orphans == 0)
                return $"{references} mapped{emptyNote} — every [SerializeReference] type resolves.";

            var parts = new List<string>(3);
            if (missing > 0) parts.Add(missing == 1 ? "1 missing type" : $"{missing} missing types");
            if (orphans > 0) parts.Add(orphans == 1 ? "1 orphaned rid" : $"{orphans} orphaned rids");
            if (empties > 0) parts.Add(empties == 1 ? "1 unassigned field" : $"{empties} unassigned fields");

            var action = missing > 0
                ? "Fix a missing type inline from its card."
                : "Clear an orphaned rid from its card.";

            return $"{references} mapped · {string.Join(" · ", parts)}. {action}";
        }

        private void HideOverview() => _overview?.AddClass(OverviewHiddenClass);

        // One serialized object document: a clickable header band (styled like the Project References group header — a
        // gradient row carrying the component / ScriptableObject name, a reference count and a collapse chevron) over a
        // collapsible body that holds each root's reference subtree as a stack of separate node cards (indented by
        // depth) plus a trailing "Orphaned" group for any rids no root reaches. The header is dropped when the asset
        // has a single document (see showHeader) — there it would only restate the ObjectField above it.
        private VisualElement BuildDocument(string assetPath, ReferenceGraphDocument document, bool showHeader)
        {
            var hasIssues = DocumentHasIssues(document);

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
                AppendNode(body, assetPath, document, root.Rid, root.Label, depth: 0, visited);
            }

            foreach (var root in document.Roots)
            {
                if (root.IsEmpty)
                {
                    body.AddChild(BuildEmptySlotCard(root.Label, depth: 0));
                    continue;
                }

                if (RootIsMissing(document, root.Rid)) continue;
                var visited = new HashSet<long>();
                AppendNode(body, assetPath, document, root.Rid, root.Label, depth: 0, visited);
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
                .AddChild(new Label(BuildDocumentCountText(document))
                    .AddClass(DocumentCountClass)
                    .SetPickingMode(PickingMode.Ignore)));

            return new VisualElement()
                .AddClass(DocumentClass)
                .AddChild(header)
                .AddChild(body);
        }

        // The header's dim subtitle: total managed-reference count, annotated with the missing / orphaned tallies when
        // present — the same "N · M" two-part shape the Project References group header uses.
        private static string BuildDocumentCountText(ReferenceGraphDocument document)
        {
            var total = document.Nodes.Count;
            var missing = 0;
            foreach (var node in document.Nodes)
                if (!node.Resolves && !node.StoredType.IsEmpty) missing++;

            var orphans = document.Orphans.Count;

            var text = total == 1 ? "1 reference" : $"{total} references";
            if (missing > 0) text += $" · {missing} missing";
            if (orphans > 0) text += orphans == 1 ? " · 1 orphaned" : $" · {orphans} orphaned";
            return text;
        }

        // Appends a node's card (indented by depth) and then, recursively, its children's cards as further-indented
        // siblings — a flat, scannable stack rather than nested boxes. The full field path is threaded down: each child
        // joins its own field path (relative to the parent's data block) onto the parent's path, so a nested reference
        // shows where it lives from the document root (e.g. "_primaryWeapon._chargeEffect"). An empty child slot renders
        // as an "<None>" leaf and never recurses. The visited set makes the walk cycle-safe: a rid already on the
        // current path renders as a back-edge leaf ("↩ rid N") instead of recursing forever.
        private void AppendNode(VisualElement container, string assetPath, ReferenceGraphDocument document, long rid, string pathLabel, int depth, HashSet<long> visited)
        {
            if (!visited.Add(rid))
            {
                container.AddChild(BuildBackEdgeCard(depth, rid));
                return;
            }

            var node = document.FindNode(rid);
            container.AddChild(BuildNodeCard(assetPath, document, node, rid, pathLabel, depth, isOrphan: false));

            foreach (var edge in document.ChildrenOf(rid))
            {
                var childPath = CombinePath(pathLabel, edge.Label);
                if (edge.IsEmpty)
                    container.AddChild(BuildEmptySlotCard(childPath, depth + 1));
                else
                    AppendNode(container, assetPath, document, edge.Rid, childPath, depth + 1, visited);
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
        // missing / orphaned, a quiet light label otherwise) with the MISSING / SHARED badges beside it and, for a
        // missing reference, the inline Fix dropdown docked to the right — the whole band toggles the picker. Bottom
        // line: the dim field path the reference sits under, then the rid; an orphan card adds a Clear action here.
        // Indented by depth so the stack still reads as a tree. The Fix band and the Clear button are the only
        // interactive parts.
        private VisualElement BuildNodeCard(string assetPath, ReferenceGraphDocument document, ReferenceGraphNode? node, long rid, string pathLabel, int depth, bool isOrphan)
        {
            var missing = node is { Resolves: false } && !node.Value.StoredType.IsEmpty;

            var card = new AspidBox(AspidBoxPreset.Default.SetTheme(ThemeStyle.Type.Darkness))
                .AddClass(NodeClass);
            card.style.marginLeft = depth * IndentStep;
            if (isOrphan) card.AddClass(NodeOrphanClass);

            // Top band — type identity + status badges on the left, the Fix action docked right.

            // The type name drives its own colour through an Aspid status label: a missing / orphaned type wears the
            // same amber pill the Project References group header uses; a healthy type stays a quiet light label.
            var typePreset = AspidLabelPreset.Default
                .SetLabelSize(AspidLabelSizeStyle.Type.H5)
                .SetLineSize(AspidDividingLineSizeStyle.Type.None);
            typePreset = missing || isOrphan
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
                if (SerializeReferenceSettings.RidColorsEnabled)
                {
                    var chip = new VisualElement().AddClass(ChipClass);
                    chip.style.backgroundColor = SerializeReferenceRidColor.ForRid(rid);
                    shared.AddChild(chip);
                }

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
                var fileId = document.FileId;
                AspidGradientButton band = null;
                band = new AspidGradientButton(FixCollapsedText, _ => TogglePicker(assetPath, fileId, rid, band))
                    .AddClass(NodeBandClass);
                band.AddLeadingContent(bandRow);
                card.AddChild(band);
            }
            else
            {
                card.AddChild(bandRow);
            }

            // Bottom line — the dim field path, the rid, and (for an orphan) the Clear action.
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
        private static VisualElement BuildBackEdgeCard(int depth, long rid)
        {
            var card = new AspidBox(AspidBoxPreset.Default.SetTheme(ThemeStyle.Type.Darkness))
                .AddClass(NodeClass)
                .AddClass(NodeBackEdgeClass);
            card.style.marginLeft = depth * IndentStep;

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
        // path — but with no badges, no Fix and no recursion: there is nothing to repair here, only an empty field to
        // fill in the Inspector. Indented by depth so it still reads in its tree position.
        private static VisualElement BuildEmptySlotCard(string pathLabel, int depth)
        {
            var card = new AspidBox(AspidBoxPreset.Default.SetTheme(ThemeStyle.Type.Darkness))
                .AddClass(NodeClass)
                .AddClass(NodeEmptyClass);
            card.style.marginLeft = depth * IndentStep;

            // The placeholder type label — a quiet dim "<None>". A plain Label (like a back-edge leaf), so the --empty
            // USS rule tints it dim italic rather than an AspidLabel painting its own status colour.
            var typeLabel = new Label(EmptySlotText)
                .AddClass(NodeTypeClass)
                .SetPickingMode(PickingMode.Ignore);

            var bandRow = new VisualElement()
                .AddClass(NodeBandRowClass)
                .AddChild(typeLabel);
            bandRow.pickingMode = PickingMode.Ignore;
            card.AddChild(bandRow);

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
                group.AddChild(BuildNodeCard(assetPath, document, node, node.Rid, pathLabel: null, depth: 0, isOrphan: true));
            }

            return group;
        }

        // Removes a single orphaned RefIds entry from the saved asset. Re-derives the orphan set fresh (the on-screen
        // graph may be stale) and only removes a rid that is still genuinely orphaned, then reimports and rescans.
        private void ClearOrphan(string assetPath, long fileId, long rid)
        {
            if (!EditorUtility.DisplayDialog(
                    "Drop Orphaned Entry",
                    $"Remove the orphaned managed-reference entry (rid {rid}) from\n{assetPath}?\n\n" +
                    "This edits the asset file directly and cannot be undone.",
                    "Remove", "Cancel"))
                return;

            // Guard against a stale graph: confirm the rid is still an orphan against a fresh scan before deleting.
            var stillOrphan = false;
            foreach (var document in SerializeReferenceGraphScanner.Build(assetPath))
                if (document.FileId == fileId && document.Orphans.Contains(rid)) { stillOrphan = true; break; }

            if (!stillOrphan)
            {
                Rescan();
                return;
            }

            if (!SerializeReferenceYamlEditor.TryRemoveEntry(assetPath, fileId, rid)) return;

            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            SerializeReferenceRepairSuggestions.ClearCache();
            SerializeReferenceTypeUsageIndex.ClearCache();
            Rescan();
        }

        // The picker expands inline as an accordion welded into the clicked node's card, directly under its Fix band —
        // the same selector view the Project References group picker hosts, attached the same way (the card frames it). One
        // panel at a time; the Fix cue flips to ▲ while open and clicking it again collapses it. The candidate list is
        // constrained to the rid's declared
        // field type (recovered from the asset's managed-reference fields), so a repair cannot pick an incompatible
        // type that would null on import; a rid whose field type is unresolvable falls back to an unconstrained picker.
        private void TogglePicker(string assetPath, long fileId, long rid, AspidGradientButton fixButton)
        {
            var wasOpen = _openPickerRow == fixButton;
            ClosePicker();
            if (wasOpen) return;

            var constraint = ResolveConstraint(assetPath, fileId, rid);
            var baseType = constraint ?? typeof(object);

            var view = new TypeSelectorView(
                types: new[] { baseType },
                currentAqn: string.Empty,
                allow: TypeAllow.None,
                onSelected: assemblyQualifiedName => ApplyFix(assetPath, fileId, rid, assemblyQualifiedName),
                filter: SerializeReferenceHelpers.IsAssignableManagedReference,
                additionalTypes: baseType == typeof(object) ? null : GenericTypeResolver.GetAssignableGenericDefinitions(baseType),
                argumentFilter: SerializeReferenceHelpers.IsValidGenericArgument,
                onDismiss: ClosePicker);

            _openPicker = new AspidBox(AspidBoxPreset.Default.SetTheme(ThemeStyle.Type.Darkness))
                .AddClass(PickerClass)
                .AddChild(view);

            _openPickerRow = fixButton;
            if (fixButton is not null) fixButton.Text = FixExpandedText;

            // The Fix band is a direct child of the node card; drop the picker right below it inside the card — reading
            // as a dropdown welded under the band, with the bottom meta line shifting beneath it — mirroring the Project
            // Audit group picker. The ?? fallback keeps a sane target if the band is ever hosted outside a card.
            var card = fixButton?.parent;
            var container = card ?? _list;
            container.InsertChild(container.IndexOf(fixButton) + 1, _openPicker);

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
            if (_openPickerRow is not null) _openPickerRow.Text = FixCollapsedText;
            _openPickerCard?.RemoveClass(NodePickingClass);

            _openPicker = null;
            _openPickerRow = null;
            _openPickerCard = null;
        }

        private void ApplyFix(string assetPath, long fileId, long rid, string assemblyQualifiedName)
        {
            var type = string.IsNullOrEmpty(assemblyQualifiedName)
                ? null
                : Type.GetType(assemblyQualifiedName, throwOnError: false);

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

        // Recovers the declared field type backing rid so the Fix picker is constrained the same way the Repair window
        // constrains its own. The asset's whole managed-reference constraint map (keyed by document file id + rid) is
        // built once and looked up by the owning document's exact (fileId, rid) key, so a rid that collides across
        // documents resolves to this document's field type rather than another's. Returns null (unconstrained) when no
        // field points at the rid (an orphaned payload) or the field type is unresolvable.
        private static Type ResolveConstraint(string assetPath, long fileId, long rid)
        {
            var map = SerializeReferenceHelpers.BuildConstraintMap(assetPath);
            return map.TryGetValue((fileId, rid), out var constraint) ? constraint : null;
        }

        // Future work: a "Make unique" action on a SHARED node — cloning the aliased reference so the two fields no
        // longer affect each other (mirrors SerializeReferenceHelpers.MakeReferenceUnique, which operates on a live
        // SerializedProperty). v1 is read-only except for the missing-type Fix above.
    }
}
