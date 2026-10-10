using System;
using UnityEngine.UIElements;
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
        private const string DocumentHeaderIssuesClass = DocumentHeaderClass + "--issues";
        private const string DocumentHeaderRowClass = RootClass + "__document-header-row";
        private const string DocumentTitleClass = RootClass + "__document-title";
        private const string DocumentCountClass = RootClass + "__document-count";
        private const string DocumentBodyClass = RootClass + "__document-body";

        private const string OrphanGroupClass = RootClass + "__orphan-group";
        private const string OrphanGroupHeaderClass = RootClass + "__orphan-group-header";

        private const string DocumentChevronExpanded = "▼";
        private const string DocumentChevronCollapsed = "▶";

        private VisualElement BuildDocument(string assetPath, SerializeReferenceGraphPlan plan, bool showHeader)
        {
            var document = plan.Document;
            var (broken, migrations) = SerializeReferenceGraphAnalysis.CountUnresolved(assetPath, document, _constraints);
            var hasIssues = document.Orphans.Count > 0 || broken > 0;

            var body = new VisualElement().AddClass(DocumentBodyClass);

            var header = showHeader ? BuildDocumentHeader(document, body, hasIssues, broken, migrations) : null;

            VisualElement orphans = null;
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
                        body.AddChild(BuildEmptySlotCard(assetPath, document.FileId, card.Path));
                        break;
                    case CardKind.Orphan:
                        orphans ??= BuildOrphanGroup();
                        orphans.AddChild(BuildNodeCard(assetPath, document, document.FindNode(card.Rid), card.Rid,
                            pathLabel: null, isOrphan: true, isRepeat: false));
                        break;
                }
            }

            if (orphans is not null) body.AddChild(orphans);

            if (header is null)
                return new VisualElement().AddClass(DocumentClass).AddChild(body);

            return new VisualElement()
                .AddClass(DocumentClass)
                .AddChild(header)
                .AddChild(body);
        }

        private AspidGradientButton BuildDocumentHeader(ReferenceGraphDocument document, VisualElement body,
            bool hasIssues, int broken, int migrations)
        {
            var collapsed = false;
            AspidGradientButton header = null;

            var toggle = new Action(() =>
            {
                collapsed = !collapsed;
                body.style.display = collapsed ? DisplayStyle.None : DisplayStyle.Flex;
                header.Text = collapsed ? DocumentChevronCollapsed : DocumentChevronExpanded;
            });

            header = new AspidGradientButton(DocumentChevronExpanded, _ => toggle())
                .AddClass(DocumentHeaderClass);
            if (hasIssues) header.AddClass(DocumentHeaderIssuesClass);
            header.tooltip = $"fileId {document.FileId}";
            RegisterNavTarget(header, toggle);

            header.AddLeadingContent(new VisualElement()
                .AddClass(DocumentHeaderRowClass)
                .SetPickingMode(PickingMode.Ignore)
                .AddChild(new Label(document.TypeName)
                    .AddClass(DocumentTitleClass)
                    .SetPickingMode(PickingMode.Ignore))
                .AddChild(new Label(SerializeReferenceGraphSummary.BuildDocumentCountText(document, broken, migrations))
                    .AddClass(DocumentCountClass)
                    .SetPickingMode(PickingMode.Ignore)));

            return header;
        }

        private static VisualElement BuildOrphanGroup()
        {
            var group = new AspidBox(AspidBoxPreset.Default.SetTheme(ThemeStyle.Type.Darkness))
                .AddClass(OrphanGroupClass);

            return group.AddChild(new AspidLabel("Orphaned", AspidLabelPreset.Default
                    .SetLabelStatus(StatusStyle.Type.Warning)
                    .SetLabelSize(AspidLabelSizeStyle.Type.H5)
                    .SetLineSize(AspidDividingLineSizeStyle.Type.None))
                .AddClass(OrphanGroupHeaderClass));
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
