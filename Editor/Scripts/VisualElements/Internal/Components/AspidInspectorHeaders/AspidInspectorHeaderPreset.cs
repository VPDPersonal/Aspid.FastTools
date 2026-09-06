// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors.Internal
{
    internal struct AspidInspectorHeaderPreset
    {
        public static AspidInspectorHeaderPreset Default => new AspidInspectorHeaderPreset()
            .SetText(string.Empty)
            .SetSubtext(string.Empty)
            .SetStatus(StatusStyle.Type.Success);

        public string Text;

        public string Subtext;

        public StatusStyle.Type Status;

        public AspidInspectorHeaderPreset SetText(string value)
        {
            Text = value;
            return this;
        }

        public AspidInspectorHeaderPreset SetSubtext(string value)
        {
            Subtext = value;
            return this;
        }

        public AspidInspectorHeaderPreset SetStatus(StatusStyle.Type value)
        {
            Status = value;
            return this;
        }
    }
}
