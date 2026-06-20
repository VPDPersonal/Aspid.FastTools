using System;
using System.IO;

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

        // RefIds present in the empty-fields fixture below.
        public const long EmptyRailgunRid = 1001;  // _primaryWeapon  (resolvable, holds a cleared nested _chargeEffect)
        public const long EmptyPistolRid = 1002;   // _sidearms[0]    (resolvable)

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

        /// <summary>Writes <paramref name="yaml"/> to a fresh temp file (never under <c>Assets/</c>) and returns its path.</summary>
        public static string WriteTemp(string yaml)
        {
            var dir = Path.Combine(Path.GetTempPath(), "aspid-sr-tests");
            Directory.CreateDirectory(dir);
            var path = Path.Combine(dir, Guid.NewGuid().ToString("N") + ".prefab");
            File.WriteAllText(path, yaml);
            return path;
        }

        /// <summary>Deletes a temp fixture file if present.</summary>
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
