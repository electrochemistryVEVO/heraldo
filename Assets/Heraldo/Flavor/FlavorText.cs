using Heraldo.Character;
using Heraldo.Global.Enums;
using UnityEngine;

namespace Heraldo.Flavor
{
    public class FlavorText : Interactable
    {
        public string Text = "Hello World";
        //It's necessary
        public GameObject nextCorpsePrefab;
        public override string GetDescription()
        {
            return "Inspeccionar";
        }
        public override void Interact(FPSController controller)
        {
            if (WeaponPrerequisite == controller.LastWpnIndex || WeaponPrerequisite < 0)
            {
                GameObject _uiManager = GameObject.FindWithTag("UI Canvas");
                _uiManager.SendMessage("FlavorText",Text);
                if (nextCorpsePrefab != null)
                {
                    Pickup();
                    controller.ChangeCorpsePrefab(nextCorpsePrefab);
                }
                Destroy(gameObject);
            }
        }
    }
}