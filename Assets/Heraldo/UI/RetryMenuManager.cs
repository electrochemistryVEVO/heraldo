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
            LoadingScreen.SetActive(true);
            LandingScreen.SetActive(false);
            SceneManager.LoadSceneAsync("Scenes/MainMenu")!.completed += operation => 
                LoadingScreen.SetActive(false) ;
        }

        private void Awake()
        {
            QuitButton.onClick.AddListener(Quit);
            RetryButton.onClick.AddListener(LoadLevel);
        }
    }
}