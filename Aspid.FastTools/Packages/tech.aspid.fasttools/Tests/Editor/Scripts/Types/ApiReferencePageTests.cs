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
        [TestCase(typeof(CharacterController), "file:///unity/ScriptReference/CharacterController.html")]
        [TestCase(typeof(Application.LogCallback), "file:///unity/ScriptReference/Application.LogCallback.html")]
        [TestCase(typeof(ObjectPool<string>), "file:///unity/ScriptReference/Pool.ObjectPool_1.html")]
        [TestCase(typeof(UnityEditor.EditorWindow), "file:///unity/ScriptReference/EditorWindow.html")]
        [TestCase(typeof(CharacterController[]), "file:///unity/ScriptReference/CharacterController.html")]
        [TestCase(typeof(ProfilerMarker), "file:///unity/ScriptReference/Unity.Profiling.ProfilerMarker.html")]
        [TestCase(typeof(NativeArray<int>), "file:///unity/ScriptReference/Unity.Collections.NativeArray_1.html")]
        [TestCase(typeof(string), "https://learn.microsoft.com/dotnet/api/system.string")]
        [TestCase(typeof(List<int>), "https://learn.microsoft.com/dotnet/api/system.collections.generic.list-1")]
        public void EditorBuiltInType_GetsItsApiReferencePage(Type type, string expected) =>
            Assert.AreEqual(expected, TypeExtensions.GetApiReferencePage(type));

        [Test]
        public void ProjectType_GetsNoApiReferencePage() =>
            Assert.IsNull(TypeExtensions.GetApiReferencePage(typeof(MonoScriptLookupFixtures)));
    }
}
