using System;
using UnityEngine;
using Aspid.FastTools.Types;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.EditorTools
{
    // Plain data. Everything visual lives in Editor/: a custom inspector and a catalog window.
    [CreateAssetMenu(menuName = "Aspid/FastTools/Samples/Ability Config", fileName = "Ability")]
    public sealed class AbilityConfig : ScriptableObject
    {
        [SerializeField] private string _abilityName = "New Ability";
        [SerializeField] [TextArea] private string _description;
        [SerializeField] [Min(0)] private int _manaCost = 10;
        [SerializeField] [Min(0f)] private float _cooldown = 1f;

        // Written by the catalog window through TypeSelectorWindow; the attribute gives the plain inspector
        // the same picker.
        [TypeSelector(typeof(IAbilityEffect), Allow = TypeAllow.None)]
        [SerializeField] private string _effectType;

        public string AbilityName => _abilityName;

        public string Description => _description;

        public int ManaCost => _manaCost;

        public float Cooldown => _cooldown;

        public Type EffectType => string.IsNullOrEmpty(_effectType) ? null : Type.GetType(_effectType);
    }
}
