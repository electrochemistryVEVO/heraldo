using Heraldo.Weapon;
using StarterAssets;
using UnityEngine;

namespace Heraldo.Character
{
    public class FPSController : MonoBehaviour
    {
        public float CrouchSpeed = 1.0f;
        public float CrouchGroundOffset;
        public float GroundOffset;
        
        //Components within the instance
        private Inputs _inputs;
        private float _move_speed;
        private CharacterController characterController;
        private FirstPersonController _controller; 
        private Transform _camera;
        
        //Global game objects referenced
        private BulletImpact _impact;
        
        //Debounces
        private bool _shoot_deb = false;

        private void Start()
        {
            
            characterController = GetComponent<CharacterController>();
            _controller = GetComponent<FirstPersonController>();
            _move_speed = _controller.MoveSpeed;
            _camera = transform.GetChild(0);
            _inputs = GetComponent<Inputs>();
            _impact = GameObject.FindWithTag("Particle Manager").GetComponent<BulletImpact>();
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
            //Disparar si se puede
            if (_inputs.shoot && !_shoot_deb)
            {
                _shoot_deb = true;
                //Raycast
                RaycastHit hit;
                //Center point in camera = 
                bool wasHit = Physics.Raycast(_camera.position,_camera.TransformDirection(Vector3.forward), out hit,Mathf.Infinity);
                if (wasHit)
                {
                    if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Enemy"))
                    {
                        _impact.FleshImpact(hit.point,hit.normal,hit.transform.gameObject);
                    }
                    else  _impact.Impact(hit.point, hit.normal);
                }
                //Debug.Log("Shot!");
            }
            else
            {
                if (!_inputs.shoot) _shoot_deb = false;
            }
        }
    }
}