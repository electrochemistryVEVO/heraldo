using System;
using System.Threading.Tasks;
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
        private LevelUIManager _uiManager;
        private ObjectiveManager _objectiveManager;

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
                Task.Run(() => JsonParser.WriteSaveData(new GameSaveData(LevelScene)));
                _uiManager.ChangeLoseCondition(LevelScene);
                _uiManager.FadeToBlack(2f);
            }
        }
    }
}