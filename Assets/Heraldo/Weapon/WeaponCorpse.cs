using Heraldo.Global.Enums;
using UnityEngine;

namespace Heraldo.Weapon
{
    public class WeaponCorpse : GenericWeapon
    {
        public override void Awake()
        {
            base.Awake();
            Description = "Francisco Bolognesi";
        }

        public override TimelineID ShootTimelineId()
        {
            return TimelineID.CANT_SHOOT;
        }
        protected override void FireAt(Vector3 position, Vector3 direction)
        {
            
        }
    }
}