using System;
using UnityEngine;

namespace Heraldo.Character.Enemy
{
    public class LineOfSight : MonoBehaviour
    {
        //public GameObject ParentEntity;
        private void OnTriggerStay(Collider other)
        {
            //Debug.Log(other.gameObject.name);
            transform.parent.parent.SendMessage("TriggerStay",other);
        }

        private void OnTriggerExit(Collider other)
        {
            transform.parent.parent.SendMessage("TriggerExit",other);
        }
        
        private void OnTriggerEnter(Collider other)
        {
            
            transform.parent.parent.SendMessage("TriggerEnter",other);
        }
    }
}