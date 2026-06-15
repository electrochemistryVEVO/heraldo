using System;
using System.Linq;
using System.Threading;
using Heraldo.UI;
using Unity.Cinemachine;
using UnityEngine;

namespace Heraldo.Cutscenes
{
    public class CutsceneManager : MonoBehaviour
    {
        public Cutscene currentCutscene;
        private int _currentPart;
        private int _currentAction = -1;
        private Transform[] _actorList;
        private Transform _prevCameraTarget;
        public CinemachineVirtualCameraBase currentCamera;
        private CinemachineThirdPersonFollow _currentCameraTransposer;
        private bool _cutsceneMode = false;
        private LevelUIManager _uiCanvas;

        public bool CutsceneMode
        {
            get => _cutsceneMode;
            set
            {
                _cutsceneMode = value;
                if (value == false) endCutscene();
            }
        }

        //Events
        public delegate void CutsceneEnded();

        public CutsceneEnded endCutscene;

        private void Awake()
        {
            _currentCameraTransposer = (CinemachineThirdPersonFollow)currentCamera.GetCinemachineComponent(CinemachineCore.Stage.Body);
            _uiCanvas = GameObject.FindWithTag("UI Canvas").GetComponent<LevelUIManager>();
        }

        private void Update()
        {
            if (CutsceneMode)
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
            }
        }

        public void StartCutscene()
        {
            _actorList = currentCutscene.parts.Select(x => transform.Find(x.author)).ToArray();
;           CutsceneMode = true;
            _currentCameraTransposer.ShoulderOffset = Vector3.back * 5;
            Thread t1 = new Thread(() =>
            {
                for (_currentPart=0; _currentPart < currentCutscene.parts.Count; _currentPart++)
                {
                    Debug.Log("Switching to actor "+_currentPart);
                    for (int i = 0; i < currentCutscene.actions.Count; i++)
                    {
                        if (currentCutscene.actions[i].order == _currentPart)
                        {
                            _currentAction = i;
                            break;
                        }
                    }
                    //currentCamera.LookAt = currentCutscene.Stage.transform.Find(
                    //    currentCutscene.parts[_currentPart].author);
                    Thread.Sleep(Mathf.RoundToInt(currentCutscene.parts[_currentPart].time*1000));
                    //Setear texto en la interfaz
                }

                _currentCameraTransposer.ShoulderOffset = Vector3.zero;
                CutsceneMode = false;
            });
            _prevCameraTarget = currentCamera.Follow;
            t1.Start();
        }

        public void PrepareCutscene(Cutscene cutscene)
        {
            currentCutscene = cutscene;
            StartCutscene();
        }
    }
}