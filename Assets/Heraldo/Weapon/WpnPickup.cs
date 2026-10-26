using Heraldo.Character;
using Heraldo.Flavor;
using UnityEngine;

namespace Heraldo.Weapon
{
    public class WpnPickup : Interactable
    {
        public int WeaponIndex;
        public int AmmoCount;
        public override string GetDescription()
        {
            return "Recargar munición (" +AmmoCount+")";
        }
        public override void Interact(FPSController controller)
        {
            controller.PickupWeapon(AmmoCount,WeaponIndex);
            Destroy(gameObject);
        }
    }
}