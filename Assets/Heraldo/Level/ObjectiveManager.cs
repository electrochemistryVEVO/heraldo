using System;
using System.Collections.Generic;
using Heraldo.Flavor;
using Heraldo.Global.Enums;
using Heraldo.Global.SO;
using Heraldo.Global.Structs;
using Heraldo.Level;
using Heraldo.UI;
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

        private void Awake()
        {
            if(currentObjective.ID == ObjectiveID.NO_OBJECTIVE)
                ChangeObjective(objectiveMap.Objectives[0].ID);
            
        } 

        public void ChangeObjective(ObjectiveID id, Interactable interactable = null)
        {
            Debug.Log("[CHANGE-OBJECTIVE] Objetivo cambiado a: "+id);
            currentObjective = objectiveMap.FindObjective(id);
            uiManager.ObjectiveText(currentObjective.Title);
            positionBillboard.position = currentObjective.Position;
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
            ChangeObjective(currentObjective.nextObjectiveId);
        }
    }
}