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
        private CapsuleCollider _hitbox;
        private bool _crouched = false;

        public void OnTakeDamage(DamageInfo info)
        {
            controller.OnTakeDamage(info);
        }

        private void Awake()
        {
            _hitbox = GetComponent<CapsuleCollider>();
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
            _hitbox.height = 1.5f;
            _hitbox.center = Vector3.up* (-0.25f);
        }

        public void Stand()
        {
            _hitbox.height = 2f;
            _hitbox.center = Vector3.zero;
        }
    }
}