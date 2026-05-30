using System;
using StarterAssets;
using UnityEngine;

namespace Heraldo.Weapon
{
    public class GenericWeapon : MonoBehaviour
    {
        public float Range;
        public int MaxAmmo;
        public int AmmoCount;
        public bool Enabled;

        public void Fire(Vector3 position,Vector3 direction)
        {
            if (!Enabled) return;
            Debug.Log("Ammo: " + AmmoCount);
            AmmoCount = Math.Max(AmmoCount-1,0); 
            if(AmmoCount>0) FireAt(position, direction);
        }
        protected virtual void FireAt(Vector3 position,Vector3 direction){}
    }
}