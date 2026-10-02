using System;
using Heraldo.Global.Structs;
using UnityEngine;

namespace Heraldo.Character
{
    public class PlayerHitbox : MonoBehaviour
    {
        //Workaround que tuve que hacer para que el hitbox del jugador registrara los golpes
        public FPSController controller;
        public Collider crouchHitbox;
        private Collider _hitbox;
        private bool _crouched = false;

        public void OnTakeDamage(DamageInfo info)
        {
            controller.OnTakeDamage(info);
        }

        private void Awake()
        {
            _hitbox = GetComponent<Collider>();
        }

        private void Update()
        {
            if (controller.IsCrouched != _crouched)
            {
                _crouched = controller.IsCrouched;
                if (_crouched) Crouch();
                else Stand();
            }
        }

        public void Crouch()
        {
            _hitbox.enabled = false;
            crouchHitbox.enabled = true;
        }

        public void Stand()
        {
            _hitbox.enabled = true;
            crouchHitbox.enabled = false;
        }
    }
}