using System;
using UnityEngine;
using Aspid.FastTools.Types;
using Aspid.FastTools.Samples.Types.Spawning;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.PlayerBuildCheck
{
    // Resolves the stored type names in a player and logs one AOTCHECK line per field, then quits.
    // Required fields resolve on every target. Reference fields hold names that stripping may remove: they are only logged.
    public sealed class PlayerBuildProbe : MonoBehaviour
    {
        [SerializeField] private SerializableType _preserved;
        [SerializeField] private SerializableType<Component> _engine;
        [SerializeField] private SerializableType _generic;
        [SerializeField] private SerializableType<ISpawnPattern> _samplePattern;

        [SerializeField] private SerializableType _plain;
        [SerializeField] private SerializableType<Component> _strippedEngine;

        private static string Backend =>
#if ENABLE_IL2CPP
            "il2cpp";
#else
            "mono";
#endif

        private static string Profiler =>
#if ENABLE_PROFILER
            "on";
#else
            "off";
#endif

        private static string Development =>
#if DEVELOPMENT_BUILD
            "on";
#else
            "off";
#endif

        private void Start()
        {
            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);

            using (this.Marker())
            {
                Report(kind: "required", field: "preserved", value: _preserved);
                Report(kind: "required", field: "engine", value: _engine);
                Report(kind: "required", field: "generic", value: _generic);
                Report(kind: "required", field: "samplePattern", value: _samplePattern);
                Report(kind: "reference", field: "plain", value: _plain);
                Report(kind: "reference", field: "strippedEngine", value: _strippedEngine);
            }

            using (this.Marker().WithName("Instantiate"))
                ReportInstance();

            Debug.Log($"AOTCHECK profiler={Profiler} development={Development} debug={Debug.isDebugBuild} " +
                $"backend={Backend} platform={Application.platform} unity={Application.unityVersion}");
            Debug.Log("AOTCHECK DONE");
            Application.Quit();
        }

        private void ReportInstance()
        {
            if (_samplePattern.Type is not { } type)
            {
                Debug.Log("AOTCHECK required samplePatternInstance MISSING type not resolved");
                return;
            }

            try
            {
                var pattern = (ISpawnPattern)Activator.CreateInstance(type);
                var position = pattern.GetPosition(index: 1, count: 4, radius: 2f);
                Debug.Log($"AOTCHECK required samplePatternInstance ok {type.FullName} {position}");
            }
            catch (Exception exception)
            {
                Debug.Log($"AOTCHECK required samplePatternInstance MISSING {exception.GetType().Name}: {exception.Message}");
            }
        }

        private static void Report(string kind, string field, SerializableTypeBase value)
        {
            var state = value?.Type is null
                ? "MISSING"
                : "ok";

            Debug.Log($"AOTCHECK {kind} {field} {state} name={value?.AssemblyQualifiedName} " +
                $"type={value?.Type?.FullName} IsMissing={value?.IsMissing}");
        }
    }
}
