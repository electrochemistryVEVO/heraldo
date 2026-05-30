using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Heraldo.Character.Pathfinding;
using StarterAssets;
using Unity.VisualScripting;
using UnityEngine;

namespace Heraldo.Character
{
    enum EnemyState
    {
        ALIVE,
        PATROL,
        HUNT,
        ENGAGE,
        DEAD
    }
    public class EnemyController : MonoBehaviour
    {
        public int MaxHealth = 100;
        public bool IsAlly = false;
        public Transform RotatingObject;
        
        private int _health;
        private StarterAssetsInputs _inputs;
        private AStarGrid _aStarGrid;
        private Vector2? _walkToPoint = null;
        private Vector3? _aimAtPoint = null;
        private GameObject _enemyRoutines; 
        private List<WeakReference<GameObject>> _currentTargets; //Ordenado por prioridad (cual esta mas cerca)
        
        //Children
        public BoxCollider VisionCone;
        
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

        private EnemyState Enemy_State
        {
            get => _enemyState;
            set
            {
                _enemyState = value;
                if(_enemyState == EnemyState.DEAD) OnDied();
                
            }
        }

        
        private bool isObjectEnemy(GameObject obj)
        {
            return IsAlly ? obj.CompareTag("Enemy Character") : (obj.CompareTag("Player") || obj.CompareTag("Ally"));
        }

        void Start()
        {
            Health = MaxHealth;
            Enemy_State = EnemyState.ALIVE;
            _inputs = GetComponent<StarterAssetsInputs>();
            _walkToPoint = Vector2.zero;
            _aStarGrid = GetComponent<AStarGrid>();
            _pathNodeReached = new TaskCompletionSource<bool>();
            _enemyRoutines = GameObject.FindWithTag(IsAlly ? "Ally Routines" :"Enemy Routines");
            _currentTargets = new List<WeakReference<GameObject>>();

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
            FollowRoutine(minRoutine);
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
                if (move.magnitude <= 0.2)
                {
                    _inputs.move = Vector2.zero;
                    _pathNodeReached.TrySetResult(true);
                }
                else
                {
                    Vector3 _move = transform.InverseTransformDirection(new Vector3(move.x,0,move.y)/ move.magnitude);
                    _inputs.move = new Vector2(_move.x, _move.z);
                }
            }
            else _inputs.move = Vector2.zero;
            //Apuntar a puntos
            if (_aimAtPoint != null)
            {
                transform.LookAt(new Vector3(_aimAtPoint.Value.x,transform.position.y,_aimAtPoint.Value.z),Vector3.up);
            }
        }

        public void TriggerStay(Collider other)
        {
            //TODO: Check with raycast if there's nothing solid between enemy and player
            if (isObjectEnemy(other.gameObject))
            {
                SpotPlayer(other.gameObject);
            }
        }

        public void TriggerExit(Collider other)
        {
            //var bufTargets = new List<WeakReference<GameObject>>(_currentTargets);
            if (isObjectEnemy(other.gameObject))
            {
                foreach (var weakRef in _currentTargets)
                {
                    GameObject _target;
                    bool hasTarget = weakRef.TryGetTarget(out _target);
                    if (hasTarget)
                    {
                        if (_target == other.gameObject)
                        { 
                            _currentTargets.Remove(weakRef);
                            break;
                        }
                    }
                }
                if(_currentTargets.Count<=0)
                    SearchForPlayer(other.gameObject);
            }
        }
        
        public void TriggerEnter(Collider other)
        {
            //Debug.Log(other.name);
            if (isObjectEnemy(other.gameObject))
            {
                Debug.Log("Enemy spotted: "+other.gameObject.name);
                _currentTargets.Add(new WeakReference<GameObject>(other.gameObject));
            }
        }

        async Task FollowRoutine(Transform routine)
        {
            
            Transform[] steps = routine.gameObject.GetComponentsInChildren<Transform>();
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
                GameObject point = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                point.transform.position = new Vector3(_walkToPoint.Value.x,transform.position.y,_walkToPoint.Value.y);
                bool reached = await _pathNodeReached.Task;
                _pathNodeReached = new TaskCompletionSource<bool>();
                Debug.Log(reached);
                if (!reached) return;
                path = path.next;
                i++;
            }
        }

        private void SpotPlayer(GameObject player)
        {
            //Interrumpir
            //Debug.Log("Player spotted");
            _pathNodeReached.TrySetResult(false);
            _walkToPoint = null;
            //TODO: Fire at player
            //Check if this enemy is the closest
            foreach (var weakRef in _currentTargets)
            {
                GameObject closestEnemy;
                bool hasClosestEnemy = weakRef.TryGetTarget(out closestEnemy);
                if (hasClosestEnemy)
                {
                    if (closestEnemy == player) continue;
                    if ((transform.position - closestEnemy.transform.position).magnitude
                        < (transform.position - player.transform.position).magnitude)
                        return;
                }
            }
            Enemy_State = EnemyState.ENGAGE;
            _aimAtPoint = player.transform.position;
        }

        private async Task SearchForPlayer(GameObject player)
        {
            Debug.Log("Searching for player");
            //Interrumpir
            _pathNodeReached.TrySetResult(false);
            _walkToPoint = null;
            //Trace path to last known position of player
            if(_aimAtPoint!=null)
            {
                Enemy_State = EnemyState.HUNT;
                _pathNodeReached = new TaskCompletionSource<bool>();
                await CalculatePath(player.transform.position); 
            } 
            Debug.Log("Player lost");
            Enemy_State = EnemyState.PATROL;
        }

        public void OnTakeDamage(int damage)
        {
            if (_enemyState != EnemyState.DEAD)
            {
                Health = Math.Max(Health-damage,0);
                Debug.Log("Health: " + Health);
            }
        }

        void WalkToPoint(Vector2 point)
        {
            _walkToPoint = point; 
        }

        void OnDied()
        {
            Debug.Log("Dead");
        }
    }
}