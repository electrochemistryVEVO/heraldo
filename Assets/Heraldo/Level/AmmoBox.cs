using System;
using Heraldo.Character;
using Heraldo.Flavor;
using Heraldo.Global.Enums;
using UnityEngine;

namespace Heraldo.Level
{
    public class AmmoBox : Interactable
    {
        public int WeaponIndex;
        public BattlePhase Battle_Phase = BattlePhase.MORRO_ONE;
        public int AmmoPool;
        public int AmmoCount;
        public override string GetDescription()
        {
            return "Recargar munición (" +AmmoPool+" restantes)";
        }
        public override void Interact(FPSController controller)
        {
            int ammoDif = controller.PickupWeapon(Math.Min(AmmoPool,AmmoCount),WeaponIndex);
            AmmoPool = Math.Clamp(AmmoPool - ammoDif, 0, int.MaxValue);
        }
    }
}