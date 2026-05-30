using System;
using UnityEngine;

namespace Heraldo.Character.Enemy
{
    public class LineOfSight : MonoBehaviour
    {
        public GameObject ParentEntity;
        private void OnTriggerStay(Collider other)
        {
            //Debug.Log(other.gameObject.name);
            ParentEntity.SendMessage("TriggerStay",other);
        }

        private void OnTriggerExit(Collider other)
        {
            ParentEntity.SendMessage("TriggerExit",other);
        }
        
        private void OnTriggerEnter(Collider other)
        {
            
            ParentEntity.SendMessage("TriggerEnter",other);
        }
    }
}