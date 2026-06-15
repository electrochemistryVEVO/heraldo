using Heraldo.Global.Structs;
using StarterAssets;
using UnityEngine;

namespace Heraldo.Weapon
{
    public class WeaponRifle : GenericWeapon
    {
        public LayerMask hittableLayers;
        private BulletImpact _impact;
        
        void Start()
        {
            _impact = GameObject.FindWithTag("Particle Manager").GetComponent<BulletImpact>();
        }
        protected override void FireAt(Vector3 position,Vector3 direction)
        {
            RaycastHit hit;
            bool wasHit = Physics.Raycast(position,direction, out hit,Mathf.Infinity,LayerMask.GetMask("Ally","Enemy","Default"));
            if (wasHit)
            {
                if ((hittableLayers & (1 << hit.transform.gameObject.layer))!=0)
                {
                    Debug.Log(hit.collider.name);
                    //Taking damage
                    hit.transform.gameObject.SendMessage("OnTakeDamage", 
                        new DamageInfo(20,hit.point),
                        SendMessageOptions.DontRequireReceiver);
                    //Impact VFX
                    _impact.FleshImpact(hit.point,hit.normal);
                }
                else  _impact.Impact(hit.point, hit.normal);
            }
        }
    }
}