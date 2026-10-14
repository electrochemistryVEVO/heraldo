using Heraldo.Character;
using UnityEngine;

namespace Heraldo.Flavor
{
    public class Interactable : MonoBehaviour
    {
        //Determine if the object will delete when picked up
        public bool OneTimeUse = false;
        public virtual void Interact(FPSController controller) {}
    }
}