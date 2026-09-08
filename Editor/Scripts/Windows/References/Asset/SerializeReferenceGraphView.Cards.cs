using System;
using UnityEngine.UIElements;
using Aspid.FastTools.UIElements;
using System.Collections.Generic;
using Aspid.FastTools.UIElements.Editors.Internal;

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

        private VisualElement BuildDocument(string assetPath, ReferenceGraphDocument document, bool showHeader)
        {
            var (broken, migrations) = SerializeReferenceGraphAnalysis.CountUnresolved(assetPath, document, _constraints);
            var hasIssues = document.Orphans.Count > 0 || broken > 0;

            var body = new VisualElement().AddClass(DocumentBodyClass);

            var header = showHeader ? BuildDocumentHeader(document, body, hasIssues, broken, migrations) : null;

            foreach (var root in document.Roots)
            {
                if (root.IsEmpty || !SerializeReferenceGraphAnalysis.RootIsMissing(document, root.Rid)) continue;
                AppendNode(body, assetPath, document, root.Rid, root.Label, new HashSet<long>());
            }

            foreach (var root in document.Roots)
            {
                if (root.IsEmpty)
                {
                    body.AddChild(BuildEmptySlotCard(assetPath, document.FileId, root.Label));
                    continue;
                }

                if (SerializeReferenceGraphAnalysis.RootIsMissing(document, root.Rid)) continue;
                AppendNode(body, assetPath, document, root.Rid, root.Label, new HashSet<long>());
            }

            var orphans = BuildOrphanGroup(assetPath, document);
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
                var childPath = SerializeReferenceGraphAnalysis.CombinePath(pathLabel, edge.Label);
                if (edge.IsEmpty)
                    container.AddChild(BuildEmptySlotCard(assetPath, document.FileId, childPath));
                else
                    AppendNode(container, assetPath, document, edge.Rid, childPath, visited);
            }

            // Remove only the current path: sibling subtrees may legitimately share this reference.
            visited.Remove(rid);
        }

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
    }
}
