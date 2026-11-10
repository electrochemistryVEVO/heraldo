using System;
using System.Collections.Generic;
using System.Linq;
using Heraldo.Global.Enums;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;

namespace Heraldo.Character.Enemy
{
    public struct Spot
    {
        public Vector3 position;

        public int index;
        //public bool taken; 
        [CanBeNull] public EnemyController occupier;
        [CanBeNull] public CoverBarricade coverBarricade;

        public Spot(CoverBarricade coverBarricade, Vector3 position,int index)
        {
            this.position = position;
            this.index = index;
            //this.taken = false;
            this.occupier = null;
            this.coverBarricade = coverBarricade;
        }
    }
    public class CoverBarricade : MonoBehaviour
    {
        public Transform spots;
        public BattlePhase Battle_Phase = BattlePhase.MORRO_ONE;
        public bool IsAlly;
        private Spot[] _spotPositions;
        

        private void Awake()
        {
            List<Transform> positions = spots.GetComponentsInChildren<Transform>().ToList();
            positions.Remove(spots);
            _spotPositions = positions.Select((val,index) => new Spot(this,val.position,index)).ToArray();
        }

        public string GetCoverBarricadeId()
        {
            return (IsAlly ? "Barricade" : "Cover") + transform.GetSiblingIndex();
        }

        public string GetFreeSpotId(int ind)
        {
            return (IsAlly ? "Barricade" : "Cover") + transform.GetSiblingIndex() + ":" + ind; 
        }

        public bool HasFreeSpots()
        {
            return _spotPositions.Any((val) => val.occupier.IsUnityNull());
        }

        public void VacateSpots()
        {
            for (int i=0;i<_spotPositions.Length;i++)
            {
                _spotPositions[i].occupier = null;
            }
        }

        public bool IsInPhase(BattlePhase phase)
        {
            //If it's enemy, it has to be higher
            //If it's ally, it has to be higher
            return Battle_Phase >= phase;
        }

        public void SetSpotOccupier(EnemyController npc, int index)
        {
            //Debug.Log("[SET-SPOT-OCCUPIER] "+GetFreeSpotId(index));
            //Debug.Log("[SET-SPOT-OCCUPIER] Is npc null? " + (npc.IsUnityNull() ? "YES" : "NO"));
           // Debug.Log("[SET-SPOT-OCCUPIER] Was "+GetFreeSpotId(index)+" null? "+ (_spotPositions[index].occupier.IsUnityNull()?"YES":"NO")) ;
            _spotPositions[index].occupier = npc;
        }

        public void OnDrawGizmos()
        {
            foreach (var child in spots.GetComponentsInChildren<Transform>())
            {
                Gizmos.DrawCube(child.position,new Vector3(1f,4f,1f));
            }
        }

        public Spot ClosestSpot(Vector3 point)
        { 
            int _index = -1;
            int i = -1;
            Vector3 result = _spotPositions.Aggregate(Vector3.negativeInfinity,
                (acum, val) =>
                {
                    i++;
                    if (!val.occupier.IsUnityNull() 
                        || !((acum - point).magnitude > (val.position - point).magnitude)) return acum;
                    _index = i;
                    return val.position;
                }
                );
            if (_index<0) throw new Exception("No existen puntos disponibles en este cover");
            //Debug.Log("[CLOSEST-SPOT] "+GetFreeSpotId(_index));
            //Debug.Log("[CLOSEST-SPOT] Is "+ GetFreeSpotId(_index) +" null? "+(_spotPositions[_index].occupier.IsUnityNull()?"YES":"NO"));
            //index = _index;
            return _spotPositions[_index];
        }
    }
}