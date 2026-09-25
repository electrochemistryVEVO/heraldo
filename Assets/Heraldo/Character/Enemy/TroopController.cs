using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Heraldo.Global.Structs;
using UnityEngine;

namespace Heraldo.Character.Enemy
{
    public enum TroopState
    {
        ATTACK,
        DEFEND,
        PATROL
    }
    public class TroopController : MonoBehaviour
    {
        public TroopState Troop_State = TroopState.PATROL;
        private List<EnemyController> _allyTroopers;
        public Transform AllyTroop;
        public Transform covers;
        private CoverBarricade[] _coverList;
        private CoverBarricade[] _barricadeList;
        private List<String> _occupiedSpots;
        public Transform barricades;
        public Transform EnemyTroop;
        private List<EnemyController> _enemyTroopers;
        
        //Respawning
        public float TrooperRespawnTime = 5f;
        public GameObject TrooperPrefab;
        public Transform TrooperRespawnSpot;
        private Queue<Action> respawnTaskQueue = new Queue<Action>();
        private float _respawnCount;
        
        
        //Delegates
        public delegate void TrooperDown(Guid nme);
        public TrooperDown trooperDown;

        private void Update()
        { 
            _respawnCount -= Time.deltaTime;
            if (_respawnCount < 0)
            {
                _respawnCount = TrooperRespawnTime;
                if (respawnTaskQueue.Count > 0)respawnTaskQueue.Dequeue()();
            } 
        }

        private void Awake()
        {
            //Get onready vars
            _allyTroopers = AllyTroop.GetComponentsInChildren<EnemyController>().ToList();
            _enemyTroopers = EnemyTroop.GetComponentsInChildren<EnemyController>().ToList();
            _coverList = covers.GetComponentsInChildren<CoverBarricade>();
            _barricadeList = barricades.GetComponentsInChildren<CoverBarricade>();
            _occupiedSpots = new();
            _respawnCount = TrooperRespawnTime;
            
            AssignPositions();
        }

        private Spot FindClosestSpot(EnemyController npc)
        {
            Vector3 pos = npc.transform.position;
            CoverBarricade[] coverArray = npc.IsAlly ? _barricadeList : _coverList;
            Spot[] spots = coverArray
                .Where((val) => val.HasFreeSpots())
                .Select((val)=>val.ClosestSpot(pos)).ToArray();
            if (spots.Length == 0) throw new Exception("No spots found");
            int indClosest = -1;
            int ind = -1;
            Spot result = spots.Aggregate(new Spot(Vector3.negativeInfinity,-1),
                (acum, val) =>
                {
                    ind++;
                    if((acum.position - pos).magnitude < (val.position - pos).magnitude) return acum;
                    indClosest = ind;
                    return val;
                });
            if(indClosest >= 0)
            {
                Debug.Log("[POSITION-CLOSEST] "+result.position);
                coverArray[indClosest].SetSpotOccupier(npc,result.index);
                TrooperDown cb = (nme) => { };
                cb = (nme) =>
                {
                    Debug.Log("[ENEMY-SPAWN] Enemy "+npc.ID+" is currently active");
                    if (nme != npc.ID) return;
                    Debug.Log("[ENEMY-SPAWN] Enemy "+npc.ID+" died. Preparing to respawn enemy");
                    trooperDown -= cb;
                    coverArray[indClosest].SetSpotOccupier(null,result.index);
                    //if (respawnTaskQueue.Count > 0) respawnTaskQueue.Peek()?.Wait(); 
                    respawnTaskQueue.Enqueue(new Action(() =>
                    {
                        //await Task.Delay(Mathf.RoundToInt(TrooperRespawnTime));
                        
                        var obj = GameObject.Instantiate(TrooperPrefab, EnemyTroop, false);
                        obj.transform.position = TrooperRespawnSpot.position;
                        obj.transform.rotation = TrooperRespawnSpot.rotation; 
                        //coverArray[indClosest].SetSpotOccupier(obj.GetComponent<EnemyController>(),result.index);
                        EnemyController _nme = obj.GetComponent<EnemyController>();
                        _nme.Troop = this;
                        //Debug.Log("[ENEMY-SPAWN] New enemy spawned. Entity Id: "+_nme.ID);
                        AssignPositionToTrooper(_nme,FindClosestSpot(_nme));
                    }));
                };
                trooperDown += cb;
                //Debug.Log("[IND-CLOSEST] "+indClosest);
                //Debug.Log("[ID-CLOSEST] " + coverArray[indClosest].GetCoverBarricadeId());
                //_occupiedSpots.Add(coverArray[indClosest].GetCoverBarricadeId());
            }
            return result;
        }

        private void AssignPositions()
        {
            List<EnemyController> troopers = _allyTroopers.Concat(_enemyTroopers).ToList();
            foreach (var trooper in troopers)
            {
                AssignPositionToTrooper(trooper,FindClosestSpot(trooper));
            } 
        }

        private void AssignPositionToTrooper(EnemyController trooper,Spot spot)
        {
            //Debug.Log("Closest spot selected: "+spot.position);
            trooper.FollowExternalRoutine(new Routine(new []{spot.position},false));
        }
        
    }
}