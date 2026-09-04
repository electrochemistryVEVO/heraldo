using Heraldo.Global.Enums;
using Heraldo.Global.Structs;
using StarterAssets;
using UnityEngine;

namespace Heraldo.Weapon
{
    public class WeaponRifle : GenericWeapon
    {
        public LayerMask hittableLayers;
        private BulletImpact _impact;
        static readonly float RIFLE_SHOOT_SOUND_RADIUS = 120.0f;
        private static readonly float RIFLE_LAND_SOUND_RADIUS = 60.0f;
        
        void Start()
        {
            _impact = GameObject.FindWithTag("Particle Manager").GetComponent<BulletImpact>();
        }

        public override TimelineID ShootTimelineId()
        {
            return TimelineID.FPS_SHOTGUN_SHOOT_RELOAD;
        }

        protected override void FireAt(Vector3 position,Vector3 direction)
        {
            
            RaycastHit hit;
            bool wasHit = Physics.Raycast(position,direction, out hit,Range,LayerMask.GetMask("Ally","Enemy","Default"));
            if (wasHit)
            {
                //Shot lands either way
                _hearingManager.MakeSound(hit.point,RIFLE_LAND_SOUND_RADIUS);
                if ((hittableLayers & (1 << hit.transform.gameObject.layer))!=0)
                {
                    //Taking damage
                    hit.transform.gameObject.SendMessage("OnTakeDamage", 
                        new DamageInfo(20,position),
                        SendMessageOptions.DontRequireReceiver);
                    //Impact VFX
                    _impact.FleshImpact(hit.point,hit.normal);
                }
                else
                {
                    _impact.Impact(hit.point, hit.normal);
                    //Sound of gun firing (not necessary when enemy already knows your location)
                    _hearingManager.MakeSound(position,RIFLE_SHOOT_SOUND_RADIUS);
                }
            }
            
        }
    }
}