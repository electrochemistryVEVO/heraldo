using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Heraldo.Character;
using Heraldo.Character.Enemy;
using Heraldo.Global;
using Heraldo.Global.Enums;
using Heraldo.Level;
using Heraldo.UI;
using Heraldo.VFX.Lighting;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.AI;

namespace Heraldo.Cutscenes
{
    public class CutsceneManager : MonoBehaviour
    {
        public Cutscene currentCutscene;
        
        //Estos dos tienen procesos automaticos que deben de detenerse en las cutscenes
        public TroopController troop;
        public DayNightManager dayNight;
        public ObjectiveManager objectiveManager;
        public List<EnemyController> actorList;
        public List<GameObject> things;
        public Transform spots;
        public FPSController player;

        private int _speechIndex = 0;
        private int _currentPart;
        private int _currentAction = -1; 
        private Transform _prevCameraTarget;
        public CinemachineVirtualCameraBase currentCamera;
        private CinemachineThirdPersonFollow _currentCameraTransposer; 
        private LevelUIManager _uiCanvas;
        private FPSController _player;
        private Transform[] _spotList;

        public bool CutsceneMode = false;

        //Events
        public delegate void CutsceneEnded();

        public CutsceneEnded endCutscene;

        private void Awake()
        {
            _currentCameraTransposer = (CinemachineThirdPersonFollow)currentCamera.GetCinemachineComponent(CinemachineCore.Stage.Body);
            _uiCanvas = GameObject.FindWithTag("UI Canvas").GetComponent<LevelUIManager>();
            _spotList = spots.GetComponentsInChildren<Transform>()
                .Where(val => val.gameObject != spots.gameObject)
                .ToArray();
            CutsceneMode = !JsonParser.SaveFileExists();
            if (CutsceneMode)
            {
                troop.enabled = false;
                //dayNight.enabled = false;
                objectiveManager.enabled = false;
                //_uiCanvas.gameObject.SetActive(false);
            }
        }
        

        private void Start()
        {
            if (CutsceneMode)
            {
                _uiCanvas.CutsceneMode();
                player.ToggleInputs();
                StartCutscene().RunSynchronously();
            }
        }

        private Task ProcessCutsceneAction(CutsceneAction action)
        {
            switch (action.type)
            {
                case CutsceneActionType.ENEMY_SPAWN:
                    Awaitable.MainThreadAsync();
                    GameObject nme = Instantiate(action.what);
                    var _where = _spotList[action.where].position;
                    Debug.Log("[ENEMY-SPAWN] Where = "+_where);
                    if (action.relativeToPlayer) _where = player.transform.TransformPoint(_where);
                    nme.GetComponent<NavMeshAgent>().Warp(_where);
                    actorList.Add(nme.GetComponent<EnemyController>());
                    return Task.CompletedTask;
                case CutsceneActionType.CHARACTER_MOVE:
                {
                    EnemyController who = actorList[action.who];
                    Vector3 where = _spotList[action.where].position;
                    if (action.relativeToPlayer) where = player.transform.TransformPoint(where);
                    return who.WalkToPoint(new Vector2(where.x, where.z));
                }
                case CutsceneActionType.WAIT:
                {
                    return Task.Delay((int)MathF.Round(action.howMuch*1000));
                }
                case CutsceneActionType.ADVANCE_SPEECH:
                {
                    Awaitable.MainThreadAsync();
                    var part = currentCutscene.parts[_speechIndex++]; 
                    _uiCanvas.CutsceneText(part.actor,part.text); 
                    return Task.Delay((int)MathF.Round(action.howMuch*1000));
                }
                case CutsceneActionType.PLAYER_ROTATE_TOWARDS:
                {
                    Awaitable.MainThreadAsync();
                    var where = _spotList[action.where].position;
                    if (action.relativeToPlayer) where = player.transform.TransformPoint(where);
                    return player.RotateTowards(where,action.howMuch);
                }
                case CutsceneActionType.FADE_TO_BLACK:
                {
                    return _uiCanvas.FadeToBlack(action.howMuch);
                }
                case CutsceneActionType.FADE_FROM_BLACK:
                {
                    return _uiCanvas.FadeFromBlack(action.howMuch);
                }
                case CutsceneActionType.CHARACTER_SPECIAL_ANIMATION:
                {
                    var who = actorList[action.who];
                    who.StartSpecialAnim((SpecialAnims)action.whatIndex);
                    return Task.CompletedTask;
                }
                case CutsceneActionType.PLAYER_REPOSITION:
                {
                    var spot = _spotList[action.where];
                    player.transform.position = spot.position;
                    player.transform.rotation = spot.rotation;
                    return Task.CompletedTask;
                }
                case CutsceneActionType.CHARACTER_REPOSITION:
                {
                    var who = actorList[action.who];
                    var where = _spotList[action.where].position;
                    who.GetComponent<NavMeshAgent>().Warp(where);
                    return Task.CompletedTask;
                }
                case CutsceneActionType.DESTROY_OBJECT:
                {
                    var what = things[action.whatIndex];
                    what.SetActive(false);
                    things[action.whatIndex] = null;
                    return Task.CompletedTask;
                }
                case CutsceneActionType.NEXT_LEVEL:
                {
                    return _uiCanvas.NextLevel(player.returnSaveData(action.text));
                }
                case CutsceneActionType.WAKE_ENEMIES:
                {
                    troop.WakeEnemies();
                    return Task.CompletedTask;
                }
                case CutsceneActionType.CHARACTER_EQUIP_WEAPON:
                {
                    var who = actorList[action.who];
                    who.ChangeWeapon(action.whatIndex);
                    return Task.CompletedTask;
                }
                case CutsceneActionType.SPAWN_OBJECT:
                {
                    var what = Instantiate(action.what);
                    things.Add(Instantiate(what));
                    var where = _spotList[action.where];
                    what.transform.position = where.position;
                    return Task.CompletedTask;
                }
            }
            return null;
        }

        public async Task StartCutscene()
        {
            try
            {
                CutsceneMode = true; 
                _speechIndex = 0;
                int i = 0;
                foreach (var actions in currentCutscene.actions
                             .GroupBy(val => val.order)
                             .OrderBy(val => val.Key))
                {
                    //CutscenePart part = currentCutscene.parts.Find(val => val.order == i); 
                    //if(!part.HasValue) _uiCanvas.CutsceneText();
                    //else _uiCanvas.CutsceneText(part.Value.subjectName,part.Value.text);
                    await Task.WhenAll(actions.Select(ProcessCutsceneAction));
                    i++;
                }
                await Awaitable.MainThreadAsync();
                _uiCanvas.CutsceneText();
                FinishCutscene();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private void FinishCutscene()
        {
            CutsceneMode = false;
            troop.enabled = true;
            dayNight.enabled = true;
            objectiveManager.enabled = true;
            _uiCanvas.CutsceneMode(false);
            player.ToggleInputs(true);
        }

        public void OnDrawGizmos()
        {
            foreach (var positions in spots.GetComponentsInChildren<Transform>())
            {
                Gizmos.DrawWireSphere(positions.position,5f);
            }
        }

        public Task PrepareCutscene(Cutscene cutscene)
        {
            _uiCanvas.CutsceneMode();
            player.ToggleInputs();
            currentCutscene = cutscene;
            return StartCutscene();
        }
    }
}