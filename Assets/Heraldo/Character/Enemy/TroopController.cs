using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Heraldo.Character.Pathfinding;
using Heraldo.Global.Structs;
using Newtonsoft.Json;
using OpenCover.Framework.Model;
using UnityEngine;
using UnityEngine.AI;
using File = System.IO.File;

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
        static readonly float TIME_BETWEEN_ENEMY_PATHFINDING = 0.25f;
        private static readonly int ENEMY_PATHFINDING_BATCH_SIZE = 50;
        
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
        public List<Transform> TrooperRespawnSpots;
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

        private Vector3 FindClosestRespawn(Vector3 vec)
        {
            return TrooperRespawnSpots.Aggregate(Vector3.negativeInfinity, (acum, val) =>
                (acum - vec).magnitude > (val.position - vec).magnitude ? val.position : acum);
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
            CoverBarricade closest = null;
            Spot result = spots.Aggregate(new Spot(null,Vector3.negativeInfinity,-1),
                (acum, val) =>
                {
                    if((acum.position - pos).magnitude < (val.position - pos).magnitude) return acum;
                    closest = val.coverBarricade;
                    return val;
                }); 
            if(closest is not null)
            {
                //Debug.Log("[POSITION-CLOSEST] "+closest.GetFreeSpotId(result.index));
                closest.SetSpotOccupier(npc,result.index);
                TrooperDown cb = (nme) => { };
                cb = (nme) =>
                {
                    Vector3 pos = npc.transform.position;
                    //Debug.Log("[ENEMY-SPAWN] Enemy "+npc.ID+" is currently active");
                    if (nme != npc.ID) return;
                    //Debug.Log("[ENEMY-SPAWN] Enemy "+npc.ID+" died. Preparing to respawn enemy");
                    trooperDown -= cb;
                    closest.SetSpotOccupier(null,result.index);
                    _enemyTroopers.Remove(npc);
                    //if (respawnTaskQueue.Count > 0) respawnTaskQueue.Peek()?.Wait(); 
                    respawnTaskQueue.Enqueue(new Action(() =>
                    {
                        //await Task.Delay(Mathf.RoundToInt(TrooperRespawnTime));
                        
                        var obj = GameObject.Instantiate(TrooperPrefab, EnemyTroop, false);
                        //Nunca codifiques mas en tu vida
                        obj.GetComponent<NavMeshAgent>().Warp(FindClosestRespawn(pos));
                        //obj.transform.rotation = TrooperRespawnSpot.rotation; 
                        //closest.SetSpotOccupier(obj.GetComponent<EnemyController>(),result.index);
                        EnemyController _nme = obj.GetComponent<EnemyController>();
                        _nme.Troop = this;
                        //Debug.Log("[ENEMY-SPAWN] New enemy spawned. Entity Id: "+_nme.ID);
                        AssignPositionToTrooper(_nme,FindClosestSpot(_nme));
                    }));
                };
                trooperDown += cb;
                //Debug.Log("[IND-CLOSEST] "+indClosest);
                //Debug.Log("[ID-CLOSEST] " + closest.GetCoverBarricadeId());
                //_occupiedSpots.Add(closest.GetCoverBarricadeId());
            }
            return result;
        }

        IEnumerator WriteToJson(List<EnemyController> troopers)
        {
            yield return new WaitForSeconds(5);
            Debug.Log("Writing paths to JSON...");
            Dictionary<string, List<Path>> paths = new();
            foreach (var trooper in troopers)
            {
                trooper.CapturePath(paths);
            }
            File.WriteAllText(
                "pathCache.json",
                JsonConvert.SerializeObject(troopers,
                    Newtonsoft.Json.Formatting.Indented));
            Debug.Log("Paths written");
        }

        private void AssignPositions()
        {
            List<EnemyController> troopers = _allyTroopers.Concat(_enemyTroopers).ToList();
            foreach (var trooper in troopers)
            {
                Spot spot = FindClosestSpot(trooper);
                AssignPositionToTrooper(trooper,spot);
            }
            
            //StartCoroutine(WriteToJson(troopers));
        }

        private void AssignPositionToTrooper(EnemyController trooper,Spot spot)
        {
            //Debug.Log("Closest spot selected: "+spot.position)
            trooper.FollowExternalRoutine(new Routine(new []{spot.position},false));
        }

        private IEnumerator _ChangeWeapons(int index)
        {
            int i = 0;
            foreach (var enemyTrooper in _enemyTroopers)
            {
                enemyTrooper.ChangeWeapon(index);
                i++;
                if(i%ENEMY_PATHFINDING_BATCH_SIZE==0)
                {
                    yield return new WaitForSeconds(TIME_BETWEEN_ENEMY_PATHFINDING);
                }
            }
        }

        public void ChangeWeapons(int index)
        {
            StartCoroutine(_ChangeWeapons(index));
        }
        
    }
}