using System;
using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Aspid.FastTools.UIElements;
using Aspid.FastTools.UIElements.Editors.Internal;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Ids.Editors
{
    internal static class IdStructUIToolkitDraw
    {
        private const string StyleSheetPath = "UI/Ids/Aspid-FastTools-Id-Drawer";
     
        private const string RowClass = "aspid-fasttools-id-drawer__row";
        private const string LabelClass = "aspid-fasttools-id-drawer__label";
        private const string DropdownClass = "aspid-fasttools-id-drawer__dropdown";
        private const string OpenButtonClass = "aspid-fasttools-id-drawer__open-button";
        private const string ButtonGroupClass = "aspid-fasttools-id-drawer__button-group";
        
        private const string OpenButtonTooltip = "Open the registry asset in Inspector";
        
        public static VisualElement Draw(IsStructDrawerContext ctx, bool isUnique)
        {
            var openButton = BuildOpenButton(ctx);
            var dropdownButton = BuildDropdownButton(openButton, ctx);
            
            var root = new VisualElement()
                .AddStyleSheetsFromResource(StyleSheetPath)
                .AddStyleSheetsFromResource(AspidStyles.DefaultStyleSheet)
                .AddChild(new PropertyField(ctx.Property).SetDisplay(DisplayStyle.None));
            
            Action onRegistryChanged = () => RefreshUI(dropdownButton, openButton, ctx);
            root.RegisterCallback<AttachToPanelEvent>(_ => IdRegistryResolver.RegistryChanged += onRegistryChanged);
            root.RegisterCallback<DetachFromPanelEvent>(_ => IdRegistryResolver.RegistryChanged -= onRegistryChanged);
            
            var buttonGroup = new VisualElement()
                .AddClass(ButtonGroupClass)
                .AddChild(dropdownButton)
                .AddChild(openButton);     
            
            var mainRow = new VisualElement()
                .AddClass(RowClass)
                .AddChild(buttonGroup);
            
            if (!string.IsNullOrEmpty(ctx.Label))
                mainRow.InsertChild(0, new Label(ctx.Label).AddClass(LabelClass));

            root.AddChild(mainRow);
            if (isUnique)
                root.AddChild(BuildWarningLabel(ctx));

            return root;
        }

        private static Button BuildDropdownButton(
            Button openButton,
            IsStructDrawerContext ctx)
        {
            var dropdownButton = new Button()
                .AddClass(DropdownClass);
            
            dropdownButton.AddClicked(() => OnClicked(Refresh));
            dropdownButton.TrackPropertyValue(ctx.IntIdProperty, _ => Refresh());
            dropdownButton.schedule.Execute(Refresh).StartingIn(0);
            return dropdownButton;
            
            void OnClicked(Action onApplied)
            {
                var window = EditorWindow.focusedWindow;
                if (window is null) return;
            
                var worldBound = dropdownButton.worldBound;
            
                var screenRect = new Rect(
                    window.position.x + worldBound.xMin,
                    window.position.y + worldBound.yMin,
                    worldBound.width,
                    worldBound.height);

                IdSelectorDropdownWindow.Show(screenRect, ctx, selected =>
                {
                    IsStructDrawerHelper.ApplySelection(selected, ctx);
                    onApplied();
                });
            }
            
            void Refresh() =>
                RefreshUI(dropdownButton, openButton, ctx);
        }

        private static Button BuildOpenButton(IsStructDrawerContext ctx) => new Button()
            .AddClass(OpenButtonClass)
            .SetTooltip(OpenButtonTooltip)
            .AddClicked(ctx.OpenRegistryAsset)
            .AddChild(new Image());

        private static Label BuildWarningLabel(IsStructDrawerContext ctx)
        {
            var warningLabel = new Label(text: "⚠ ID is not unique among assets of this type")
                .AddClass(ThemeStyle.LightClass)
                .AddClass(StatusStyle.WarningClass);
            
            warningLabel.TrackPropertyValue(ctx.StringIdProperty, prop =>
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
                var unique = UniqueIdIndex.IsUnique(ctx.DeclaringType, ctx.StringIdProperty.stringValue, ctx.GetCurrentAssetGuid());
                warningLabel.SetDisplay(unique ? DisplayStyle.None : DisplayStyle.Flex);
            }
        }
        
        private static void RefreshUI(
            Button dropdown,
            Button openButton,
            IsStructDrawerContext ctx)
        {
            var registry = ctx.FindRegistry();
            IsStructDrawerHelper.SyncStringFromInt(ctx);
            
            var caption = IsStructDrawerHelper.BuildCaption(ctx, out var isMissing);

            dropdown.SetText(caption);
            dropdown.EnableInClassList(StatusStyle.ErrorClass, isMissing);
            dropdown.EnableInClassList(ThemeStyle.LightnessClass, isMissing);
            
            openButton.SetEnabled(registry is not null);
        }
    }
}
