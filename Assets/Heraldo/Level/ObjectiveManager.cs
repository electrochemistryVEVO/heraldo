using System;
using System.Collections.Generic;
using Heraldo.Flavor;
using Heraldo.Global.Enums;
using Heraldo.Global.SO;
using Heraldo.Global.Structs;
using Heraldo.Level;
using Heraldo.UI;
using Heraldo.UI.Components;
using Heraldo.VFX.Lighting;
using NUnit.Framework;
using UnityEngine;

namespace Heraldo.Level
{
    public class ObjectiveManager : MonoBehaviour
    {
        public Transform positionBillboard;
        public Objective currentObjective;
        public ObjectiveMap objectiveMap;
        public List<ObjectiveZone> objectiveZones;
        private ObjectiveZone _currentZone = null;
        public LevelUIManager uiManager;
        public DayNightManager dayNightManager; 
        private bool _firstChange;

        private void Awake()
        {
            _firstChange = false;
            positionBillboard.gameObject.SetActive(false);
        }

        private void Start()
        {
            positionBillboard.gameObject.SetActive(true);
            if(currentObjective.ID == ObjectiveID.NO_OBJECTIVE)
            {
                currentObjective = objectiveMap.Objectives[0];
                uiManager.SetMarkerPosition(currentObjective.HasToInteract
                    ? MarkerType.OBJECTIVE_INTERACT : MarkerType.OBJECTIVE,currentObjective.Position);
                uiManager.InitObjective(currentObjective);
            }
        } 

        public void ChangeObjective(ObjectiveID id, bool completed = false)
        {
            Debug.Log("[CHANGE-OBJECTIVE] Objetivo cambiado a: "+id);
            currentObjective = objectiveMap.FindObjective(id);
            if(completed)uiManager.ShowCompleteObjective(currentObjective);
            else
            {
                if (_firstChange)
                {
                    uiManager.InitObjective(currentObjective);
                    _firstChange = true;
                }
                else uiManager.ObjectiveText(currentObjective);
            }
            positionBillboard.position = currentObjective.Position;
            uiManager.SetMarkerPosition(currentObjective.HasToInteract
            ? MarkerType.OBJECTIVE_INTERACT : MarkerType.OBJECTIVE,currentObjective.Position);
            _currentZone = objectiveZones.Find(val => val.objectiveId == currentObjective.ID);
        }

        private void Update()
        {
            //Billboard
            positionBillboard.position = currentObjective.Position;
            //Zone
            if (_currentZone is not null)
            {
                if(_currentZone.playerOutsideZone) dayNightManager.StopClock();
                else dayNightManager.ResumeClock();
            }
        }

        public void CompleteObjective()
        {
            Debug.Log("[CHANGE-OBJECTIVE] Objetivo completado - "+currentObjective.ID);
            ChangeObjective(currentObjective.nextObjectiveId,true);
        }
    }
}