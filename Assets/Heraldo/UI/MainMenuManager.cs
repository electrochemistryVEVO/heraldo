using System;
using Heraldo.Global;
using Heraldo.Global.Structs;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Heraldo.UI
{
    
    public class MainMenuManager : MonoBehaviour
    {
        public Button QuitButton;
        public Button StartButton;
        public Button ContinueButton;
        public GameObject LoadingScreen;
        public GameObject LandingScreen;

        private GameSaveData _saveData;
        
        void LoadLevel(string scn = "Scenes/MainScene")
        {
            Debug.Log("Load level button pressed");
            LoadingScreen.SetActive(true);
            LandingScreen.SetActive(false);
            SceneManager.LoadSceneAsync(scn)!.completed += operation => 
                LoadingScreen.SetActive(false) ;
        }

        void Quit()
        {
            Debug.Log("Quit button pressed");
            Application.Quit();
        }

        private void SaveDataTest()
        {
            JsonParser.WriteSaveData(new GameSaveData("Scenes/ScnPhase1"));
        }

        private void Awake()
        {
            //SaveDataTest();  
            
        }

        private void Start()
        {
            QuitButton.onClick.AddListener(Quit);
            StartButton.onClick.AddListener(() => { JsonParser.DeleteSaveData(); LoadLevel(); });
            if (JsonParser.SaveFileExists())
            {
                string scn = "Scenes/MainScene";
                GameSaveData? data = JsonParser.ReadSaveData();
                if (data.HasValue) scn = data.Value.lastScene;
                Debug.Log("You can now continue from " + scn);
                ContinueButton.onClick.AddListener(() => LoadLevel(scn));
            }
            else ContinueButton.gameObject.SetActive(false);
        }
    }
}