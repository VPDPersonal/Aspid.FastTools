using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using Aspid.FastTools.Samples.Types.Spawning;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.PlayerBuildCheck
{
    // Batch-mode entry point of scripts/player-builds.sh:
    //   -executeMethod Aspid.FastTools.PlayerBuildCheck.PlayerBuilds.Build
    //   -playerBuildTargets mac-mono,mac-il2cpp-minimal,mac-il2cpp-high,ios-sim -playerBuildOutput <dir>
    // Builds every target without BuildOptions.Development, appends a line per target to <dir>/builds.tsv and exits
    // with 1 when one fails.
    public static class PlayerBuilds
    {
        private const string ProductName = "PlayerBuildCheck";
        private const string BundleId = "tech.aspid.fasttools.playerbuildcheck";
        private const string ScenePath = "Assets/PlayerBuildCheck/PlayerBuildCheck.unity";

        private static readonly Target[] _targets =
        {
            new("mac-mono", BuildTarget.StandaloneOSX, ScriptingImplementation.Mono2x, ManagedStrippingLevel.Disabled),
            new("mac-il2cpp-minimal", BuildTarget.StandaloneOSX, ScriptingImplementation.IL2CPP, ManagedStrippingLevel.Minimal),
            new("mac-il2cpp-high", BuildTarget.StandaloneOSX, ScriptingImplementation.IL2CPP, ManagedStrippingLevel.High),
            new("ios-sim", BuildTarget.iOS, ScriptingImplementation.IL2CPP, ManagedStrippingLevel.High),
        };

        public static void Build()
        {
            try
            {
                var ids = Argument("-playerBuildTargets").Split(',', StringSplitOptions.RemoveEmptyEntries);
                var output = Argument("-playerBuildOutput");
                Directory.CreateDirectory(output);

                CreateScene();

                var hasFailed = false;

                foreach (var id in ids)
                {
                    var target = _targets.FirstOrDefault(target => target.Id == id)
                        ?? throw new ArgumentException($"Unknown target {id}");

                    var summary = BuildOne(target, output);
                    hasFailed |= summary.result != BuildResult.Succeeded;
                    // Appended: the script runs the Editor once per build target group into one output folder.
                    File.AppendAllLines(Path.Combine(output, "builds.tsv"), new[]
                    {
                        $"{id}\t{summary.result}\t{summary.totalErrors}\t{summary.totalWarnings}\t{summary.options}",
                    });
                }

                EditorApplication.Exit(hasFailed ? 1 : 0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorApplication.Exit(2);
            }
        }

        private static BuildSummary BuildOne(Target target, string output)
        {
            var namedTarget = NamedBuildTarget.FromBuildTargetGroup(BuildPipeline.GetBuildTargetGroup(target.BuildTarget));
            PlayerSettings.productName = ProductName;
            PlayerSettings.SetApplicationIdentifier(namedTarget, BundleId);
            PlayerSettings.SetScriptingBackend(namedTarget, target.Backend);
            PlayerSettings.SetManagedStrippingLevel(namedTarget, target.Stripping);

            if (target.BuildTarget == BuildTarget.StandaloneOSX)
                UnityEditor.OSXStandalone.UserBuildSettings.architecture = OSArchitecture.ARM64;

            if (target.BuildTarget == BuildTarget.iOS)
            {
                PlayerSettings.iOS.sdkVersion = iOSSdkVersion.SimulatorSDK;
                PlayerSettings.iOS.simulatorSdkArchitecture = AppleMobileArchitectureSimulator.ARM64;
                // Xcode 27 refuses the default deployment target of Unity 6000.0 (13.0) for the simulator.
                PlayerSettings.iOS.targetOSVersionString = "15.0";
            }

            var path = target.BuildTarget == BuildTarget.iOS
                ? Path.Combine(output, target.Id)
                : Path.Combine(output, target.Id, $"{ProductName}.app");

            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = path,
                target = target.BuildTarget,
                options = BuildOptions.None,
            });

            var summary = report.summary;
            Debug.Log($"PLAYERBUILD {target.Id} result={summary.result} errors={summary.totalErrors} " +
                $"warnings={summary.totalWarnings} options={summary.options} time={summary.totalTime} path={path}");

            foreach (var message in report.steps.SelectMany(step => step.messages))
            {
                if (message.type is LogType.Error or LogType.Exception or LogType.Warning)
                    Debug.Log($"PLAYERBUILD {target.Id} {message.type}: {message.content}");
            }

            return summary;
        }

        private static void CreateScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var probe = new GameObject(ProductName).AddComponent<PlayerBuildProbe>();

            // The names are written as strings: a typeof in the player code would keep the type from stripping.
            var serialized = new SerializedObject(probe);
            Set(serialized, field: "_preserved", name: typeof(PlayerBuildProbePreserved).AssemblyQualifiedName);
            Set(serialized, field: "_engine", name: typeof(Transform).AssemblyQualifiedName);
            Set(serialized, field: "_generic", name: typeof(List<int>).AssemblyQualifiedName);
            Set(serialized, field: "_plain", name: typeof(PlayerBuildProbeTarget).AssemblyQualifiedName);
            Set(serialized, field: "_strippedEngine", name: typeof(BoxCollider).AssemblyQualifiedName);
            Set(serialized, field: "_samplePattern", name: typeof(LinePattern).AssemblyQualifiedName);
            serialized.ApplyModifiedPropertiesWithoutUndo();

            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath)!);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
        }

        private static void Set(SerializedObject serialized, string field, string name)
        {
            var property = serialized.FindProperty($"{field}._assemblyQualifiedName")
                ?? throw new InvalidOperationException($"{field}._assemblyQualifiedName not found");

            property.stringValue = name;
        }

        private static string Argument(string name)
        {
            var args = Environment.GetCommandLineArgs();
            var index = Array.IndexOf(args, name);

            return index >= 0 && index + 1 < args.Length
                ? args[index + 1]
                : throw new ArgumentException($"{name} <value> is required");
        }

        private sealed class Target
        {
            public Target(string id, BuildTarget buildTarget, ScriptingImplementation backend, ManagedStrippingLevel stripping)
            {
                Id = id;
                BuildTarget = buildTarget;
                Backend = backend;
                Stripping = stripping;
            }

            public string Id { get; }

            public BuildTarget BuildTarget { get; }

            public ScriptingImplementation Backend { get; }

            public ManagedStrippingLevel Stripping { get; }
        }
    }
}
