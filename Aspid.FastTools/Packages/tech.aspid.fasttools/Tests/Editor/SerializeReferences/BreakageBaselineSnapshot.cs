using UnityEditor;

namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    // The session's own baselines of both breakage detectors. A change of the excluded folders resets them, so a test
    // that changes the folders takes a snapshot first and restores it after it puts the folders back.
    internal sealed class BreakageBaselineSnapshot
    {
        private readonly string _types;
        private readonly string _names;
        private readonly bool _typesEstablished;
        private readonly bool _namesEstablished;

        public BreakageBaselineSnapshot()
        {
            _types = SerializeReferenceBreakageDetector.ExportBaseline();
            _typesEstablished = SerializeReferenceBreakageDetector.IsEstablished;

            _names = TypeNameBreakageDetector.ExportBaseline();
            _namesEstablished = TypeNameBreakageDetector.IsEstablished;
        }

        public void Restore()
        {
            // A sweep the test started must not finish over the restored baseline.
            SerializeReferenceBreakageDetector.ResetForTests();
            TypeNameBreakageDetector.ResetForTests();

            SessionState.SetBool(SerializeReferenceBreakageDetector.EstablishedKey, _typesEstablished);
            SerializeReferenceBreakageDetector.ImportBaseline(_types);
            SerializeReferenceBreakageDetector.PersistBaseline();

            SessionState.SetBool(TypeNameBreakageDetector.EstablishedKey, _namesEstablished);
            TypeNameBreakageDetector.ImportBaseline(_names);
            TypeNameBreakageDetector.PersistBaseline();
        }
    }
}
