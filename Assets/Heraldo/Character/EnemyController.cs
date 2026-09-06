using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Heraldo.Character.Enemy;
using Heraldo.Character.Pathfinding;
using Heraldo.Global.Structs;
using Heraldo.Weapon;
using StarterAssets;
using UnityEngine;

namespace Heraldo.Character
{
    public enum EnemyState
    {
        ALIVE,
        PATROL,
        HUNT,
        ENGAGE,
        SEEK,
        SEEK_GUARD,
        DEAD
    }
    

    public class EnemyController : MonoBehaviour
    {
        private static readonly float DIST_THRESHOLD = 0.2f;
        private static readonly int Speed = Animator.StringToHash("speed");

        public int MaxHealth = 100;
        public bool IsAlly = false;
        public float RespiteTime = 2.5f;
        public float TurningSpeed = 0.5f;
        public TroopController Troop;
        public GenericWeapon currentWeapon;
        public Transform RotatingTransform; 
        public Animator NpcAnimator;
        public EnemyWeaponHandler WpnHandler;
        public MeshRenderer corpseMesh;
        
        private WeakReference<GameObject> _rotatingObject;
        private int _health;
        private StarterAssetsInputs _inputs;
        private AStarGrid _aStarGrid;
        private Vector2? _walkToPoint = null;
        private Vector3? _aimAtPoint = null;
        private GameObject _enemyRoutines; 
        private List<WeakReference<GameObject>> _currentTargets; //Ordenado por prioridad (cual esta mas cerca)
        private WeakReference<GameObject> _activeTarget;
        public GameObject ActiveTarget {
            get
            {
                GameObject _target;
                bool hasTarget = _activeTarget.TryGetTarget(out _target);
                if (hasTarget)
                {
                    return _target;
                }
                //TriggerExit(_target.GetComponent<Collider>());
                return null;
            }
        }
        
        //Children
        
        //Events
        private TaskCompletionSource<bool> _pathNodeReached;
        //True = continue with the process, False = interrupt

        private int Health
        {
            get => _health;
            set
            {
                _health = value;
                if (value <= 0) Enemy_State = EnemyState.DEAD;
            }
        }

        private EnemyState _enemyState;

        public EnemyState Enemy_State
        {
            get => _enemyState;
            set
            {
                _enemyState = value;
                switch (_enemyState)
                {
                    case EnemyState.DEAD:
                        Die();
                        break;
                    case EnemyState.HUNT:
                        SearchForPlayer();
                        break;
                    case EnemyState.PATROL:
                        FollowRoutine();
                        break;
                }
            }
        }

        private void Die()
        {
            GameObject _obj;
            bool gotObj = _rotatingObject.TryGetTarget(out _obj);
            if(gotObj)
            {
                _obj.SendMessage("QueueFree");
            }
            corpseMesh.enabled = true;
            Destroy(this.GetComponent<FirstPersonController>());
            Destroy(this.GetComponent<CharacterController>());
            Destroy(this);
        }

        IEnumerator ScanForEnemies()
        {
            Vector3 toYourLeft = transform.TransformPoint(new Vector3(0.5f, 0, -0.5f));
            Vector3 toYourRight = transform.TransformPoint(new Vector3(0.5f, 0, -0.5f));
            _aimAtPoint = toYourLeft;
            yield return new WaitForSeconds(3);
            _aimAtPoint = toYourRight;
            yield return new WaitForSeconds(3);
            _aimAtPoint = null;
            if (Enemy_State == EnemyState.SEEK)
            {
                Enemy_State = EnemyState.PATROL;
                //FollowRoutine();
            }
            else ScanForEnemies();
            //TODO: Add interruption when player is spotted
        }
        
        public bool isObjectEnemy(GameObject obj)
        {
            return IsAlly ? obj.CompareTag("Enemy Character") : (obj.CompareTag("Player") || obj.CompareTag("Ally"));
        }

        void Start()
        {
            Health = MaxHealth;
            Enemy_State = EnemyState.ALIVE;
            _inputs = GetComponent<StarterAssetsInputs>();
            _walkToPoint = Vector2.zero;
            _aStarGrid = GameObject.FindWithTag("AStarGrid").GetComponent<AStarGrid>();
            _pathNodeReached = new TaskCompletionSource<bool>();
            _enemyRoutines = GameObject.FindWithTag(IsAlly ? "Ally Routines" :"Enemy Routines");
            _currentTargets = new List<WeakReference<GameObject>>();
            _rotatingObject = new WeakReference<GameObject>(RotatingTransform.gameObject);
            FollowRoutine();
            //CalculatePath(Vector3.zero);
        }

        private void Update()
        {
            //Calcular movimiento
            if (_walkToPoint != null)
            {
                //Debug.Log(_walkToPoint);
                Vector2 walkToPoint = _walkToPoint.Value;
                Vector2 move = new Vector2(walkToPoint.x-transform.position.x, walkToPoint.y - transform.position.z );
                if (move.magnitude <= DIST_THRESHOLD)
                {
                    _inputs.move = Vector2.zero;
                    _pathNodeReached.TrySetResult(true);
                }
                else
                {
                    Vector3 _move = transform.InverseTransformDirection(new Vector3(move.x,0,move.y)/ move.magnitude);
                    _inputs.move = new Vector2(_move.x, _move.z);
                } 
                Vector3 lookAtWalkToPoint = new Vector3(walkToPoint.x, transform.position.y, walkToPoint.y);
                //if((lookAtWalkToPoint-transform.position).magnitude>DIST_THRESHOLD)
                  //  transform.LookAt(lookAtWalkToPoint);
            }
            else _inputs.move = Vector2.zero;
            NpcAnimator.SetFloat(Speed,_inputs.move.magnitude);
        }

        void LateUpdate()
        {
            //Apuntar a puntos 
            if (_aimAtPoint != null)
            {
                Vector3 nextLookAt = new Vector3(_aimAtPoint.Value.x, transform.position.y, _aimAtPoint.Value.z);
                //Vector3 currentLookAt = transform.TransformPoint(Vector3.forward) * nextLookAt.magnitude;
                RotatingTransform.LookAt(//Vector3.Lerp(currentLookAt,nextLookAt,TurningSpeed),
                    nextLookAt,
                    Vector3.up);
            }
            //Reciclaje de TriggerExit para borrar elementos que fueron borrados
            //DeallocateDead();
        }

        

        /*
         *
         * private void DeallocateDead()
           {
               foreach (var weakRef in _currentTargets.ToList())
               {
                   GameObject _target;
                   bool hasTarget = weakRef.TryGetTarget(out _target);
                   if (!hasTarget || _target ==null)
                   { 
                       _currentTargets.Remove(weakRef);
                   } 
               }
               
               if (_currentTargets.Count <= 0 && Enemy_State == EnemyState.ENGAGE)
               {
                   Debug.Log("Does this ever get fired");
                   Enemy_State = EnemyState.HUNT;
                   SearchForPlayer();
               }
               else
               {
                   //Debug.Log("Numero de objetivos: "+_currentTargets.Count);
               }
               //SearchForPlayer(other.gameObject);
           }

           public void TriggerExit(Collider other)
           {
               if (!this.enabled) return;
               //var bufTargets = new List<WeakReference<GameObject>>(_currentTargets);
               lock (_currentTargets)
               {
                   if (isObjectEnemy(other.gameObject))
                   {
                       foreach (var weakRef in _currentTargets.ToList())
                       {
                           GameObject _target;
                           bool hasTarget = weakRef.TryGetTarget(out _target);
                           if (hasTarget && _target!=null)
                           {
                               if (_target == other.gameObject)
                               {
                                   _currentTargets.Remove(weakRef);
                                   break;
                               }
                           }
                           else _currentTargets.Remove(weakRef);
                       }

                       //if (_currentTargets.Count <= 0)
                       //SearchForPlayer(other.transform.position);
                   }
               }
           }
           
           public void TriggerEnter(Collider other)
           {
               if (!this.enabled) return;
               //Debug.Log(other.name);
               if (isObjectEnemy(other.gameObject))
               {
                   Debug.Log("Enemy spotted: "+other.gameObject.name);
                   _currentTargets.Add(new WeakReference<GameObject>(other.gameObject));
               }
           }
           
           public void TriggerStay(Collider other)
           {
               if (!this.enabled) return;
               if (isObjectEnemy(other.gameObject) && CheckIfInLOS(other))
               {
                   SpotPlayer(other.gameObject);
               }
               else TriggerExit(other); //Debe de haber una manera mas efectiva de hacer esto
           }
         */
        
        private bool CheckIfInLOS(Collider other)
        {
            Vector3 pos = transform.position;
            Vector3 dist = other.gameObject.transform.position - pos;
            RaycastHit hit;
            bool wasHit = Physics.Raycast(pos,dist.normalized, out hit,dist.magnitude,LayerMask.GetMask("Default"));
            return !wasHit || hit.collider.gameObject == other.gameObject;
        }

        async Task FollowRoutine()
        {
            double minDist = Double.MaxValue;
            Transform minRoutine = null;
            foreach(var routine in _enemyRoutines.GetComponentsInChildren<Transform>())
            {
                if(routine == _enemyRoutines.transform || routine.parent != _enemyRoutines.transform)continue; //Getting rid of parent or grandchildren as a possibility
                double dist = (routine.position - transform.position).magnitude;
                if (dist < minDist)
                {
                    minDist = dist;
                    minRoutine = routine;
                }
            }

            if (minRoutine == null) return;
            Debug.Log(minRoutine.name);
            Transform[] steps = minRoutine.gameObject.GetComponentsInChildren<Transform>();
            if (steps.Length == 0) return;
            Debug.Log(steps.Length);
            int i = 0;
            int j = 0;
            while (j<5)
            {
                Transform currentStep = steps[i + 1];
                Debug.Log(currentStep.gameObject.name);
                _walkToPoint = new Vector2(currentStep.position.x,currentStep.position.z);
                _aimAtPoint = _walkToPoint;
                Debug.Log(_walkToPoint);
                bool reached = await _pathNodeReached.Task;
                _pathNodeReached = new TaskCompletionSource<bool>();
                if (!reached) return;
                i = (i + 1) % (steps.Length - 1);
                j++;
            }
        }
        
        async Task CalculatePath(Vector3 pos)
        {
            Path path = await _aStarGrid.GetPath(transform.position, pos);
            Debug.Log("Path returned");
            if (path == null)
            {
                Debug.Log("Returned null");
                return;
            }
            int i = 0;
            path = path.next;
            while (path != null && i<50)
            {
                Debug.Log("Siguiente objetivo: (" + path.pos_x + ";" + path.pos_y + ")");
                _walkToPoint = new Vector2(path.realPos.x, path.realPos.z);
                _aimAtPoint = _walkToPoint;
                //GameObject point = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                //point.transform.position = new Vector3(_walkToPoint.Value.x,transform.position.y,_walkToPoint.Value.y);
                bool reached = await _pathNodeReached.Task;
                _pathNodeReached = new TaskCompletionSource<bool>();
                Debug.Log(reached);
                if (!reached) return;
                path = path.next;
                i++;
            }
        }

        public void SpotPlayer(GameObject player)
        { 
            //Interrumpir
            //Debug.Log("Player spotted");
            _pathNodeReached.TrySetResult(false);
            _walkToPoint = null;
            //Check if this enemy is the closest
            /*
            foreach (var weakRef in _currentTargets.ToList())
            {
                GameObject closestEnemy;
                bool hasClosestEnemy = weakRef.TryGetTarget(out closestEnemy);
                if (hasClosestEnemy && closestEnemy != null)
                {
                    if (closestEnemy == player) continue;
                    if ((transform.position - closestEnemy.transform.position).magnitude
                        < (transform.position - player.transform.position).magnitude)
                        return;
                } 
                else _currentTargets.Remove(weakRef);
            } */
            Enemy_State = EnemyState.ENGAGE;
            _aimAtPoint = player.transform.position;
            WpnHandler.Attack(player.transform);
            //if(_fireTimeElapsed)StartCoroutine(AttackEnemy(player.transform.position));
        }

        private async Task SearchForPlayer()
        {
            Debug.Log("Searching for player");
            //Interrumpir
            if(Enemy_State != EnemyState.HUNT)_pathNodeReached.TrySetResult(false);
            _walkToPoint = null;
            //Trace path to last known position of player
            if(_aimAtPoint!=null)
            {
                _enemyState = EnemyState.HUNT;
                _pathNodeReached = new TaskCompletionSource<bool>();
                await CalculatePath(_aimAtPoint.Value); 
            } 
            Debug.Log("Player lost");
            Enemy_State = EnemyState.PATROL;
            
        }

        public void OnTakeDamage(DamageInfo damage)
        {
            if (_enemyState != EnemyState.DEAD)
            {
                Health = Math.Max(Health-damage.damage,0);
                Debug.Log("Health: " + Health);
                //Search for the one who shot me
                if (damage.enemyPos is not null)
                {
                    _aimAtPoint = damage.enemyPos;
                    SearchForPlayer();
                }
            }
        }

        public void BeAlerted(Vector3 position)
        {
            if (_enemyState != EnemyState.ENGAGE)
            {
                OnTakeDamage(new DamageInfo(0,position));
            }
        }

        void WalkToPoint(Vector2 point)
        {
            _walkToPoint = point; 
        }
    }
}