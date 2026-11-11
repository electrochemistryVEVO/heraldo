using System;
using System.Threading.Tasks;
using Heraldo.Character;
using Heraldo.Global;
using Heraldo.Global.Enums;
using Heraldo.Global.Structs;
using Heraldo.UI;
using UnityEngine;

namespace Heraldo.Level
{
    public class LevelTransition : MonoBehaviour
    {
        public ObjectiveID objectiveToFulfill;
        public string LevelScene;
        public bool EndGame = false;
        private LevelUIManager _uiManager;
        private ObjectiveManager _objectiveManager;
        public FPSController player;

        private void Awake()
        {
            _uiManager = GameObject.FindWithTag("UI Canvas").GetComponent<LevelUIManager>();
            _objectiveManager = GameObject.FindWithTag("ObjectiveManager")
                .GetComponent<ObjectiveManager>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && _objectiveManager.currentObjective.ID == objectiveToFulfill)
            {
                if (EndGame) _uiManager.EndGame();
                else _uiManager.NextLevel(player.returnSaveData(LevelScene));
            }
        }
    }
}