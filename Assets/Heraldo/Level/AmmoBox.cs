using System;
using Heraldo.Character;
using Heraldo.Flavor;
using UnityEngine;

namespace Heraldo.Level
{
    public class AmmoBox : Interactable
    {
        public int WeaponIndex;
        public int AmmoPool;
        public int AmmoCount;
        public override void Interact(FPSController controller)
        {
            int ammoDif = controller.PickupWeapon(Math.Min(AmmoPool,AmmoCount),WeaponIndex);
            AmmoPool = Math.Clamp(AmmoPool - ammoDif, 0, int.MaxValue);
        }
    }
}