using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

using StarterAssets;

namespace Heraldo.Character
{
    public class Inputs : MonoBehaviour
    {
        public bool crouch;
        public bool shoot;
#if ENABLE_INPUT_SYSTEM
        public void OnCrouch(InputValue value)
        {
            Debug.Log("Crouch");
            CrouchInput(value.isPressed);
        }

        public void OnShoot(InputValue value)
        {
            ShootInput(value.isPressed);
        }
#endif
        public void CrouchInput(bool newCrouchState)
        {
            crouch = newCrouchState;
        }

        public void ShootInput(bool doShoot)
        {
            shoot = doShoot;
        }
    }
}