using System;
using System.IO;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    /// <summary>
    /// Real-format Unity YAML fixtures and temp-file helpers for the <see cref="SerializeReferenceYamlEditor"/> tests.
    /// The prefab text is copied verbatim from the <c>Samples~/SerializeReferences</c> demo so the parser is exercised
    /// against Unity's exact indentation, <c>references:</c>/<c>RefIds:</c>/<c>data:</c> layout and field-pointer shapes
    /// (single managed ref, list of managed refs, nested managed ref). The strings are written at column 0 inside the
    /// verbatim literal — their leading whitespace IS the YAML indentation and must not be reflowed.
    /// </summary>
    internal static class YamlFixtures
    {
        // The MonoBehaviour document's local file id (its "--- !u!114 &<fileID>" anchor).
        public const long MonoBehaviourFileId = 6500000000000000003L;

        // RefIds present in the fixture, by stored type.
        public const long RailgunRid = 1001;      // _primaryWeapon         (resolvable)
        public const long GhostPistolRid = 1002;  // _sidearms[0]           (MISSING — class starts with "Ghost")
        public const long ShotgunRid = 1003;      // _sidearms[1]           (resolvable)
        public const long FreezeEffectRid = 1004; // _onHitEffect           (resolvable)
        public const long BurnEffectRid = 1005;   // _primaryWeapon._chargeEffect (resolvable, nested in Railgun)

        // A MonoBehaviour with one missing managed reference (GhostPistol), a single ref field, a list of refs, and a
        // nested ref (Railgun -> _chargeEffect -> BurnEffect).
        public const string MissingTypePrefab =
@"%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!1 &6500000000000000001
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  serializedVersion: 6
  m_Component:
  - component: {fileID: 6500000000000000002}
  - component: {fileID: 6500000000000000003}
  m_Layer: 0
  m_Name: LoadoutMissingType
  m_TagString: Untagged
  m_Icon: {fileID: 0}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!114 &6500000000000000003
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 6500000000000000001}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: 884d53b5154744d3af6948b1eef02505, type: 3}
  m_Name:
  m_EditorClassIdentifier: Aspid.FastTools.Samples.SerializeReferences::Aspid.FastTools.Samples.SerializeReferences.Loadout
  _primaryWeapon:
    rid: 1001
  _sidearms:
  - rid: 1002
  - rid: 1003
  _onHitEffect:
    rid: 1004
  references:
    version: 2
    RefIds:
    - rid: 1001
      type: {class: Railgun, ns: Aspid.FastTools.Samples.SerializeReferences, asm: Aspid.FastTools.Samples.SerializeReferences}
      data:
        _chargeTime: 2
        _chargeEffect:
          rid: 1005
    - rid: 1002
      type: {class: GhostPistol, ns: Aspid.FastTools.Samples.SerializeReferences, asm: Aspid.FastTools.Samples.SerializeReferences}
      data:
        _damage: 15
        _magazineSize: 12
    - rid: 1003
      type: {class: Shotgun, ns: Aspid.FastTools.Samples.SerializeReferences, asm: Aspid.FastTools.Samples.SerializeReferences}
      data:
        _pellets: 8
        _spreadAngle: 25
    - rid: 1004
      type: {class: FreezeEffect, ns: Aspid.FastTools.Samples.SerializeReferences, asm: Aspid.FastTools.Samples.SerializeReferences}
      data:
        _duration: 2.5
        _slowPercent: 40
    - rid: 1005
      type: {class: BurnEffect, ns: Aspid.FastTools.Samples.SerializeReferences, asm: Aspid.FastTools.Samples.SerializeReferences}
      data:
        _duration: 3
        _damagePerSecond: 5
";

        // A MonoBehaviour where the MISSING reference (GhostPistol, rid 1002) is ALIASED across two slots — both
        // _primaryWeapon and _sidearms[0] point at the one rid — alongside a singly-pointed sibling (Shotgun, rid 1003,
        // _sidearms[1]) and a healthy effect (rid 1004). Pins the all-pointer-null behaviour and the pointer-count helper:
        // clearing rid 1002 must null BOTH aliased slots (count 2) while leaving the Shotgun slot intact. Same indentation
        // and layout as MissingTypePrefab; reuses MonoBehaviourFileId / GhostPistolRid / ShotgunRid.
        public const string AliasedMissingTypePrefab =
@"%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!1 &6500000000000000001
GameObject:
  serializedVersion: 6
  m_Component:
  - component: {fileID: 6500000000000000003}
  m_Name: LoadoutAliasedMissing
--- !u!114 &6500000000000000003
MonoBehaviour:
  m_GameObject: {fileID: 6500000000000000001}
  m_Enabled: 1
  m_Script: {fileID: 11500000, guid: 884d53b5154744d3af6948b1eef02505, type: 3}
  m_Name:
  _primaryWeapon:
    rid: 1002
  _sidearms:
  - rid: 1002
  - rid: 1003
  _onHitEffect:
    rid: 1004
  references:
    version: 2
    RefIds:
    - rid: 1002
      type: {class: GhostPistol, ns: Aspid.FastTools.Samples.SerializeReferences, asm: Aspid.FastTools.Samples.SerializeReferences}
      data:
        _damage: 15
        _magazineSize: 12
    - rid: 1003
      type: {class: Shotgun, ns: Aspid.FastTools.Samples.SerializeReferences, asm: Aspid.FastTools.Samples.SerializeReferences}
      data:
        _pellets: 8
        _spreadAngle: 25
    - rid: 1004
      type: {class: FreezeEffect, ns: Aspid.FastTools.Samples.SerializeReferences, asm: Aspid.FastTools.Samples.SerializeReferences}
      data:
        _duration: 2.5
        _slowPercent: 40
";

        // The single-object document file id of the nested-list-pointer fixture below, and its resolvable WeaponRack entry.
        public const long NestedListPointerFileId = 8800000000000000003L;
        public const long NestedListPointerRackRid = 1001;

        // A MonoBehaviour whose one managed reference (WeaponRack, rid 1001) holds a List<IWeapon> _weapons with a single
        // element pointing at a MISSING entry (GhostPistol, rid 1002). The nested "- rid: 1002" list element sits DEEPER
        // than the RefIds entry headers and is immediately followed (inside the 4-line type lookahead) by rid 1002's own
        // "type: {class: GhostPistol …}". FindMissingReferences must treat only the entry-indent "- rid:" lines as entries,
        // so rid 1002 is reported exactly once — never doubled by also reading the nested pointer as a phantom entry.
        public const string NestedListPointerPrefab =
@"%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &8800000000000000003
MonoBehaviour:
  m_GameObject: {fileID: 8800000000000000001}
  m_Enabled: 1
  m_Script: {fileID: 11500000, guid: 884d53b5154744d3af6948b1eef02505, type: 3}
  m_Name:
  _loadout:
    rid: 1001
  references:
    version: 2
    RefIds:
    - rid: 1001
      type: {class: WeaponRack, ns: Aspid.FastTools.Samples.SerializeReferences, asm: Aspid.FastTools.Samples.SerializeReferences}
      data:
        _weapons:
        - rid: 1002
    - rid: 1002
      type: {class: GhostPistol, ns: Aspid.FastTools.Samples.SerializeReferences, asm: Aspid.FastTools.Samples.SerializeReferences}
      data:
        _damage: 15
        _magazineSize: 12
";

        // The document and rids of the nested-children fixture below.
        public const long NestedChildrenFileId = 8900000000000000003L;
        public const long NestedChildrenSequenceRid = 100; // _root                         (Sequence)
        public const long NestedChildrenChildRid = 101;    // _root._children[0]            (ChildA)
        public const long NestedChildrenGhostRid = 102;    // _root._children[1], _weapons[0] (MISSING — GhostChild)

        // A MonoBehaviour whose Sequence entry (rid 100) comes FIRST and lists both children in its data block
        // ("_children: - rid: 101 - rid: 102"), followed by the healthy ChildA entry (101) and the missing GhostChild entry
        // (102), which a top-level list (_weapons) also points at. Each nested "- rid: N" pointer is followed by the entry
        // of a DIFFERENT rid, so a lookup that takes the first "- rid: N" at any indent lands on the wrong entry.
        public const string NestedChildrenPrefab =
@"%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &8900000000000000003
MonoBehaviour:
  m_GameObject: {fileID: 8900000000000000001}
  m_Enabled: 1
  m_Script: {fileID: 11500000, guid: 884d53b5154744d3af6948b1eef02505, type: 3}
  m_Name:
  _root:
    rid: 100
  _weapons:
  - rid: 102
  references:
    version: 2
    RefIds:
    - rid: 100
      type: {class: Sequence, ns: Game, asm: Assembly-CSharp}
      data:
        _children:
        - rid: 101
        - rid: 102
    - rid: 101
      type: {class: ChildA, ns: Game, asm: Assembly-CSharp}
      data:
        _a: 1
    - rid: 102
      type: {class: GhostChild, ns: Game, asm: Assembly-CSharp}
      data:
        _b: 2
";

        // The document of the shadowed-list fixture below.
        public const long ShadowedListFileId = 9100000000000000003L;

        // A MonoBehaviour where an EARLIER field's list of structs (_presets) holds a nested list named exactly like the
        // later TOP-LEVEL list (_weapons), with a null slot at index 0 and rid 300 at index 1. The top-level _weapons is
        // [-2, -2, 300], so only a lookup at the m_Script indent reaches the real top-level slots.
        public const string ShadowedListPrefab =
@"%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &9100000000000000003
MonoBehaviour:
  m_GameObject: {fileID: 9100000000000000001}
  m_Enabled: 1
  m_Script: {fileID: 11500000, guid: 884d53b5154744d3af6948b1eef02505, type: 3}
  m_Name:
  _presets:
  - _name: a
    _weapons:
    - rid: -2
    - rid: 300
  _weapons:
  - rid: -2
  - rid: -2
  - rid: 300
  references:
    version: 2
    RefIds:
    - rid: -2
      type: {class: , ns: , asm: }
    - rid: 300
      type: {class: Pistol, ns: Aspid.FastTools.Samples.SerializeReferences, asm: Aspid.FastTools.Samples.SerializeReferences}
      data:
        _damage: 10
";

        // The documents of the negative-anchor fixtures below. Unity writes signed 64-bit fileIDs, and sub-assets
        // (AddObjectToAsset) and prefab components often get a negative one.
        public const long NegativeSubAssetFileId = -5000000000000000001L;
        public const long NegativeMainFileId = 11400000L;
        public const long NegativeSharedRid = 1000;

        // A .asset sorted by fileID the way Unity writes it: the negative sub-asset comes BEFORE the main object. Both
        // documents use rid 1000; the sub-asset's is MISSING (GhostNode), the main object's is healthy (HealthyNode).
        public const string NegativeSubAssetBeforeMainAsset =
@"%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &-5000000000000000001
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_Script: {fileID: 11500000, guid: b7874533c7294db1b8aa77e7d4102c9f, type: 3}
  m_Name: SubNode
  _payload:
    rid: 1000
  references:
    version: 2
    RefIds:
    - rid: 1000
      type: {class: GhostNode, ns: Game, asm: Assembly-CSharp}
      data:
        _n: 1
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_Script: {fileID: 11500000, guid: b7874533c7294db1b8aa77e7d4102c9f, type: 3}
  m_Name: MainNode
  _payload:
    rid: 1000
  references:
    version: 2
    RefIds:
    - rid: 1000
      type: {class: HealthyNode, ns: Game, asm: Assembly-CSharp}
      data:
        _n: 2
";

        // The components of the negative-anchor prefab below, and the rids of the second component's references.
        public const long NegativePrefabFirstFileId = 101L;
        public const long NegativePrefabSecondFileId = -4000000000000000002L;
        public const string NegativePrefabSecondScriptGuid = "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb";
        public const long NegativePrefabPayloadRid = 7000; // _payload (healthy ChildA)
        public const long NegativePrefabGhostRid = 7001;   // _list[0] (MISSING — GhostNode)

        // A prefab, whose documents Unity does NOT sort: a MonoBehaviour with a positive fileID and its own references
        // block, followed by a second MonoBehaviour with a NEGATIVE fileID holding a healthy reference, a list element
        // pointing at a missing one and an empty required string field.
        public const string NegativeComponentAfterPositivePrefab =
@"%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!1 &100
GameObject:
  m_Component:
  - component: {fileID: 101}
  - component: {fileID: -4000000000000000002}
  m_Name: Player
--- !u!114 &101
MonoBehaviour:
  m_GameObject: {fileID: 100}
  m_Script: {fileID: 11500000, guid: aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa, type: 3}
  m_Name:
  _main:
    rid: 1000
  references:
    version: 2
    RefIds:
    - rid: 1000
      type: {class: HealthyNode, ns: Game, asm: Assembly-CSharp}
      data:
        _n: 1
--- !u!114 &-4000000000000000002
MonoBehaviour:
  m_GameObject: {fileID: 100}
  m_Script: {fileID: 11500000, guid: bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb, type: 3}
  m_Name:
  requiredString:
  _payload:
    rid: 7000
  _list:
  - rid: 7001
  references:
    version: 2
    RefIds:
    - rid: 7000
      type: {class: ChildA, ns: Game, asm: Assembly-CSharp}
      data:
        _n: 1
    - rid: 7001
      type: {class: GhostNode, ns: Game, asm: Assembly-CSharp}
      data:
        _n: 2
";

        // The Railgun entry of the empty-fields fixture below: _primaryWeapon, resolvable, holds a cleared nested _chargeEffect.
        public const long EmptyRailgunRid = 1001;

        // A MonoBehaviour exercising unassigned (null-sentinel) [SerializeReference] slots written by Unity as
        // "rid: -2" (ManagedReferenceUtility.RefIdNull): a cleared top-level field (_onHitEffect), a null list element
        // (_sidearms[1]) and a cleared nested field (Railgun._chargeEffect), alongside assigned references so the
        // RefIds block still exists. Same indentation / layout Unity emits, copied from the demo shape.
        public const string EmptyFieldsPrefab =
@"%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!1 &6500000000000000001
GameObject:
  serializedVersion: 6
  m_Component:
  - component: {fileID: 6500000000000000003}
  m_Name: LoadoutEmptyFields
--- !u!114 &6500000000000000003
MonoBehaviour:
  m_GameObject: {fileID: 6500000000000000001}
  m_Enabled: 1
  m_Script: {fileID: 11500000, guid: 884d53b5154744d3af6948b1eef02505, type: 3}
  m_Name:
  _primaryWeapon:
    rid: 1001
  _sidearms:
  - rid: 1002
  - rid: -2
  _onHitEffect:
    rid: -2
  references:
    version: 2
    RefIds:
    - rid: 1001
      type: {class: Railgun, ns: Aspid.FastTools.Samples.SerializeReferences, asm: Aspid.FastTools.Samples.SerializeReferences}
      data:
        _chargeTime: 2
        _chargeEffect:
          rid: -2
    - rid: 1002
      type: {class: Pistol, ns: Aspid.FastTools.Samples.SerializeReferences, asm: Aspid.FastTools.Samples.SerializeReferences}
      data:
        _damage: 15
        _magazineSize: 12
";

        // A ScriptableObject whose only [SerializeReference] fields are all unassigned: a single null field (_weapon)
        // and an empty list (_alternates), so the RefIds block holds nothing but Unity's shared null sentinel
        // ("- rid: -2"). It carries zero real nodes yet one field pointer, so the scanner must still surface it (one
        // "<None>" root) rather than dropping the whole document as "no managed references". Mirrors the demo's
        // BrokenWeaponPreset after its broken reference is cleared to <None>.
        public const string AllUnassignedAsset =
@"%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_Script: {fileID: 11500000, guid: b7874533c7294db1b8aa77e7d4102c9f, type: 3}
  m_Name: BrokenWeaponPreset
  _weapon:
    rid: -2
  _alternates: []
  references:
    version: 2
    RefIds:
    - rid: -2
      type: {class: , ns: , asm: }
";

        // The single-object document file id, the rid and the exact (un-quoted) class identity of the generic fixture below.
        public const long QuotedGenericFileId = 11400000L;
        public const long QuotedGenericRid = 2001L;
        public const string QuotedGenericClass = "Modifier`1[[System.Single, mscorlib]]";

        // A ScriptableObject whose single [SerializeReference] field stores a CLOSED GENERIC type, written by Unity with
        // a single-quoted `Name`N[[arg, asm]]` class identity (the form Unity emits for generics like Foo`1[[...]]).
        // Exercises the quoted-class parse branch of the reader — the generic round-trip the risk register calls out.
        public const string QuotedGenericAsset =
@"%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_Script: {fileID: 11500000, guid: b7874533c7294db1b8aa77e7d4102c9f, type: 3}
  m_Name: GenericModifierAsset
  _modifier:
    rid: 2001
  references:
    version: 2
    RefIds:
    - rid: 2001
      type: {class: 'Modifier`1[[System.Single, mscorlib]]', ns: Aspid.FastTools.Samples.SerializeReferences, asm: Aspid.FastTools.Samples.SerializeReferences}
      data:
        _value: 1.5
";

        // The single-object document file id of the List-of-struct fixture below.
        public const long ListOfStructFileId = 7700000000000000003L;

        // A MonoBehaviour with a List of a PLAIN serializable struct (a "slot") that itself holds a [SerializeReference]
        // managed reference. Exercises ResolveSequenceItem's container branch — descending a sequence-of-mappings element
        // into a nested managed reference (`_slots.Array.data[i]._weapon`), the List<Struct> path the reader advertises.
        public const string ListOfStructAsset =
@"%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &7700000000000000003
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_Script: {fileID: 11500000, guid: 884d53b5154744d3af6948b1eef02505, type: 3}
  m_Name: SlottedLoadoutAsset
  _slots:
  - _weapon:
      rid: 3001
    label: primary
  - _weapon:
      rid: 3002
    label: backup
  references:
    version: 2
    RefIds:
    - rid: 3001
      type: {class: Pistol, ns: Aspid.FastTools.Samples.SerializeReferences, asm: Aspid.FastTools.Samples.SerializeReferences}
      data:
        _damage: 10
        _magazineSize: 7
    - rid: 3002
      type: {class: Shotgun, ns: Aspid.FastTools.Samples.SerializeReferences, asm: Aspid.FastTools.Samples.SerializeReferences}
      data:
        _pellets: 6
        _spreadAngle: 20
";

        // The single-object document file id and the two rids of the shadowed-field fixture below.
        public const long ShadowedFileId = 9000000000000000003L;
        public const long ShadowedNestedRid = 4001;   // _config._weapon (nested inside a plain serializable container)
        public const long ShadowedTopLevelRid = 4002; // _weapon         (the real top-level field)

        // A MonoBehaviour where an EARLIER field's plain serializable container (_config) holds a key named exactly like
        // a later TOP-LEVEL field (_weapon). The nested _weapon (rid 4001) appears first in the document, so a reader
        // that matched the first path segment at any indent would resolve "_weapon" to the nested pointer; matching at
        // the document's top-level field indent (the m_Script line's indent) must resolve it to rid 4002.
        public const string ShadowedFieldPrefab =
@"%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &9000000000000000003
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_Script: {fileID: 11500000, guid: 884d53b5154744d3af6948b1eef02505, type: 3}
  m_Name: ShadowedLoadout
  _config:
    _weapon:
      rid: 4001
    _label: primary
  _weapon:
    rid: 4002
  references:
    version: 2
    RefIds:
    - rid: 4001
      type: {class: Pistol, ns: Aspid.FastTools.Samples.SerializeReferences, asm: Aspid.FastTools.Samples.SerializeReferences}
      data:
        _damage: 10
        _magazineSize: 7
    - rid: 4002
      type: {class: Shotgun, ns: Aspid.FastTools.Samples.SerializeReferences, asm: Aspid.FastTools.Samples.SerializeReferences}
      data:
        _pellets: 8
        _spreadAngle: 25
";

        // The script guid of the scene required-field fixtures below — maps (via the test's injected resolver) to
        // RequiredTestObject's required fields. Any other guid (the mixed fixture's bbbb… script) is unknown to the resolver.
        public const string RequiredSceneScriptGuid = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";

        // The known-script MonoBehaviour document's file id in the scene fixtures (its "--- !u!114 &<fileID>" anchor).
        public const long RequiredSceneMonoFileId = 101L;

        // A scene with one MonoBehaviour whose required managed reference (requiredRef) and required string field
        // (requiredString) are both left unset — the managed reference at Unity's null id (-2), the string empty. Exact
        // .unity layout: a GameObject document followed by its MonoBehaviour, m_Script carrying the script guid.
        public const string RequiredSceneUnset =
@"%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!1 &100
GameObject:
  m_Component:
  - component: {fileID: 101}
  m_Name: Hero
--- !u!114 &101
MonoBehaviour:
  m_GameObject: {fileID: 100}
  m_Enabled: 1
  m_Script: {fileID: 11500000, guid: aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa, type: 3}
  m_Name:
  requiredRef:
    rid: -2
  requiredString:
  references:
    version: 2
    RefIds:
    - rid: -2
      type: {class: , ns: , asm: }
";

        // The same scene with both required fields set: the managed reference points at a real rid, the string holds an
        // assembly-qualified name. No violations expected.
        public const string RequiredSceneSet =
@"%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!1 &100
GameObject:
  m_Component:
  - component: {fileID: 101}
  m_Name: Hero
--- !u!114 &101
MonoBehaviour:
  m_GameObject: {fileID: 100}
  m_Enabled: 1
  m_Script: {fileID: 11500000, guid: aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa, type: 3}
  m_Name:
  requiredRef:
    rid: 5001
  requiredString: 'Aspid.FastTools.Samples.SerializeReferences.Pistol, Aspid.FastTools.Samples.SerializeReferences'
  references:
    version: 2
    RefIds:
    - rid: 5001
      type: {class: Pistol, ns: Aspid.FastTools.Samples.SerializeReferences, asm: Aspid.FastTools.Samples.SerializeReferences}
      data:
        _damage: 5
";

        // A scene whose MonoBehaviour has a required SerializableType field (requiredType) left unset: the wrapper is
        // written as a block whose nested _assemblyQualifiedName scalar is empty. One violation expected.
        public const string RequiredSceneSerializableTypeUnset =
@"%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!1 &100
GameObject:
  m_Component:
  - component: {fileID: 101}
  m_Name: Hero
--- !u!114 &101
MonoBehaviour:
  m_GameObject: {fileID: 100}
  m_Enabled: 1
  m_Script: {fileID: 11500000, guid: aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa, type: 3}
  m_Name:
  requiredType:
    _assemblyQualifiedName:
";

        // The same scene with the SerializableType populated — its nested _assemblyQualifiedName holds a name. No violation.
        public const string RequiredSceneSerializableTypeSet =
@"%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!1 &100
GameObject:
  m_Component:
  - component: {fileID: 101}
  m_Name: Hero
--- !u!114 &101
MonoBehaviour:
  m_GameObject: {fileID: 100}
  m_Enabled: 1
  m_Script: {fileID: 11500000, guid: aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa, type: 3}
  m_Name:
  requiredType:
    _assemblyQualifiedName: Aspid.FastTools.Samples.SerializeReferences.Pistol, Aspid.FastTools.Samples.SerializeReferences
";

        // A scene whose required fields live INSIDE a plain [Serializable] container (_loadout) — both unset: the
        // managed reference (primary) at the null id, the string (typeName) empty. Unity writes the container as a
        // nested mapping, so the keys sit one indent level below the document's top-level fields (ASP-52).
        public const string RequiredSceneNestedUnset =
@"%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!1 &100
GameObject:
  m_Component:
  - component: {fileID: 101}
  m_Name: Hero
--- !u!114 &101
MonoBehaviour:
  m_GameObject: {fileID: 100}
  m_Enabled: 1
  m_Script: {fileID: 11500000, guid: aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa, type: 3}
  m_Name:
  _loadout:
    primary:
      rid: -2
    typeName:
  references:
    version: 2
    RefIds:
    - rid: -2
      type: {class: , ns: , asm: }
";

        // The same nested-container scene with both fields set: primary points at a real rid, typeName holds a name.
        // No violations expected.
        public const string RequiredSceneNestedSet =
@"%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!1 &100
GameObject:
  m_Component:
  - component: {fileID: 101}
  m_Name: Hero
--- !u!114 &101
MonoBehaviour:
  m_GameObject: {fileID: 100}
  m_Enabled: 1
  m_Script: {fileID: 11500000, guid: aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa, type: 3}
  m_Name:
  _loadout:
    primary:
      rid: 5001
    typeName: 'Aspid.FastTools.Samples.SerializeReferences.Pistol, Aspid.FastTools.Samples.SerializeReferences'
  references:
    version: 2
    RefIds:
    - rid: 5001
      type: {class: Pistol, ns: Aspid.FastTools.Samples.SerializeReferences, asm: Aspid.FastTools.Samples.SerializeReferences}
      data:
        _damage: 5
";

        // The nested-container scene with the WHOLE _loadout key absent (object saved before the container field was
        // added). Like a top-level absent key, this needs a reserialize, not a build failure — no violations expected.
        public const string RequiredSceneNestedContainerAbsent =
@"%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!1 &100
GameObject:
  m_Component:
  - component: {fileID: 101}
  m_Name: Hero
--- !u!114 &101
MonoBehaviour:
  m_GameObject: {fileID: 100}
  m_Enabled: 1
  m_Script: {fileID: 11500000, guid: aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa, type: 3}
  m_Name:
";

        // The same scene where BOTH required keys are ABSENT from the MonoBehaviour — the shape Unity writes when the
        // object was last saved before the [TypeSelector(Required = true)] fields were added (also stripped / nested-prefab
        // docs). Reserializing fills the defaults; until then the gate must NOT flag the missing keys, so no violations
        // are expected. Same .unity layout as RequiredSceneUnset, minus the requiredRef / requiredString lines.
        public const string RequiredSceneAbsentKeys =
@"%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!1 &100
GameObject:
  m_Component:
  - component: {fileID: 101}
  m_Name: Hero
--- !u!114 &101
MonoBehaviour:
  m_GameObject: {fileID: 100}
  m_Enabled: 1
  m_Script: {fileID: 11500000, guid: aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa, type: 3}
  m_Name:
";

        // A two-MonoBehaviour scene: the first (guid aaaa…, fileID 101) has requiredRef SET but requiredString EMPTY
        // (one violation); the second (guid bbbb…, fileID 201) leaves both unset but its script is unknown to the
        // resolver, so it must be skipped entirely. Proves per-document resolution and the unknown-script skip.
        public const string RequiredSceneMixedUnknownScript =
@"%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!1 &100
GameObject:
  m_Component:
  - component: {fileID: 101}
  m_Name: Hero
--- !u!114 &101
MonoBehaviour:
  m_GameObject: {fileID: 100}
  m_Script: {fileID: 11500000, guid: aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa, type: 3}
  m_Name:
  requiredRef:
    rid: 5001
  requiredString:
  references:
    version: 2
    RefIds:
    - rid: 5001
      type: {class: Pistol, ns: Aspid.FastTools.Samples.SerializeReferences, asm: Aspid.FastTools.Samples.SerializeReferences}
      data:
        _damage: 5
--- !u!1 &200
GameObject:
  m_Component:
  - component: {fileID: 201}
  m_Name: Other
--- !u!114 &201
MonoBehaviour:
  m_GameObject: {fileID: 200}
  m_Script: {fileID: 11500000, guid: bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb, type: 3}
  m_Name:
  requiredRef:
    rid: -2
  requiredString:
  references:
    version: 2
    RefIds:
    - rid: -2
      type: {class: , ns: , asm: }
";

        /// <summary>
        /// Writes <paramref name="yaml"/> to a fresh temp file (never under <c>Assets/</c>) and returns its path.
        /// </summary>
        public static string WriteTemp(string yaml)
        {
            var dir = Path.Combine(Path.GetTempPath(), "aspid-sr-tests");
            Directory.CreateDirectory(dir);
            var path = Path.Combine(dir, Guid.NewGuid().ToString("N") + ".prefab");
            File.WriteAllText(path, yaml);

            return path;
        }

        /// <summary>
        /// Deletes a temp fixture file if present.
        /// </summary>
        public static void Delete(string path)
        {
            try
            {
                if (!string.IsNullOrEmpty(path) && File.Exists(path)) File.Delete(path);
            }
            catch
            {
                // Best-effort cleanup; a leftover temp file is harmless.
            }
        }
    }
}
