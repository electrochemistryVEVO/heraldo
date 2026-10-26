using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Heraldo.Character.Enemy;
using Heraldo.Character.Pathfinding;
using Heraldo.Global.Structs;
using Heraldo.Weapon;
using StarterAssets;
using Unity.Properties;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using Quaternion = UnityEngine.Quaternion;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

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
        private static readonly float DIST_THRESHOLD = 1f;
        private static readonly float DELTA_THRESHOLD = 0.01f;
        private static readonly int MAX_FRAMES_FROZEN = 120;
        
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
        private CapsuleCollider _capsule;
        
        private WeakReference<GameObject> _rotatingObject;
        private int _health;
        private StarterAssetsInputs _inputs;
        private AStarGrid _aStarGrid;
        private NavMeshAgent _agent;
        //private NavMeshObstacle _obstacle;
        private Vector2? _walkToPoint = null;
        private Vector3? _aimAtPoint = null;
        public GameObject enemyRoutines; 
        private List<WeakReference<GameObject>> _currentTargets; //Ordenado por prioridad (cual esta mas cerca)
        private WeakReference<GameObject> _activeTarget;
        private Task _activeMovementTask;
        private List<Path> _activeRoute = null;
        private FirstPersonController _firstPerson;
        
        //Ayuda a controlar si npc se ha quedado atascado
        private Vector3 _prevPos;
        private int _framesFrozen = MAX_FRAMES_FROZEN;

        private Color _gizmoColor = Color.magenta;
        
        //ID
        private Guid _id = Guid.Empty;

        public Guid ID => _id;

        private Routine? _overdriveRoutine;
        
        private Routine? _currentRoutine;
        public Routine? CurrentRoutine => _currentRoutine;
        
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

        private EnemyState _enemyState = EnemyState.ALIVE;

        public EnemyState Enemy_State
        {
            get => _enemyState;
            set
            {
                _enemyState = value;
                Debug.Log("[ENEMY] "+name+" state: "+_enemyState);
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
            Debug.Log("Task was stopped");
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
            Debug.Log("[DEATH] Enemy "+ID+" died.");
            //Mandar mensaje a tropa
            if (Troop) Troop.trooperDown(ID);
            //Activar cadaver
            corpseObject.transform.parent = transform.parent;
            corpseObject.SetActive(true);
            //Destruir
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

        private void Awake()
        {
            _id = Guid.NewGuid();
            _gizmoColor = new Color(Random.value, Random.value, Random.value);
            Health = MaxHealth;
            Enemy_State = EnemyState.ALIVE;
            _inputs = GetComponent<StarterAssetsInputs>();
            _agent = GetComponent<NavMeshAgent>();
            //_obstacle = GetComponent<NavMeshObstacle>();
            _firstPerson = GetComponent<FirstPersonController>();
            _walkToPoint = Vector2.zero;
            _aStarGrid = GameObject.FindWithTag("AStarGrid").GetComponent<AStarGrid>();
            _pathNodeReached = new TaskCompletionSource<bool>();
            //enemyRoutines = GameObject.FindWithTag(IsAlly ? "Ally Routines" :"Enemy Routines");
            _currentTargets = new List<WeakReference<GameObject>>();
            _rotatingObject = new WeakReference<GameObject>(RotatingTransform.gameObject); 
            _prevPos = transform.position;
            _capsule = GetComponent<CapsuleCollider>();
            /*
            if (Troop is null)
            {
                Transition(null);
                _activeMovementTask = FollowRoutine();
            }*/
            //CalculatePath(Vector3.zero);
        } 

        private void Update()
        {
            
            //Calcular movimiento
            if (_walkToPoint != null)
            {
                //Debug.Log(_walkToPoint);
                 
                //_obstacle.carving = false;
                Vector2 walkToPoint = _walkToPoint.Value;
                Vector2 move = new Vector2(walkToPoint.x-transform.position.x, walkToPoint.y - transform.position.z );
                if (move.magnitude > 50f)
                {
                    _inputs.sprint = true;
                }
                else _inputs.sprint = false;
                if (move.magnitude <= DIST_THRESHOLD)
                {
                    _inputs.move = Vector2.zero;
                    _pathNodeReached.TrySetResult(true);
                }
                else
                {
                    //Vector3 _move = Glide();
                    Vector3 _move = transform.InverseTransformDirection(new Vector3(move.x,0,move.y)/move.magnitude);
                    _inputs.move = new Vector2(_move.x, _move.z);
                } 
                Vector3 lookAtWalkToPoint = new Vector3(walkToPoint.x, transform.position.y, walkToPoint.y);
                if((lookAtWalkToPoint-transform.position).magnitude>DIST_THRESHOLD)
                    transform.LookAt(lookAtWalkToPoint); 
            }
            else
            {
                _inputs.move = Vector2.zero;
                //_obstacle.carving = true;
            }
            NpcAnimator.SetFloat(Speed,_inputs.move.magnitude);
            _prevPos = transform.position;
        }

        //Ajusta la direccion cuando tiene un enemigo en frente
        Vector3 Glide()
        {
            
            float[] allDirs = new[] { 0,90f,-90f,180f };
            Vector2 walkToPoint = _walkToPoint.Value;
            Vector2 move2d = new Vector2(walkToPoint.x-transform.position.x, walkToPoint.y - transform.position.z );
            Vector3 move = transform.InverseTransformDirection(new Vector3(move2d.x,0,move2d.y)/move2d.magnitude);
            foreach (var dir in allDirs)
            {
                Vector3 _move = Quaternion.AngleAxis(dir,Vector3.up) * move;
                //Debug.Log("[STEERING] Current movement: "+_move);
                //Determinar si se tiene que cambiar ruta
                //Maxima pendiente de desliz: 45 grados
                RaycastHit hit; 
                //Vector3 bottom = Capsule.bounds.center - new Vector3(0,Capsule.bounds.extents.y/2, 0);
                bool hasHit = Physics.CapsuleCast(_capsule.transform.position - new Vector3(0,_capsule.height - _capsule.radius,0),
                    _capsule.transform.position + new Vector3(0,_capsule.height - _capsule.radius,0),
                    _capsule.bounds.extents.x,
                    _move,
                    out hit, _firstPerson.MoveSpeed,
                    LayerMask.GetMask("Ally", "Enemy", "NavObstacle"));
                //Vector3 top = Capsule.bounds.center + new Vector3(0,Capsule.bounds.extents.y/2, 0);
                if (hasHit && MathF.Abs(Vector3.Dot(hit.normal.normalized,Vector3.up)) < MathF.Sqrt(2)/2)
                {
                    //Debug.Log("[RAYCAST] current enemy: "+name);
                    //Debug.Log("[RAYCAST] hit enemy: " + hit.collider.gameObject.name);
                    //Remap your route 
                    // or try other options
                    //_walkToPoint = null;
                    continue;
                }

                return _move;
            }

            return Vector3.zero;
        }
        
        void LateUpdate()
        {
            float deltaPos = (transform.position - _prevPos).magnitude;
            //Apuntar a puntos 
            if (_aimAtPoint != null)
            {
                Vector3 nextLookAt = new Vector3(_aimAtPoint.Value.x, transform.position.y, _aimAtPoint.Value.z);
                //Vector3 currentLookAt = transform.TransformPoint(Vector3.forward) * nextLookAt.magnitude;
                RotatingTransform.LookAt(//Vector3.Lerp(currentLookAt,nextLookAt,TurningSpeed),
                    _aimAtPoint.Value,
                    Vector3.up);
            }
            
            //Reciclaje de TriggerExit para borrar elementos que fueron borrados
            //DeallocateDead();
        }

        public Routine? GetClosestRoutine()
        {
            double minDist = Double.MaxValue;
            Transform minRoutine = null;
            foreach(var routine in enemyRoutines.GetComponentsInChildren<Transform>())
            {
                if(routine == enemyRoutines.transform || routine.parent != enemyRoutines.transform)continue; //Getting rid of parent or grandchildren as a possibility
                double dist = (routine.position - transform.position).magnitude;
                if (dist < minDist)
                {
                    minDist = dist;
                    minRoutine = routine;
                }
            }
            
            if (minRoutine == null) return null; 
            Routine result = Routine.FromGameObject(minRoutine.gameObject);
            return result;
        }

        async Task<bool> FollowRoutine()
        {
            Debug.Log("[FOLLOW-ROUTINE] Start");
            Routine? minRoutine;
            if (_overdriveRoutine.HasValue) minRoutine = _overdriveRoutine;
            else
            {
                Debug.Log("Following nearest routine");
                double minDist = Double.MaxValue;
                minRoutine = GetClosestRoutine();
                if (minRoutine == null) return false;
            }

            _currentRoutine = minRoutine;
            Vector3[] steps = minRoutine.Value.NodeList;
            if (steps.Length == 0) return false; 
            int i = 0;
            int j = 0;
            while (j<50)
            {
                Vector3 currentStep = steps[i]; 
                Debug.Log("[FOLLOW-ROUTINE] Current routine of "+name+": "+currentStep);
                //_walkToPoint = new Vector2(currentStep.position.x,currentStep.position.z);
                //_aimAtPoint = _walkToPoint; 
                bool reached;
                if (j == 0)
                {
                    //Solo usar pathfinding para el primero, asumir camino fijo para los otros
                    reached = await CalculatePath(currentStep);
                }
                else
                {
                    _walkToPoint = new Vector2(currentStep.x, currentStep.z);
                    reached = await _pathNodeReached.Task;
                }
                _pathNodeReached = new TaskCompletionSource<bool>();
                //bool reached = await _pathNodeReached.Task; 
                if (!reached) return false;
                i = (i+1) % (steps.Length);
                if (!minRoutine.Value.Loopable && (i % steps.Length == 0)) break;
                j++;
            }

            return true;
        }

        private void OnDrawGizmosSelected()
        {
            enemyRoutines = GameObject.FindWithTag(IsAlly ? "Ally Routines" :"Enemy Routines");
            Routine? routine = GetClosestRoutine();
            if(routine.HasValue)
                Gizmos.DrawLine(transform.position,routine.Value.NodeList[0]);
            
            if (_activeRoute is not null)
            {
                Vector3 prevPathPos = _activeRoute[0].realPos;
                foreach (var path in _activeRoute)
                {
                    if (!path.dirty) continue;
                    Gizmos.color = _gizmoColor;
                    Gizmos.DrawLine(prevPathPos,path.realPos);
                    Gizmos.DrawSphere(path.realPos,5f);
                    Gizmos.color = Color.red;
                    if(path.arrived)Gizmos.DrawSphere(path.realPos,4);
                    prevPathPos = path.realPos;
                } 
            }
            
            if (_walkToPoint.HasValue)
            { 
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(new Vector3(_walkToPoint.Value.x,transform.position.y,_walkToPoint.Value.y),10);
            }

            if (_overdriveRoutine is not null)
            {
                Gizmos.DrawLine(transform.position,_overdriveRoutine.Value.NodeList[0]);
                Gizmos.DrawWireSphere(_overdriveRoutine.Value.NodeList[0],5f);
            }
        }

        public string GetKey()
        {
            return (IsAlly ? "Ally" : "Enemy") + transform.GetSiblingIndex();
        }

        public void CapturePath(Dictionary<string,List<Path>> dict)
        {
            dict.Add(GetKey(),_activeRoute);
        }
        
        public async Task FollowExternalRoutine(Routine routine)
        {
            _overdriveRoutine = routine;
            // Si aun no esta preparando Start(), cambia a Patrol
            //if (Enemy_State != EnemyState.ALIVE) Enemy_State = EnemyState.PATROL;
            Debug.Log("[EXTERNAL-ROUTINE] Trooper: "+name);
            Enemy_State = EnemyState.PATROL; 
        }

        public void FollowInternalRoutine()
        {
            _overdriveRoutine = null;
            FollowRoutine();
        }

        public async Task CalculatePathExternal(Vector3 pos)
        {
            await CalculatePath(pos);
        }
        
        async Task<bool> CalculatePath(Vector3 pos)
        {
            try
            {
                _pathNodeReached.TrySetResult(false);
                _pathNodeReached = new TaskCompletionSource<bool>();
                Debug.Log("[CALCULATE-PATH] "+name+": "+pos);
                //List<Path> pathList = await _aStarGrid.GetPath(transform.position, pos);
                bool arrived = false;
                while (true)
                {
                    List<Path> pathList = await GetPath(pos);
                    Debug.Log("[CALCULATE-PATH] "+name+": Path returned");
                    if (pathList.Count == 0)
                    {
                        Debug.Log("[CALCULATE-PATH] "+name+": Returned null");
                        _activeRoute = null;
                        return false;
                    }
                    int i = 0;
                    //path = path.next;
                    _activeRoute = pathList;
                    foreach (var path in pathList)
                    {
                        if (path == null)
                        {
                            Debug.Log("[CALCULATE-PATH] "+name+": Returned null path");
                            _activeRoute = null;
                            return false;
                        }
                        //Debug.Log("Siguiente objetivo: (" + path.pos_x + ";" + path.pos_y + ")");
                        _walkToPoint = new Vector2(path.realPos.x, path.realPos.z);
                        Debug.Log("[CALCULATE-PATH] "+name+": Walking to position - "+_walkToPoint);
                        bool reached = await _pathNodeReached.Task;
                        path.dirty = false;
                        if (!reached)
                        {
                            Debug.Log("[CALCULATE-PATH] "+name+": Path has been canceled");
                            _activeRoute = null;
                            return false;
                        }
                        if (path.arrived)
                        {
                            arrived = true;
                            break;
                        }
                        _pathNodeReached = new TaskCompletionSource<bool>();
                        //path = path.next;
                        i++;
                    }
                    _activeRoute = null;
                    if (arrived) break;
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                return false;
            }
            //Debug.Log("Path finished with "+i+" iterations");
            return true;
        }

        public void SpotPlayer(GameObject player)
        {  
            //Interrumpir
            //Debug.Log(player.name+" spotted");
            //_pathNodeReached.TrySetResult(false);
            //_pathNodeReached = new(); 
            if(Enemy_State != EnemyState.ENGAGE)
            {
                if (_overdriveRoutine is null || _enemyState == EnemyState.HUNT) _walkToPoint = null;
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
                //_overdriveRoutine = new Routine(new[] { _aimAtPoint.Value }, false);
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

        async Task<List<Path>> GetPath(Vector3 goal)
        {
            List<Path> pathList = new();
            try
            {
                //_obstacle.enabled = false;
                //_agent.enabled = true; 
                Debug.Log("[CALCULATE-PATH] "+name+": Entered GetPath");
                int timeoutFrames = 300;
                NavMeshHit hit;
                if (!NavMesh.SamplePosition(goal,out hit,float.MaxValue,NavMesh.AllAreas)) throw new Exception("Destination not reachable");
                goal = hit.position;
                
                //await Awaitable.MainThreadAsync();
                int i = 10;
                _agent.destination = goal; 
                
                //Start waiting

                for (; timeoutFrames > 0; timeoutFrames--)
                {
                    await Awaitable.EndOfFrameAsync();
                    //Debug.Log("[CALCULATE-PATH] "+name+": This is going to tank the framerate");
                    if (_agent.path.corners.Length > pathList.Count)
                    {
                        pathList = _agent.path.corners.Select(corner => new Path(0, 0, corner, 0))
                            .ToList();
                    }
                    if (!_agent.pathPending)
                    {
                        //pathList = _agent.path.corners.Select(corner => new Path(0, 0, corner, 0))
                        //    .ToList(); 
                        //Si no tiene mas de 1 esquina, es un parcial que no puedo usar
                        if (pathList.Count <= 1)
                        {
                            timeoutFrames--;
                            continue;
                        }
                        //Set arrived status
                        if (_agent.path.status == NavMeshPathStatus.PathComplete)
                        {
                            if(Vector3.Distance(pathList[^1].realPos,goal) < DIST_THRESHOLD)
                                pathList[^1].arrived = true;
                        }
                        else
                        { 
                            if(_agent.path.status == NavMeshPathStatus.PathInvalid)
                                throw new Exception("Current path is invalid");
                        }
                        Debug.Log("[CALCULATE-PATH] "+name+": "
                                  +(_agent.path.status == NavMeshPathStatus.PathComplete ? "Arrived " : "Partially arrived ")
                                  +" GetPath with pathList count = "+pathList.Count);
                        pathList.RemoveAt(0);
                        return pathList;
                    }
                }

                if (pathList.Count <= 1)
                    throw new Exception("Timeout reach for path calculation");
                Debug.Log("[CALCULATE-PATH] "+name+": Left GetPath with pathList count = "+pathList.Count);
                pathList.RemoveAt(0);
                return pathList;
                //Delete first corner (it's the position of the npc)

            }
            catch (OperationCanceledException e)
            {
                Debug.LogException(e);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            } 
            //_agent.enabled = false;
            //_obstacle.enabled = true;
            return pathList;
        }

        public void BeAlerted(Vector3 position, bool duringCombat = false)
        {
            if(duringCombat)Debug.Log("[ALERTED] Enemy alerted to position "+position);
            if ((_enemyState != EnemyState.ENGAGE) ^ duringCombat)
            {
                OnTakeDamage(new DamageInfo(0,position));
            }
            else
            {
                Debug.Log("[ALERTED] Denied. duringCombat : " + duringCombat);
            }
        }

        public bool CurrentlyFollowingPath()
        {
            return _activeRoute is not null;
        } 

        public void ChangeWeapon(int index)
        {
            WpnHandler.SwitchWeapon(index);
            _inputs.sprint = true;
        }

        public void WalkToPoint(Vector2 point)
        {
            _walkToPoint = point; 
        }
    }
}