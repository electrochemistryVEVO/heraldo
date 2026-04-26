using System;
using UnityEngine;

namespace Heraldo.Character
{
    enum EnemyState
    {
        ALIVE,
        DEAD
    }
    public class EnemyController : MonoBehaviour
    {
        public int MaxHealth = 100;
        private int _health;

        private int Health
        {
            get => _health;
            set
            {
                _health = value;
                if (value <= 0) Enemy_State = EnemyState.DEAD;
            }
        }

        private EnemyState _enemyState;

        private EnemyState Enemy_State
        {
            get => _enemyState;
            set
            {
                _enemyState = value;
                if(_enemyState == EnemyState.DEAD) OnDied();
                
            }
        }

        void Start()
        {
            Health = MaxHealth;
            Enemy_State = EnemyState.ALIVE;
        }

        public void OnTakeDamage(int damage)
        {
            Health = Math.Max(Health-damage,0);
            Debug.Log("Health: " + Health);
        }

        void OnDied()
        {
            Debug.Log("Dead");
        }
    }
}