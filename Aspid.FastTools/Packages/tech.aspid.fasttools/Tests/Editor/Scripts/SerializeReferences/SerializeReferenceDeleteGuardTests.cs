using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using NUnit.Framework;
using System.Collections;
using UnityEngine.TestTools;
using System.Collections.Generic;
using Aspid.FastTools.SerializeReferences.Editors;
using PromptAnswer = Aspid.FastTools.SerializeReferences.Editors.SerializeReferenceDeleteGuard.PromptAnswer;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Tests.Editors.SerializeReferences
{
    /// <summary>
    /// Coverage for <see cref="SerializeReferenceDeleteGuard"/>: which types a deleted script takes with it, how their
    /// usages are counted without and with a warm <see cref="SerializeReferenceTypeUsageIndex"/>, and how many times
    /// one delete action asks.
    /// </summary>
    [TestFixture]
    internal sealed class SerializeReferenceDeleteGuardTests
    {
        // GUIDs from the fixture scripts' .meta files, so the lookup does not depend on where the package is installed.
        private const string FixturesScriptGuid = "e6234341c7874a99b575757146d497c8";
        private const string SettingsScriptGuid = "16332bb6fc28429f8b7968f66638aadd";
        private const string GenericScriptGuid = "0d4aeac1171c40c59cd879d9b181d89a";
        private const string NamesakeScriptGuid = "5b0e3c7a9d2f4e18a6c1b7d3e9f02a41";
        private const string PartialScriptGuid = "8c2f4a61e7b94d0fa3e5c9b1d6a7f203";
        private const string PartialPartScriptGuid = "a4d91e7c3b6f482e9c0a5f1b7e2d8c64";
        private const string ProbeAssetPath = "Assets/__AspidDeleteGuardProbe__.asset";

        private const double DelayCallTimeoutSeconds = 10;

        // The asset name is serialized as m_Name, so the text names DeleteGuardPistol although it holds no reference.
        private const string PlainAssetPath = "Assets/__AspidDeleteGuardPlain_DeleteGuardPistol__.asset";

        [TearDown]
        public void TearDown()
        {
            AssetDatabase.DeleteAsset(ProbeAssetPath);
            AssetDatabase.DeleteAsset(PlainAssetPath);
            SerializeReferenceTypeUsageIndex.Reset();
            SerializeReferenceDeleteGuard.ResetSweep();
            SerializeReferenceDeleteGuard.ResetAnswer();
            SerializeReferenceDeleteGuard.ResetSeams();
        }

        [Test]
        public void ResolveCandidateTypes_FileWithoutClassNamedAfterIt_ReturnsEveryDeclaredReferenceType()
        {
            var types = Resolve(FixturesScriptGuid);

            // DeleteGuardPistol<T> of another script shares the name, not the arity, and the global DeleteGuardPistol
            // the name, not the namespace, so both stay out.
            CollectionAssert.AreEquivalent(
                new[] { typeof(DeleteGuardPistol), typeof(DeleteGuardRifle), typeof(DeleteGuardArmory.Crate) },
                types);
        }

        [Test]
        public void ResolveCandidateTypes_GenericTypeInNestedNamespaceBlocks_ReturnsOnlyThatType()
        {
            var types = Resolve(GenericScriptGuid);

            CollectionAssert.AreEquivalent(new[] { typeof(DeleteGuardPistol<>) }, types);
        }

        [Test]
        public void ResolveCandidateTypes_ScriptWithOnlyUnityObjectTypes_ReturnsNothing()
        {
            // The script names other fixture types in a comment and a string, which declare nothing.
            var types = Resolve(SettingsScriptGuid);

            CollectionAssert.IsEmpty(types);
        }

        [Test]
        public void ResolveCandidateTypes_GlobalAndUnicodeNamedTypes_ReturnsThemWithoutNamespacedNamesake()
        {
            var types = Resolve(NamesakeScriptGuid);

            CollectionAssert.AreEquivalent(new[] { typeof(global::DeleteGuardPistol), typeof(ОружиеDeleteGuard) }, types);
        }

        [Test]
        public void ResolveCandidateTypes_PartialTypeWithAnotherPart_IsKeptOnlyWhenEveryPartIsDeleted()
        {
            CollectionAssert.AreEquivalent(new[] { typeof(DeleteGuardRevolver) }, Resolve(PartialScriptGuid));
            CollectionAssert.IsEmpty(Resolve(PartialPartScriptGuid));
            CollectionAssert.AreEquivalent(new[] { typeof(DeleteGuardRevolver), typeof(DeleteGuardShotgun) },
                Resolve(PartialScriptGuid, PartialPartScriptGuid));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void CountUsages_CountsSiblingAndNestedTypes(bool warmIndex)
        {
            CreateProbe(new DeleteGuardPistol(), new DeleteGuardArmory.Crate());
            if (warmIndex) SerializeReferenceTypeUsageIndex.FindUsages("warm-up");

            Assert.AreEqual(warmIndex, SerializeReferenceTypeUsageIndex.IsWarm);

            var types = Resolve(FixturesScriptGuid);
            var samples = new List<string>();
            var counts = SerializeReferenceDeleteGuard.CountUsages(types, samples);

            Assert.IsNotNull(counts);
            Assert.AreEqual(1, counts[typeof(DeleteGuardPistol)]);
            Assert.AreEqual(1, counts[typeof(DeleteGuardArmory.Crate)]);
            Assert.AreEqual(0, counts[typeof(DeleteGuardRifle)]);
            CollectionAssert.AreEqual(new[] { ProbeAssetPath }, samples);
        }

        // Unity writes the Cyrillic class name as escapes, so the file never holds the name as written.
        [TestCase(false)]
        [TestCase(true)]
        public void CountUsages_CountsTypeWithNonAsciiName(bool warmIndex)
        {
            CreateProbe(new ОружиеDeleteGuard(), b: null);
            if (warmIndex) SerializeReferenceTypeUsageIndex.FindUsages("warm-up");

            StringAssert.DoesNotContain("ОружиеDeleteGuard", File.ReadAllText(ProbeAssetPath));

            var counts = SerializeReferenceDeleteGuard.CountUsages(Resolve(NamesakeScriptGuid), samplePaths: null);

            Assert.IsNotNull(counts);
            Assert.AreEqual(1, counts[typeof(ОружиеDeleteGuard)]);
        }

        [Test]
        public void CountUsages_AssetNamingClassWithoutManagedReferences_IsNotCounted()
        {
            CreatePlainAsset();

            var counts = SerializeReferenceDeleteGuard.CountUsages(new List<Type> { typeof(DeleteGuardPistol) }, samplePaths: null);

            Assert.AreEqual(0, counts[typeof(DeleteGuardPistol)]);
        }

        [Test]
        public void CountUsages_ReusedSweep_SecondCallReadsOnlyAssetsThatHoldManagedReferences()
        {
            CreateProbe(new DeleteGuardPistol(), new DeleteGuardArmory.Crate());
            CreatePlainAsset();

            var first = SerializeReferenceDeleteGuard.CountUsages(
                new List<Type> { typeof(DeleteGuardPistol) }, samplePaths: null, reuseSweep: true);

            Assert.AreEqual(1, first[typeof(DeleteGuardPistol)]);

            var swept = SerializeReferenceDeleteGuard.SweptReferencePaths;
            CollectionAssert.Contains(swept, ProbeAssetPath);
            CollectionAssert.DoesNotContain(swept, PlainAssetPath);

            var samples = new List<string>();
            var second = SerializeReferenceDeleteGuard.CountUsages(
                new List<Type> { typeof(DeleteGuardArmory.Crate), typeof(DeleteGuardRifle) }, samples, reuseSweep: true);

            Assert.AreEqual(1, second[typeof(DeleteGuardArmory.Crate)]);
            Assert.AreEqual(0, second[typeof(DeleteGuardRifle)]);
            CollectionAssert.AreEqual(new[] { ProbeAssetPath }, samples);
            Assert.AreSame(swept, SerializeReferenceDeleteGuard.SweptReferencePaths, "The second call must not sweep the project again.");

            SerializeReferenceDeleteGuard.ResetSweep();
            Assert.IsNull(SerializeReferenceDeleteGuard.SweptReferencePaths);
        }

        [Test]
        public void CountUsages_WithoutReusedSweep_KeepsNothing()
        {
            CreateProbe(new DeleteGuardPistol(), new DeleteGuardArmory.Crate());

            var counts = SerializeReferenceDeleteGuard.CountUsages(new List<Type> { typeof(DeleteGuardPistol) }, samplePaths: null);

            Assert.AreEqual(1, counts[typeof(DeleteGuardPistol)]);
            Assert.IsNull(SerializeReferenceDeleteGuard.SweptReferencePaths);
        }

        [Test]
        public void CountUsages_ReusedSweep_AssetSavedAfterSweep_IsCounted()
        {
            var types = new List<Type> { typeof(DeleteGuardPistol) };

            var first = SerializeReferenceDeleteGuard.CountUsages(types, samplePaths: null, reuseSweep: true);

            Assert.AreEqual(0, first[typeof(DeleteGuardPistol)]);
            Assert.IsNotNull(SerializeReferenceDeleteGuard.SweptReferencePaths);

            // The import runs the postprocessor, which drops the kept list, so the next call sweeps again.
            CreateProbe(new DeleteGuardPistol(), new DeleteGuardArmory.Crate());

            var second = SerializeReferenceDeleteGuard.CountUsages(types, samplePaths: null, reuseSweep: true);

            Assert.AreEqual(1, second[typeof(DeleteGuardPistol)]);
        }

        [Test]
        public void CountUsages_CancelledSweep_CancelsLaterCallsThatReuseIt()
        {
            CreateProbe(new DeleteGuardPistol(), new DeleteGuardArmory.Crate());
            SerializeReferenceDeleteGuard.KeepSweepUntilTickEnds(cancelled: true);

            var types = new List<Type> { typeof(DeleteGuardPistol) };

            Assert.IsNull(SerializeReferenceDeleteGuard.CountUsages(types, samplePaths: null, reuseSweep: true));

            // A call without reuseSweep is stateless and ignores the flag.
            var counts = SerializeReferenceDeleteGuard.CountUsages(types, samplePaths: null);

            Assert.AreEqual(1, counts[typeof(DeleteGuardPistol)]);
        }

        [Test]
        public void CountUsages_CancelledSweepAndWarmIndex_StillCounts()
        {
            CreateProbe(new DeleteGuardPistol(), new DeleteGuardArmory.Crate());
            SerializeReferenceTypeUsageIndex.FindUsages("warm-up");
            SerializeReferenceDeleteGuard.KeepSweepUntilTickEnds(cancelled: true);

            var counts = SerializeReferenceDeleteGuard.CountUsages(
                new List<Type> { typeof(DeleteGuardPistol) }, samplePaths: null, reuseSweep: true);

            Assert.IsNotNull(counts);
            Assert.AreEqual(1, counts[typeof(DeleteGuardPistol)]);
        }

        [Test]
        public void GuardAsset_SingleUsedScript_ShowsThePlainDialog()
        {
            CreateListProbe(new DeleteGuardPistol());
            var scriptPath = ScriptPath(FixturesScriptGuid);
            var prompts = StubPrompts(selected: new[] { scriptPath }, PromptAnswer.Delete);

            Assert.AreEqual(AssetDeleteResult.DidNotDelete, SerializeReferenceDeleteGuard.GuardAsset(scriptPath));

            Assert.AreEqual(1, prompts.Count);
            Assert.AreEqual("Delete Script", prompts[0].Title);
            Assert.IsFalse(prompts[0].OfferRest);
            Assert.AreEqual(
                "\"DeleteGuardPistol\" is used as a [SerializeReference] managed reference in 1 place(s):\n\n" +
                $"{ProbeAssetPath}\n\n" +
                "Deleting the script will leave those references missing.",
                prompts[0].Message);
        }

        [Test]
        public void GuardAsset_SingleUsedScript_CancelKeepsIt()
        {
            CreateListProbe(new DeleteGuardPistol());
            var scriptPath = ScriptPath(FixturesScriptGuid);
            var prompts = StubPrompts(selected: new[] { scriptPath }, PromptAnswer.Cancel);

            Assert.AreEqual(AssetDeleteResult.FailedDelete, SerializeReferenceDeleteGuard.GuardAsset(scriptPath));
            Assert.AreEqual(1, prompts.Count);
        }

        [Test]
        public void GuardAsset_UnusedScript_NeverAsks()
        {
            CreateListProbe(new DeleteGuardPistol());
            var scriptPath = ScriptPath(SettingsScriptGuid);
            var prompts = StubPrompts(selected: new[] { scriptPath });

            Assert.AreEqual(AssetDeleteResult.DidNotDelete, SerializeReferenceDeleteGuard.GuardAsset(scriptPath));
            Assert.AreEqual(0, prompts.Count);
        }

        [Test]
        public void GuardAsset_SeveralUsedScriptsInOneAction_DeleteAllAsksOnce()
        {
            CreateListProbe(new DeleteGuardPistol(), new ОружиеDeleteGuard(), new DeleteGuardRevolver());
            var scripts = UsedScriptPaths();
            var prompts = StubPrompts(selected: scripts, PromptAnswer.DeleteAll);

            foreach (var script in scripts)
                Assert.AreEqual(AssetDeleteResult.DidNotDelete, SerializeReferenceDeleteGuard.GuardAsset(script));

            Assert.AreEqual(1, prompts.Count);
            Assert.IsTrue(prompts[0].OfferRest);
            StringAssert.Contains("\"Delete All Anyway\"", prompts[0].Message);
        }

        [Test]
        public void GuardAsset_SeveralUsedScriptsInOneAction_CancelKeepsAllWithOneDialog()
        {
            CreateListProbe(new DeleteGuardPistol(), new ОружиеDeleteGuard(), new DeleteGuardRevolver());
            var scripts = UsedScriptPaths();
            var unused = ScriptPath(SettingsScriptGuid);
            var prompts = StubPrompts(selected: scripts.Append(unused).ToArray(), PromptAnswer.Cancel);

            foreach (var script in scripts)
                Assert.AreEqual(AssetDeleteResult.FailedDelete, SerializeReferenceDeleteGuard.GuardAsset(script));

            // A script that needs no check is deleted all the same: the answer covers only what would have asked.
            Assert.AreEqual(AssetDeleteResult.DidNotDelete, SerializeReferenceDeleteGuard.GuardAsset(unused));
            Assert.AreEqual(1, prompts.Count);
            Assert.IsTrue(prompts[0].OfferRest);
        }

        [Test]
        public void GuardAsset_SeveralUsedScriptsInOneAction_DeleteAnywayAnswersForThatScriptOnly()
        {
            CreateListProbe(new DeleteGuardPistol(), new ОружиеDeleteGuard(), new DeleteGuardRevolver());
            var scripts = UsedScriptPaths();
            var prompts = StubPrompts(selected: scripts, PromptAnswer.Delete, PromptAnswer.Delete, PromptAnswer.Delete);

            foreach (var script in scripts)
                Assert.AreEqual(AssetDeleteResult.DidNotDelete, SerializeReferenceDeleteGuard.GuardAsset(script));

            Assert.AreEqual(3, prompts.Count);
            Assert.IsTrue(prompts.All(prompt => prompt.OfferRest));
        }

        [Test]
        public void GuardAsset_NoSelection_OffersTheAnswerForTheRestFromTheSecondDialog()
        {
            CreateListProbe(new DeleteGuardPistol(), new ОружиеDeleteGuard(), new DeleteGuardRevolver());
            var scripts = UsedScriptPaths();
            var prompts = StubPrompts(selected: Array.Empty<string>(), PromptAnswer.Delete, PromptAnswer.DeleteAll);

            foreach (var script in scripts)
                Assert.AreEqual(AssetDeleteResult.DidNotDelete, SerializeReferenceDeleteGuard.GuardAsset(script));

            // Nothing says that more is coming, so the first dialog is the plain one; the second knows it is a batch.
            CollectionAssert.AreEqual(new[] { false, true }, prompts.Select(prompt => prompt.OfferRest).ToArray());
        }

        [Test]
        public void GuardAsset_PlainCancel_DoesNotCancelTheRest()
        {
            CreateListProbe(new DeleteGuardPistol(), new ОружиеDeleteGuard());
            var scripts = UsedScriptPaths();
            var prompts = StubPrompts(selected: Array.Empty<string>(), PromptAnswer.Cancel, PromptAnswer.Delete);

            Assert.AreEqual(AssetDeleteResult.FailedDelete, SerializeReferenceDeleteGuard.GuardAsset(scripts[0]));
            Assert.AreEqual(AssetDeleteResult.DidNotDelete, SerializeReferenceDeleteGuard.GuardAsset(scripts[1]));
            Assert.AreEqual(2, prompts.Count);
        }

        [Test]
        public void GuardAsset_SelectionWithoutTheScript_OffersNothing()
        {
            CreateListProbe(new DeleteGuardPistol());
            var scriptPath = ScriptPath(FixturesScriptGuid);

            // The selected scripts are not the one being deleted (a script deleting by code).
            var prompts = StubPrompts(
                selected: new[] { ScriptPath(NamesakeScriptGuid), ScriptPath(SettingsScriptGuid) },
                PromptAnswer.Delete);

            SerializeReferenceDeleteGuard.GuardAsset(scriptPath);

            Assert.IsFalse(prompts[0].OfferRest);
        }

        [Test]
        public void GuardAsset_SelectedAssetsThatAreNotScripts_OfferNothing()
        {
            CreateListProbe(new DeleteGuardPistol());
            var scriptPath = ScriptPath(FixturesScriptGuid);
            var prompts = StubPrompts(selected: new[] { scriptPath, ProbeAssetPath }, PromptAnswer.Delete);

            SerializeReferenceDeleteGuard.GuardAsset(scriptPath);

            Assert.IsFalse(prompts[0].OfferRest);
        }

        [Test]
        public void GuardAsset_FolderAndScriptInOneAction_ShareTheAnswer()
        {
            CreateListProbe(new DeleteGuardPistol());
            var scriptPath = ScriptPath(FixturesScriptGuid);
            var folderPath = scriptPath[..scriptPath.LastIndexOf('/')];
            var prompts = StubPrompts(selected: new[] { folderPath, scriptPath }, PromptAnswer.DeleteAll);

            Assert.IsTrue(AssetDatabase.IsValidFolder(folderPath));
            Assert.AreEqual(AssetDeleteResult.DidNotDelete, SerializeReferenceDeleteGuard.GuardAsset(folderPath));
            Assert.AreEqual(AssetDeleteResult.DidNotDelete, SerializeReferenceDeleteGuard.GuardAsset(scriptPath));

            Assert.AreEqual(1, prompts.Count);
            Assert.AreEqual("Delete Folder", prompts[0].Title);
            Assert.IsTrue(prompts[0].OfferRest);
        }

        [Test]
        public void GuardAsset_AfterTheAnswerIsReset_AsksAgain()
        {
            CreateListProbe(new DeleteGuardPistol(), new ОружиеDeleteGuard(), new DeleteGuardRevolver());
            var scripts = UsedScriptPaths();
            var prompts = StubPrompts(selected: scripts, PromptAnswer.DeleteAll, PromptAnswer.Cancel);

            Assert.AreEqual(AssetDeleteResult.DidNotDelete, SerializeReferenceDeleteGuard.GuardAsset(scripts[0]));
            Assert.AreEqual(AssetDeleteResult.DidNotDelete, SerializeReferenceDeleteGuard.GuardAsset(scripts[1]));
            Assert.AreEqual(1, prompts.Count);

            SerializeReferenceDeleteGuard.ResetAnswer();

            Assert.AreEqual(AssetDeleteResult.FailedDelete, SerializeReferenceDeleteGuard.GuardAsset(scripts[2]));
            Assert.AreEqual(2, prompts.Count);
            Assert.IsTrue(prompts[1].OfferRest, "The selection still holds more scripts.");
        }

        // The answer ends with the editor tick that holds the delete: a delete in the next tick asks again.
        [UnityTest]
        public IEnumerator GuardAsset_InTheNextEditorTick_AsksAgain()
        {
            CreateListProbe(new DeleteGuardPistol(), new ОружиеDeleteGuard());
            var scripts = UsedScriptPaths();
            var prompts = StubPrompts(selected: scripts, PromptAnswer.DeleteAll, PromptAnswer.DeleteAll);

            SerializeReferenceDeleteGuard.GuardAsset(scripts[0]);
            SerializeReferenceDeleteGuard.GuardAsset(scripts[1]);

            Assert.AreEqual(1, prompts.Count);

            // delayCall runs in registration order, so once this marker has run, the guard's reset has too. The
            // batch-mode test runner can hold delayCall back for many ticks, so the wait is bounded by time, not frames.
            var flushed = false;
            EditorApplication.delayCall += () => flushed = true;

            var deadline = EditorApplication.timeSinceStartup + DelayCallTimeoutSeconds;
            while (!flushed && EditorApplication.timeSinceStartup < deadline)
                yield return null;

            Assert.IsTrue(flushed, "delayCall did not run in time.");

            SerializeReferenceDeleteGuard.GuardAsset(scripts[1]);

            Assert.AreEqual(2, prompts.Count);
        }

        [Test]
        public void GuardAsset_CancelledSweep_CancelsWithoutAsking()
        {
            CreateListProbe(new DeleteGuardPistol());
            var scriptPath = ScriptPath(FixturesScriptGuid);
            var prompts = StubPrompts(selected: new[] { scriptPath });
            SerializeReferenceDeleteGuard.KeepSweepUntilTickEnds(cancelled: true);

            Assert.AreEqual(AssetDeleteResult.FailedDelete, SerializeReferenceDeleteGuard.GuardAsset(scriptPath));
            Assert.AreEqual(0, prompts.Count);
        }

        private static List<Type> Resolve(params string[] scriptGuids) =>
            SerializeReferenceDeleteGuard.ResolveCandidateTypes(scriptGuids.Select(AssetDatabase.GUIDToAssetPath).ToList());

        private static string ScriptPath(string scriptGuid) => AssetDatabase.GUIDToAssetPath(scriptGuid);

        // The scripts that declare the types of CreateListProbe(Pistol, Cyrillic, Revolver), in that order.
        private static string[] UsedScriptPaths() =>
            new[] { FixturesScriptGuid, NamesakeScriptGuid, PartialScriptGuid }.Select(ScriptPath).ToArray();

        // Replaces the dialog with one that records each prompt and answers from the queue (Delete once it runs dry).
        private static List<Prompt> StubPrompts(IReadOnlyList<string> selected, params PromptAnswer[] answers)
        {
            var prompts = new List<Prompt>();
            var queue = new Queue<PromptAnswer>(answers);

            SerializeReferenceDeleteGuard.GetSelectedPaths = () => selected;
            SerializeReferenceDeleteGuard.ShowPrompt = (title, message, offerRest) =>
            {
                prompts.Add(new Prompt(title, message, offerRest));
                return queue.Count > 0 ? queue.Dequeue() : PromptAnswer.Delete;
            };

            return prompts;
        }

        private static void CreateListProbe(params ITestWeapon[] weapons)
        {
            var probe = ScriptableObject.CreateInstance<ReferenceListTestObject>();
            probe.weapons.AddRange(weapons);

            AssetDatabase.CreateAsset(probe, ProbeAssetPath);
            AssetDatabase.SaveAssets();

            SerializeReferenceTypeUsageIndex.Reset();
        }

        private static void CreatePlainAsset()
        {
            AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<DeleteGuardSettingsFixture>(), PlainAssetPath);
            AssetDatabase.SaveAssets();

            SerializeReferenceTypeUsageIndex.Reset();
        }

        private static void CreateProbe(ITestWeapon a, ITestWeapon b)
        {
            var probe = ScriptableObject.CreateInstance<LinkerTestObject>();
            probe.a = a;
            probe.b = b;

            AssetDatabase.CreateAsset(probe, ProbeAssetPath);
            AssetDatabase.SaveAssets();

            // Creating the asset patches a warm index in place; each case starts from a cold one.
            SerializeReferenceTypeUsageIndex.Reset();
        }

        private readonly struct Prompt
        {
            public string Title { get; }

            public string Message { get; }

            public bool OfferRest { get; }

            public Prompt(string title, string message, bool offerRest)
            {
                Title = title;
                Message = message;
                OfferRest = offerRest;
            }
        }
    }
}
