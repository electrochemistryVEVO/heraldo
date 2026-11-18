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
        private const float NavMeshSearchDistance = 2f;

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

        private TaskCompletionSource<bool> _pressStart = new();

        public bool CutsceneMode = false;

        //Events
        public delegate void CutsceneEnded();

        public CutsceneEnded endCutscene;


        public void PressStart()
        {
            _pressStart.TrySetResult(true);
        }
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
                //player.Paused = true;
                //dayNight.enabled = false;
                objectiveManager.enabled = false;
                //_uiCanvas.gameObject.SetActive(false);
            }
        }
        

        private async void Start()
        {
            if (CutsceneMode)
            {
                _uiCanvas.CutsceneMode();
                player.ToggleInputs();
                await StartCutscene();
            }
        }

        private Task ProcessCutsceneAction(CutsceneAction action)
        {
            switch (action.type)
            {
                case CutsceneActionType.ENEMY_SPAWN:
                case CutsceneActionType.ALLY_SPAWN:
                {
                    var spot = _spotList[action.where];
                    var position = spot.position;
                    if (action.relativeToPlayer) position = player.transform.TransformPoint(position);
                    if (!action.what || !action.what.TryGetComponent<NavMeshAgent>(out var prefabAgent) ||
                        !action.what.TryGetComponent<EnemyController>(out _))
                        throw new InvalidOperationException("El prefab requiere NavMeshAgent y EnemyController en su raíz.");

                    var destination = FindNavMeshPosition(prefabAgent, spot, position);
                    var instance = Instantiate(action.what, destination + Vector3.up * prefabAgent.baseOffset,
                        action.what.transform.rotation);
                    var agent = instance.GetComponent<NavMeshAgent>();
                    if (!agent.isOnNavMesh)
                    {
                        Destroy(instance);
                        throw new InvalidOperationException(
                            $"'{action.what.name}' no se vinculó al NavMesh en '{spot.name}' ({destination}).");
                    }

                    actorList.Add(instance.GetComponent<EnemyController>());
                    return Task.CompletedTask;
                }
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
                    var part = currentCutscene.parts[_speechIndex++]; 
                    _uiCanvas.CutsceneText(part.actor,part.text); 
                    return Task.Delay((int)MathF.Round(action.howMuch*1000));
                }
                case CutsceneActionType.PLAYER_ROTATE_TOWARDS:
                {
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
                    player.Warp(spot.position,spot.rotation);
                    return Task.CompletedTask;
                }
                case CutsceneActionType.CHARACTER_REPOSITION:
                {
                    var who = actorList[action.who];
                    var spot = _spotList[action.where];
                    var agent = who.GetComponent<NavMeshAgent>();
                    var position = spot.position;
                    if (action.relativeToPlayer) position = player.transform.TransformPoint(position);
                    var destination = FindNavMeshPosition(agent, spot, position);
                    if (!agent.isActiveAndEnabled || !agent.Warp(destination))
                        throw new InvalidOperationException(
                            $"No se pudo trasladar a '{who.name}' a '{spot.name}' ({destination}).");
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
                case CutsceneActionType.SKIP_TO_LEVEL:
                {
                    _uiCanvas.SkipToLevel(player.returnSaveData(action.text));
                    return Task.CompletedTask;
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
                    var where = _spotList[action.where];
                    things.Add(Instantiate(action.what, where.position, action.what.transform.rotation));
                    return Task.CompletedTask;
                }
                case CutsceneActionType.PLAYER_HOLSTER_WEAPON:
                {
                    player.HolsterWeapon();
                    return Task.CompletedTask;
                }
                case CutsceneActionType.WAIT_FOR_INPUT:
                {
                    _pressStart = new();
                    return _pressStart.Task;
                }
                case CutsceneActionType.ACTIVATE_OBJECT:
                {
                    var thing = things[action.whatIndex];
                    thing.SetActive(true);
                    return Task.CompletedTask;
                }
                case CutsceneActionType.END_GAME:
                {
                    return _uiCanvas.EndGame();
                }
            }
            throw new NotSupportedException($"Acción de cutscene sin implementar: {action.type}");
        }

        private static Vector3 FindNavMeshPosition(NavMeshAgent agent, Transform spot, Vector3 position)
        {
            var filter = new NavMeshQueryFilter { agentTypeID = agent.agentTypeID, areaMask = agent.areaMask };
            if (NavMesh.SamplePosition(position, out var hit, NavMeshSearchDistance, filter))
                return hit.position;

            throw new InvalidOperationException(
                $"No hay NavMesh compatible a {NavMeshSearchDistance} m de '{spot.name}' ({position}) " +
                $"para '{agent.name}' (tipo {agent.agentTypeID}, máscara {agent.areaMask}).");
        }

        private Task StartAction(CutsceneAction action)
        {
            try
            {
                return ProcessCutsceneAction(action);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException(
                    $"Acción {action.type}, actor {action.who}, punto {action.where}, objeto {action.whatIndex}.", e);
            }
        }

        public async Task StartCutscene()
        {
            int? currentOrder = null;
            try
            {
                await Awaitable.MainThreadAsync();
                CutsceneMode = true; 
                _speechIndex = 0;
                foreach (var actions in currentCutscene.actions
                             .GroupBy(val => val.order)
                             .OrderBy(val => val.Key))
                {
                    await Awaitable.MainThreadAsync();
                    currentOrder = actions.Key;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    Debug.Log($"[Cutscene] {currentCutscene.name}, bloque {currentOrder}: " +
                              string.Join(", ", actions.Select(action => action.type)), this);
#endif
                    await Task.WhenAll(actions.Select(StartAction));
                }
                await Awaitable.MainThreadAsync();
                _uiCanvas.CutsceneText();
                FinishCutscene();
            }
            catch (Exception e)
            {
                await Awaitable.MainThreadAsync();
                if (!this) return;

                Debug.LogError($"[Cutscene] Escena {gameObject.scene.name}, cinemática {currentCutscene?.name}, " +
                               $"bloque {currentOrder}: {e}", this);
                await _uiCanvas.FadeFromBlack(0f);
                _uiCanvas.CutsceneText();
                FinishCutscene(completed: false);
            }
        }

        private void FinishCutscene(bool completed = true)
        {
            CutsceneMode = false;
            troop.enabled = true;
            dayNight.enabled = true;
            objectiveManager.enabled = true;
            _uiCanvas.CutsceneMode(false);
            player.ToggleInputs(true);
            if (completed && currentCutscene.ID == CutsceneID.INITIAL_CUTSCENE)
            {
                player.InitialSaveData();
                player.Paused = true;
                _uiCanvas.ShowHideTutorial();
            }
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
