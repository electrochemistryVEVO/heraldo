using StarterAssets;
using UnityEngine;

namespace Heraldo.Weapon
{
    public class GenericWeapon : MonoBehaviour
    {
        public float Range;
        public float MaxAmmo;
        public virtual void FireAt(FirstPersonController shooter,Vector3 point){}
    }
}