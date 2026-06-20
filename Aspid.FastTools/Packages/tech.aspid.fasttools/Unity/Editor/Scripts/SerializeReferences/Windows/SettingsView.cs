using UnityEngine.UIElements;
using Aspid.FastTools.UIElements.Editors.Internal;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    /// <summary>
    /// The Settings tab (the rightmost square tab): the SerializeReference toolset's project-wide settings surfaced
    /// inside the window so the toolset is self-contained. It renders the very same controls as the Project Settings
    /// page — both call <see cref="SerializeReferenceSettingsUI.BuildControls"/>, bound to
    /// <see cref="SerializeReferenceSettings"/>, so there is one definition and no duplicated UI.
    /// </summary>
    internal sealed class SettingsView : VisualElement
    {
        public SettingsView()
        {
            style.flexGrow = 1;
            style.paddingLeft = 14;
            style.paddingRight = 14;
            style.paddingTop = 10;

            Add(new AspidLabel("Settings") { LabelSize = AspidLabelSizeStyle.Type.H2 });

            // A vertical scroll guards the controls against the window's 360px min-height — the toolbar and footer
            // already claim a slice, so the excluded-folders field can overflow on a short window.
            var scroll = new ScrollView(ScrollViewMode.Vertical) { style = { flexGrow = 1, marginTop = 8 } };
            SerializeReferenceSettingsUI.BuildControls(scroll.contentContainer);
            Add(scroll);
        }
    }
}
