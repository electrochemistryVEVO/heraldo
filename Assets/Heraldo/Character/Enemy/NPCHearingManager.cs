using System;
using System.Collections.Generic;
using Heraldo.Global.Structs;
using UnityEngine;

namespace Heraldo.Character.Enemy
{
    public class NPCHearingManager : MonoBehaviour
    {
        private static readonly int STEP = 60; //Frames per real update 
        private int _stepCount = STEP; 
        public void MakeSound(Vector3 position,float radius)
        {
            Collider[] colliders = Array.Empty<Collider>();
            Physics.OverlapSphereNonAlloc(position, radius,colliders);
            foreach (var collider in colliders)
            {
                collider.gameObject.SendMessage("BeAlerted", position);
            }
        }
 
    }
}