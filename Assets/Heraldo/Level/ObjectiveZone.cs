using System;
using Heraldo.Global.Enums;
using UnityEngine;

namespace Heraldo.Level
{
    public class ObjectiveZone : MonoBehaviour
    {
        //When player is outside this zone, timer won't advance
        public ObjectiveID objectiveId = ObjectiveID.FIRST_LINE_OF_DEFENSE;
        public bool playerOutsideZone = true;

        private void OnTriggerStay(Collider other)
        {
            if (other.CompareTag("Player")) playerOutsideZone = false;
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player")) playerOutsideZone = true;
        }
    }
}