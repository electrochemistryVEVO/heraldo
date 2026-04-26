using StarterAssets;
using UnityEngine;

namespace Heraldo.Weapon
{
    public class GenericWeapon : MonoBehaviour
    {
        public virtual void FireAt(FirstPersonController shooter,Vector3 point){}
    }
}