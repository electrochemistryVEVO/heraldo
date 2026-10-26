using System;
using System.Collections.Generic;
using UnityEngine;

namespace Heraldo.Character.Enemy
{
    public class LineOfSight : MonoBehaviour
    {
        //Frames before enemy goes hunting
        static private int MERCY_FRAMES = 60;
        private int _mercyFrames = MERCY_FRAMES;
        
        private List<Collider> _collidersOnProbation;
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
            bool wasHit = Physics.Raycast(pos,dist.normalized, out hit,dist.magnitude,LayerMask.GetMask("Default"));
            return !wasHit || hit.collider.gameObject == other;
        }

        private void Awake()
        {
            _currentTargets = new();
        }

        private void Update()
        {
            //DeallocateDead();
            if (npc.Enemy_State == EnemyState.ENGAGE)
            {
                if (--_mercyFrames <= 0)
                {
                    _mercyFrames = MERCY_FRAMES;
                    npc.Enemy_State = EnemyState.HUNT;
                }
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
                    if (_currentTargets.Count <= 0  && npc.Enemy_State == EnemyState.ENGAGE)
                        npc.Enemy_State = EnemyState.HUNT;
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