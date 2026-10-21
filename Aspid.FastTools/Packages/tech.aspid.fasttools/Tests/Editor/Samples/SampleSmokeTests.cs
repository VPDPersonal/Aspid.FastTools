using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using NUnit.Framework;
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
    // Opens the imported samples and pins every value and outcome their tutorials quote. The samples are found by the
    // GUIDs of their kept files, so the test works wherever they were imported. A consumer who worked through the
    // tutorials has changed those values on purpose, so the test runs only when ci.yml passes the strict-setup argument.
    internal sealed class SampleSmokeTests
    {
        private const string StrictSetupArgument = "-aspidFastToolsStrictSetup";

        private const string TypesScene = "437b8a1e72b0e44e3ad8760e9cf7661e";
        private const string EnumValuesScene = "b9ccf7ebdae7141378708ad956275a8e";
        private const string ProfilerMarkersScene = "20347b3dd8b2f4546a71220948a68c4b";
        private const string LoadoutAsset = "26c6164c4710f41be9713c49c9536f1e";
        private const string FireballAsset = "c49a3f2ebc4b5471586f3bee253ca112";

        private const string AbilityCatalogMenu = "Tools/Aspid 🐍/FastTools/Samples/Ability Catalog";

        private static readonly string[] MarkerNames =
        {
            "Flock.Update (18)",
            "Flock.ApplyTransforms (25)",
            "FlockSimulation.Step (24)",
            "FlockSimulation.Steering (26)",
            "FlockSimulation.Integrate (29)",
            "FlockSimulation.Steering.Agent (40)",
        };

        private readonly List<string> _messages = new();

        [SetUp]
        public void RequireStrictSetup()
        {
            if (!Environment.GetCommandLineArgs().Contains(StrictSetupArgument))
                Assert.Ignore($"The sample smoke tests run only with {StrictSetupArgument}: tutorials change the samples on purpose.");
        }

        [TestCase(TypesScene, "Spawner")]
        [TestCase(EnumValuesScene, "Surface Rules")]
        [TestCase(ProfilerMarkersScene, "Flock")]
        public void Scene_OpensClean_WithItsLessonFirstAndTheCameraLast(string guid, string firstRoot)
        {
            Listen();
            var scene = EditorSceneManager.OpenScene(PathOf(guid), OpenSceneMode.Additive);
            try
            {
                var roots = scene.GetRootGameObjects();
                var objects = roots.SelectMany(root => root.GetComponentsInChildren<Transform>(includeInactive: true)).Select(transform => transform.gameObject).ToArray();

                Assert.AreEqual(0, objects.Sum(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount), "A scene object has a missing script.");
                Assert.IsFalse(scene.isDirty, "Opening the scene must not change it.");
                Assert.AreEqual(firstRoot, roots.First().name, "Welcome's Open selects the first root: the teaching object.");

                var camera = roots.Last();
                Assert.AreEqual("Main Camera", camera.name, "The stock Main Camera is the last root.");
                Assert.AreEqual(CameraClearFlags.SolidColor, camera.GetComponent<Camera>().clearFlags);
                Assert.IsTrue(camera.TryGetComponent<AudioListener>(out _), "Without an AudioListener, Play Mode logs a warning.");
                Assert.AreEqual(1, objects.Count(gameObject => gameObject.TryGetComponent<Camera>(out _)), "A sample scene has one camera.");
                Assert.IsEmpty(objects.Where(gameObject => gameObject.TryGetComponent<Light>(out _)), "A sample scene has no lights.");
                Assert.IsEmpty(objects.Where(gameObject => gameObject.TryGetComponent<Collider>(out _)), "A sample scene has no colliders.");

                var foreign = objects
                    .SelectMany(gameObject => gameObject.GetComponents<Renderer>())
                    .SelectMany(renderer => renderer.sharedMaterials)
                    .Where(material => material == null || !material.shader.name.StartsWith("Aspid/Samples/", StringComparison.Ordinal))
                    .Select(material => material == null ? "<none>" : $"{material.name} ({material.shader.name})");
                Assert.IsEmpty(foreign, "Every renderer draws with the sample's own Studio shader.");
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, removeScene: true);
                StopListening();
            }
        }

        [Test]
        public void Types_ShipsAGruntSpawnerWithTheCirclePattern()
        {
            WithScene(TypesScene, roots =>
            {
                var spawner = Find(roots, "Spawner", "EnemySpawner");
                StringAssert.Contains(".CirclePattern,", spawner.FindProperty("_pattern._assemblyQualifiedName").stringValue);
                StringAssert.Contains(".Grunt,", spawner.FindProperty("_enemyType._assemblyQualifiedName").stringValue);

                var enemy = Find(roots, "Enemy", "Grunt");
                Assert.AreEqual(250f, enemy.FindProperty("_health").floatValue);
            });
        }

        [Test]
        public void EnumValues_LooksUpTheSpeedsAndColorsTheTutorialQuotes()
        {
            WithScene(EnumValuesScene, roots =>
            {
                var rules = roots.Single(root => root.name == "Surface Rules").GetComponents<MonoBehaviour>().Single(component => component.GetType().Name == "SurfaceRules");
                var type = rules.GetType();
                var getSpeed = type.GetMethod("GetSpeed");
                var getColor = type.GetMethod("GetColor");
                var terrain = getSpeed.GetParameters()[0].ParameterType;
                var surface = getColor.GetParameters()[0].ParameterType;

                float Speed(string flags) => (float)getSpeed.Invoke(rules, new[] { Enum.Parse(terrain, flags) });

                Assert.AreEqual(1f, Speed("None"), 1e-5f, "No row: Default Value.");
                Assert.AreEqual(1.2f, Speed("Hot"), 1e-5f, "The exact row.");
                Assert.AreEqual(2.2f, Speed("Wet, Slippery"), 1e-5f, "The exact combined row.");
                Assert.AreEqual(0.6f, Speed("Wet, Hot"), 1e-5f, "No exact row: the first row whose flags it contains.");

                var defaultColor = new SerializedObject(rules).FindProperty("_surfaceColors._defaultValue").colorValue;
                Assert.AreEqual(defaultColor, getColor.Invoke(rules, new[] { Enum.Parse(surface, "Water") }), "Water has no row: Default Value.");
            });
        }

        [Test]
        public void ProfilerMarkers_ShipsAFlockOf120()
        {
            WithScene(ProfilerMarkersScene, roots => Assert.AreEqual(120, Find(roots, "Flock", "Flock").FindProperty("_count").intValue));
        }

        [Test]
        public void Loadout_HoldsAPistolWithDamage37AndOnePistolSidearm()
        {
            var loadout = AssetDatabase.LoadMainAssetAtPath(PathOf(LoadoutAsset));
            Assert.IsFalse(SerializationUtility.HasManagedReferencesWithMissingTypes(loadout), "No sample ships a broken reference.");

            var serialized = new SerializedObject(loadout);
            var primary = serialized.FindProperty("_primary");
            StringAssert.EndsWith(".Pistol", primary.managedReferenceFullTypename);
            Assert.AreEqual(37, primary.FindPropertyRelative("_damage").intValue);

            var sidearms = serialized.FindProperty("_sidearms");
            Assert.AreEqual(1, sidearms.arraySize);
            StringAssert.EndsWith(".Pistol", sidearms.GetArrayElementAtIndex(0).managedReferenceFullTypename);
        }

        [Test]
        public void Fireball_Costs30ManaWithA4SecondCooldownAndDealsDamage()
        {
            var fireball = new SerializedObject(AssetDatabase.LoadMainAssetAtPath(PathOf(FireballAsset)));
            Assert.AreEqual(30, fireball.FindProperty("_manaCost").intValue);
            Assert.AreEqual(4f, fireball.FindProperty("_cooldown").floatValue);
            StringAssert.StartsWith("Aspid.FastTools.Samples.EditorTools.DamageEffect,", fireball.FindProperty("_effectType").stringValue);
        }

        [Test]
        public void AbilityCatalogStyleSheet_LoadsFromResources()
        {
            Assert.IsNotNull(Resources.Load<StyleSheet>("AbilityCatalog"), "AddStyleSheetFromResources(\"AbilityCatalog\") must find the sample's sheet.");
        }

        [UnityTest]
        public IEnumerator Types_SpawnsEightGruntsInPlayMode()
        {
            yield return new EnterPlayMode();
            Listen();
            try
            {
                yield return EditorSceneManager.LoadSceneAsyncInPlayMode(PathOf(TypesScene), new LoadSceneParameters(LoadSceneMode.Single));
                for (var frame = 0; frame < 3; frame++)
                    yield return null;

                var spawner = SceneManager.GetActiveScene().GetRootGameObjects().Single(root => root.name == "Spawner").transform;
                Assert.AreEqual(8, spawner.childCount, "Start spawns eight enemies under the Spawner.");
                for (var i = 0; i < spawner.childCount; i++)
                    Assert.IsTrue(spawner.GetChild(i).GetComponents<MonoBehaviour>().Any(component => component.GetType().Name == "Grunt"));
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
        public IEnumerator AbilityCatalog_OpensWithFireballSelected()
        {
            Listen();
            EditorWindow window = null;
            try
            {
                Assert.IsTrue(EditorApplication.ExecuteMenuItem(AbilityCatalogMenu), $"No menu item {AbilityCatalogMenu}.");
                window = Resources.FindObjectsOfTypeAll<EditorWindow>().Single(open => open.GetType().Name == "AbilityCatalogWindow");
                yield return null;

                var list = window.rootVisualElement.Q<ListView>();
                Assert.AreEqual(3, list.itemsSource.Count, "Fireball, Healing Word and Arcane Ward.");
                Assert.AreEqual("Fireball", ((Object)list.selectedItem).name);
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

        private static SerializedObject Find(GameObject[] roots, string root, string component) =>
            new(roots.Single(gameObject => gameObject.name == root).GetComponents<MonoBehaviour>().Single(behaviour => behaviour.GetType().Name == component));

        private void WithScene(string guid, Action<GameObject[]> check)
        {
            var scene = EditorSceneManager.OpenScene(PathOf(guid), OpenSceneMode.Additive);
            try { check(scene.GetRootGameObjects()); }
            finally { EditorSceneManager.CloseScene(scene, removeScene: true); }
        }

        // LogAssert fails a test on an error only; a warning, such as a missing audio listener, is collected here.
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
