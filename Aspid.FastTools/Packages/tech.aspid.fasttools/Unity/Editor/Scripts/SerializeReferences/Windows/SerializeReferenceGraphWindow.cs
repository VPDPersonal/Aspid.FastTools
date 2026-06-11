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
    internal sealed class SerializeReferenceGraphWindow : EditorWindow
    {
        private const string StyleSheetPath = "UI/SerializeReferences/Aspid-FastTools-ReferenceGraph";

        private const string RootClass = "aspid-fasttools-reference-graph";
        private const string BackgroundClass = RootClass + "__background";
        private const string ContentClass = RootClass + "__content";
        private const string CardClass = RootClass + "__card";
        private const string CardHeaderClass = RootClass + "__card-header";
        private const string FieldRowClass = RootClass + "__field-row";
        private const string AssetClass = RootClass + "__asset";
        private const string RescanClass = RootClass + "__rescan";
        private const string EmptyClass = RootClass + "__empty";
        private const string ScrollClass = RootClass + "__scroll";

        private const string DocumentClass = RootClass + "__document";
        private const string DocumentHeaderClass = RootClass + "__document-header";

        private const string NodeClass = RootClass + "__node";
        private const string NodeBackEdgeClass = NodeClass + "--back-edge";
        private const string NodeLabelClass = RootClass + "__node-label";
        private const string NodeLabelMissingClass = NodeLabelClass + "--missing";
        private const string NodeLabelOrphanClass = NodeLabelClass + "--orphan";
        private const string NodeRootLabelClass = RootClass + "__node-root-label";
        private const string NodeTypeClass = RootClass + "__node-type";
        private const string NodeRidClass = RootClass + "__node-rid";
        private const string NodeBadgesClass = RootClass + "__node-badges";

        private const string BadgeClass = RootClass + "__badge";
        private const string BadgeMissingClass = BadgeClass + "--missing";
        private const string BadgeSharedClass = BadgeClass + "--shared";

        private const string ChipClass = RootClass + "__chip";
        private const string FixClass = RootClass + "__fix";
        private const string OrphanGroupClass = RootClass + "__orphan-group";
        private const string OrphanGroupHeaderClass = RootClass + "__orphan-group-header";
        private const string PickerClass = RootClass + "__picker";

        private const string FixCollapsedText = "Fix  ▼";
        private const string FixExpandedText = "Fix  ▲";

        // Indentation step (px) applied per tree depth so nested cards read as a hierarchy.
        private const float IndentStep = 16f;

        private Object _target;
        private ObjectField _assetField;
        private AspidHelpBox _empty;
        private ScrollView _scroll;
        private VisualElement _list;

        private VisualElement _openPicker;
        private AspidGradientButton _openPickerRow;

        [MenuItem("Tools/Aspid 🐍/Managed References FastTools", priority = 21)]
        private static void Open() => Open(Selection.activeObject);

        public static void Open(Object target)
        {
            var window = GetWindow<SerializeReferenceGraphWindow>();
            window.titleContent = new GUIContent("Managed References");
            window.minSize = new Vector2(480f, 360f);
            window.SetTarget(target);
            window.Show();
        }

        private void CreateGUI()
        {
            var root = rootVisualElement;
            root.AddStyleSheetsFromResource(AspidStyles.DefaultStyleSheet)
                .AddStyleSheetsFromResource(StyleSheetPath)
                .AddClass(RootClass);

            // Mirror the Repair window: an animated black dotted canvas fills the window, the content flows above it
            // so the dark Aspid cards read against black instead of the muddy inspector grey.
            var background = new AspidAnimatedDotsBackground()
                .AddClass(BackgroundClass)
                .SetPickingMode(PickingMode.Ignore);

            var assetHeader = new AspidLabel("Asset", AspidLabelPreset.Default
                    .SetLabelTheme(ThemeStyle.Type.Lightness)
                    .SetLabelSize(AspidLabelSizeStyle.Type.H3)
                    .SetLineTheme(ThemeStyle.Type.Dark)
                    .SetLineStatus(StatusStyle.Type.Success))
                .AddClass(CardHeaderClass);

            _assetField = new ObjectField
            {
                objectType = typeof(Object),
                allowSceneObjects = false,
                value = _target,
            };
            _assetField.AddClass(AssetClass);
            _assetField.RegisterValueChangedCallback(evt => SetTarget(evt.newValue));

            var rescan = new AspidGradientButton("Rescan", _ => Rescan())
                .AddClass(RescanClass);

            var fieldRow = new VisualElement()
                .AddClass(FieldRowClass)
                .AddChild(_assetField)
                .AddChild(rescan);

            var card = new AspidBox(AspidBoxPreset.Default.SetTheme(ThemeStyle.Type.Darkness))
                .AddClass(CardClass)
                .AddChild(assetHeader)
                .AddChild(fieldRow);

            // The empty state (no asset / no managed references) shares one info help-box centred below the card; a
            // successful scan swaps it for the per-document tree list inside a scroll view.
            _empty = new AspidHelpBox(AspidHelpBoxPreset.Default.SetMessageType(HelpBoxMessageType.Info))
                .AddClass(EmptyClass);

            _list = new VisualElement();
            _scroll = new ScrollView().AddClass(ScrollClass);
            _scroll.AddChild(_list);

            var content = new VisualElement()
                .AddClass(ContentClass)
                .AddChild(card)
                .AddChild(_empty)
                .AddChild(_scroll);

            root.AddChild(background)
                .AddChild(content);

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
                ShowEmpty("Select a saved asset (a prefab or ScriptableObject) to map its managed-reference graph.");
                return;
            }

            var documents = SerializeReferenceGraphScanner.Build(assetPath);
            if (documents.Count == 0)
            {
                ShowEmpty("No [SerializeReference] managed references in this asset.");
                return;
            }

            ShowResults();
            foreach (var document in documents)
                _list.AddChild(BuildDocument(assetPath, document));
        }

        private void ShowEmpty(string message)
        {
            _scroll.style.display = DisplayStyle.None;
            _empty.style.display = DisplayStyle.Flex;
            _empty.Message = message;
        }

        private void ShowResults()
        {
            _empty.style.display = DisplayStyle.None;
            _scroll.style.display = DisplayStyle.Flex;
        }

        // One serialized object document: a header card naming the component / ScriptableObject, then each root's
        // reference subtree, then a trailing "Orphaned" group for any rids no root reaches.
        private VisualElement BuildDocument(string assetPath, ReferenceGraphDocument document)
        {
            var header = new AspidLabel(document.TypeName, AspidLabelPreset.Default
                    .SetLabelTheme(ThemeStyle.Type.Lightness)
                    .SetLabelSize(AspidLabelSizeStyle.Type.H4)
                    .SetLineTheme(ThemeStyle.Type.Dark))
                .AddClass(DocumentHeaderClass);
            header.tooltip = $"fileId {document.FileId}";

            var container = new VisualElement()
                .AddClass(DocumentClass)
                .AddChild(header);

            foreach (var root in document.Roots)
            {
                var visited = new HashSet<long>();
                container.AddChild(BuildNode(assetPath, document, root.Rid, root.Label, depth: 0, visited));
            }

            var orphans = BuildOrphanGroup(assetPath, document);
            if (orphans is not null) container.AddChild(orphans);

            return container;
        }

        // Renders a node and (recursively) its children as indented cards. The visited set makes the walk cycle-safe:
        // a rid already on the current path renders as a back-edge leaf ("↩ rid N") instead of recursing forever.
        private VisualElement BuildNode(string assetPath, ReferenceGraphDocument document, long rid, string rootLabel, int depth, HashSet<long> visited)
        {
            var node = document.FindNode(rid);

            if (!visited.Add(rid))
                return BuildBackEdgeRow(depth, rid);

            var card = new AspidBox(AspidBoxPreset.Default.SetTheme(ThemeStyle.Type.Darkness))
                .AddClass(NodeClass);
            card.style.marginLeft = depth * IndentStep;

            var row = BuildNodeRow(assetPath, document, node, rid, rootLabel);
            card.AddChild(row);

            foreach (var childRid in document.ChildrenOf(rid))
                card.AddChild(BuildNode(assetPath, document, childRid, rootLabel: null, depth + 1, visited));

            // Leaving the recursion: drop the rid so a sibling subtree may legitimately reference it again (shared),
            // while a back-edge on the current path is still caught above.
            visited.Remove(rid);
            return card;
        }

        // The node row: an optional root field label, the type short name (yellow + tooltip when missing), the dim
        // rid, the MISSING / SHARED badges and — for a missing reference — a trailing gradient Fix button that opens
        // the inline type picker. A flat flex row keeps layout predictable; the Fix button is the only interactive part.
        private VisualElement BuildNodeRow(string assetPath, ReferenceGraphDocument document, ReferenceGraphNode? node, long rid, string rootLabel)
        {
            var missing = node is { Resolves: false } && !node.Value.StoredType.IsEmpty;

            var row = new VisualElement().AddClass(NodeLabelClass);
            if (missing) row.AddClass(NodeLabelMissingClass);

            if (!string.IsNullOrEmpty(rootLabel))
            {
                row.AddChild(new Label($"{rootLabel}:")
                    .AddClass(NodeRootLabelClass)
                    .SetPickingMode(PickingMode.Ignore));
            }

            var typeLabel = new Label(node?.ShortName ?? $"rid {rid}")
                .AddClass(NodeTypeClass)
                .SetPickingMode(PickingMode.Ignore);
            if (node is not null && !node.Value.StoredType.IsEmpty)
                typeLabel.tooltip = node.Value.FullName;
            row.AddChild(typeLabel);

            row.AddChild(new Label($"rid {rid}")
                .AddClass(NodeRidClass)
                .SetPickingMode(PickingMode.Ignore));

            var badges = new VisualElement()
                .AddClass(NodeBadgesClass)
                .SetPickingMode(PickingMode.Ignore);

            if (missing)
                badges.AddChild(new Label("MISSING").AddClass(BadgeClass).AddClass(BadgeMissingClass));

            if (document.Shared.Contains(rid))
            {
                var chip = new VisualElement().AddClass(ChipClass);
                chip.style.backgroundColor = SharedChipColor(rid);
                badges.AddChild(new Label("SHARED").AddClass(BadgeClass).AddClass(BadgeSharedClass).AddChild(chip));
            }

            row.AddChild(badges);

            if (missing)
            {
                // Self-referencing capture matches the Repair window so the click handler can hand TogglePicker the
                // button it lives on to anchor the inline picker directly below the row's card.
                AspidGradientButton fixButton = null;
                fixButton = new AspidGradientButton(FixCollapsedText, _ => TogglePicker(assetPath, rid, fixButton))
                    .AddClass(FixClass);
                row.AddChild(fixButton);
            }

            return row;
        }

        // A back-edge to a rid already on the current render path — shown as a dim leaf so cycles terminate visibly.
        private static VisualElement BuildBackEdgeRow(int depth, long rid)
        {
            var card = new AspidBox(AspidBoxPreset.Default.SetTheme(ThemeStyle.Type.Darkness))
                .AddClass(NodeClass)
                .AddClass(NodeBackEdgeClass);
            card.style.marginLeft = depth * IndentStep;

            card.AddChild(new Label($"↩ rid {rid}")
                .AddClass(NodeLabelClass)
                .SetPickingMode(PickingMode.Ignore));

            return card;
        }

        // Trailing warning-tinted group listing rids no root reaches — leftover payloads from deleted fields or
        // broken parents. Each orphan gets its own node card (so a missing orphan is still fixable and the inline
        // picker anchors uniformly: Fix button → row → card), but without recursion into children.
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

                var card = new AspidBox(AspidBoxPreset.Default.SetTheme(ThemeStyle.Type.Darkness))
                    .AddClass(NodeClass);

                var row = BuildNodeRow(assetPath, document, node, node.Rid, rootLabel: null);
                row.AddClass(NodeLabelOrphanClass);

                card.AddChild(row);
                group.AddChild(card);
            }

            return group;
        }

        // Deterministic chip colour for a shared rid: a golden-ratio hue rotation off the rid hash keeps distinct
        // rids visually separated while the same rid always maps to the same colour. Saturation/value are fixed to
        // sit legibly on the dark palette.
        private static Color SharedChipColor(long rid)
        {
            const float goldenRatioConjugate = 0.618033988749895f;
            var hash = unchecked((uint)(rid * 2654435761));
            var hue = (hash / (float)uint.MaxValue + goldenRatioConjugate * (hash & 0xFF)) % 1f;
            return Color.HSVToRGB(hue, 0.55f, 0.85f);
        }

        // The picker expands inline as an accordion panel directly below the clicked row's card — the same selector
        // view the dropdown window hosts, boxed in the window's dark style. One panel at a time; the Fix cue flips to
        // ▲ while open and clicking it again collapses it. v1 uses an unconstrained picker (typeof(object));
        // narrowing to the declared field type would need the live SerializedObject scan the Repair window runs.
        private void TogglePicker(string assetPath, long rid, AspidGradientButton fixButton)
        {
            var wasOpen = _openPickerRow == fixButton;
            ClosePicker();
            if (wasOpen) return;

            var view = new TypeSelectorView(
                types: new[] { typeof(object) },
                currentAqn: string.Empty,
                allow: TypeAllow.None,
                onSelected: assemblyQualifiedName => ApplyFix(assetPath, rid, assemblyQualifiedName),
                filter: SerializeReferenceHelpers.IsAssignableManagedReference,
                additionalTypes: null,
                argumentFilter: SerializeReferenceHelpers.IsValidGenericArgument,
                onDismiss: ClosePicker);

            _openPicker = new AspidBox(AspidBoxPreset.Default.SetTheme(ThemeStyle.Type.Darkness))
                .AddClass(PickerClass)
                .AddChild(view);

            _openPickerRow = fixButton;
            if (fixButton is not null) fixButton.Text = FixExpandedText;

            // Anchor below the node's enclosing card (Fix button → row → card) so the panel reads as belonging to the
            // whole row, not squeezed under the button. Fall back to the list when the chain is unexpectedly shallow.
            var anchor = fixButton?.parent?.parent ?? _list;
            var parent = anchor.parent ?? _list;
            parent.InsertChild(parent.IndexOf(anchor) + 1, _openPicker);
            view.FocusSearch();
        }

        private void ClosePicker()
        {
            _openPicker?.RemoveFromHierarchy();
            if (_openPickerRow is not null) _openPickerRow.Text = FixCollapsedText;

            _openPicker = null;
            _openPickerRow = null;
        }

        private void ApplyFix(string assetPath, long rid, string assemblyQualifiedName)
        {
            var type = string.IsNullOrEmpty(assemblyQualifiedName)
                ? null
                : Type.GetType(assemblyQualifiedName, throwOnError: false);

            if (type is null) return;

            // The missing entry lives in exactly one document; the scanner does not carry the owning fileId on a
            // node, so rewrite is attempted per document until one succeeds. (A rid is unique within a document; a
            // collision across documents would at worst rewrite the first match — acceptable for v1.)
            var documents = SerializeReferenceGraphScanner.Build(assetPath);
            foreach (var document in documents)
            {
                if (document.FindNode(rid) is null) continue;
                if (!SerializeReferenceYamlEditor.TryRewriteType(assetPath, document.FileId, rid, ManagedTypeName.FromType(type)))
                    continue;

                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
                break;
            }

            Rescan();
        }

        // Future work: a "Make unique" action on a SHARED node — cloning the aliased reference so the two fields no
        // longer affect each other (mirrors SerializeReferenceHelpers.MakeReferenceUnique, which operates on a live
        // SerializedProperty). v1 is read-only except for the missing-type Fix above.
    }
}
