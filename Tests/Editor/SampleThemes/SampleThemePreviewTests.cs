using System;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Aspid.FastTools.Editors.Internal;

namespace Aspid.FastTools.Editors.Tests
{
    /// <summary>
    /// Covers the editor hooks of <see cref="SampleThemePreview"/>: they exist only while a Light or Dark preview is on,
    /// so a project that keeps the default Authored mode gets no per-frame or per-camera callbacks.
    /// </summary>
    [TestFixture]
    internal sealed class SampleThemePreviewTests
    {
        private SampleThemeMode _mode;

        [SetUp]
        public void SetUp() =>
            _mode = SampleThemePreview.Mode;

        [TearDown]
        public void TearDown() =>
            SampleThemePreview.SetMode(_mode);

        [Test]
        public void Authored_LeavesNoEditorHooks()
        {
            SampleThemePreview.SetMode(SampleThemeMode.Authored);

            Assert.AreEqual(0, HookCount(EditorApplication.update), "Authored must not hook EditorApplication.update.");
            Assert.AreEqual(0, HookCount(Camera.onPreCull), "Authored must not hook Camera.onPreCull.");
            Assert.AreEqual(0, HookCount(Camera.onPostRender), "Authored must not hook Camera.onPostRender.");
        }

        [Test]
        public void Preview_SubscribesOnce_AndAuthoredUnsubscribes()
        {
            SampleThemePreview.SetMode(SampleThemeMode.Dark);
            SampleThemePreview.SetMode(SampleThemeMode.Light);

            Assert.AreEqual(1, HookCount(EditorApplication.update), "A preview must hook EditorApplication.update once.");
            Assert.AreEqual(1, HookCount(Camera.onPreCull), "A preview must hook Camera.onPreCull once.");
            Assert.AreEqual(1, HookCount(Camera.onPostRender), "A preview must hook Camera.onPostRender once.");

            SampleThemePreview.SetMode(SampleThemeMode.Authored);

            Assert.AreEqual(0, HookCount(EditorApplication.update), "Authored must remove the update hook.");
            Assert.AreEqual(0, HookCount(Camera.onPreCull), "Authored must remove the pre-cull hook.");
            Assert.AreEqual(0, HookCount(Camera.onPostRender), "Authored must remove the post-render hook.");
        }

        private static int HookCount(Delegate callback) =>
            callback?.GetInvocationList().Count(handler => handler.Method.DeclaringType == typeof(SampleThemePreview)) ?? 0;
    }
}
