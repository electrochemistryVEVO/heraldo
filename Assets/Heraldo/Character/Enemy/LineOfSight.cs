using System;
using System.Collections.Generic;
using UnityEngine;

namespace Heraldo.Character.Enemy
{
    public class LineOfSight : MonoBehaviour
    {
        private List<Collider> _collidersOnProbation;
        private List<WeakReference<GameObject>> _currentTargets; //Ordenado por prioridad (cual esta mas cerca)

        public EnemyController npc;
        //public GameObject ParentEntity;
        private void OnTriggerStay(Collider other)
        {
            //Debug.Log(other.gameObject.name);
            //transform.parent.parent.SendMessage("TriggerStay",other);
            TriggerStay(other);
        }

        private void OnTriggerExit(Collider other)
        {
            //transform.parent.parent.SendMessage("TriggerExit",other);
            TriggerExit(other);
        }
        
        private void OnTriggerEnter(Collider other)
        {
            TriggerEnter(other);
            //transform.parent.parent.SendMessage("TriggerEnter",other);
        }
        
        private bool CheckIfInLOS(Collider other)
        {
            Vector3 pos = transform.parent.position;
            Vector3 dist = other.gameObject.transform.position - pos;
            RaycastHit hit;
            bool wasHit = Physics.Raycast(pos,dist.normalized, out hit,dist.magnitude,LayerMask.GetMask("Default"));
            return !wasHit || hit.collider.gameObject == other.gameObject;
        }

        private void Awake()
        {
            _currentTargets = new();
        }

        private void Update()
        {
            DeallocateDead();
        }

        public void TriggerExit(Collider other)
        {
            if (!this.enabled) return;
            //var bufTargets = new List<WeakReference<GameObject>>(_currentTargets);
            lock (_currentTargets)
            {
                if (npc.isObjectEnemy(other.gameObject))
                {
                    foreach (var weakRef in _currentTargets)
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
            if (npc.isObjectEnemy(other.gameObject))
            {
                Debug.Log("Enemy spotted: "+other.gameObject.name);
                _currentTargets.Add(new WeakReference<GameObject>(other.gameObject));
            }
        }
        
        public void TriggerStay(Collider other)
        {
            if (!this.enabled) return;
            if (npc.isObjectEnemy(other.gameObject) && CheckIfInLOS(other))
            {
                npc.SpotPlayer(other.gameObject);
            }
            else TriggerExit(other); //Debe de haber una manera mas efectiva de hacer esto
        }
        
        private void DeallocateDead()
        {
            foreach (var weakRef in _currentTargets)
            {
                GameObject _target;
                bool hasTarget = weakRef.TryGetTarget(out _target);
                if (!hasTarget || _target ==null)
                { 
                    _currentTargets.Remove(weakRef);
                    
                } 
            }
            
            
            if (_currentTargets.Count <= 0 && npc.Enemy_State == EnemyState.ENGAGE)
            {
                Debug.Log("Does this ever get fired");
                npc.Enemy_State = EnemyState.HUNT;
                //SearchForPlayer();
            }
            else
            {
                Debug.Log("Numero de objetivos: "+_currentTargets.Count);
            }
            //SearchForPlayer(other.gameObject);
        }
    }
}