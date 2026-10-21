using UnityEngine;
using System.Collections;
using Aspid.FastTools.Types;
using System.Collections.Generic;

// ReSharper disable CheckNamespace
namespace Aspid.FastTools.Samples.SerializeReferences
{
    public sealed class Loadout : MonoBehaviour
    {
        [SerializeField] private TrainingDummy _target;
        [SerializeField] [Min(0.1f)] private float _fireInterval = 1f;

        [TypeSelector]
        [SerializeReference] private IWeapon _primary;

        [TypeSelector]
        [SerializeReference] private List<IWeapon> _sidearms = new();

        [TypeSelector]
        [SerializeReference] private StatusEffect _onHit;

        [SerializeField] private LineRenderer _shotBeam;

        private int _shot;

        public IWeapon Primary => _primary;

        public IReadOnlyList<IWeapon> Sidearms => _sidearms;

        private IEnumerator Start()
        {
            while (true)
            {
                yield return new WaitForSeconds(_fireInterval * (1f + _target.Slow));
                Fire();
            }
        }

        private void Fire()
        {
            var index = _shot++ % (_sidearms.Count + 1);
            var weapon = index == 0
                ? _primary
                : _sidearms[index - 1];

            if (weapon is null)
                return;

            StartCoroutine(ShowShot());
            _target.TakeDamage(weapon.Fire());
            _onHit?.Apply(_target);
            (weapon as Railgun)?.ChargeEffect?.Apply(_target);
        }

        private IEnumerator ShowShot()
        {
            _shotBeam.SetPosition(index: 0, position: transform.position + Vector3.up * 0.7f);
            _shotBeam.SetPosition(index: 1, position: _target.transform.position);
            _shotBeam.enabled = true;

            yield return new WaitForSeconds(seconds: 0.08f);

            _shotBeam.enabled = false;
        }
    }
}
