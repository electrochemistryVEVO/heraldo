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
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            button.onClick.AddListener(() => { SceneManager.LoadScene("Scenes/MainMenu"); });
        }
    }
}