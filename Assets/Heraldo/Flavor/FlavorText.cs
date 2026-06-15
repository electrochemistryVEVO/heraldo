using Heraldo.Character;
using UnityEngine;

namespace Heraldo.Flavor
{
    public class FlavorText : Interactable
    {
        public string Text = "Hello World";
        public override void Interact(FPSController controller)
        {
            GameObject _uiManager = GameObject.FindWithTag("UI Canvas");
            _uiManager.SendMessage("FlavorText",Text);
        }
    }
}