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
        public void MakeSound(Vector3 position,float radius,bool isAlly)
        {
            Collider[] colliders = Array.Empty<Collider>();
            // var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            // sphere.GetComponent<Collider>().enabled = false;
            // sphere.transform.position = position;
            // sphere.transform.localScale = Vector3.one*radius;
            Physics.OverlapSphereNonAlloc(position, radius,colliders,LayerMask.GetMask(isAlly ? "Enemy" : "Ally"));
            foreach (var collider in colliders)
            {
                Debug.Log("[MAKE-SOUND] Alerting "+collider.name);
                collider.gameObject.SendMessage("BeAlerted", position);
            }
        }
 
    }
}