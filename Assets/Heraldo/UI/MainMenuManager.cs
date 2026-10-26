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

        private void Awake()
        {
            if (JsonParser.SaveFileExists())
            {
                ContinueButton.enabled = true;
                string scn = "Scenes/MainScene";
                GameSaveData? data = JsonParser.ReadSaveData();
                if (data.HasValue) scn = data.Value.lastScene;
                ContinueButton.onClick.AddListener(() => LoadLevel(scn));
            }
            
            QuitButton.onClick.AddListener(Quit);
            StartButton.onClick.AddListener(() => { JsonParser.DeleteSaveData(); LoadLevel(); });
        }
    }
}