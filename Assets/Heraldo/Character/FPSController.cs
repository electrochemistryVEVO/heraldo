using System;
using System.Collections;
using System.Threading.Tasks; 
using Heraldo.Cutscenes;
using Heraldo.Flavor;
using Heraldo.Global;
using Heraldo.Global.Enums;
using Heraldo.Global.Structs;
using Heraldo.UI;
using Heraldo.Weapon;
using Heraldo.Weapon.Holster;
using LitMotion;
using LitMotion.Extensions;
using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Heraldo.Character
{
    public class FPSController : MonoBehaviour
    {
        private static readonly int SpecialAnim = Animator.StringToHash("specialAnim");
        public float CrouchSpeed = 1.0f;
        public float CrouchGroundOffset;
        public float ProneSpeed = 0.5f;
        public float ProneGroundOffset;
        public float GroundOffset;
        public bool IsCrouched = false;
        public float InteractRadius;
        public Transform HumanoidLeftGrip;
        public Animator CharacterAnimator;
        public TimelineManager timelineManager;
        public GameObject Weapon;
        private GenericWeapon _currentWeapon;
        GenericWpnHolster _arsenal;
        public int LastWpnIndex => _currentWeapon is not null ? _currentWeapon.transform.GetSiblingIndex() : -1;
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
        
        //Cutscenes
        private CutsceneManager _cutsceneManager;
        
        //UI
        private LevelUIManager _uiCanvas;
        
        //Physics
        private Vector3 _prevPos;
        
        //Input
        private PlayerInput _playerInput;
        private bool _playerInputEnabled = true;
        
        //Health
        public int MaxHealth = 100;
        private int _health;
        
        //FirstPersonController variables
        private float _ogWalkSpeed;
        private float _ogSprintSpeed;
        private float _ogRotationSpeed;

        private float SpreadMultiplier()
        {
            Vector3 velocity = (transform.position - _prevPos) / Time.deltaTime;
            return velocity.magnitude / _ogSprintSpeed;
        }

        private void Awake()
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
            _health = MaxHealth;
            _ogWalkSpeed = _controller.MoveSpeed;
            _ogSprintSpeed = _controller.SprintSpeed;
            _ogRotationSpeed = _controller.RotationSpeed;
            _cutsceneManager = GameObject.FindWithTag("CutsceneManager").GetComponent<CutsceneManager>();
            
            LoadSaveData();
        }

        private void Start()
        {
            InitialSaveData();
            
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            _uiCanvas.HealthText(_health,MaxHealth);
            _uiCanvas.ShowHideAmmoText();
            //_currentWeapon = Weapon.GetComponent<GenericWeapon>();
        }

        public void OnTakeDamage(DamageInfo info)
        {
            _health = Math.Clamp(_health - info.damage, 0, MaxHealth);
            _uiCanvas.HealthText(_health,MaxHealth);
            if (info.damage > 0)
            {
                _uiCanvas.TakeDamage(info.damage,MaxHealth);
            }
            if (_health <= 0) Die();
        }

        void Die()
        {
            if (!_cutsceneManager.CutsceneMode)
            {
                //Animation that makes player tumble down
                //Disable input
                _playerInput.enabled = false;
                //Fade to black
                _uiCanvas.FailLevel();
            }
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
            _controller.MoveSpeed = (isCrouched && _controller.Grounded) ? CrouchSpeed : _controller.MoveSpeed;
            _camera.localPosition = new Vector3(
                _camera.localPosition.x, 
                (isCrouched && _controller.Grounded) ? CrouchGroundOffset : GroundOffset, 
                _camera.localPosition.z
                );
            IsCrouched = isCrouched;
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
            if (_inputs.shoot)
            {
                _inputs.shoot = false;
                if (_currentWeapon !=null)
                {
                    bool fired = _currentWeapon.Fire(_camera.position,_camera.TransformDirection(Vector3.forward),spreadMultiplier);
                    if (fired)
                    {
                        TimelineID id = _currentWeapon.ShootTimelineId();
                        if (id != TimelineID.CANT_SHOOT)
                        {
                            StartCoroutine(_WeaponCooldown(_currentWeapon.ReloadTime));
                            timelineManager.timelineDict[id].Play();
                            _uiCanvas.AmmoText(_currentWeapon.AmmoCount); 
                        }
                    }
                }
                //Debug.Log("Shot!");
            }
        }
        //Antes de recoger, por motivos de UI, identificar el arma que esta mas cerca
        private void CalculateNearestInteractable()
        {
            int MAX_COLLIDERS = 5;
            Collider[] nearWpns = new Collider[MAX_COLLIDERS];
            //Weapon pickups
            int numWpns = Physics.OverlapSphereNonAlloc(transform.position, InteractRadius,nearWpns,_wpnPickupMask);
            if (numWpns <= 0)
            {
                _uiCanvas.SeeInteractable(null);
                GameObject obj1;
                if (_nearestInteractableRef.TryGetTarget(out obj1) 
                   && obj1 != null)
                    obj1.SendMessage("OnLeave",SendMessageOptions.DontRequireReceiver);
                return;
            }
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

            GameObject obj;
            Interactable interactable = nearWpns[bestI].gameObject.GetComponent<Interactable>();
            if (interactable)
            {
                _uiCanvas.SeeInteractable(interactable.GetDescription());
                interactable.OnHover();
                if (_nearestInteractableRef.TryGetTarget(out obj) 
                    && obj != null
                    && obj != interactable.gameObject)
                {
                    Interactable prevInteractable = obj.GetComponent<Interactable>();
                    prevInteractable.OnLeave();
                }
                _nearestInteractableRef.SetTarget(nearWpns[bestI].gameObject);
            }
        }
        //Recoger armas
        public int PickupWeapon(int ammoCount,int weaponIndex)
        {
            int result = _arsenal.PickupWeapon(ammoCount,weaponIndex);
            _uiCanvas.AmmoText(_currentWeapon.AmmoCount);
            return result;
        }

        public void ToggleInputs(bool enable = false)
        {
            _inputs.ModifyInputs(new string[]
            {
                "crouch","shoot","holster","switch_weapon",
                "pickup"
            },enable);
            _controller.MoveSpeed = enable ? _ogWalkSpeed : 0;
            _controller.SprintSpeed = enable ? _ogSprintSpeed : 0;
            _controller.RotationSpeed = enable ? _ogRotationSpeed : 0;
        }
        private void Pickup()
        {
            GameObject obj;
            bool hasTarget = _nearestInteractableRef.TryGetTarget(out obj);
            if (hasTarget && obj!=null)
            {
                Debug.Log("Interactable: " + obj.name);
                //Debug.Log("Picked up " + obj.name);
                var wpn = obj.GetComponent<Interactable>();
                wpn.Interact(this);
            }
            else Debug.Log("No interactable");
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
            if (_currentWeapon != null)
            {
                _currentWeapon.Enabled = false;
                _currentWeapon.ShowHide(false);
            } 
            _currentWeapon = null;
            _uiCanvas.ShowHideAmmoText();
            //Destroy(Weapon);
        }
        private IEnumerator _WeaponCooldown(float seconds)
        {
            _inputs.ModifyInputs(new string[]{"shoot","holster","pickup","select_weapon"},false);
            _uiCanvas.ToggleAmmoLoad(true);
            yield return new WaitForSeconds(seconds);
            _inputs.ModifyInputs(new string[]{"shoot","holster","pickup","select_weapon"},true);
            _uiCanvas.ToggleAmmoLoad(false);
        }

        public void EquipWeapon(GenericWeapon weapon)
        {
            weapon.Enabled = true;
            _currentWeapon = weapon;
            weapon.ShowHide();
            StartCoroutine(_WeaponCooldown(0.7f));
            //weapon.ViewModel.transform.SetParent(HumanoidLeftGrip);
            _uiCanvas.ShowHideAmmoText(true);
            _uiCanvas.AmmoText(_currentWeapon.AmmoCount);
            _uiCanvas.WeaponDescriptionText(weapon.Description); 
        }

        public void PickupCorpse()
        {
            _arsenal.HoldCorpse();
        }
        
        //Cutscene actions
        public async Task RotateTowards(Vector3 target,float time)
        {
            Debug.Log("[ROTATE-TOWARDS] target: "+target);
            Quaternion q1 = transform.rotation;
            q1.SetLookRotation(Vector3.ProjectOnPlane(target - transform.position,transform.up));
            await LMotion.Create(transform.rotation, q1, time)
                .WithEase(Ease.OutSine)
                .BindToRotation(transform)
                .AddTo(gameObject)
                .ToAwaitable();
        }
        
        //Requests
        public void ChangeCorpsePrefab(GameObject obj)
        {
            _arsenal.corpsePrefab = obj;
        }
        
        //Save data
        public GameSaveData returnSaveData(string nextScene)
        {
            return new GameSaveData(nextScene, _health, _arsenal.GetAmmoCounts()[0]);
        }

        private void InitialSaveData()
        {
            if (!JsonParser.SaveFileExists())
                JsonParser.WriteSaveData(new GameSaveData("Scenes/MainScene", 
                    MaxHealth, 
                    _arsenal.GetAmmoCounts()[0],
                    "04:50"));
        }
        
        //Load data
        private void LoadSaveData()
        {
            if (!JsonParser.SaveFileExists()) return;
            GameSaveData? data = JsonParser.ReadSaveData();
            if (!data.HasValue) return;
            _health = data.Value.playerHealth;
            _arsenal.SetAmmoCount(0,data.Value.rifleAmmo);
        }
    }
}