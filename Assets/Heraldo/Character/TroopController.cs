using System;
using System.Collections.Generic;
using UnityEngine;

namespace Heraldo.Character
{
    public enum TroopState
    {
        ATTACK,
        DEFEND,
        PATROL
    }
    public class TroopController : MonoBehaviour
    {
        public TroopState Troop_State = TroopState.PATROL;
        private List<EnemyController> _troopers;
        private WeakReference<TroopController> _enemyTroop;

        private void Awake()
        {
            _troopers = new List<EnemyController>(GetComponentsInChildren<EnemyController>());
            //If it's attack or patrol, behavior is normal
            //If it's defend, every enemy needs to stay in place
            if (Troop_State == TroopState.DEFEND)
            {
                foreach (EnemyController trooper in _troopers)
                {
                    trooper.Enemy_State = EnemyState.SEEK_GUARD;
                }
            }
        }

        //Functions for attackers
        private void AttackPositions()
        {
            
        }
        
        //Functions for defenders
        private void DefendPositions()
        {
            
        }
        
        //Functions for both
        private void SpotEnemy(EnemyController controller)
        {
            //If enemy has a troop, troop rules are in effect
            //If not, chase and put down like you would a sick dog
            if (controller.Troop != null)
            {
                _enemyTroop.SetTarget(controller.Troop);
                switch (Troop_State)
                {
                    case TroopState.ATTACK:
                        AttackPositions();
                        break;
                    case TroopState.DEFEND:
                        DefendPositions();
                        break;
                }
            }
        }
    }
}