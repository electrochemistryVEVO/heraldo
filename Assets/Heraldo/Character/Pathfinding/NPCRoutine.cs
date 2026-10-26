using System;
using UnityEngine;

namespace Heraldo.Character.Pathfinding
{
    public class NPCRoutine : MonoBehaviour
    {
        private Transform[] _routineList;

        private void OnDrawGizmos()
        {
            foreach(var routine in GetComponentsInChildren<Transform>())
            {
                Gizmos.DrawSphere(routine.position,10f);
                Gizmos.DrawLine(transform.position,routine.position);
            }
        }

        private void OnDrawGizmosSelected()
        {
            foreach(var routine in GetComponentsInChildren<Transform>())
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(routine.position,15f);
            }
        }
    }
}
