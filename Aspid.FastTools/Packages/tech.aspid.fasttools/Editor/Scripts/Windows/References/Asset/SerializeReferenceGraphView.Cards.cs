using System;
using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;
using System.Collections.Generic;
using Aspid.FastTools.UIElements;
using Aspid.FastTools.UIElements.Editors.Internal;
using Object = UnityEngine.Object;
using CardKind = Aspid.FastTools.SerializeReferences.Editors.SerializeReferenceGraphPlan.CardKind;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    internal sealed partial class SerializeReferenceGraphView
    {
        private const string DocumentClass = RootClass + "__document";
        private const string DocumentHeaderClass = RootClass + "__document-header";
        private const string DocumentBodyClass = RootClass + "__document-body";

        private const string SectionClass = RootClass + "__section";

        private const string OrphanedTooltip =
            "Entries in the file's references block that no field points at. Clear deletes one from the file after you "
            + "confirm; it has no Undo.";

        private const string DocumentChevronExpanded = "▼";
        private const string DocumentChevronCollapsed = "▶";

        private VisualElement BuildDocument(string assetPath, SerializeReferenceGraphPlan plan,
            IReadOnlyList<GateViolation> required, IReadOnlyList<TypeNameField> typeNames, bool showHeader)
        {
            var document = plan.Document;
            var (broken, migrations) = SerializeReferenceGraphAnalysis.CountUnresolved(assetPath, document, _constraints);

            var body = new VisualElement().AddClass(DocumentBodyClass);

            var header = showHeader
                ? BuildDocumentHeader(document.TypeName, document.Source, body,
                    $"{SerializeReferenceGraphSummary.BuildDocumentCountText(document, broken, migrations)}\nfileId {document.FileId}")
                : null;

            // The cards follow the fields of the object; orphans have no field, so they are built after the tree, and the
            // keyboard visits them in the order they are drawn.
            var orphans = new List<long>();

            foreach (var card in plan.Cards)
            {
                switch (card.Kind)
                {
                    case CardKind.Node:
                    case CardKind.Repeat:
                        body.AddChild(BuildNodeCard(assetPath, document, document.FindNode(card.Rid), card.Rid, card.Path,
                            isOrphan: false, isRepeat: card.Kind == CardKind.Repeat));
                        break;
                    case CardKind.BackEdge:
                        body.AddChild(BuildBackEdgeCard(card.Rid));
                        break;
                    case CardKind.Empty:
                        // An optional empty field is a one-line row in its place; an empty [Required] one keeps its amber card.
                        body.AddChild(string.IsNullOrEmpty(card.Path) || IsFieldRequiredUnset(document.FileId, card.Path)
                            ? BuildEmptySlotCard(assetPath, document.FileId, card.Path)
                            : BuildUnassignedRow(card.Path, band => OpenLivePicker(assetPath, document.FileId, card.Path, band)));
                        break;
                    case CardKind.Orphan:
                        orphans.Add(card.Rid);
                        break;
                }
            }

            AddFieldCards(body, assetPath, typeNames, required, qualify: false);

            if (orphans.Count > 0)
            {
                // The cards below are the count, so the heading carries only its name.
                var section = BuildSection("Orphaned", StatusStyle.Type.Warning, OrphanedTooltip);

                foreach (var rid in orphans)
                {
                    section.AddChild(BuildNodeCard(assetPath, document, document.FindNode(rid), rid,
                        pathLabel: null, isOrphan: true, isRepeat: false));
                }

                body.AddChild(section);
            }

            if (header is null)
                return new VisualElement().AddClass(DocumentClass).AddChild(body);

            return new VisualElement()
                .AddClass(DocumentClass)
                .AddChild(header)
                .AddChild(body);
        }

        // An object with type name fields but no managed references, which the graph has no document for.
        private VisualElement BuildFieldsDocument(string assetPath, Object source, long fileId,
            IReadOnlyList<TypeNameField> typeNames, IReadOnlyList<GateViolation> required)
        {
            var body = new VisualElement().AddClass(DocumentBodyClass);
            var header = BuildDocumentHeader(SerializeReferenceGraphScanner.DisplayNameOf(source), source, body,
                $"fileId {fileId}");

            AddFieldCards(body, assetPath, typeNames, required, qualify: false);

            return new VisualElement()
                .AddClass(DocumentClass)
                .AddChild(header)
                .AddChild(body);
        }

        // The object that holds the fields: a heading a level above the sections inside it, which folds its cards.
        private VisualElement BuildDocumentHeader(string title, Object source, VisualElement body, string tooltip)
        {
            var collapsed = false;
            ReferenceSectionHeading header = null;

            var toggle = new Action(() =>
            {
                collapsed = !collapsed;
                body.style.display = collapsed ? DisplayStyle.None : DisplayStyle.Flex;
                header.TrailingText = collapsed ? DocumentChevronCollapsed : DocumentChevronExpanded;
            });

            // A scene is read as text only, so its objects have no loaded source and no icon.
            var icon = source != null ? AssetPreview.GetMiniThumbnail(source) : null;

            // The name gets the whole row; the counts are in the tooltip, and the cards below show what is wrong.
            header = ReferenceSectionHeading.ForObject(title, icon)
                .SetOnClick(toggle);
            header.AddToClassList(DocumentHeaderClass);
            header.TrailingText = DocumentChevronExpanded;
            header.tooltip = tooltip;
            RegisterNavTarget(header, toggle);

            return header;
        }

        // The type name fields in their field order, an empty required one as its amber card, then the required
        // violations none of them covers. qualify names the component, for cards outside their object's heading.
        private void AddFieldCards(VisualElement parent, string assetPath, IReadOnlyList<TypeNameField> typeNames,
            IReadOnlyList<GateViolation> required, bool qualify)
        {
            var covered = new HashSet<(long, string)>();

            foreach (var field in typeNames)
            {
                var label = FieldLabel(field.ComponentName, field.FieldPath, qualify);
                var violation = required.FirstOrDefault(entry =>
                    entry.Rid == 0 && entry.FileId == field.FileId && entry.FieldPath == field.FieldPath);

                if (!string.IsNullOrEmpty(field.TypeName))
                {
                    parent.AddChild(BuildTypeNameCard(assetPath, field, label));
                }
                else if (violation.FieldPath is not null)
                {
                    covered.Add((field.FileId, field.FieldPath));
                    parent.AddChild(BuildRequiredOnlyCard(violation, label));
                }
                else
                {
                    parent.AddChild(BuildUnassignedRow(label,
                        band => OpenTypeNamePicker(assetPath, field.FileId, field.FieldPath, band)));
                }
            }

            foreach (var violation in required)
            {
                if (covered.Contains((violation.FileId, violation.FieldPath))) continue;

                parent.AddChild(BuildRequiredOnlyCard(violation, FieldLabel(violation.ComponentName, violation.FieldPath, qualify)));
            }
        }

        private static string FieldLabel(string componentName, string fieldPath, bool qualify)
        {
            var path = SerializeReferenceGraphEditor.ToGraphPath(fieldPath);
            return qualify && !string.IsNullOrEmpty(componentName) ? $"{componentName}.{path}" : path;
        }

        // Orphans under their own heading, after the cards of the tree.
        private static VisualElement BuildSection(string title, StatusStyle.Type status, string tooltip)
        {
            var heading = new ReferenceSectionHeading(title, status) { tooltip = tooltip };
            return new VisualElement()
                .AddClass(SectionClass)
                .AddChild(heading);
        }

        private static VisualElement BuildHiddenNotice(int hidden)
        {
            var cards = hidden == 1 ? "1 more card is" : $"{hidden} more cards are";
            return new AspidHelpBox(AspidHelpBoxPreset.Default.SetMessageType(HelpBoxMessageType.Info))
                .SetMessage($"{cards} not shown: the window draws at most {SerializeReferenceGraphPlan.MaxCards} reference cards. " +
                            "Project References repairs missing types without that limit.");
        }
    }
}
