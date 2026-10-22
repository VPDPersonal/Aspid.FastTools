using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using System.Reflection;
using System.Collections;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Unity.Profiling.LowLevel.Unsafe;
using Object = UnityEngine.Object;

namespace Aspid.FastTools.Editors.Tests
{
    // Opens the imported samples and pins the values and the Play behaviour that their tutorials and GIFs show. The
    // samples are found by the GUIDs of their kept files, so the test works wherever they were imported. A consumer who
    // worked through the tutorials has changed those values on purpose, so the test runs only when ci.yml passes the
    // strict-setup argument.
    internal sealed class SampleSmokeTests
    {
        private const string StrictSetupArgument = "-aspidFastToolsStrictSetup";

        private const string TypesScene = "437b8a1e72b0e44e3ad8760e9cf7661e";
        private const string SerializeReferencesScene = "b596d2bc3fe634a92b1a5929055fcecc";
        private const string EnumValuesScene = "b9ccf7ebdae7141378708ad956275a8e";
        private const string ProfilerMarkersScene = "20347b3dd8b2f4546a71220948a68c4b";
        private const string FireballAsset = "c49a3f2ebc4b5471586f3bee253ca112";

        private const string AbilityCatalogMenu = "Tools/Aspid 🐍/FastTools/Samples/Ability Catalog";

        private static readonly string[] MarkerNames =
        {
            "Flock.Update (21)",
            "Flock.ApplyTransforms (28)",
            "Flock.InitializeAgents (53)",
            "FlockSimulation.Step (32)",
            "FlockSimulation.Steering (34)",
            "FlockSimulation.Integrate (37)",
            "FlockSimulation.Steering.Agent (47)",
        };

        private readonly List<string> _messages = new();

        [SetUp]
        public void RequireStrictSetup()
        {
            if (!Environment.GetCommandLineArgs().Contains(StrictSetupArgument))
                Assert.Ignore($"The sample smoke tests run only with {StrictSetupArgument}: tutorials change the samples on purpose.");
        }

        [TestCase(TypesScene)]
        [TestCase(SerializeReferencesScene)]
        [TestCase(EnumValuesScene)]
        [TestCase(ProfilerMarkersScene)]
        public void Scene_OpensClean_WithNoLightsOrPhysics(string guid)
        {
            Listen();
            var scene = EditorSceneManager.OpenScene(PathOf(guid), OpenSceneMode.Additive);
            try
            {
                var objects = Objects(scene);

                Assert.AreEqual(0, objects.Sum(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount), "A scene object has a missing script.");
                Assert.IsFalse(scene.isDirty, "Opening the scene must not change it.");
                Assert.AreEqual(1, objects.Count(gameObject => gameObject.TryGetComponent<Camera>(out _)), "A sample scene has one camera.");
                Assert.IsEmpty(objects.Where(gameObject => gameObject.TryGetComponent<Light>(out _)), "A sample scene has no lights.");
                Assert.IsEmpty(objects.Where(gameObject => gameObject.TryGetComponent<Collider>(out _)), "A sample scene needs no Physics module.");
                Assert.IsFalse(objects.SelectMany(gameObject => gameObject.GetComponents<MonoBehaviour>()).Any(SerializationUtility.HasManagedReferencesWithMissingTypes),
                    "No sample ships a broken reference.");

                // Meshes draw with the sample's own Studio shader; TextMesh labels draw with their font's material.
                var foreign = objects
                    .Where(gameObject => !gameObject.TryGetComponent<TextMesh>(out _))
                    .SelectMany(gameObject => gameObject.GetComponents<Renderer>())
                    .SelectMany(renderer => renderer.sharedMaterials)
                    .Where(material => material == null || !material.shader.name.StartsWith("Aspid/Samples/", StringComparison.Ordinal))
                    .Select(material => material == null ? "<none>" : $"{material.name} ({material.shader.name})");
                Assert.IsEmpty(foreign, "Every mesh draws with the sample's own Studio shader.");
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, removeScene: true);
                StopListening();
            }
        }

        [Test]
        public void Types_ShipsAGruntSpawnerWithAnEliteEveryFourthOnTheCircle()
        {
            WithScene(TypesScene, scene =>
            {
                var spawner = Find(scene, "EnemySpawner");
                StringAssert.Contains(".Grunt,", spawner.FindProperty("_enemyType._assemblyQualifiedName").stringValue);
                StringAssert.Contains(".ArmoredGrunt,", spawner.FindProperty("_eliteType").stringValue);
                Assert.AreEqual(4, spawner.FindProperty("_eliteEvery").intValue);
                Assert.AreEqual(8, spawner.FindProperty("_count").intValue);
                StringAssert.Contains(".CirclePattern,", spawner.FindProperty("_pattern._assemblyQualifiedName").stringValue);
                Assert.IsNotNull(spawner.FindProperty("_body").objectReferenceValue, "Spawned enemies need the capsule mesh.");

                Assert.IsNotNull(Find(scene, "Archer"), "The placed enemy is an Archer, the one the tutorial switches in place.");
            });
        }

        [Test]
        public void SerializeReferences_LoadoutFiresARailgunWithBurnAndFreezesOnHit()
        {
            WithScene(SerializeReferencesScene, scene =>
            {
                var loadout = Find(scene, "Loadout");
                var primary = loadout.FindProperty("_primary");
                StringAssert.EndsWith(".Railgun", primary.managedReferenceFullTypename);
                Assert.AreEqual(45, primary.FindPropertyRelative("_damage").intValue);
                StringAssert.EndsWith(".BurnEffect", primary.FindPropertyRelative("_chargeEffect").managedReferenceFullTypename);

                var sidearms = loadout.FindProperty("_sidearms");
                Assert.AreEqual(2, sidearms.arraySize);
                StringAssert.EndsWith(".Pistol", sidearms.GetArrayElementAtIndex(0).managedReferenceFullTypename);
                Assert.AreEqual(10, sidearms.GetArrayElementAtIndex(0).FindPropertyRelative("_damage").intValue);
                StringAssert.EndsWith(".Shotgun", sidearms.GetArrayElementAtIndex(1).managedReferenceFullTypename);

                StringAssert.EndsWith(".FreezeEffect", loadout.FindProperty("_onHit").managedReferenceFullTypename);
            });
        }

        [Test]
        public void EnumValues_LooksUpTheSpeedsTheTutorialQuotes()
        {
            WithScene(EnumValuesScene, scene =>
            {
                var walker = Behaviour(scene, "Walker");
                var table = walker.GetType().GetField("_speedByTerrain", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(walker);
                var flags = walker.GetType().Assembly.GetTypes().Single(type => type.Name == "TerrainFlags");
                var getValue = table.GetType().GetMethods().First(method => method.Name == "GetValue" && !method.IsGenericMethodDefinition
                    && method.GetParameters().Length == 1 && method.GetParameters()[0].ParameterType.IsAssignableFrom(flags));

                float Speed(string key) => (float)getValue.Invoke(table, new[] { Enum.Parse(flags, key) });

                Assert.AreEqual(1f, Speed("None"), 1e-5f, "No row: Default Value.");
                Assert.AreEqual(1.6f, Speed("Hot"), 1e-5f, "The exact row.");
                Assert.AreEqual(0.5f, Speed("Wet, Slippery"), 1e-5f, "The exact combined row.");
                Assert.AreEqual(0.8f, Speed("Wet, Hot"), 1e-5f, "No exact row: the first row whose flags it contains.");
            });
        }

        [Test]
        public void ProfilerMarkers_ShipsAFlockOf120()
        {
            WithScene(ProfilerMarkersScene, scene => Assert.AreEqual(120, Find(scene, "Flock").FindProperty("_count").intValue));
        }

        [Test]
        public void Fireball_Costs30ManaWithA4SecondCooldownAndDealsDamage()
        {
            var fireball = new SerializedObject(AssetDatabase.LoadMainAssetAtPath(PathOf(FireballAsset)));
            Assert.AreEqual(30, fireball.FindProperty("_manaCost").intValue);
            Assert.AreEqual(4f, fireball.FindProperty("_cooldown").floatValue);
            StringAssert.StartsWith("Aspid.FastTools.Samples.EditorTools.Effects.DamageEffect,", fireball.FindProperty("_effectType").stringValue);
        }

        [UnityTest]
        public IEnumerator Types_SpawnsAWaveOfEightWithElitesInPlayMode()
        {
            yield return new EnterPlayMode();
            Listen();
            try
            {
                yield return EditorSceneManager.LoadSceneAsyncInPlayMode(PathOf(TypesScene), new LoadSceneParameters(LoadSceneMode.Single));
                for (var frame = 0; frame < 3; frame++)
                    yield return null;

                var spawner = Behaviour(SceneManager.GetActiveScene(), "EnemySpawner").transform;
                Assert.AreEqual(8, spawner.childCount, "The first wave spawns eight enemies under the spawner.");
                var elites = Enumerable.Range(0, spawner.childCount).Count(i => spawner.GetChild(i).GetComponents<MonoBehaviour>().Any(component => component.GetType().Name == "ArmoredGrunt"));
                Assert.AreEqual(2, elites, "Every fourth enemy of the wave is an elite.");
            }
            finally
            {
                StopListening();
            }

            yield return new ExitPlayMode();
        }

        [UnityTest]
        public IEnumerator Scenes_Play60CleanFrames([Values(SerializeReferencesScene, EnumValuesScene)] string guid)
        {
            yield return new EnterPlayMode();
            Listen();
            try
            {
                yield return EditorSceneManager.LoadSceneAsyncInPlayMode(PathOf(guid), new LoadSceneParameters(LoadSceneMode.Single));
                for (var frame = 0; frame < 60; frame++)
                    yield return null;
            }
            finally
            {
                StopListening();
            }

            yield return new ExitPlayMode();
        }

        [UnityTest]
        public IEnumerator ProfilerMarkers_Runs60CleanFramesAndRegistersTheMarkersOfItsTutorial()
        {
            yield return new EnterPlayMode();
            Listen();
            try
            {
                yield return EditorSceneManager.LoadSceneAsyncInPlayMode(PathOf(ProfilerMarkersScene), new LoadSceneParameters(LoadSceneMode.Single));
                for (var frame = 0; frame < 60; frame++)
                    yield return null;

                Assert.AreEqual(120, Behaviour(SceneManager.GetActiveScene(), "Flock").transform.childCount, "The flock builds 120 agents.");

                var handles = new List<ProfilerRecorderHandle>();
                ProfilerRecorderHandle.GetAvailable(handles);
                var names = handles.Select(handle => ProfilerRecorderHandle.GetDescription(handle).Name).ToList();
                CollectionAssert.IsSubsetOf(MarkerNames, names, "The generated marker names changed: the tutorial quotes them.");
            }
            finally
            {
                StopListening();
            }

            yield return new ExitPlayMode();
        }

        [UnityTest]
        [Category("Graphics")]
        public IEnumerator AbilityCatalog_ListsTheFourAbilitiesByName()
        {
            Listen();
            EditorWindow window = null;
            try
            {
                Assert.IsTrue(EditorApplication.ExecuteMenuItem(AbilityCatalogMenu), $"No menu item {AbilityCatalogMenu}.");
                window = Resources.FindObjectsOfTypeAll<EditorWindow>().Single(open => open.GetType().Name == "AbilityCatalogWindow");
                yield return null;

                var list = window.rootVisualElement.Q<ListView>();
                CollectionAssert.AreEqual(new[] { "Arcane Ward", "Fireball", "Healing Word", "Sprint" },
                    list.itemsSource.Cast<Object>().Select(ability => new SerializedObject(ability).FindProperty("_abilityName").stringValue));
            }
            finally
            {
                if (window != null)
                    window.Close();
            }

            yield return null;
            StopListening();
        }

        private static string PathOf(string guid)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(path))
                Assert.Fail($"No asset with GUID {guid}: import the samples, as make-unity-test-project.sh does.");

            return path;
        }

        private static GameObject[] Objects(Scene scene) => scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<Transform>(includeInactive: true))
            .Select(transform => transform.gameObject)
            .ToArray();

        private static MonoBehaviour Behaviour(Scene scene, string type) => Objects(scene)
            .SelectMany(gameObject => gameObject.GetComponents<MonoBehaviour>())
            .Single(behaviour => behaviour.GetType().Name == type);

        private static SerializedObject Find(Scene scene, string type) => new(Behaviour(scene, type));

        private static void WithScene(string guid, Action<Scene> check)
        {
            var scene = EditorSceneManager.OpenScene(PathOf(guid), OpenSceneMode.Additive);
            try { check(scene); }
            finally { EditorSceneManager.CloseScene(scene, removeScene: true); }
        }

        // LogAssert fails a test on an error only; a warning, such as a zero-length look rotation, is collected here.
        private void Listen()
        {
            _messages.Clear();
            Application.logMessageReceived += OnLogMessage;
        }

        private void StopListening()
        {
            Application.logMessageReceived -= OnLogMessage;
            LogAssert.NoUnexpectedReceived();
            Assert.IsEmpty(_messages, "The sample logged to the Console.");
        }

        private void OnLogMessage(string message, string stackTrace, LogType type) => _messages.Add($"{type}: {message}");
    }
}
