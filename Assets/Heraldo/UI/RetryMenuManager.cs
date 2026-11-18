using System;
using Heraldo.Global;
using Heraldo.Global.Structs;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Heraldo.UI
{
    public class RetryMenuManager : MonoBehaviour
    {
        public Button QuitButton;
        public Button RetryButton;
        public GameObject LoadingScreen;
        public GameObject LandingScreen;
        protected string _scnToLoad;
        
        void LoadLevel()
        {
            Debug.Log("Load level button pressed");
            LoadingScreen.SetActive(true);
            LandingScreen.SetActive(false);
            SceneManager.LoadSceneAsync(_scnToLoad);
        }

        void Quit()
        {
            Debug.Log("Quit button pressed");
            LoadingScreen.SetActive(true);
            LandingScreen.SetActive(false);
            SceneManager.LoadSceneAsync("Scenes/MainMenu");
        }

        public void Awake()
        {
            if (JsonParser.SaveFileExists())
            {
                Debug.Log("Save file exists!");
                string scn = "Scenes/MainScene";
                GameSaveData? data = JsonParser.ReadSaveData();
                if (data.HasValue) scn = data.Value.lastScene;
                _scnToLoad = scn;
            }
            else _scnToLoad = "Scenes/MainScene";
            Debug.Log("Retry menu awakened");
            QuitButton.onClick.AddListener(Quit);
            RetryButton.onClick.AddListener(LoadLevel);
        }

        public virtual void Start()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}