using System;
using Heraldo.Weapon;
using Heraldo.Weapon.Holster;
using StarterAssets;
using UnityEngine;

namespace Heraldo.Character
{
    public class FPSController : MonoBehaviour
    {
        public float CrouchSpeed = 1.0f;
        public float CrouchGroundOffset;
        public float GroundOffset;
        public float InteractRadius;
        public GameObject Weapon;
        private GenericWeapon _currentWeapon;
        private GenericWpnHolster _arsenal;
        private WeakReference<GameObject> _nearestInteractableRef;
        
        //Components within the instance
        private Inputs _inputs;
        private float _move_speed;
        private CharacterController characterController;
        private FirstPersonController _controller;
        private Transform _camera;
        
        //Global game objects referenced
        private BulletImpact _impact;
        
        //Layer masks
        private LayerMask _wpnPickupMask;
        private LayerMask _enemyMask;
        
        //Debounces
        private bool _shoot_deb = false;

        private void Start()
        {
            
            characterController = GetComponent<CharacterController>();
            _controller = GetComponent<FirstPersonController>();
            _move_speed = _controller.MoveSpeed;
            _camera = transform.GetChild(0);
            _arsenal = transform.GetChild(2).GetComponent<GenericWpnHolster>();
            _inputs = GetComponent<Inputs>();
            _impact = GameObject.FindWithTag("Particle Manager").GetComponent<BulletImpact>();
            _wpnPickupMask = LayerMask.GetMask("WpnPickup");
            _nearestInteractableRef = new WeakReference<GameObject>(null);
            //_currentWeapon = Weapon.GetComponent<GenericWeapon>();
        }
        
        private void Update()
        {
            bool isCrouched = _inputs.crouch;
            //Modificar valocidad de caminata
            _controller.MoveSpeed = (isCrouched && _controller.Grounded) ? CrouchSpeed : _move_speed;
            _camera.localPosition = new Vector3(
                _camera.localPosition.x, 
                (isCrouched && _controller.Grounded) ? CrouchGroundOffset : GroundOffset, 
                _camera.localPosition.z
                );
            //Calcular arma mas cercana
            CalculateNearestInteractable();
            //Equipar armas
            if (_inputs.pickup)
            {
                _inputs.pickup = false;
                Pickup();
            }
            //Disparar si se puede
            if (_inputs.shoot && !_shoot_deb)
            {
                _shoot_deb = true;
                _currentWeapon?.Fire(_camera.position,_camera.TransformDirection(Vector3.forward));
                //Debug.Log("Shot!");
            }
            else
            {
                if (!_inputs.shoot) _shoot_deb = false;
            }
        }
        //Antes de recoger, por motivos de UI, identificar el arma que esta mas cerca
        private void CalculateNearestInteractable()
        {
            int MAX_COLLIDERS = 5;
            Collider[] nearWpns = new Collider[MAX_COLLIDERS];
            //Weapon pickups
            int numWpns = Physics.OverlapSphereNonAlloc(transform.position, InteractRadius,nearWpns,_wpnPickupMask);
            if (numWpns <= 0) return;
            double bestDistance =  double.MaxValue;
            int bestI = 0;
            //Pick up nearest weapon
            for (int i = 0; i < numWpns; i++)
            {
                //Weapons in front of player get an advantage
                double distance = (nearWpns[i].transform.position + nearWpns[i].transform.forward - transform.position).magnitude;
                if (distance <= bestDistance)
                {
                    bestDistance = distance;
                    bestI = i;
                }
            }
            _nearestInteractableRef.SetTarget(nearWpns[bestI].gameObject);
            //Corpse pickups (later)
        }
        //Recoger armas
        private void Pickup()
        {
            GameObject obj;
            bool hasTarget = _nearestInteractableRef.TryGetTarget(out obj);
            if (hasTarget)
            {
                //Debug.Log("Picked up " + obj.name);
                var wpn = obj.GetComponent<WpnPickup>();
                _arsenal.PickupWeapon(wpn.AmmoCount,wpn.WeaponIndex);
                Destroy(obj);
                //TODO: Automatically change to correct weapon index
            }
        }
        
        //Logica de armas
        public void EraseWeapon()
        {
            _currentWeapon = null;
            //Destroy(Weapon);
        }

        public void EquipWeapon(GenericWeapon weapon)
        {
            _currentWeapon = weapon;
        }
    }
}