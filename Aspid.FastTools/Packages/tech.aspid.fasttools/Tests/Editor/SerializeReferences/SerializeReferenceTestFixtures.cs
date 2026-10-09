using System;
using UnityEngine;
using Aspid.FastTools.Types;
using System.Collections.Generic;
using UnityEngine.Scripting.APIUpdating;

namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    // Shared fixture types for the SerializeReferences EditMode tests. They live in one file on purpose: several
    // fixtures (Inspector, NoticeLayout, SharedAlias, RepairSuggestion, MovedFromResolver, CiGate) assert against the
    // same types, and a fixture declared inside one test file silently couples the others to it.

    internal interface ITestWeapon { }

    [Serializable]
    internal sealed class TestSword : ITestWeapon
    {
        public int damage;
    }

    // Declared field types that Unity names with '/' between nested types (Outer/IEffect).
    internal sealed class NestedTypeHolder
    {
        internal interface IEffect { }

        [Serializable]
        internal sealed class Impl : IEffect { }
    }

    internal interface ITestGenericEffect<T> { }

    internal sealed class NestedFieldTypeObject : ScriptableObject
    {
        [SerializeReference] public NestedTypeHolder.IEffect effect;
        [SerializeReference] public ITestGenericEffect<NestedTypeHolder.Impl> generic;
        [SerializeReference] public NestedTypeHolder.IEffect[] effects = { null };
    }

    // Two managed-reference fields, used to prove Link to Existing actually shares one rid.
    internal sealed class LinkerTestObject : ScriptableObject
    {
        [SerializeReference] public ITestWeapon a;
        [SerializeReference] public ITestWeapon b;
    }

    // A managed-reference list, used to note a clear on one of its elements.
    internal sealed class ReferenceListTestObject : ScriptableObject
    {
        [SerializeReference] public List<ITestWeapon> weapons = new();
    }

    // A list of references where one kind holds references of its own, for the missing-type probe of a list with unsaved
    // edits. The marker interface keeps these types out of the ITestWeapon pickers.
    internal interface ITestGear { }

    [Serializable]
    internal sealed class TestBlade : ITestGear
    {
        public int damage;
    }

    [Serializable]
    internal sealed class TestHolster : ITestGear
    {
        [SerializeReference] public ITestGear item;
        [SerializeReference] public List<ITestGear> spares = new();
    }

    internal sealed class GearListTestObject : ScriptableObject
    {
        [SerializeReference] public List<ITestGear> gear = new();
    }

    // A required managed reference and a required string type field.
    internal sealed class RequiredTestObject : ScriptableObject
    {
        [SerializeReference, TypeSelector(Required = true)] public ITestWeapon requiredRef;
        [TypeSelector(Required = true)] public string requiredString;
    }

    // A required field of every ISerializableType wrapper, top-level and inside a by-value container: the object-load
    // scan must report each by the wrapper's path, as the scene scan does, not by its backing string.
    internal sealed class RequiredWrapperTestObject : ScriptableObject
    {
        [TypeSelector(Required = true)] public SerializableType type;
        [TypeSelector(Required = true)] public SerializableType<ITestWeapon> weaponType;
        [TypeSelector(Required = true)] public SerializableMonoScript script;
        public RequiredWrapperLoadout loadout = new();
    }

    [Serializable]
    internal sealed class RequiredWrapperLoadout
    {
        [TypeSelector(Required = true)] public SerializableType type;
    }

    // Wrapper fields whose picker settings the project-wide type name repair reads from the declaring type.
    [Serializable]
    internal sealed class TypeNameConstraintHolder
    {
        public SerializableType<ITestWeapon> weapon;
        [TypeSelector(Allow = TypeAllow.None)] public List<SerializableType> concrete = new();
        public SerializableMonoScript script;
    }

    // A plain [Serializable] by-value container holding required fields — the nesting shape the required gate must
    // recurse into (ASP-52): the fields appear in YAML as children of the container key, not at the document top level.
    [Serializable]
    internal sealed class RequiredLoadout
    {
        [SerializeReference, TypeSelector(Required = true)] public ITestWeapon primary;
        [TypeSelector(Required = true)] public string typeName;
    }

    // A component whose only required fields live inside a nested serializable container ([SerializeField] private,
    // so the walk is proven to include non-public serialized fields).
    internal sealed class NestedRequiredTestObject : ScriptableObject
    {
        [SerializeField] private RequiredLoadout _loadout = new();
    }

    // A self-referential serializable shape: the reflection walk must terminate on the cycle instead of recursing
    // forever (Unity itself refuses such graphs at serialize time, but GetRequiredFields sees the raw type).
    [Serializable]
    internal sealed class RequiredCycleNode
    {
        public RequiredCycleNode next;
        [TypeSelector(Required = true)] public string typeName;
    }

    internal sealed class CycleRequiredTestObject : ScriptableObject
    {
        public RequiredCycleNode root;
    }

    // Top-level (namespace-scoped) candidate pool for the ranking tests. The marker interface keeps the TypeCache
    // pool down to these two types, so the assertions never race additions elsewhere in the project.
    internal interface IRepairRankTarget { }

    // The "moved without [MovedFrom]" shape the SerializeReferences sample's MovedWeaponPreset.asset demonstrates:
    // ranked by simple-name match plus the orphaned data's field-shape overlap.
    [Serializable]
    internal sealed class RelocatedRanged : IRepairRankTarget
    {
        [SerializeField] private int _damage;
        [SerializeField] private int _magazineSize;
    }

    // A declared rename: the recorded old class name must out-rank every heuristic with the top score.
    [Serializable]
    [MovedFrom(false, null, null, "OldRenamedRanged")]
    internal sealed class RenamedRanged : IRepairRankTarget
    {
        [SerializeField] private int _damage;
    }
}
