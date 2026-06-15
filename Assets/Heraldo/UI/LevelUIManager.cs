using System;
using System.Collections;
using System.Threading;
using Heraldo.Cutscenes;
using JetBrains.Annotations;
using NUnit.Framework.Constraints;
using TMPro;
using UnityEngine;

namespace Heraldo.UI
{
    public class LevelUIManager : MonoBehaviour
    {
        public GameObject crosshair;
        public TMP_Text crosshairText;
        public TMP_Text flavorText;
        public TMP_Text cutsceneActorNameText;
        public TMP_Text cutsceneText;
        private Color ALLY_COLOR = Color.green;
        private Color ENEMY_COLOR = Color.red;
        private bool showFlavorText = false;
        
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
    }
}