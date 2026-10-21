using System;
using UnityEngine;
using Aspid.FastTools.Types;

namespace Aspid.FastTools.Samples.EditorTools
{
    public sealed class AbilityConfig : ScriptableObject
    {
        public const int DefaultManaCost = 10;
        public const float DefaultCooldown = 1f;

        [SerializeField] private string _abilityName = "New Ability";
        [SerializeField, TextArea] private string _description;
        [SerializeField, Min(0)] private int _manaCost = DefaultManaCost;
        [SerializeField, Min(0)] private float _cooldown = DefaultCooldown;

        [TypeSelector(typeof(IAbilityEffect), Allow = TypeAllow.None)]
        [SerializeField] private string _effectType;

        public string AbilityName => _abilityName;

        public int ManaCost => _manaCost;

        public float Cooldown => _cooldown;

        public Type EffectType => string.IsNullOrEmpty(_effectType) ? null : Type.GetType(_effectType);
    }
}
