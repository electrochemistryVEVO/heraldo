using System;
using System.Collections;
using Heraldo.Flavor;
using Heraldo.UI;
using Heraldo.Weapon;
using Heraldo.Weapon.Holster;
using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Heraldo.Character
{
    public class FPSController : MonoBehaviour
    {
        public float CrouchSpeed = 1.0f;
        public float CrouchGroundOffset;
        public float ProneSpeed = 0.5f;
        public float ProneGroundOffset;
        public float GroundOffset;
        public float InteractRadius;
        public Transform HumanoidLeftGrip;
        public GameObject Weapon;
        private GenericWeapon _currentWeapon;
        GenericWpnHolster _arsenal;
        private WeakReference<GameObject> _nearestInteractableRef;
        
        //Components within the instance
        private Inputs _inputs;
        private float _move_speed;
        private CharacterController characterController;
        private FirstPersonController _controller;
        private Transform _camera;
        public Transform FpsArms;
        private Vector3 _deltaPosFpsArms;
        
        //Global game objects referenced
        private BulletImpact _impact;
        private PlayableDirector _playableFpsShoot;
        
        //Layer masks
        private LayerMask _wpnPickupMask;
        private LayerMask _enemyMask;
        
        //UI
        private LevelUIManager _uiCanvas;
        
        //Debounces
        private bool _shoot_deb = false;
        
        //Physics
        private Vector3 _prevPos;
        
        //Input
        private PlayerInput _playerInput;
        private bool _playerInputEnabled = true;

        private float SpreadMultiplier()
        {
            Vector3 velocity = (transform.position - _prevPos) / Time.deltaTime;
            return velocity.magnitude / _controller.SprintSpeed;
        }

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
            _uiCanvas = GameObject.FindWithTag("UI Canvas").GetComponent<LevelUIManager>();
            _playerInput = GetComponent<PlayerInput>();
            _prevPos = transform.position;
            _playableFpsShoot = GameObject.FindWithTag("MainCamera").GetComponent<PlayableDirector>();
            _deltaPosFpsArms = _controller.CinemachineCameraTarget.transform.position - FpsArms.position;
            //_currentWeapon = Weapon.GetComponent<GenericWeapon>();
        }

        public void TriggerPlayerInput(bool value)
        {
            _playerInputEnabled = value;
        }
        
        private void Update()
        {
            //Ajustar brazotes fps
            //FpsArms.position = _controller.CinemachineCameraTarget.transform.position - _deltaPosFpsArms;
            //FpsArms.rotation = _controller.CinemachineCameraTarget.transform.rotation;
            
            //Ajustar crosshair
            float spreadMultiplier = SpreadMultiplier();
            _uiCanvas.SetCrosshairSpread(spreadMultiplier);
            _prevPos = transform.position;
            
            //Desactivar o activar player input cuando sea posible
            _playerInput.enabled = _playerInputEnabled;
            
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
            //Modificar el crosshair
            AdjustCrosshair();
            //Equipar armas
            if (_inputs.pickup)
            {
                _inputs.pickup = false;
                Pickup();
            }
            //Disparar si se puede
            if (_inputs.shoot && !_shoot_deb)
            {
                //TODO: Erase _shoot_deb, there's already a cooldown system
                _shoot_deb = true;
                if (_currentWeapon !=null)
                {
                    StartCoroutine(WeaponCooldown(_currentWeapon.ReloadTime));
                    _currentWeapon.Fire(_camera.position,_camera.TransformDirection(Vector3.forward),spreadMultiplier);
                    _playableFpsShoot.Play();
                    _uiCanvas.AmmoText(_currentWeapon.AmmoCount.ToString()); 
                }
                
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
            if (hasTarget && obj!=null)
            {
                //Debug.Log("Picked up " + obj.name);
                var wpn = obj.GetComponent<Interactable>();
                if (wpn is WpnPickup)
                {
                    var _wpn = (WpnPickup)wpn;
                    _arsenal.PickupWeapon(_wpn.AmmoCount,_wpn.WeaponIndex);
                }
                else
                {
                    wpn.Interact(this);
                }
                Destroy(obj);
                //TODO: Automatically change to correct weapon index
            }
        }

        private void AdjustCrosshair()
        {
            string[] mask = new[] { "Ally", "Enemy" };
            RaycastHit hit;
            bool wasHit = Physics.Raycast(_camera.position, 
                _camera.TransformDirection(Vector3.forward),
                out hit,Mathf.Infinity, LayerMask.GetMask(mask));
            _uiCanvas.OnSubjectChange(wasHit ? hit.collider.gameObject : null,!wasHit);
        }
        
        //Logica de armas
        public void EraseWeapon()
        {
            _currentWeapon.ShowHide(false);
            _currentWeapon = null;
            _uiCanvas.ShowHideAmmoText();
            //Destroy(Weapon);
        }

        private IEnumerator WeaponCooldown(float seconds)
        {
            _inputs.ModifyInputs(new string[]{"shoot","holster","pickup","select_weapon"},false);
            _uiCanvas.ToggleAmmoLoad(true);
            yield return new WaitForSeconds(seconds);
            _inputs.ModifyInputs(new string[]{"shoot","holster","pickup","select_weapon"},true);
            _uiCanvas.ToggleAmmoLoad(false);
        }

        public void EquipWeapon(GenericWeapon weapon)
        {
            _currentWeapon = weapon;
            weapon.ShowHide();
            weapon.ViewModel.transform.SetParent(HumanoidLeftGrip);
            _uiCanvas.AmmoText(weapon.AmmoCount.ToString());
            _uiCanvas.ShowHideAmmoText(true);
        }
    }
}