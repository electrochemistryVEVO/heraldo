using System;
using System.Collections.Generic;
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
        public bool start;
        public bool pickup;
        public int selectedWeapon;
        private StarterAssetsInputs _otherInputs;

        private void Awake()
        {
            _otherInputs = GetComponent<StarterAssetsInputs>();
        }

        private Dictionary<string, bool> enabledInputMap = new Dictionary<string, bool>()
        {
            {"crouch",true},
            {"shoot",true},
            {"holster",true},
            {"pickup",true},
            {"select_weapon",true},
            {"continue",true}
        };

        public bool toggleLock = true;

        public void ModifyInputs(string[] inputs,bool value)
        { 
            foreach (var key in inputs)
            {
                Debug.Log($"[INPUTS] ${(value ? "Enabled" : "Disabled")}: ${key}");
                enabledInputMap[key] = value;
            }
        }
#if ENABLE_INPUT_SYSTEM
        public void OnCrouch(InputValue value)
        {
            bool enabled;
            enabledInputMap.TryGetValue("crouch", out enabled);
            if(toggleLock && enabled && !_otherInputs.sprint)CrouchInput(value.isPressed);
        }

        public void OnShoot(InputValue value)
        {
            Debug.Log("[INPUTS] Shoot enabled? = "+enabledInputMap["shoot"]);
            Debug.Log("[INPUTS] Toggle lock? = "+toggleLock);
            if(toggleLock && enabledInputMap["shoot"])ShootInput(value.isPressed);
        }

        public void OnDrawHolster(InputValue value)
        {
            bool enabled;
            enabledInputMap.TryGetValue("holster", out enabled);
            if(toggleLock && enabled)SwitchHolster();
        }

        public void OnWpnSelect1(InputValue value)
        {
            bool enabled;
            enabledInputMap.TryGetValue("select_weapon", out enabled);
            if (toggleLock && enabled)
            {
                SelectWeapon(0);
                holster = false;
            }
        }
        public void OnWpnSelect2(InputValue value)
        {
            bool enabled;
            enabledInputMap.TryGetValue("select_weapon", out enabled);
            if (toggleLock && enabled)
            {
                SelectWeapon(1);
                holster = false;
            }
        } 

        public void OnInteract(InputValue value)
        {
            if (toggleLock && !value.isPressed && enabledInputMap["pickup"])
            {
                PickupInput(true);
            }
        }

        public void OnContinue(InputValue value)
        {
            Debug.Log("[INPUTS] Start");
            if (value.isPressed && enabledInputMap["continue"])
            {
                ContinueInput(true);
            }
        }
        
#endif
        public void CrouchInput(bool newCrouchState)
        {
            crouch = newCrouchState;
        }

        public void ShootInput(bool doShoot)
        {
            Debug.Log("[INPUTS] Shoot enabled? = "+enabledInputMap["shoot"]);
            if(enabledInputMap["shoot"])shoot = doShoot;
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

        public void ContinueInput(bool newStart)
        {
            start = newStart;
        }
    }
}