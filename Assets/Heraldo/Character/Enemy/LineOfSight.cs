using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Object = System.Object;

namespace Heraldo.Character.Enemy
{
    public class LineOfSight : MonoBehaviour
    {
        //Frames before enemy goes hunting
        private static readonly int MERCY_FRAMES = 60;
        private static readonly int SPOT_MERCY_FRAMES = 30;
        private int _mercyFrames = MERCY_FRAMES;
        private int _spotMercyFrames = SPOT_MERCY_FRAMES;
        
        private List<Collider> _collidersOnProbation;
        [DeallocateOnJobCompletion]
        private NativeArray<RaycastHit> _raycastHits;
        [DeallocateOnJobCompletion]
        private NativeArray<RaycastCommand> _commands;
        private JobHandle _raycastCheckHandle;
        private Object _raycastCheckLock = new Object();
        private Dictionary<GameObject, bool> _losCheckDict = new();
        
        private List<WeakReference<GameObject>> _currentTargets; //Ordenado por prioridad (cual esta mas cerca)

        public EnemyController npc;
        //public GameObject ParentEntity;
        private void OnTriggerStay(Collider other)
        {
            //Debug.Log(other.gameObject.name);
            //transform.parent.parent.SendMessage("TriggerStay",other);
            //if(other.name=="PlayerCapsule Variant")Debug.Log("Object "+other.name+" is still in LOS");
            TriggerStay(other);
        }

        private void OnTriggerExit(Collider other)
        {
            //Debug.Log("Object "+other.name+" left LOS");
            //transform.parent.parent.SendMessage("TriggerExit",other);
            TriggerExit(other);
        }
        
        private void OnTriggerEnter(Collider other)
        {
            //Debug.Log("Object "+other.name+" entered LOS");
            TriggerEnter(other);
            //transform.parent.parent.SendMessage("TriggerEnter",other);
        }
        
        private bool CheckIfInLOS(GameObject other)
        {
            Vector3 pos = transform.parent.position;
            Vector3 dist = other.transform.position - pos;
            RaycastHit hit;
            //bool wasHit = Physics.Raycast(pos,dist.normalized, out hit,dist.magnitude,LayerMask.GetMask("Default"));
            //return !wasHit || hit.collider.gameObject == other;
            bool result;
            bool isIn = _losCheckDict.TryGetValue(other, out result);
            //Debug.Log("[CHECK-IF-IN-LOS] Object is "+other.name+", Value is " + (isIn ? "present" : "not present"));
            if(isIn)
            {
                //Debug.Log("[CHECK-IF-IN-LOS] Object is "+other.name+", Result is " + (result ? "present" : "not present"));
                return result;
            }
            // lock (_currentTargets)
            // {
            //     _currentTargets.Add(new WeakReference<GameObject>(other));
            // };
            return false;
        }

        private void Awake()
        {
            _currentTargets = new();
        }

        private void Update()
        {
            //Update raycasts
            _spotMercyFrames--;
            if (_spotMercyFrames <= 0)
            {
                _spotMercyFrames = SPOT_MERCY_FRAMES;
                lock (_raycastCheckLock)
                {
                    if (_raycastCheckHandle.IsCompleted)
                    {
                        List<GameObject> copyTargets = new();
                        lock (_currentTargets)
                        {
                            int i = 0;
                            int count = _currentTargets.Count;
                            for (; i < count; i++)
                            {
                                var val = _currentTargets[i];
                                GameObject obj;
                                if (val.TryGetTarget(out obj) && obj != null) copyTargets.Add(obj);
                                else
                                {
                                    _currentTargets.RemoveAt(i);
                                    count--;
                                }
                            }
                            _currentTargets.ForEach(val =>
                            {
                                
                            });
                        }
                        _commands = new(copyTargets.Count, Allocator.TempJob);
                        _commands.CopyFrom(copyTargets
                            .Select(val => new RaycastCommand(transform.parent.position,
                                (val.transform.position - transform.parent.position).normalized, 
                                new QueryParameters(LayerMask.GetMask("Enemy","Ally","Default")),
                                (val.transform.position - transform.parent.position).magnitude)).ToArray());
                        _raycastHits = new(65, Allocator.TempJob);
                        _raycastCheckHandle = RaycastCommand.ScheduleBatch(_commands
                            , _raycastHits, 1, 1,default);
                        _raycastCheckHandle.Complete();
                        HandleRaycastCheck();
                    }
                }
            }
            //Change state to hunt automatically
            if (npc.Enemy_State == EnemyState.ENGAGE || npc.Enemy_State == EnemyState.ENGAGE_ROUTE
                || (npc.Enemy_State == EnemyState.PATROL && npc.HasOverdriveRoutine))
            {
                if (--_mercyFrames <= 0)
                {
                    _mercyFrames = MERCY_FRAMES;
                    //if(npc.Enemy_State != EnemyState.PATROL)npc.Enemy_State = EnemyState.HUNT;
                    npc.Enemy_State = EnemyState.HUNT;
                }
            }
        }

        private void HandleRaycastCheck()
        {
            try
            {
                //_raycastCheckHandle.Complete();
                _commands.Dispose();
                List<GameObject> targets = new();
                lock (_currentTargets)
                {
                    _currentTargets.ForEach(val =>
                    {
                        GameObject obj;
                        if (val.TryGetTarget(out obj)) targets.Add(obj);
                    });
                }
                lock (_losCheckDict)
                {
                    foreach (var raycastHit in _raycastHits)
                    {
                        if (raycastHit.collider is not null &&
                            raycastHit.collider.gameObject.layer == 
                            LayerMask.NameToLayer(npc.IsAlly ? "Enemy" : "Ally"))
                        {
                            _losCheckDict[raycastHit.collider.gameObject] = true;
                            targets.Remove(raycastHit.collider.gameObject);
                        }
                        //Cause of no objects found: raycastHit collider is always null
                    }

                    if(_raycastHits.IsCreated)_raycastHits.Dispose();
                    foreach (var target in targets)
                    {
                        _losCheckDict[target] = false;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                if(_commands.IsCreated)_commands.Dispose();
                if(_raycastHits.IsCreated)_raycastHits.Dispose();
            }
        }

        public void TriggerExit(Collider other)
        {
            if (!this.enabled) return;
            //var bufTargets = new List<WeakReference<GameObject>>(_currentTargets);
            lock (_currentTargets)
            {
                if (npc.isObjectEnemy(other.gameObject))
                {
                    int count = _currentTargets.Count;
                    for (int i=0;i<count;i++)
                    {
                        var weakRef = _currentTargets[i];
                        GameObject _target;
                        bool hasTarget = weakRef.TryGetTarget(out _target);
                        if (hasTarget && _target !=null)
                        {
                            if (_target == other.gameObject)
                            {
                                _currentTargets.Remove(weakRef);
                                break;
                            }
                        }
                        else
                        {
                            _currentTargets.Remove(weakRef);
                            count--;
                        }
                    }
                    //SearchForPlayer(other.transform.position);
                }
            }
        }
        
        public void TriggerEnter(Collider other)
        {
            if (!this.enabled) return;
            if (npc.isObjectEnemy(other.gameObject))
            {
                //Debug.Log("Enemy spotted: "+other.gameObject.name);
                lock (_currentTargets)
                { 
                    _currentTargets.Add(new WeakReference<GameObject>(other.gameObject));
                }
            }
        }
        
        public void TriggerStay(Collider other)
        {
            if (!this.enabled) return;
            if (npc.isObjectEnemy(other.gameObject) 
                && CheckIfInLOS(other.gameObject)
                && CheckIfClosestEnemy(other.gameObject)
                )
            {
                //Reset mercy frames
                _mercyFrames = MERCY_FRAMES;
                npc.SpotPlayer(other.gameObject);
            }
            //else TriggerExit(other); //Debe de haber una manera mas efectiva de hacer esto
        }
        
        public bool CheckIfClosestEnemy(GameObject player)
        { 
            //Check if this enemy is the closest
            lock (_currentTargets)
            {
                int count = _currentTargets.Count;
                for (int i=0;i<count;i++)
                {
                    var weakRef = _currentTargets[i];
                    GameObject closestEnemy;
                    bool hasClosestEnemy = weakRef.TryGetTarget(out closestEnemy);
                    if (hasClosestEnemy && closestEnemy != null)
                    {
                        if (closestEnemy == player) continue;
                        if ((transform.position - closestEnemy.transform.position).magnitude
                            <= (transform.position - player.transform.position).magnitude)
                            return false;
                    }
                    else
                    {
                        _currentTargets.RemoveAt(i);
                        count--;
                    }
                }
            }
            return true;
            //if(_fireTimeElapsed)StartCoroutine(AttackEnemy(player.transform.position));
        }
    }
}