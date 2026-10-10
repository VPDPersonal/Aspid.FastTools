using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace UnityEngine
{
    // Stands in for UnityEngine.Debug. The engine logs only from its catch blocks; the messages are collected so
    // FailOnLogErrorAttribute can fail the test the way Unity's Test Runner fails a test on an unexpected error log.
    internal static class Debug
    {
        private static readonly List<string> _errors = new();

        public static void LogError(object message) =>
            _errors.Add(message?.ToString() ?? "Null");

        internal static string[] TakeErrors()
        {
            var errors = _errors.ToArray();
            _errors.Clear();
            return errors;
        }
    }
}
