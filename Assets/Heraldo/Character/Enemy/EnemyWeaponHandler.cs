using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Heraldo.Cutscenes;
using Heraldo.Global.Enums;
using Heraldo.Weapon;
using UnityEngine;
using UnityEngine.AI;

namespace Heraldo.Character.Enemy
{
    public class EnemyWeaponHandler : MonoBehaviour
    {
        private static readonly int SelectedWeapon = Animator.StringToHash("selectedWeapon");
        public Transform Target;
        public TimelineManager Timelines;
        public float RespiteTime = 1.0f;
        public EnemyController npc;
        public Animator NpcAnimator;
        
        private GenericWeapon[] _weapons;
        private int _selectedIndex = 0;
        private GenericWeapon _selectedWeapon;
        private bool _fireTimeElapsed = true;
        private NavMeshAgent _npcAgent;

        private void Awake()
        {
            _weapons = GetComponentsInChildren<GenericWeapon>();
            _npcAgent = npc.GetComponent<NavMeshAgent>();
            //SwitchWeapon(0);
        }

        private void Start()
        {
            SwitchWeapon(npc.currentWeapon.transform.GetSiblingIndex());
        }

        public void Attack(Transform player)
        {
            Target = player;
            //AdaptToPlayer();
            if (_fireTimeElapsed)
            {
                if ((transform.position - player.position).magnitude <= _selectedWeapon.Range)
                    StartCoroutine(AttackEnemy(player.position));
                else
                {
                    Debug.Log("[WEAPON-HANDLER] " + npc.name + " cambia WalkToPoint a " + player.position);
                    //NavMeshHit hit;
                    npc.Enemy_State = EnemyState.ENGAGE_ROUTE;
                    return;
                }
            }
            npc.Enemy_State = EnemyState.ENGAGE;
        }

        void AdaptToPlayer()
        {
            float minDist = Single.MaxValue;
            int minIndex = 0;
            int i = 0;
            foreach (var wpn in _weapons)
            {
                float dist = (transform.position - Target.position).magnitude;
                if (wpn.AmmoCount != 0 && dist < wpn.Range && minDist >= dist)
                {
                    minDist = dist;
                    break;
                    //_selectedWeapon = wpn;
                } 
                i++;
            }
            SwitchWeapon(i);
        }
        
        IEnumerator AttackEnemy(Vector3 enemyPos)
        { 
            bool fired = _selectedWeapon.Fire(transform.position,enemyPos - transform.position);
            if (fired)
            {
                _fireTimeElapsed = false;
                Timelines.timelineDict[_selectedWeapon.ShootTimelineId()].Play();
                yield return new WaitForSeconds(_selectedWeapon.ReloadTime + RespiteTime);
                _fireTimeElapsed = true;
            }
        }

        public void SwitchWeapon(int index)
        {
            //Debug.Log(index);
            if(_selectedWeapon)
            {
                _selectedWeapon.ShowHide(false);
                _selectedWeapon.Enabled = false;
            }
            _selectedWeapon = _weapons[index];
            _selectedWeapon.Enabled = true;
            _selectedWeapon.ShowHide();
            npc.currentWeapon = _selectedWeapon;
            NpcAnimator.SetInteger(SelectedWeapon,index);
        }

        public Vector3? NextMovementAction()
        {
            if (Target)
            {
                Vector3 targetPos = Target.position;
                Debug.Log("Target position: "+Target.position);
                Debug.Log("Current position: "+transform.position);
                if ((transform.position - targetPos).magnitude > _selectedWeapon.Range)
                {
                    //Move forwards of backwards
                    Debug.Log("Approaching");
                    return targetPos +
                           (transform.position - targetPos).normalized * _selectedWeapon.Range;
                }
                //Move sideways
                Debug.Log("Strafing");
                return targetPos +
                       Vector3.Cross((transform.position - targetPos).normalized, Vector3.up);
            }
            Debug.Log("Selected nothing");
            return null;
        }
    }
}