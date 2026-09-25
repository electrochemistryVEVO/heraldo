using System;
using System.Collections;
using System.Threading;
using Heraldo.Cutscenes;
using JetBrains.Annotations;
using NUnit.Framework.Constraints;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Heraldo.UI
{
    public class LevelUIManager : MonoBehaviour
    {
        private static readonly int Distance = Shader.PropertyToID("_Distance");
        public Image crosshair;
        public TMP_Text crosshairText;
        public TMP_Text flavorText;
        public TMP_Text cutsceneActorNameText;
        public TMP_Text cutsceneText;
        public TMP_Text ammoCountText;
        public TMP_Text healthText;
        public Image ammoLoad;
        public Image blackFadePanel;
        
        private Color ALLY_COLOR = Color.green;
        private Color ENEMY_COLOR = Color.red;
        private bool showFlavorText = false;

        private Material _crosshairMaterial;
        
        public void OnSubjectChange([CanBeNull] GameObject subject,bool isNull=false)
        {
            if (isNull) crosshairText.text = "";
            else
            {
                if (subject.layer == LayerMask.NameToLayer("Enemy"))
                    crosshairText.color = ENEMY_COLOR;
                else
                    crosshairText.color = ALLY_COLOR;
                crosshairText.text = subject.name;
            }
        }

        private void Awake()
        {
            _crosshairMaterial = Instantiate(crosshair.material);
        }

        private void Update()
        {
            if (!showFlavorText)
            {
                Color color = flavorText.color;
                flavorText.color = new Color(color.r, color.g, color.b, color.a/2);
            }
        }
 
        public void FlavorText(string text)
        {
            if (!showFlavorText) //Doubles as debounce
            {
                showFlavorText = true;
                flavorText.text = text;
                Color color = flavorText.color;
                flavorText.color = new Color(color.r, color.g, color.b,1);
                Thread t1 = new Thread(() =>
                {
                    Thread.Sleep(2000);
                    showFlavorText = false;
                });
                t1.Start();
            }
        }

        public void CutsceneText(string subjectName="",string text="")
        {
            cutsceneActorNameText.text = subjectName;
            cutsceneText.text = text;
        }

        public void AmmoText(string num="")
        {
            int _num;
            if (int.TryParse(num, out _num))
            {
                ammoCountText.text = _num==0?"No ammo!":_num<0?"Melee weapon":"Ammo Count: "+num;
            } 
            else Debug.Log("Invalid ammo quantity");
        }

        public void HealthText(string num = "")
        {
            healthText.text = "Health: " + num;
        }

        public void ShowHideAmmoText(bool show=false)
        {
            ammoCountText.alpha = show ? 1.0f : 0.0f;
        }

        public void ToggleAmmoLoad(bool value)
        {
            ammoLoad.color = new Color(ammoLoad.color.r,ammoLoad.color.g,ammoLoad.color.b,value ? 1.0f : 0);
        }

        public void SetCrosshairSpread(float spreadMultiplier)
        {
            float dist = crosshair.material.GetFloat(Distance);
            float finalDist = Mathf.Lerp(dist, 1 - spreadMultiplier, 0.5f); 
            crosshair.material.SetFloat(Distance,finalDist); 
            //crosshair.material = _crosshairMaterial;
        }
        
        

        IEnumerator _FadeToBlack(float seconds)
        {
            float secs = 0;
            while (secs < seconds)
            {
                blackFadePanel.color = Color.Lerp(blackFadePanel.color,
                    new Color(blackFadePanel.color.r,
                        blackFadePanel.color.g,
                        blackFadePanel.color.b,
                        1f),secs/seconds);
                secs += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }

            Cursor.lockState = CursorLockMode.None;
            SceneManager.LoadScene("Scenes/RetryScreen");
        }

        public void FadeToBlack(float seconds)
        {
            Coroutine c1 = StartCoroutine(_FadeToBlack(seconds));
        }
        
    }
}