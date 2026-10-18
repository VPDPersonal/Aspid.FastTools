using System;
using UnityEngine;
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
        private const string NodeCompactClass = NodeClass + "--compact";
        private const string UnassignedFieldClass = RootClass + "__unassigned-field";
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
        private const string NodeTypeIconClass = RootClass + "__node-type-icon";
        private const string NodeRidClass = RootClass + "__node-rid";
        private const string NodeBadgesClass = RootClass + "__node-badges";

        private const string SharedClass = RootClass + "__shared";
        private const string SharedTextClass = RootClass + "__shared-text";

        private const string ChipClass = RootClass + "__chip";
        private const string ClearOrphanClass = RootClass + "__clear-orphan";

        private const string FixCollapsedText = "Fix Missing  ▼";
        private const string ChangeCollapsedText = "Change  ▼";
        private const string AssignCollapsedText = "Assign  ▼";
        private const string ClearCollapsedText = "Clear";

        private const string AssignRequiredCollapsedText = "Assign Required  ▼";

        private const string MigrateFixCollapsedText = "Fix  ▼";

        private const string EmptySlotText = TypeSelectorHelpers.NoneOption;

        // A repeat is a reference whose children sit on another card: this card keeps its own field path, but does not draw them again.
        private VisualElement BuildNodeCard(string assetPath, ReferenceGraphDocument document, ReferenceGraphNode? node, long rid, string pathLabel, bool isOrphan, bool isRepeat)
        {
            var missing = node is { Resolves: false, StoredType: { IsEmpty: false } };

            // Unity migrates reachable references in memory; orphaned payloads are never loaded.
            Type migrationTarget = null;
            var isMigration = missing && !isOrphan &&
                SerializeReferenceGraphAnalysis.IsPendingMigration(assetPath, document.FileId, rid,
                    node.Value.StoredType, _constraints, out migrationTarget);

            SerializeReferenceRepairSuggestions.RepairCandidate suggestion = default;
            var hasSuggestion = missing && !isMigration &&
                SerializeReferenceGraphAnalysis.TryGetSuggestion(assetPath, document.FileId, rid,
                    node.Value.StoredType, _constraints, out suggestion);

            var title = node?.ShortName ?? $"rid {rid}";
            var suggestionTitle = hasSuggestion ? GetQualifiedTitle(suggestion.Type) : null;
            if (hasSuggestion && TryQualifySameNames(node.Value.StoredType, suggestion.Type, out var storedTitle, out _))
                title = storedTitle;

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

            var typeLabel = new AspidLabel(title, typePreset)
                .AddClass(NodeTypeClass)
                .SetPickingMode(PickingMode.Ignore);
            if (node is not null && !node.Value.StoredType.IsEmpty)
                typeLabel.tooltip = node.Value.FullName;

            var nodeType = node is { Resolves: true } ? SerializeReferenceHelpers.ResolveStoredType(node.Value.StoredType) : null;
            var bandRow = BuildBandRow(typeLabel, BuildBadges(document, rid), nodeType);

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

                var action = isMigration
                    ? BuildMigrationRow(assetPath, fileId, rid, migrationTarget)
                    : hasSuggestion
                        ? BuildSmartFixRow(assetPath, fileId, rid, suggestion, suggestionTitle)
                        : null;
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
                // Orphans have no live property to edit, so their band holds the file-level Clear, which asks before
                // it deletes the payload.
                var band = new AspidGradientButton(ClearCollapsedText, _ => ClearOrphan(assetPath, fileId, rid))
                    .AddClass(NodeBandClass);
                band.AddLeadingContent(bandRow);
                card.AddChild(band);
                RegisterNavBand(band, card, () => ClearOrphan(assetPath, fileId, rid));
                AddBandDivider(card, band, sweepModifier: null);
            }

            var meta = BuildFooter(pathLabel, isRepeat ? $"rid {rid} · children on another card" : $"rid {rid}");

            // The Fix Missing band takes the header of an orphan whose type is missing, so its Clear stays in the footer.
            if (isOrphan && missing) meta.AddChild(BuildClearOrphanButton(assetPath, fileId, rid));

            card.AddChild(meta);

            return card;
        }

        // Clear deletes the payload from the file without Undo, so it is a plain button that turns red on hover.
        private Button BuildClearOrphanButton(string assetPath, long fileId, long rid)
        {
            var clear = new Button(() => ClearOrphan(assetPath, fileId, rid)) { text = "Clear" }
                .AddClass(AspidStyles.ActionButtonClass)
                .AddClass(AspidStyles.ActionButtonDangerClass)
                .AddClass(ClearOrphanClass);
            RegisterNavTarget(clear, () => ClearOrphan(assetPath, fileId, rid));

            return clear;
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

        // An optional field that holds nothing, in its place among the fields: one muted band with the field path and
        // Assign, so it takes a line instead of a card. Only an empty field offers Assign, so no <None> is needed.
        private VisualElement BuildUnassignedRow(string pathLabel, Action<AspidGradientButton> openPicker)
        {
            var card = new AspidBox(AspidBoxPreset.Default.SetTheme(ThemeStyle.Type.Darkness))
                .AddClass(NodeClass)
                .AddClass(NodeEmptyClass)
                .AddClass(NodeCompactClass);

            var field = new Label(pathLabel)
                .AddClass(UnassignedFieldClass)
                .SetPickingMode(PickingMode.Ignore);

            AspidGradientButton band = null;
            band = new AspidGradientButton(AssignCollapsedText, _ => openPicker(band))
                .AddClass(NodeBandClass);
            band.AddLeadingContent(BuildBandRow(field, badges: null));
            RegisterNavBand(band, card, () => openPicker(band));

            return card.AddChild(band);
        }

        // Required fields with no slot card; scene assets cannot be object-loaded for inline editing.
        private VisualElement BuildRequiredOnlyCard(GateViolation violation, string pathLabel)
        {
            var isReference = violation.Rid != 0;

            var card = new AspidBox(AspidBoxPreset.Default.SetTheme(ThemeStyle.Type.Darkness))
                .AddClass(NodeClass);

            var bandRow = BuildBandRow(
                BuildRequiredNoneLabel(isReference ? "Required reference is not set" : "Required type is not set"),
                badges: null);

            if (SerializeReferenceHelpers.IsScene(violation.AssetPath))
            {
                card.AddChild(bandRow);
                AddBandDivider(card, band: null, sweepModifier: null);
            }
            else
            {
                var graphPath = SerializeReferenceGraphEditor.ToGraphPath(violation.FieldPath);
                AspidGradientButton band = null;

                void OpenPicker()
                {
                    if (isReference) OpenLivePicker(violation.AssetPath, violation.FileId, graphPath, band);
                    else OpenTypeNamePicker(violation.AssetPath, violation.FileId, violation.FieldPath, band);
                }

                band = new AspidGradientButton(AssignRequiredCollapsedText, _ => OpenPicker())
                    .AddClass(NodeBandClass)
                    .AddClass(NodeBandMissingClass);
                band.AddLeadingContent(bandRow);
                card.AddChild(band);
                RegisterNavBand(band, card, OpenPicker);
                AddBandDivider(card, band, NodeSweepMissingClass);
            }

            card.AddChild(BuildFooter(pathLabel, "unassigned"));

            return card;
        }

        // A type name field that holds a type, drawn like a reference card.
        private VisualElement BuildTypeNameCard(string assetPath, TypeNameField field, string pathLabel)
        {
            var type = Type.GetType(field.TypeName, throwOnError: false);

            var card = new AspidBox(AspidBoxPreset.Default.SetTheme(ThemeStyle.Type.Darkness))
                .AddClass(NodeClass);

            var typePreset = AspidLabelPreset.Default
                .SetLabelSize(AspidLabelSizeStyle.Type.H5)
                .SetLineSize(AspidDividingLineSizeStyle.Type.None);
            typePreset = type is null
                ? typePreset.SetLabelStatus(StatusStyle.Type.Warning)
                : typePreset.SetLabelTheme(ThemeStyle.Type.Lightness);

            var typeLabel = new AspidLabel(TypeSelectorHelpers.GetTypeSelectorTitle(type,
                        assemblyQualifiedName: TypeSelectorHelpers.GetMissingDisplayName(field.TypeName)), typePreset)
                .AddClass(NodeTypeClass)
                .SetPickingMode(PickingMode.Ignore);
            typeLabel.tooltip = type?.FullName ?? field.TypeName;

            AspidGradientButton band = null;
            void OpenPicker() => OpenTypeNamePicker(assetPath, field.FileId, field.FieldPath, band);

            band = new AspidGradientButton(ChangeCollapsedText, _ => OpenPicker())
                .AddClass(NodeBandClass);
            band.AddLeadingContent(BuildBandRow(typeLabel, badges: null, type));
            card.AddChild(band);
            RegisterNavBand(band, card, OpenPicker);
            AddBandDivider(card, band, sweepModifier: null);

            card.AddChild(BuildFooter(pathLabel, "type name"));

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

        // type adds the icon the type picker shows for it.
        private static VisualElement BuildBandRow(VisualElement typeLabel, VisualElement badges, Type type = null)
        {
            var row = new VisualElement().AddClass(NodeBandRowClass);

            var icon = TypeSelectorIconResolver.ResolveForType(type);
            if (icon) row.AddChild(new Image { image = icon, scaleMode = ScaleMode.ScaleToFit }.AddClass(NodeTypeIconClass));

            row.AddChild(typeLabel);
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

            // Read like the counts beside a Project References title: a coloured dot and a muted word, not a pill. The
            // dot has the colour of the rid, so the cards that share one instance show the same dot.
            var chip = new VisualElement().AddClass(ChipClass);
            chip.style.backgroundColor = SerializeReferenceRidColor.ForRid(rid);

            var shared = new VisualElement()
                .AddClass(SharedClass)
                .AddChild(chip)
                .AddChild(new Label("shared").AddClass(SharedTextClass));
            shared.tooltip = "Several fields point at this one instance, so editing it changes each of them. "
                + "Cards with a dot of the same colour share it.";

            return badges.AddChild(shared);
        }

        private VisualElement BuildMigrationRow(string assetPath, long fileId, long rid, Type migrationTarget) =>
            BuildNodeActionRow(
                $"Migrate → {GetQualifiedTitle(migrationTarget)}",
                $"This entry resolves to {migrationTarget.FullName} via its declared [MovedFrom] — Unity " +
                "already migrates it in memory when the asset loads. Migrating rewrites the stored type " +
                "name in the file so it matches the code.",
                info: true,
                () => ApplyFix(assetPath, fileId, rid, migrationTarget.AssemblyQualifiedName));

        private VisualElement BuildSmartFixRow(string assetPath, long fileId, long rid,
            SerializeReferenceRepairSuggestions.RepairCandidate suggestion, string targetTitle) =>
            BuildNodeActionRow(
                $"Smart Fix → {targetTitle}",
                SerializeReferenceHelpers.GetSuggestionDetail(suggestion),
                info: false,
                () => ApplyFix(assetPath, fileId, rid, suggestion.Type.AssemblyQualifiedName));

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
