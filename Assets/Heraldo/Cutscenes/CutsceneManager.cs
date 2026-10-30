using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Heraldo.Character;
using Heraldo.Character.Enemy;
using Heraldo.Level;
using Heraldo.UI;
using Heraldo.VFX.Lighting;
using Unity.Cinemachine;
using UnityEngine;

namespace Heraldo.Cutscenes
{
    public class CutsceneManager : MonoBehaviour
    {
        public Cutscene currentCutscene;
        public CutsceneList cutsceneList;
        
        //Estos dos tienen procesos automaticos que deben de detenerse en las cutscenes
        public TroopController troop;
        public DayNightManager dayNight;
        public ObjectiveManager objectiveManager;
        public List<EnemyController> actorList;
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
        private Vector3[] _spotList;

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
                .Select(val => val.position)
                .ToArray(); 
            if (CutsceneMode)
            {
                troop.enabled = false;
                dayNight.enabled = false;
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
                case CutsceneActionType.CHARACTER_MOVE:
                {
                    EnemyController who = actorList[action.who];
                    Vector3 where = _spotList[action.where];
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
                    if (part.actor >= 0)
                    {
                        var actor = actorList[part.actor];
                        _uiCanvas.CutsceneText(actor.name,part.text); 
                    }
                    else _uiCanvas.CutsceneText();
                    return Task.Delay((int)MathF.Round(action.howMuch*1000));
                }
                case CutsceneActionType.PLAYER_ROTATE_TOWARDS:
                {
                    Awaitable.MainThreadAsync();
                    var where = _spotList[action.where];
                    return player.RotateTowards(where);
                }
            }
            return null;
        }

        private void Update()
        {
            /*if (CutsceneMode)
            {
                //Parts 
                currentCamera.Follow = _actorList[_currentPart];
                _uiCanvas.CutsceneText(currentCutscene.parts[_currentPart].subjectName,
                    currentCutscene.parts[_currentPart].text);
                //Actions
                if (_currentAction >= 0)
                {
                    CutsceneAction action = currentCutscene.actions[_currentAction];
                    Instantiate(action.spawn, action.where, Quaternion.identity);
                    _currentAction = -1;
                } 
            }
            else
            {
                if (_prevCameraTarget is null) return;
                currentCamera.Follow = _prevCameraTarget;
                _uiCanvas.CutsceneText();
                _prevCameraTarget = null;
            }*/
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

        public void PrepareCutscene(Cutscene cutscene)
        {
            currentCutscene = cutscene;
            StartCutscene();
        }
    }
}