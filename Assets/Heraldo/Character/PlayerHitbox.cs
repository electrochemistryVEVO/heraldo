using Heraldo.Global.Structs;
using UnityEngine;

namespace Heraldo.Character
{
    public class PlayerHitbox : MonoBehaviour
    {
        //Workaround que tuve que hacer para que el hitbox del jugador registrara los golpes
        public FPSController controller;

        public void OnTakeDamage(DamageInfo info)
        {
            controller.OnTakeDamage(info);
        }
    }
}