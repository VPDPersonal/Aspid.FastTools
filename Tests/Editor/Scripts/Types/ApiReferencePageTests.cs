using System;
using UnityEngine;
using NUnit.Framework;
using Unity.Profiling;
using UnityEngine.Pool;
using Unity.Collections;
using System.Collections.Generic;
using Aspid.FastTools.Types.Editors;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Tests.Editors.Types
{
    /// <summary>
    /// Guards the page the open-script button falls back to for a type with no script in the project: the Scripting
    /// API page of a Unity type, the .NET API page of a <c>System</c> type, and nothing for a type of the project.
    /// </summary>
    [TestFixture]
    internal sealed class ApiReferencePageTests
    {
        [TestCase(typeof(CharacterController), "CharacterController.html")]
        [TestCase(typeof(Application.LogCallback), "Application.LogCallback.html")]
        [TestCase(typeof(ObjectPool<string>), "Pool.ObjectPool_1.html")]
        [TestCase(typeof(UnityEditor.EditorWindow), "EditorWindow.html")]
        [TestCase(typeof(CharacterController[]), "CharacterController.html")]
        [TestCase(typeof(ProfilerMarker), "Unity.Profiling.ProfilerMarker.html")]
        [TestCase(typeof(NativeArray<int>), "Unity.Collections.NativeArray_1.html")]
        [TestCase(typeof(string), "https://learn.microsoft.com/dotnet/api/system.string")]
        [TestCase(typeof(List<int>), "https://learn.microsoft.com/dotnet/api/system.collections.generic.list-1")]
        public void EditorBuiltInType_GetsItsApiReferencePage(Type type, string expected)
        {
            if (!expected.StartsWith("https://"))
                expected = TypeExtensions.UnityScriptReferenceUrl + expected;

            Assert.AreEqual(expected, TypeExtensions.GetApiReferencePage(type));
        }

        [Test]
        public void UnityPage_PointsAtTheScriptingApiOfThisEditorVersion()
        {
            var version = string.Join(".", Application.unityVersion.Split('.'), 0, 2);

            Assert.AreEqual($"https://docs.unity3d.com/{version}/Documentation/ScriptReference/BoxCollider.html",
                TypeExtensions.GetApiReferencePage(typeof(BoxCollider)));
        }

        [Test]
        public void ProjectType_GetsNoApiReferencePage() =>
            Assert.IsNull(TypeExtensions.GetApiReferencePage(typeof(MonoScriptLookupFixtures)));
    }
}
