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
                Gizmos.DrawSphere(routine.position,1f);
            }
        }
    }
}