using System;
using UnityEngine;
using NUnit.Framework;
using System.Collections.Generic;

namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    /// <summary>
    /// Coverage for the two data-preservation copiers behind type switches and Make-unique:
    /// <list type="bullet">
    /// <item><see cref="SerializeReferenceHelpers.CreateInstancePreservingData"/> carries nested
    /// <c>[SerializeReference]</c> children ACROSS a type switch by reference — JsonUtility alone drops them,
    /// which silently reset every nested reference before this coverage existed;</item>
    /// <item><see cref="SerializeReferenceHelpers.CloneManagedReferenceGraph"/> deep-copies for Make-unique /
    /// de-alias: children become independent, internal aliasing topology survives, cycles terminate.</item>
    /// </list>
    /// </summary>
    [TestFixture]
    internal sealed class SerializeReferenceCloneTests
    {
        private interface IPart { }

        [Serializable]
        private sealed class Gem : IPart
        {
            public int power;
        }

        [Serializable]
        private sealed class Weapon : IPart
        {
            public int damage;
            [SerializeReference] public IPart gem;
            [SerializeReference] public List<IPart> mods = new();
            [SerializeReference] public IPart[] attachments = Array.Empty<IPart>();
        }

        [Serializable]
        private sealed class OtherWeapon : IPart
        {
            public int damage;
            [SerializeReference] public IPart gem;
        }

        [Serializable]
        private sealed class Link : IPart
        {
            [SerializeReference] public IPart next;
        }

        [Serializable]
        private struct Slot
        {
            [SerializeReference] public IPart part;
        }

        [Serializable]
        private sealed class Socket
        {
            public int size;
            [SerializeReference] public IPart part;
        }

        // Managed references inside by-value data, plus a private one without [SerializeField].
        [Serializable]
        private sealed class Rig : IPart
        {
            [SerializeReference] public IPart gem;
            public Slot slot = new();
            public Socket socket = new();
            public Slot[] slots = Array.Empty<Slot>();
            public List<Socket> sockets = new();
            [SerializeReference] private IPart _hidden;

            public IPart Hidden
            {
                get => _hidden;
                set => _hidden = value;
            }
        }

        [Serializable]
        private sealed class OtherRig : IPart
        {
            public Slot slot = new();
            public Socket socket = new();
            public Slot[] slots = Array.Empty<Slot>();
            public List<Socket> sockets = new();
            [SerializeReference] private IPart _hidden;

            public IPart Hidden
            {
                get => _hidden;
                set => _hidden = value;
            }
        }

        [Test]
        public void CreateInstancePreservingData_TypeSwitch_CarriesNestedReferencesByIdentity()
        {
            var previous = new Weapon { damage = 7, gem = new Gem { power = 3 } };

            var switched = (OtherWeapon)SerializeReferenceHelpers.CreateInstancePreservingData(
                typeof(OtherWeapon), previous);

            Assert.AreEqual(7, switched.damage, "Plain shared fields must keep riding the JSON round-trip.");
            Assert.AreSame(previous.gem, switched.gem,
                "A nested [SerializeReference] shared by name must carry over as the same instance — the old " +
                "parent is discarded, so reuse is what preserves the child (JsonUtility alone drops it).");
        }

        [Test]
        public void CloneManagedReferenceGraph_MakesNestedChildrenIndependent()
        {
            var source = new Weapon
            {
                damage = 5,
                gem = new Gem { power = 9 },
                mods = new List<IPart> { new Gem { power = 1 } },
                attachments = new IPart[] { new Gem { power = 2 } },
            };

            var clone = (Weapon)SerializeReferenceHelpers.CloneManagedReferenceGraph(source);

            Assert.AreEqual(5, clone.damage);
            Assert.AreNotSame(source.gem, clone.gem, "Make-unique promises an independent nested instance.");
            Assert.AreEqual(9, ((Gem)clone.gem).power, "The independent copy must keep the child's data.");

            Assert.AreNotSame(source.mods, clone.mods, "A list of references must be rebuilt, never shared.");
            Assert.AreNotSame(source.mods[0], clone.mods[0]);
            Assert.AreEqual(1, ((Gem)clone.mods[0]).power);

            Assert.AreNotSame(source.attachments, clone.attachments, "An array of references must be rebuilt too.");
            Assert.AreNotSame(source.attachments[0], clone.attachments[0]);
            Assert.AreEqual(2, ((Gem)clone.attachments[0]).power);
        }

        [Test]
        public void CloneManagedReferenceGraph_PreservesInternalAliasing()
        {
            var shared = new Gem { power = 4 };
            var source = new Weapon { gem = shared, mods = new List<IPart> { shared } };

            var clone = (Weapon)SerializeReferenceHelpers.CloneManagedReferenceGraph(source);

            Assert.AreNotSame(shared, clone.gem, "The shared child itself must still be copied.");
            Assert.AreSame(clone.gem, clone.mods[0],
                "Two fields aliasing one nested instance must alias one copy — internal topology is data.");
        }

        [Test]
        public void CloneManagedReferenceGraph_TerminatesOnCycles()
        {
            var first = new Link();
            var second = new Link { next = first };
            first.next = second;

            var clone = (Link)SerializeReferenceHelpers.CloneManagedReferenceGraph(first);

            Assert.AreNotSame(first, clone);
            Assert.AreNotSame(second, clone.next);
            Assert.AreSame(clone, ((Link)clone.next).next,
                "A cyclic graph must clone into its own cycle instead of recursing forever.");
        }

        [Test]
        public void CreateInstancePreservingData_TypeSwitch_CarriesReferencesInsideByValueData()
        {
            var previous = CreateRig();

            var switched = (OtherRig)SerializeReferenceHelpers.CreateInstancePreservingData(
                typeof(OtherRig), previous);

            Assert.AreEqual(5, switched.socket.size, "Plain data inside a serializable class must keep riding JSON.");
            Assert.AreSame(previous.slot.part, switched.slot.part, "A reference inside a struct must carry over.");
            Assert.AreSame(previous.socket.part, switched.socket.part,
                "A reference inside a serializable class must carry over.");
            Assert.AreSame(previous.slots[0].part, switched.slots[0].part,
                "A reference inside an array element must carry over.");
            Assert.AreSame(previous.sockets[0].part, switched.sockets[0].part,
                "A reference inside a list element must carry over.");
        }

        [Test]
        public void CloneManagedReferenceGraph_CopiesReferencesInsideByValueData()
        {
            var source = CreateRig();

            var clone = (Rig)SerializeReferenceHelpers.CloneManagedReferenceGraph(source);

            Assert.AreEqual(5, clone.socket.size);
            AssertIndependentCopy(source.slot.part, clone.slot.part, where: "a struct");
            AssertIndependentCopy(source.socket.part, clone.socket.part, where: "a serializable class");
            AssertIndependentCopy(source.slots[0].part, clone.slots[0].part, where: "an array element");
            AssertIndependentCopy(source.sockets[0].part, clone.sockets[0].part, where: "a list element");
        }

        [Test]
        public void CloneManagedReferenceGraph_InsideByValueData_PreservesAliasingAndLeavesSourceIntact()
        {
            var shared = new Gem { power = 6 };
            var source = new Rig
            {
                gem = shared,
                slot = new Slot { part = shared },
                sockets = new List<Socket> { new() { part = shared } },
            };

            var clone = (Rig)SerializeReferenceHelpers.CloneManagedReferenceGraph(source);

            Assert.AreNotSame(shared, clone.gem);
            Assert.AreSame(clone.gem, clone.slot.part,
                "A field and a struct aliasing one instance must alias one copy.");
            Assert.AreSame(clone.gem, clone.sockets[0].part,
                "A field and a list element aliasing one instance must alias one copy.");
            Assert.AreSame(shared, source.slot.part, "Make unique must not rewrite the source's struct.");
            Assert.AreSame(shared, source.sockets[0].part, "Make unique must not rewrite the source's list.");
        }

        [Test]
        public void PrivateSerializeReferenceField_IsCarriedOnTypeSwitch()
        {
            var previous = new Rig { Hidden = new Gem { power = 8 } };

            var switched = (OtherRig)SerializeReferenceHelpers.CreateInstancePreservingData(
                typeof(OtherRig), previous);

            Assert.AreSame(previous.Hidden, switched.Hidden,
                "Unity serializes a private [SerializeReference] field without [SerializeField] too.");
        }

        [Test]
        public void PrivateSerializeReferenceField_IsClonedWithItsAliases()
        {
            var shared = new Gem { power = 8 };
            var source = new Rig { gem = shared, Hidden = shared };

            var clone = (Rig)SerializeReferenceHelpers.CloneManagedReferenceGraph(source);

            AssertIndependentCopy(source.Hidden, clone.Hidden, where: "a private field");
            Assert.AreSame(clone.gem, clone.Hidden,
                "A private field aliasing a public one must alias the same copy.");
        }

        private static Rig CreateRig() => new()
        {
            slot = new Slot { part = new Gem { power = 1 } },
            socket = new Socket { size = 5, part = new Gem { power = 2 } },
            slots = new[] { new Slot { part = new Gem { power = 3 } } },
            sockets = new List<Socket> { new() { part = new Gem { power = 4 } } },
        };

        private static void AssertIndependentCopy(IPart original, IPart copy, string where)
        {
            Assert.IsNotNull(copy, $"The reference inside {where} must not become null.");
            Assert.AreNotSame(original, copy, $"The reference inside {where} must become an independent copy.");
            Assert.AreEqual(((Gem)original).power, ((Gem)copy).power, $"The copy inside {where} must keep its data.");
        }
    }
}
