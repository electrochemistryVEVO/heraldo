using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Heraldo.Cutscenes;
using Heraldo.Global.Enums;
using Heraldo.Weapon;
using UnityEngine;

namespace Heraldo.Character.Enemy
{
    public class EnemyWeaponHandler : MonoBehaviour
    {
        private static readonly int SelectedWeapon = Animator.StringToHash("selectedWeapon");
        public Transform Target;
        public TimelineManager Timelines;
        public float RespiteTime = 1.0f; 
        public Animator NpcAnimator;
        
        private GenericWeapon[] _weapons;
        private GenericWeapon _selectedWeapon;
        private bool _fireTimeElapsed = true;

        private void Awake()
        {
            _weapons = GetComponentsInChildren<GenericWeapon>();
        }

        public void Attack(Transform player)
        {
            Target = player;
            AdaptToPlayer();
            if (_fireTimeElapsed) StartCoroutine(AttackEnemy(player.position));
        }

        void AdaptToPlayer()
        {
            float minDist = Single.MaxValue;
            int i = 0;
            foreach (var wpn in _weapons)
            {
                float dist = (transform.position - Target.position).magnitude;
                if (dist < wpn.Range && minDist >= dist)
                {
                    minDist = dist;
                    SwitchWeapon(i);
                    //_selectedWeapon = wpn;
                } 
                i++;
            }
        }
        
        IEnumerator AttackEnemy(Vector3 enemyPos)
        {
            Debug.Log("Fired");
            Debug.Log(_selectedWeapon.ReloadTime + RespiteTime);
            _fireTimeElapsed = false;
            Timelines.timelineDict[_selectedWeapon.ShootTimelineId()].Play();
            _selectedWeapon.Fire(transform.position,enemyPos - transform.position); 
            yield return new WaitForSeconds(_selectedWeapon.ReloadTime + RespiteTime);
            _fireTimeElapsed = true;
        }

        private void SwitchWeapon(int index)
        {
            if(_selectedWeapon)_selectedWeapon.ShowHide(false);
            _selectedWeapon = _weapons[index];
            _selectedWeapon.ShowHide();
            NpcAnimator.SetInteger(SelectedWeapon,index);
        }

        public void AimAtTarget(Vector3 target)
        {
            
        }
    }
}