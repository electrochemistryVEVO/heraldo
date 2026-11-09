using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using Heraldo.Cutscenes;
using Heraldo.Global;
using Heraldo.Global.Structs;
using Heraldo.UI.Components;
using JetBrains.Annotations;
using LitMotion;
using LitMotion.Extensions;
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
        public GameObject interactPopup;
        private TMP_Text _interactText;
        public ObjectivePopup objectivePopup;
        public TMP_Text flavorText;
        public TMP_Text cutsceneActorNameText;
        public TMP_Text cutsceneText;
        public TMP_Text wpnDescriptionText;
        public AmmoBar ammoCount;
        public PPHealth healthFx;
        public TMP_Text dayTimeText;
        public AnalogClock dayTimeClock;
        public WpnLoading ammoLoad;
        public Image blackFadePanel;
        
        private Color ALLY_COLOR = Color.green;
        private Color ENEMY_COLOR = Color.red;
        private bool showFlavorText = false;
        private string _losingScene = "Scenes/RetryScreen";
        

        private Material _crosshairMaterial;
        
        public void OnSubjectChange([CanBeNull] GameObject subject,bool isNull=false)
        {
            if (isNull) crosshair.color = Color.white;
            else
            {
                if (subject.layer == LayerMask.NameToLayer("Enemy"))
                {
                    crosshair.color = ENEMY_COLOR;
                    crosshairText.alpha = 0;
                }
                else
                {
                    crosshair.color = ALLY_COLOR;
                    crosshairText.alpha = 1f;
                    crosshairText.text = subject.name;
                }
                //crosshairText.text = subject.name;
            }
        }

        public void SeeInteractable([CanBeNull] string str)
        {
            _interactText.text = str ?? "";
            if (str is null)
            {
                interactPopup.SetActive(false);
            }
            else interactPopup.SetActive(true);
        }

        private void Awake()
        {
            _crosshairMaterial = Instantiate(crosshair.material);
            _interactText = interactPopup.transform.GetChild(1).GetComponent<TMP_Text>();
        }

        private void Update()
        {
            if (!showFlavorText)
            {
                Color color = flavorText.color;
                flavorText.color = new Color(color.r, color.g, color.b, color.a/2);
            }
        }

        public void CutsceneMode(bool on = true)
        {
            ammoCount.gameObject.SetActive(!on);
            ammoLoad.gameObject.SetActive(!on);
            wpnDescriptionText.gameObject.SetActive(!on);
            objectivePopup.gameObject.SetActive(!on);
            interactPopup.SetActive(!on);
            dayTimeText.gameObject.SetActive(!on);
            dayTimeClock.gameObject.SetActive(!on);
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

        public void WeaponDescriptionText(string desc)
        {
            wpnDescriptionText.text = desc;
        }

        public void CutsceneText(string subjectName="",string text="")
        {
            cutsceneActorNameText.text = subjectName;
            cutsceneText.text = text;
        }

        public void AmmoText(int num)
        {
            ammoCount.SetAmmo(num);
        }

        public void DayTimeText(string timeText)
        {
            dayTimeText.text = timeText + " AM";
            dayTimeClock.SetTime(timeText);
        }

        public void StopOrResumeDayTimeText(bool resume)
        {
            dayTimeText.color = resume ? Color.white : Color.red;
        }

        public void HealthText(int num,int maxHealth=100)
        {
            healthFx.SetHealth(num,maxHealth);
        }

        public void TakeDamage(int num, int maxHealth)
        {
            Debug.Log("[SHOW-DAMAGE] Damage shown by the UI");
            healthFx.ShowDamage(num,maxHealth);
        }

        public void ObjectiveText(Objective obj)
        {
            objectivePopup.ChangeObjective(obj);
        }

        public void InitObjective(Objective obj)
        {
            objectivePopup.InitObjective(obj);
        }

        public void ShowCompleteObjective(Objective obj)
        {
            objectivePopup.ShowCompleteObjective(obj);
        }

        public void ShowHideAmmoText(bool show=false)
        {
            wpnDescriptionText.alpha = show ? 1f : 0;
            ammoCount.ShowOrHide(show);
        }

        public void ToggleAmmoLoad(bool value)
        {
            ammoLoad.ToggleAmmoLoad(value);
        }

        public void SetCrosshairSpread(float spreadMultiplier)
        {
            
            float dist = crosshair.material.GetFloat(Distance);
            
            float finalDist = Mathf.Lerp(dist, 1 - spreadMultiplier, 0.5f); 
            crosshair.material.SetFloat(Distance,finalDist); 
            //crosshair.material = _crosshairMaterial;
        }
        
        


        public async Task FadeToBlack(float seconds)
        {
            await LMotion.Create(0, 1f, seconds)
                .Bind(alpha =>
                {
                    blackFadePanel.color = new Color(blackFadePanel.color.r,
                        blackFadePanel.color.g,
                        blackFadePanel.color.b,
                        alpha);
                })
                .AddTo(blackFadePanel.gameObject)
                .ToAwaitable();
        }
        
        public async Task FadeFromBlack(float seconds)
        {
            await LMotion.Create(1f, 0, seconds)
                .Bind(alpha =>
                {
                    blackFadePanel.color = new Color(blackFadePanel.color.r,
                        blackFadePanel.color.g,
                        blackFadePanel.color.b,
                        alpha);
                })
                .AddTo(blackFadePanel.gameObject)
                .ToAwaitable();
        }

        public Task FailLevel()
        {
            return FadeToBlack(2f).ContinueWith(async t1 =>
            {
                await Awaitable.MainThreadAsync();
                SceneManager.LoadScene(_losingScene);
            });
        }

        public Task NextLevel(GameSaveData saveData)
        {
            saveData.SetTimeString(dayTimeText.text.Split(' ')[0]);
            JsonParser.WriteSaveData(saveData);
            _losingScene = saveData.lastScene;
            return FailLevel();
        }

        public Task EndGame()
        {
            JsonParser.DeleteSaveData();
            _losingScene = "Scenes/MainMenu";
            return FailLevel();
        }

        public void ChangeLoseCondition(string scnToLoad)
        {
            _losingScene = scnToLoad;
        }
        
    }
}