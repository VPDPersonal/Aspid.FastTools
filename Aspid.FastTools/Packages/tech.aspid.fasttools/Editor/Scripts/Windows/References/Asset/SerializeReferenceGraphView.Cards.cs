using System;
using UnityEditor;
using UnityEngine.UIElements;
using System.Collections.Generic;
using Aspid.FastTools.UIElements;
using Aspid.FastTools.UIElements.Editors.Internal;
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

        private VisualElement BuildDocument(string assetPath, SerializeReferenceGraphPlan plan, bool showHeader)
        {
            var document = plan.Document;
            var (broken, migrations) = SerializeReferenceGraphAnalysis.CountUnresolved(assetPath, document, _constraints);

            var body = new VisualElement().AddClass(DocumentBodyClass);

            var header = showHeader ? BuildDocumentHeader(document, body, broken, migrations) : null;

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
                            : BuildUnassignedRow(assetPath, document.FileId, card.Path));
                        break;
                    case CardKind.Orphan:
                        orphans.Add(card.Rid);
                        break;
                }
            }

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

        // The object that holds the fields: a heading a level above the sections inside it, which folds its cards.
        private VisualElement BuildDocumentHeader(ReferenceGraphDocument document, VisualElement body, int broken,
            int migrations)
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
            var icon = document.Source != null ? AssetPreview.GetMiniThumbnail(document.Source) : null;

            // The name gets the whole row; the counts are in the tooltip, and the cards below show what is wrong.
            header = ReferenceSectionHeading.ForObject(document.TypeName, icon)
                .SetOnClick(toggle);
            header.AddToClassList(DocumentHeaderClass);
            header.TrailingText = DocumentChevronExpanded;
            header.tooltip = $"{SerializeReferenceGraphSummary.BuildDocumentCountText(document, broken, migrations)}\nfileId {document.FileId}";
            RegisterNavTarget(header, toggle);

            return header;
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
