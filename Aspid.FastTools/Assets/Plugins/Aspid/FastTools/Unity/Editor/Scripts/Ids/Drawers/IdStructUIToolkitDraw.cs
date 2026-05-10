using System;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Aspid.FastTools.UIElements;
using Aspid.FastTools.UIElements.Editors.Internal;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Ids.Editors
{
    internal static class IdStructUIToolkitDraw
    {
        public static VisualElement Draw(IsStructDrawerContext ctx, bool isUnique)
        {
            var idField = new InspectorIdField(ctx.Label, ctx.Property)
            {
                IdType = ctx.FieldType,
            };

            var root = new VisualElement()
                .AddStyleSheetsFromResource(AspidStyles.DefaultStyleSheet)
                .AddChild(new PropertyField(ctx.Property).SetDisplay(DisplayStyle.None))
                .AddChild(idField);

            Action refresh = () =>
            {
                IsStructDrawerHelper.SyncStringFromInt(ctx);
                idField.RefreshFromBoundProperty();
            };

            root.RegisterCallback<AttachToPanelEvent>(_ => IdRegistryResolver.RegistryChanged += refresh);
            root.RegisterCallback<DetachFromPanelEvent>(_ => IdRegistryResolver.RegistryChanged -= refresh);
            idField.schedule.Execute(() => refresh()).StartingIn(0);

            if (isUnique)
                root.AddChild(BuildWarningLabel(ctx));

            return root;
        }

        private static Label BuildWarningLabel(IsStructDrawerContext ctx)
        {
            var warningLabel = new Label(text: "⚠ ID is not unique among assets of this type")
                .AddClass(ThemeStyle.LightClass)
                .AddClass(StatusStyle.WarningClass);

            warningLabel.TrackPropertyValue(ctx.StringProperty, prop =>
            {
                UniqueIdIndex.RefreshAsset(prop.serializedObject.targetObject);
                Refresh();
            });

            Action onIndexChanged = Refresh;
            warningLabel.RegisterCallback<AttachToPanelEvent>(_ => UniqueIdIndex.IndexChanged += onIndexChanged);
            warningLabel.RegisterCallback<DetachFromPanelEvent>(_ => UniqueIdIndex.IndexChanged -= onIndexChanged);
            warningLabel.schedule.Execute(Refresh).StartingIn(0);

            return warningLabel;

            void Refresh()
            {
                var unique = UniqueIdIndex.IsUnique(ctx.DeclaringType, ctx.StringProperty.stringValue, ctx.GetCurrentAssetGuid());
                warningLabel.SetDisplay(unique ? DisplayStyle.None : DisplayStyle.Flex);
            }
        }
    }
}
