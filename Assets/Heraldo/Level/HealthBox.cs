using System;
using Heraldo.Character;
using Heraldo.Flavor;
using Heraldo.Global.Enums;
using Heraldo.Global.Structs;
using UnityEngine;

namespace Heraldo.Level
{
    public class HealthBox : Interactable
    {
        public int HealAmmount = 100;
        public BattlePhase Battle_Phase = BattlePhase.MORRO_ONE; 
        public override string GetDescription()
        {
            return "Recargar salud";
        }
        public override void Interact(FPSController controller)
        {
            controller.OnTakeDamage(new DamageInfo(-HealAmmount,transform.position));
        }
    }
}