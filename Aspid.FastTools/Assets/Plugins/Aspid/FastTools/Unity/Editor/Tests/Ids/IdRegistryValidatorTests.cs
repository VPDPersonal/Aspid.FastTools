#nullable enable
using System;
using System.Collections.Generic;
using NUnit.Framework;
using Aspid.FastTools.Ids.Editors;

namespace Aspid.FastTools.Ids.EditorTests
{
    [TestFixture]
    internal sealed class IdRegistryValidatorTests
    {
        [Test]
        public void IsValidName_RejectsNull()
        {
            Assert.IsFalse(IdRegistryValidator.IsValidName(null, null, out var err));
            Assert.IsNotNull(err);
        }

        [Test]
        public void IsValidName_RejectsEmpty()
        {
            Assert.IsFalse(IdRegistryValidator.IsValidName(string.Empty, null, out _));
        }

        [Test]
        public void IsValidName_RejectsWhitespaceOnly()
        {
            Assert.IsFalse(IdRegistryValidator.IsValidName("   ", null, out _));
        }

        [Test]
        public void IsValidName_AcceptsLetterStart()
        {
            Assert.IsTrue(IdRegistryValidator.IsValidName("Goblin", null, out var err));
            Assert.IsNull(err);
        }

        [Test]
        public void IsValidName_AcceptsUnderscoreStart()
        {
            Assert.IsTrue(IdRegistryValidator.IsValidName("_internal", null, out _));
        }

        [Test]
        public void IsValidName_AcceptsHyphenAndDigitsAfterFirstChar()
        {
            Assert.IsTrue(IdRegistryValidator.IsValidName("Boss_lv-2", null, out _));
        }

        [Test]
        public void IsValidName_RejectsDigitStart()
        {
            Assert.IsFalse(IdRegistryValidator.IsValidName("1Goblin", null, out _));
        }

        [Test]
        public void IsValidName_RejectsHyphenStart()
        {
            Assert.IsFalse(IdRegistryValidator.IsValidName("-Goblin", null, out _));
        }

        [Test]
        public void IsValidName_RejectsSpaces()
        {
            Assert.IsFalse(IdRegistryValidator.IsValidName("Big Goblin", null, out _));
        }

        [Test]
        public void IsValidName_RejectsLength256()
        {
            var name = new string('a', 256);
            Assert.IsFalse(IdRegistryValidator.IsValidName(name, null, out _));
        }

        [Test]
        public void IsValidName_AcceptsLength255()
        {
            var name = new string('a', 255);
            Assert.IsTrue(IdRegistryValidator.IsValidName(name, null, out _));
        }

        [Test]
        public void IsValidName_RejectsDuplicate()
        {
            Func<string, bool> isTaken = new HashSet<string> { "Goblin" }.Contains;
            Assert.IsFalse(IdRegistryValidator.IsValidName("Goblin", isTaken, out var err));
            StringAssert.Contains("already exists", err!);
        }

        [Test]
        public void IsValidName_AllowsNameNotInExistingSet()
        {
            Func<string, bool> isTaken = new HashSet<string> { "Orc" }.Contains;
            Assert.IsTrue(IdRegistryValidator.IsValidName("Goblin", isTaken, out _));
        }
    }
}
