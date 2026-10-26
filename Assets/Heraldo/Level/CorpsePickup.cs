using Heraldo.Character;
using Heraldo.Flavor;
using Heraldo.Weapon;
using UnityEngine;

namespace Heraldo.Level
{
    public class CorpsePickup : Interactable
    {
        public override string GetDescription()
        {
            return "Cargar cuerpo de Bolognesi";
        }

        public override void Interact(FPSController controller)
        {
            Pickup();
            controller.PickupCorpse();
            Destroy(gameObject);
        }
    }
}