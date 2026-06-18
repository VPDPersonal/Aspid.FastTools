using UnityEngine;
using UnityEngine.UIElements;
using Aspid.FastTools.UIElements.Editors.Internal;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    /// <summary>
    /// Placeholder for the Settings tab (the rightmost square tab). Project-wide FastTools settings will live here;
    /// the view is added ahead of the settings themselves so the window's tab layout is already in place.
    /// </summary>
    internal sealed class SettingsView : VisualElement
    {
        public SettingsView()
        {
            style.flexGrow = 1;
            style.alignItems = Align.Center;
            style.justifyContent = Justify.Center;

            Add(new AspidLabel("Settings") { LabelSize = AspidLabelSizeStyle.Type.H2 });

            var note = new Label("Project-wide FastTools settings will live here.");
            note.style.marginTop = 6;
            note.style.unityTextAlign = TextAnchor.MiddleCenter;
            Add(note);
        }
    }
}
