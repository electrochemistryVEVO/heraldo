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
using Unity.Properties;
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
        private static readonly int IsDead = Animator.StringToHash("isDead");

        public int MaxHealth = 100;
        public bool IsAlly = false;
        public float RespiteTime = 2.5f;
        public float TurningSpeed = 0.5f;
        public TroopController Troop;
        public GenericWeapon currentWeapon;
        public Transform RotatingTransform; 
        public Animator NpcAnimator;
        public EnemyWeaponHandler WpnHandler;
        public GameObject corpseObject;
        
        private WeakReference<GameObject> _rotatingObject;
        private int _health;
        private StarterAssetsInputs _inputs;
        private AStarGrid _aStarGrid;
        private Vector2? _walkToPoint = null;
        private Vector3? _aimAtPoint = null;
        private GameObject _enemyRoutines; 
        private List<WeakReference<GameObject>> _currentTargets; //Ordenado por prioridad (cual esta mas cerca)
        private WeakReference<GameObject> _activeTarget;
        private Task _activeMovementTask;
        private List<Path> _activeRoute = null;
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
                        Transition(null);
                        Die();
                        break;
                    case EnemyState.PATROL:
                        Transition(null);
                        _activeMovementTask = FollowRoutine();
                        break;
                    case EnemyState.HUNT:
                        Transition(null);
                        _activeMovementTask = SearchForPlayer();
                        break;
                }
            }
        }
        
        private void Transition(Task t1)
        {
            _pathNodeReached.TrySetResult(false);
            if (_activeMovementTask is { Status: TaskStatus.Running })
            {
                _activeMovementTask.Wait(); 
            }
            //Limpiar ruta luego de usar
            if (_activeRoute != null)
            {
                foreach (var path in _activeRoute)
                {
                    path.dirty = false;
                }
            }
            _pathNodeReached = new();
            _activeMovementTask = t1;
        }

        private void Die()
        {
            Debug.Log("Isn't that awesome, Kris? That guy just died");
            GameObject _obj;
            //_pathNodeReached.SetResult(false);
            /*
             bool gotObj = _rotatingObject.TryGetTarget(out _obj);
               if(gotObj)
               {
                   _obj.SendMessage("QueueFree");
               }
             */
            corpseObject.transform.parent = transform.parent;
            //corpseObject.transform.position = transform.position;
            corpseObject.SetActive(true);
            
            //Destroy(this.GetComponent<FirstPersonController>());
            //Destroy(this.GetComponent<CharacterController>());
            //NpcAnimator.SetBool(IsDead,true);
            //enabled = false;
            Destroy(gameObject);
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
            Transition(null);
            _activeMovementTask = FollowRoutine();
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
                if((lookAtWalkToPoint-transform.position).magnitude>DIST_THRESHOLD)
                    transform.LookAt(lookAtWalkToPoint);
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

        async Task FollowRoutine()
        {
            Debug.Log("Following nearest routine");
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
            while (j<50)
            {
                Transform currentStep = steps[i + 1]; 
                //_walkToPoint = new Vector2(currentStep.position.x,currentStep.position.z);
                //_aimAtPoint = _walkToPoint; 
                bool reached = await CalculatePath(currentStep.position);
                Debug.Log("Current step:" + (i+1));
                _pathNodeReached = new TaskCompletionSource<bool>();
                //bool reached = await _pathNodeReached.Task; 
                if (!reached) return;
                i = (i + 1) % (steps.Length - 1);
                j++;
            }
        }

        public async Task CalculatePathExternal(Vector3 pos)
        {
            await CalculatePath(pos);
        }
        
        async Task<bool> CalculatePath(Vector3 pos)
        {
            _pathNodeReached.TrySetResult(false);
            _pathNodeReached = new TaskCompletionSource<bool>();
            Debug.Log(pos);
            List<Path> pathList = await _aStarGrid.GetPath(transform.position, pos);
            Debug.Log("Path returned");
            if (pathList.Count == 0)
            {
                Debug.Log("Returned null");
                return false;
            }
            int i = 0;
            //path = path.next;
            _activeRoute = pathList;
            foreach (var path in pathList)
            {
                if (path == null)
                {
                    Debug.Log("Returned null path");
                    break;
                }
                Debug.Log("Siguiente objetivo: (" + path.pos_x + ";" + path.pos_y + ")");
                _walkToPoint = new Vector2(path.realPos.x, path.realPos.z);
                //_aimAtPoint = new Vector2(transform.position.x - path.realPos.x,transform.position.z - path.realPos.z);
                //GameObject point = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                //Debug.Log("G = "+path.g);
                //point.transform.position = new Vector3(_walkToPoint.Value.x,transform.position.y+40,_walkToPoint.Value.y);
                bool reached = await _pathNodeReached.Task;
                path.dirty = false;
                if (!reached)
                {
                    return false;
                }
                _pathNodeReached = new TaskCompletionSource<bool>();
                Debug.Log(reached); 
                //path = path.next;
                i++;
            }
            Debug.Log("Path finished with "+i+" iterations");
            return true;
        }

        public void SpotPlayer(GameObject player)
        { 
            //Interrumpir
            //Debug.Log(player.name+" spotted");
            //_pathNodeReached.TrySetResult(false);
            //_pathNodeReached = new();
            _walkToPoint = null;
            if(Enemy_State != EnemyState.ENGAGE)
            {
                Enemy_State = EnemyState.ENGAGE;
                WpnHandler.Target = player.transform;
                AskWeaponHandler().Wait();
            }
            //Enemy_State = EnemyState.ENGAGE;
            _aimAtPoint = player.transform.position;
            WpnHandler.Attack(player.transform);
            //if(_fireTimeElapsed)StartCoroutine(AttackEnemy(player.transform.position));
        }

        async Task AskWeaponHandler()
        {
            Debug.Log("Asking weapon handler");
            /*
             * int j = 0;
               while (j<50)
               {
                   //Transform currentStep = steps[i + 1]; 
                   //_walkToPoint = new Vector2(currentStep.position.x,currentStep.position.z);
                   //_aimAtPoint = _walkToPoint; 
                   Vector3? nextPos = WpnHandler.NextMovementAction();
                   Debug.Log(nextPos);
                   if (nextPos == null) return;
                   bool reached = await CalculatePath((Vector3)nextPos);
                   //bool reached = await _pathNodeReached.Task; 
                   if (!reached || Enemy_State != EnemyState.ENGAGE) return;
                   _pathNodeReached = new TaskCompletionSource<bool>();
                   //i = (i + 1) % (steps.Length - 1);
                   j++;
               } 
             */
        }

        private async Task SearchForPlayer()
        {
            Debug.Log("Searching for player");
            //Interrumpir
            /*if(Enemy_State != EnemyState.HUNT)
            {
                _pathNodeReached.TrySetResult(false);
                _pathNodeReached = new();
            }
            _walkToPoint = null;*/
            //Trace path to last known position of player
            if(_aimAtPoint!=null)
            {
                _enemyState = EnemyState.HUNT;
                bool notInterrupted = await CalculatePath(_aimAtPoint.Value);
                if (!notInterrupted) return;
            } 
            Debug.Log("Player lost");
            Enemy_State = EnemyState.PATROL;
            //Debug.Log("Calculate path to "+ (_aimAtPoint.ToString() ?? "nothing"));
            Debug.Log("This part causes a memory leak");
            
        }

        public void OnTakeDamage(DamageInfo damage)
        {
            if (_enemyState != EnemyState.DEAD)
            {
                Health = Math.Max(Health-damage.damage,0);
                if (Health == 0) return;
                Debug.Log("Health: " + Health);
                //Search for the one who shot me
                if (damage.enemyPos is not null)
                {
                    _aimAtPoint = damage.enemyPos;
                    Transition(null);
                    _activeMovementTask = SearchForPlayer();
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