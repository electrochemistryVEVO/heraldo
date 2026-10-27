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
        private string _scnToLoad;
        
        void LoadLevel()
        {
            Debug.Log("Load level button pressed");
            LoadingScreen.SetActive(true);
            LandingScreen.SetActive(false);
            SceneManager.LoadSceneAsync(_scnToLoad)!.completed += operation => 
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
            if (JsonParser.SaveFileExists())
            {
                Debug.Log("Save file exists!");
                string scn = "Scenes/MainScene";
                GameSaveData? data = JsonParser.ReadSaveData();
                if (data.HasValue) scn = data.Value.lastScene;
                _scnToLoad = scn;
            }
            QuitButton.onClick.AddListener(Quit);
            RetryButton.onClick.AddListener(LoadLevel);
        }
    }
}