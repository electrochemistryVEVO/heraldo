using System;
using System.Collections;
using System.Threading;
using Heraldo.Cutscenes;
using JetBrains.Annotations;
using NUnit.Framework.Constraints;
using TMPro;
using UnityEngine;
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
        public Image ammoLoad;
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
            ammoCountText.text = "Ammo Count: "+num;
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
    }
}