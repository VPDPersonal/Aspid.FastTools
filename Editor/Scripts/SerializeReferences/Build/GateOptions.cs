// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    internal readonly struct GateOptions
    {
        public readonly bool ScanMissingTypes;
        public readonly bool ScanRequiredFields;

        private GateOptions(bool scanMissingTypes, bool scanRequiredFields)
        {
            ScanMissingTypes = scanMissingTypes;
            ScanRequiredFields = scanRequiredFields;
        }

        public static GateOptions Full =>
            new(true, true);

        public static GateOptions MissingOnly =>
            new(true, false);

        public static GateOptions RequiredOnly =>
            new(false, true);
    }
}
