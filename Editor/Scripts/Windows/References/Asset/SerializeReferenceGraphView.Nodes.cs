using System;
using UnityEngine.UIElements;
using Aspid.FastTools.UIElements;
using Aspid.FastTools.Types.Editors;
using Aspid.FastTools.UIElements.Editors.Internal;
using static Aspid.FastTools.SerializeReferences.Editors.SerializeReferenceAuditUI;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    internal sealed partial class SerializeReferenceGraphView
    {
        private const string NodeClass = RootClass + "__node";
        private const string NodeBackEdgeClass = NodeClass + "--back-edge";
        private const string NodeEmptyClass = NodeClass + "--empty";
        private const string NodeMigrateCardClass = NodeClass + "--migrate";
        private const string NodeHeaderHoverClass = NodeClass + "--header-hover";
        private const string NodeBandClass = RootClass + "__node-band";
        private const string NodeBandMissingClass = NodeBandClass + "--missing";
        private const string NodeBandMigrateClass = NodeBandClass + "--migrate";
        private const string NodeBandRowClass = RootClass + "__node-band-row";
        private const string NodeDividerClass = RootClass + "__node-divider";
        private const string NodeSweepClass = RootClass + "__node-sweep";
        private const string NodeSweepMissingClass = NodeSweepClass + "--missing";
        private const string NodeSweepMigrateClass = NodeSweepClass + "--migrate";
        private const string NodeActionClass = RootClass + "__node-action";
        private const string NodeActionInfoClass = NodeActionClass + "--info";
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

        private const string FixCollapsedText = "Fix Missing  ▼";
        private const string ChangeCollapsedText = "Change  ▼";
        private const string AssignCollapsedText = "Assign  ▼";

        private const string AssignRequiredCollapsedText = "Assign Required  ▼";

        private const string MigrateFixCollapsedText = "Fix  ▼";

        private const string EmptySlotText = TypeSelectorHelpers.NoneOption;

        private VisualElement BuildNodeCard(string assetPath, ReferenceGraphDocument document, ReferenceGraphNode? node, long rid, string pathLabel, bool isOrphan)
        {
            var missing = node is { Resolves: false, StoredType: { IsEmpty: false } };

            // Unity migrates reachable references in memory; orphaned payloads are never loaded.
            Type migrationTarget = null;
            var isMigration = missing && !isOrphan &&
                SerializeReferenceGraphAnalysis.IsPendingMigration(assetPath, document.FileId, rid,
                    node.Value.StoredType, _constraints, out migrationTarget);

            var card = new AspidBox(AspidBoxPreset.Default.SetTheme(ThemeStyle.Type.Darkness))
                .AddClass(NodeClass);
            if (isMigration) card.AddClass(NodeMigrateCardClass);

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

            var bandRow = BuildBandRow(typeLabel, BuildBadges(document, rid));

            // Reference IDs are only unique within a serialized document.
            var fileId = document.FileId;

            if (missing)
            {
                // The serialization API cannot reassign missing types; preserve their payload through YAML edits.
                AspidGradientButton band = null;
                band = new AspidGradientButton(isMigration ? MigrateFixCollapsedText : FixCollapsedText,
                        _ => OpenMissingPicker(assetPath, fileId, rid, band))
                    .AddClass(NodeBandClass)
                    .AddClass(isMigration ? NodeBandMigrateClass : NodeBandMissingClass);
                band.AddLeadingContent(bandRow);
                card.AddChild(band);
                RegisterNavBand(band, card, () => OpenMissingPicker(assetPath, fileId, rid, band));
                AddBandDivider(card, band, isMigration ? NodeSweepMigrateClass : NodeSweepMissingClass);

                var action = BuildQuickFixRow(assetPath, fileId, rid, node.Value.StoredType, isMigration, migrationTarget);
                if (action is not null) card.AddChild(action);
            }
            else if (!isOrphan)
            {
                var graphPath = pathLabel;
                AspidGradientButton band = null;
                band = new AspidGradientButton(ChangeCollapsedText, _ => OpenLivePicker(assetPath, fileId, graphPath, band))
                    .AddClass(NodeBandClass);
                band.AddLeadingContent(bandRow);
                card.AddChild(band);
                RegisterNavBand(band, card, () => OpenLivePicker(assetPath, fileId, graphPath, band));
                AddBandDivider(card, band, sweepModifier: null);
            }
            else
            {
                // Orphans have no live property to edit, so only the file-level Clear action is available.
                card.AddChild(bandRow);
                AddBandDivider(card, band: null, sweepModifier: null);
            }

            var meta = BuildFooter(pathLabel, $"rid {rid}");

            if (isOrphan)
            {
                var clear = new AspidGradientButton("Clear", _ => ClearOrphan(assetPath, fileId, rid))
                    .AddClass(ClearOrphanClass);
                RegisterNavTarget(clear, () => ClearOrphan(assetPath, fileId, rid));
                meta.AddChild(clear);
            }

            card.AddChild(meta);

            return card;
        }

        private VisualElement BuildEmptySlotCard(string assetPath, long fileId, string pathLabel)
        {
            var isRequired = IsFieldRequiredUnset(fileId, pathLabel);

            var card = new AspidBox(AspidBoxPreset.Default.SetTheme(ThemeStyle.Type.Darkness))
                .AddClass(NodeClass);
            if (!isRequired) card.AddClass(NodeEmptyClass);

            var typeLabel = isRequired
                ? (VisualElement)BuildRequiredNoneLabel("Required reference is not set")
                : new Label(EmptySlotText).AddClass(NodeTypeClass).SetPickingMode(PickingMode.Ignore);

            var bandRow = BuildBandRow(typeLabel, badges: null);

            if (string.IsNullOrEmpty(pathLabel))
            {
                card.AddChild(bandRow);
                AddBandDivider(card, band: null, sweepModifier: null);
            }
            else
            {
                var graphPath = pathLabel;
                AspidGradientButton band = null;
                band = new AspidGradientButton(isRequired ? AssignRequiredCollapsedText : AssignCollapsedText,
                        _ => OpenLivePicker(assetPath, fileId, graphPath, band))
                    .AddClass(NodeBandClass);
                if (isRequired) band.AddClass(NodeBandMissingClass);
                band.AddLeadingContent(bandRow);
                card.AddChild(band);
                RegisterNavBand(band, card, () => OpenLivePicker(assetPath, fileId, graphPath, band));
                AddBandDivider(card, band, isRequired ? NodeSweepMissingClass : null);
            }

            card.AddChild(BuildFooter(pathLabel, "unassigned"));

            return card;
        }

        // Required string fields have no RefIds node; scene assets cannot be object-loaded for inline editing.
        private VisualElement BuildRequiredOnlyCard(GateViolation violation, ViolationFieldLabels labels)
        {
            var card = new AspidBox(AspidBoxPreset.Default.SetTheme(ThemeStyle.Type.Darkness))
                .AddClass(NodeClass);

            var bandRow = BuildBandRow(BuildRequiredNoneLabel("Required type is not set"), badges: null);

            if (SerializeReferenceHelpers.IsScene(violation.AssetPath))
            {
                card.AddChild(bandRow);
                AddBandDivider(card, band: null, sweepModifier: null);
            }
            else
            {
                AspidGradientButton band = null;
                band = new AspidGradientButton(AssignRequiredCollapsedText, _ => OpenRequiredStringPicker(violation, band))
                    .AddClass(NodeBandClass)
                    .AddClass(NodeBandMissingClass);
                band.AddLeadingContent(bandRow);
                card.AddChild(band);
                RegisterNavBand(band, card, () => OpenRequiredStringPicker(violation, band));
                AddBandDivider(card, band, NodeSweepMissingClass);
            }

            card.AddChild(BuildFooter(labels.Describe(violation), "unassigned"));

            return card;
        }

        // Terminate a cycle at its back-edge instead of rendering it recursively.
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

        private static VisualElement BuildBandRow(VisualElement typeLabel, VisualElement badges)
        {
            var row = new VisualElement()
                .AddClass(NodeBandRowClass)
                .AddChild(typeLabel);
            if (badges is not null) row.AddChild(badges);
            row.pickingMode = PickingMode.Ignore;
            return row;
        }

        private static AspidLabel BuildRequiredNoneLabel(string tooltip)
        {
            var label = new AspidLabel(EmptySlotText, AspidLabelPreset.Default
                    .SetLabelStatus(StatusStyle.Type.Warning)
                    .SetLabelSize(AspidLabelSizeStyle.Type.H5)
                    .SetLineSize(AspidDividingLineSizeStyle.Type.None))
                .AddClass(NodeTypeClass)
                .SetPickingMode(PickingMode.Ignore);
            label.tooltip = tooltip;
            return label;
        }

        private static VisualElement BuildBadges(ReferenceGraphDocument document, long rid)
        {
            var badges = new VisualElement()
                .AddClass(NodeBadgesClass)
                .SetPickingMode(PickingMode.Ignore);

            if (!document.Shared.Contains(rid)) return badges;

            var shared = new Label("SHARED").AddClass(BadgeClass).AddClass(BadgeSharedClass);
            var chip = new VisualElement().AddClass(ChipClass);
            chip.style.backgroundColor = SerializeReferenceRidColor.ForRid(rid);
            shared.AddChild(chip);

            return badges.AddChild(shared);
        }

        private VisualElement BuildQuickFixRow(string assetPath, long fileId, long rid, ManagedTypeName storedType,
            bool isMigration, Type migrationTarget)
        {
            if (isMigration)
            {
                return BuildNodeActionRow(
                    $"Migrate → {migrationTarget.Name}",
                    $"This entry resolves to {migrationTarget.FullName} via its declared [MovedFrom] — Unity " +
                    "already migrates it in memory when the asset loads. Migrating rewrites the stored type " +
                    "name in the file so it matches the code.",
                    info: true,
                    () => ApplyFix(assetPath, fileId, rid, migrationTarget.AssemblyQualifiedName));
            }

            if (!SerializeReferenceGraphAnalysis.TryGetSuggestion(assetPath, fileId, rid, storedType, _constraints, out var suggestion))
                return null;

            return BuildNodeActionRow(
                $"Smart Fix {SerializeReferenceHelpers.GetSuggestionLabel(suggestion)}",
                SerializeReferenceHelpers.GetSuggestionDetail(suggestion),
                info: false,
                () => ApplyFix(assetPath, fileId, rid, suggestion.Type.AssemblyQualifiedName));
        }

        private VisualElement BuildNodeActionRow(string text, string tooltipText, bool info, Action onClick)
        {
            var row = new Label(text).AddClass(NodeActionClass);
            if (info) row.AddClass(NodeActionInfoClass);
            row.tooltip = tooltipText;
            row.RegisterCallback<ClickEvent>(_ => onClick());
            RegisterNavTarget(row, onClick);
            return row;
        }

        // The sweep is a sibling of the button, so hover is propagated through the card class.
        private static void AddBandDivider(VisualElement card, AspidGradientButton band, string sweepModifier)
        {
            card.AddChild(new AspidDividingLine(AspidDividingLinePreset.Default
                    .SetTheme(ThemeStyle.Type.Light)
                    .SetSize(AspidDividingLineSizeStyle.Type.Thin))
                .AddClass(NodeDividerClass));

            if (band is null) return;

            var sweep = new VisualElement()
                .AddClass(NodeSweepClass)
                .SetPickingMode(PickingMode.Ignore);
            if (sweepModifier is not null) sweep.AddClass(sweepModifier);
            card.AddChild(sweep);
        }

        private static VisualElement BuildFooter(string pathLabel, string trailingText)
        {
            var meta = new VisualElement().AddClass(NodeFooterClass);

            if (!string.IsNullOrEmpty(pathLabel))
            {
                meta.AddChild(MakeSelectable(new Label($"{pathLabel}:")
                    .AddClass(NodeRootLabelClass)));
            }

            meta.AddChild(MakeSelectable(new Label(trailingText)
                .AddClass(NodeRidClass)));

            return meta;
        }

        private bool IsFieldRequiredUnset(long fileId, string pathLabel)
        {
            if (string.IsNullOrEmpty(pathLabel) || _requiredViolations.Count == 0) return false;

            var propertyPath = SerializeReferenceGraphEditor.ToSerializedPropertyPath(pathLabel);
            foreach (var violation in _requiredViolations)
            {
                if (violation.FileId == fileId && violation.FieldPath == propertyPath) return true;
            }

            return false;
        }
    }
}
