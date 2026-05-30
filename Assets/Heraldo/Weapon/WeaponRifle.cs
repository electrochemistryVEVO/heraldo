using StarterAssets;
using UnityEngine;

namespace Heraldo.Weapon
{
    public class WeaponRifle : GenericWeapon
    {
        private BulletImpact _impact;

        void Start()
        {
            _impact = GameObject.FindWithTag("Particle Manager").GetComponent<BulletImpact>();
        }
        protected override void FireAt(Vector3 position,Vector3 direction)
        {
            RaycastHit hit;
            bool wasHit = Physics.Raycast(position,direction, out hit,Mathf.Infinity);
            if (wasHit)
            {
                if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Enemy"))
                {
                    _impact.FleshImpact(hit.point,hit.normal,hit.transform.gameObject);
                }
                else  _impact.Impact(hit.point, hit.normal);
            }
        }
    }
}