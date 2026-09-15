using System;
using UnityEngine;
using Aspid.FastTools.Types;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.EditorTools
{
    /// <summary>
    /// <see cref="ScriptableObject"/> that stores ability settings and a selectable effect type.
    /// </summary>
    [CreateAssetMenu(menuName = "Aspid/FastTools/Samples/Ability Config", fileName = "Ability")]
    public sealed class AbilityConfig : ScriptableObject
    {
        [Tooltip("Ability name shown in the catalog.")]
        [SerializeField] private string _abilityName = "New Ability";

        [Tooltip("Description shown in the ability details.")]
        [SerializeField, TextArea] private string _description;

        [Tooltip("Mana consumed by the ability.")]
        [SerializeField, Min(0)] private int _manaCost = 10;

        [Tooltip("Seconds between ability uses.")]
        [SerializeField, Min(0f)] private float _cooldown = 1f;

        // Written by the catalog window through TypeSelectorWindow; the attribute gives the plain inspector
        // the same picker.
        [TypeSelector(typeof(IAbilityEffect), Allow = TypeAllow.None)]
        [Tooltip("Effect type used to describe the ability.")]
        [SerializeField] private string _effectType;

        /// <summary>
        /// Gets the name shown in the ability catalog.
        /// </summary>
        public string AbilityName => _abilityName;

        /// <summary>
        /// Gets the ability description.
        /// </summary>
        public string Description => _description;

        /// <summary>
        /// Gets the mana cost per use.
        /// </summary>
        public int ManaCost => _manaCost;

        /// <summary>
        /// Gets the cooldown in seconds.
        /// </summary>
        public float Cooldown => _cooldown;

        /// <summary>
        /// Gets the selected effect type, or <see langword="null"/> when its name is empty or cannot be resolved.
        /// </summary>
        public Type EffectType => string.IsNullOrEmpty(_effectType) ? null : Type.GetType(_effectType);
    }
}
