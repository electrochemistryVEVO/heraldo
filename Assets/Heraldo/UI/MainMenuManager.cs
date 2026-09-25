using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Heraldo.UI
{
    
    public class MainMenuManager : MonoBehaviour
    {
        public Button QuitButton;
        public Button StartButton;
        public GameObject LoadingScreen;
        public GameObject LandingScreen;
        
        void LoadLevel()
        {
            Debug.Log("Load level button pressed");
            LoadingScreen.SetActive(true);
            LandingScreen.SetActive(false);
            SceneManager.LoadSceneAsync("Scenes/MainScene")!.completed += operation => 
                LoadingScreen.SetActive(false) ;
        }

        void Quit()
        {
            Debug.Log("Quit button pressed");
            Application.Quit();
        }

        private void Awake()
        {
            QuitButton.onClick.AddListener(Quit);
            StartButton.onClick.AddListener(LoadLevel);
        }
    }
}