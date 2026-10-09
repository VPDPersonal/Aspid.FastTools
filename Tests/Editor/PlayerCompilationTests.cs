using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using UnityEngine.TestTools;
using UnityEditor.Build.Player;
using System.Collections.Generic;

namespace Aspid.FastTools.Editors.Tests
{
    // The EditMode run compiles every assembly for the Editor, where UnityEditor always exists. A Runtime/ file that
    // uses it without #if UNITY_EDITOR passes here and fails in the user's player build with CS0234. Compiling the
    // same sources for a player, without the Editor, catches that.
    internal sealed class PlayerCompilationTests
    {
        private const string RuntimeAssembly = "Aspid.FastTools.dll";
        private const string MathAssembly = "Aspid.FastTools.VisualElements.Math.dll";

        // tests.yml passes this argument to Unity. Without it, for example when a consumer project runs the package
        // tests, a project that lacks player support or com.unity.mathematics ignores the test instead of failing it.
        private const string StrictSetupArgument = "-aspidFastToolsStrictSetup";

        // A release and a development player define different symbols (DEBUG, DEVELOPMENT_BUILD), so both are compiled.
        [TestCase(ScriptCompilationOptions.None)]
        [TestCase(ScriptCompilationOptions.DevelopmentBuild)]
        public void Scripts_CompileForStandalonePlayer(ScriptCompilationOptions options)
        {
            var target = EditorUserBuildSettings.selectedStandaloneTarget;
            RequireSetup(
                BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone, target),
                $"The Editor has no {target} player support to compile the scripts for.");

            var settings = new ScriptCompilationSettings
            {
                group = BuildTargetGroup.Standalone,
                target = target,
                options = options,
            };

            var errors = new List<string>();
            var outputFolder = FileUtil.GetUniqueTempPathInProject();

            // The compiler logs each error; they are gathered here so that one failure message lists all of them.
            void OnLogMessage(string message, string stackTrace, LogType type)
            {
                if (type is LogType.Error or LogType.Exception or LogType.Assert)
                    errors.Add(message);
            }

            var ignoreFailingMessages = LogAssert.ignoreFailingMessages;
            Application.logMessageReceived += OnLogMessage;
            LogAssert.ignoreFailingMessages = true;

            ScriptCompilationResult result;
            try { result = PlayerBuildInterface.CompilePlayerScripts(settings, outputFolder); }
            finally
            {
                Application.logMessageReceived -= OnLogMessage;
                LogAssert.ignoreFailingMessages = ignoreFailingMessages;

                if (Directory.Exists(outputFolder))
                    Directory.Delete(outputFolder, recursive: true);
            }

            Assert.IsEmpty(
                errors,
                $"The scripts do not compile for the {target} player ({options}):\n{string.Join("\n", errors)}");
            CollectionAssert.Contains(result.assemblies, RuntimeAssembly);

            // Without com.unity.mathematics its asmdef is skipped and nothing compiles it: the test project needs it.
            RequireSetup(
                result.assemblies.Contains(MathAssembly),
                "Add com.unity.mathematics to the project that runs the tests.");
        }

        private static void RequireSetup(bool isReady, string message)
        {
            if (isReady) return;
            if (Environment.GetCommandLineArgs().Contains(StrictSetupArgument)) Assert.Fail(message);

            Assert.Ignore(message);
        }
    }
}
