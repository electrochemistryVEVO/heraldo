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
        public bool holster;
        public bool pickup;
        public int selectedWeapon;
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

        public void OnDrawHolster(InputValue value)
        {
            SwitchHolster();
        }

        public void OnWpnSelect1(InputValue value)
        {
            if (!value.isPressed)
            {
                SelectWeapon(1);
            }
        }
        public void OnWpnSelect2(InputValue value)
        {
            if (!value.isPressed)
            {
                SelectWeapon(2);
            }
        }
        public void OnWpnSelect3(InputValue value)
        {
            if (!value.isPressed)
            {
                SelectWeapon(3);
            }
        }

        public void OnInteract(InputValue value)
        {
            if (!value.isPressed)
            {
                PickupInput(true);
            }
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

        public void SwitchHolster()
        {
            holster = !holster;
            Debug.Log("Holster: " + holster);
        }

        public void SelectWeapon(int i)
        {
            selectedWeapon = i;
        }

        public void PickupInput(bool newPickupState)
        {
            pickup = newPickupState;
        }
    }
}