using System;
using Heraldo.Character.Enemy;
using UnityEngine;

namespace Heraldo.Level.Triggers
{
    public class AllyRespawnTrigger : MonoBehaviour
    {
        public TroopController troop;
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                troop.RespawnToTop();
                Destroy(gameObject);
            }
        }
    }
}