using System;
using UnityEngine;
using Aspid.FastTools.Types;

namespace Aspid.FastTools.SerializeReferences.Tests
{
    public interface IPrefabTestWeapon { }

    [Serializable]
    public sealed class PrefabTestSword : IPrefabTestWeapon
    {
        public int damage;
    }

    [Serializable]
    public sealed class PrefabTestBow : IPrefabTestWeapon
    {
        public int arrows;
    }

    // A component the EditMode tests save into prefabs and variants. Unity attaches only a runtime script whose file
    // carries its name, so it cannot live beside the editor-only tests.
    [AddComponentMenu("")]
    public sealed class PrefabReferenceProbe : MonoBehaviour
    {
        [SerializeReference] public IPrefabTestWeapon weapon;
        [SerializeReference, TypeSelector(Required = true)] public IPrefabTestWeapon requiredWeapon;
    }
}
