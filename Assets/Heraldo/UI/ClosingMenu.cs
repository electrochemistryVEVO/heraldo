using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Heraldo.UI
{
    public class ClosingMenu : MonoBehaviour
    {
        public Button button;

        private void Start()
        {
            button.onClick.AddListener(() => { SceneManager.LoadScene("Scenes/MainMenu"); });
        }
    }
}