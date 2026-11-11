using Heraldo.Character;
using Heraldo.Flavor;
using Heraldo.Global.Structs;
using UnityEngine;

namespace Heraldo.Level
{
    public class HealthPickup : Interactable
    {
        public int HealthCount = 20;
        public override string GetDescription()
        {
            return "Obtener kit de primeros auxilios(+" +HealthCount+"Hp)";
        }
        public override void Interact(FPSController controller)
        {
            controller.OnTakeDamage(new DamageInfo(-HealthCount,Vector3.zero));
            Destroy(gameObject);
        }
    }
}