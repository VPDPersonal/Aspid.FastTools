using UnityEngine;
using System.Collections;
using Aspid.FastTools.Types;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.SerializeReferences
{
    // TypeSelector arguments narrow the implementations offered for each managed reference.
    /// <summary>
    /// <see cref="MonoBehaviour"/> that cycles through weapons and applies damage modifiers and effects.
    /// </summary>
    public sealed class Loadout : MonoBehaviour
    {
        [Tooltip("Training dummy that receives attacks.")]
        [SerializeField] private TrainingDummy _target;

        [Tooltip("Seconds between attacks before slowing effects.")]
        [SerializeField, Min(0.1f)] private float _fireInterval = 1f;

        // Any IWeapon. Required = true: an empty field shows a notice and fails the build/CI gate when enabled.
        [Header("Weapons")]
        [TypeSelector(Required = true)]
        [Tooltip("Primary weapon in the attack cycle.")]
        [SerializeReference] private IWeapon _primary;

        // The list's + opens the picker instead of duplicating the last element.
        [TypeSelector]
        [Tooltip("Additional weapons fired in list order.")]
        [SerializeReference] private List<IWeapon> _sidearms = new();

        // Narrowed below the field type: only IMelee implementations.
        [TypeSelector(typeof(IMelee))]
        [Tooltip("Melee weapon shown in the loadout summary.")]
        [SerializeReference] private IWeapon _meleeBackup;

        // References inside plain [Serializable] containers.
        [Tooltip("Labeled weapon slots shown in the loadout summary.")]
        [SerializeField] private WeaponSlot[] _holster;

        // Abstract base: only BurnEffect / FreezeEffect are offered.
        [Header("Effects and modifiers")]
        [TypeSelector]
        [Tooltip("Effect applied after each attack.")]
        [SerializeReference] private StatusEffect _onHit;

        // Closed generic field: T is fixed, DamageModifier and Modifier<float> qualify.
        [TypeSelector]
        [Tooltip("Damage modifier applied before perks.")]
        [SerializeReference] private Modifier<float> _damageModifier;

        // Open generic entry point: the closed subclasses plus Modifier<T> with an argument page.
        [TypeSelector]
        [Tooltip("Modifiers applied in list order.")]
        [SerializeReference] private List<IModifier> _perks = new();

        [Tooltip("Beam displayed during an attack.")]
        [SerializeField, HideInInspector] private LineRenderer _shotBeam;

        private int _sidearmIndex;

        private IEnumerator Start()
        {
            while (true)
            {
                yield return new WaitForSeconds(_fireInterval * (1f + (_target?.Slow ?? 0f)));
                FireOnce();
            }
        }

        [ContextMenu("Fire Once")]
        private void FireOnce()
        {
            if (_target is null)
                return;

            var weapon = PickWeapon();
            if (weapon is null)
            {
                Debug.LogWarning("No weapon assigned.", this);
                return;
            }

            var damage = weapon.Fire();
            if (_damageModifier is not null)
                damage = _damageModifier.ModifyDamage(damage);
            foreach (var perk in _perks)
                if (perk is not null)
                    damage = perk.ModifyDamage(damage);

            if (_shotBeam != null && Application.isPlaying)
                StartCoroutine(ShowShot());
            _target.TakeDamage(damage, weapon.Name);
            _onHit?.Apply(_target);
            (weapon as Railgun)?.ChargeEffect?.Apply(_target);
        }

        private IEnumerator ShowShot()
        {
            _shotBeam.SetPosition(0, transform.position + Vector3.up * 0.7f);
            _shotBeam.SetPosition(1, _target.transform.position);
            _shotBeam.enabled = true;
            yield return new WaitForSeconds(0.08f);
            _shotBeam.enabled = false;
        }

        // Primary, then each sidearm in turn, so every configured weapon fires.
        private IWeapon PickWeapon()
        {
            if (_sidearms.Count is 0)
                return _primary;

            var total = _sidearms.Count + 1;
            var index = _sidearmIndex++ % total;
            return index is 0 ? _primary : _sidearms[index - 1];
        }

        [ContextMenu("Log Loadout")]
        private void LogLoadout()
        {
            Debug.Log($"Primary: {_primary?.Name ?? "none"} | melee: {_meleeBackup?.Name ?? "none"} | on hit: {_onHit?.Name ?? "none"}", this);

            foreach (var slot in _holster ?? System.Array.Empty<WeaponSlot>())
                Debug.Log($"Holster \"{slot.Label}\": {slot.Weapon?.Name ?? "empty"}", this);

            Debug.Log($"Damage modifier: {_damageModifier?.Describe() ?? "none"}", this);
            foreach (var perk in _perks)
                Debug.Log($"Perk: {perk?.Describe() ?? "none"}", this);
        }
    }
}
