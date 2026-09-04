using Heraldo.Global.Enums;
using Heraldo.Global.Structs;
using UnityEngine;

namespace Heraldo.Weapon
{
    public class WeaponMelee : GenericWeapon
    {
        public LayerMask hittableLayers;
        private BulletImpact _impact;
        private static readonly float MACHETE_LAND_SOUND_RADIUS =60.0f;
        
        void Start()
        {
            _impact = GameObject.FindWithTag("Particle Manager").GetComponent<BulletImpact>();
        }

        public override TimelineID ShootTimelineId()
        {
            return TimelineID.FPS_MACHETE_SLASH;
        }

        //Modify this for melee particles
        protected override void FireAt(Vector3 position,Vector3 direction)
        {
            RaycastHit hit;
            bool wasHit = Physics.Raycast(position,direction, out hit,Range,LayerMask.GetMask("Ally","Enemy","Default"));
            if (wasHit)
            {
                //Shot lands either way
                _hearingManager.MakeSound(hit.point,MACHETE_LAND_SOUND_RADIUS);
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