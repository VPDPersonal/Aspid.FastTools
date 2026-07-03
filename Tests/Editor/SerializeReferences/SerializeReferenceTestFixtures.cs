using System;
using UnityEngine;
using Aspid.FastTools.Types;
using UnityEngine.Scripting.APIUpdating;

namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    // Shared fixture types for the SerializeReferences EditMode tests. They live in one file on purpose: several
    // fixtures (Inspector, NoticeLayout, SharedAlias, RepairSuggestion, MovedFromResolver, CiGate) assert against the
    // same types, and a fixture declared inside one test file silently couples the others to it.

    internal interface ITestWeapon { }

    [Serializable]
    internal sealed class TestSword : ITestWeapon { public int damage; }

    // Two managed-reference fields, used to prove Link to Existing actually shares one rid.
    internal sealed class LinkerTestObject : ScriptableObject
    {
        [SerializeReference] public ITestWeapon a;
        [SerializeReference] public ITestWeapon b;
    }

    // A required managed reference and a required string type field.
    internal sealed class RequiredTestObject : ScriptableObject
    {
        [SerializeReference, TypeSelector(Required = true)] public ITestWeapon requiredRef;
        [TypeSelector(Required = true)] public string requiredString;
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
