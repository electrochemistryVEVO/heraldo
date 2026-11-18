using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using Heraldo.Cutscenes;
using Heraldo.Global;
using Heraldo.Global.Structs;
using Heraldo.UI.Components;
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
        public FeedbackUI feedback;
        public GameObject interactPopup;
        public GameObject tutorialScreen;
        private TMP_Text _interactText;
        public ObjectivePopup objectivePopup;
        public CompassUI compass;
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
        //Trampita xd
        private bool _popupEnabled = true;
        

        private Material _crosshairMaterial;
        
        public void OnSubjectChange([CanBeNull] GameObject subject,bool isNull=false)
        {
            if (isNull)
            {
                crosshair.color = Color.white;
                crosshairText.alpha = 0;
            }
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
            if (str is null || !_popupEnabled)
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

        public void ShowHideTutorial(bool hide = false)
        {
            Time.timeScale = hide ? 1f : 0f;
            tutorialScreen.SetActive(!hide);
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
            crosshairText.gameObject.SetActive(!on);
            objectivePopup.gameObject.SetActive(!on);
            interactPopup.SetActive(!on);
            _popupEnabled = !on;
            dayTimeText.gameObject.SetActive(!on);
            dayTimeClock.gameObject.SetActive(!on);
            crosshair.gameObject.SetActive(!on);
            compass.Active(!on);
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
                    Thread.Sleep(6000);
                    showFlavorText = false;
                });
                t1.Start();
            }
        }

        public void WeaponDescriptionText(string desc)
        {
            wpnDescriptionText.text = desc;
        }

        public void UpdateCompass(Vector3 position,Quaternion rotation)
        {
            compass.UpdateByPlayerTransform(position, rotation);
        }

        public void SetMarkerPosition(MarkerType type, Vector3 position)
        {
            compass.SetMarkerPosition(type,position);
        }

        public void FeedbackText(FeedbackType type,string text)
        {
            feedback.SpawnFeedbackText(type, text);
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
        
        


        public Task FadeToBlack(float seconds) => FadeBlackPanel(0f, 1f, seconds);

        public Task FadeFromBlack(float seconds) => FadeBlackPanel(1f, 0f, seconds);

        private async Task FadeBlackPanel(float from, float to, float seconds)
        {
            await Awaitable.MainThreadAsync();
            var panel = blackFadePanel;
            var cancellationToken = destroyCancellationToken;
            var color = panel.color;
            var start = Time.unscaledTimeAsDouble;
            color.a = from;
            panel.color = color;

            double elapsed = 0;
            while (elapsed < seconds)
            {
                await Awaitable.NextFrameAsync(cancellationToken);
                elapsed = Time.unscaledTimeAsDouble - start;
                color.a = Mathf.Lerp(from, to, (float)(elapsed / seconds));
                panel.color = color;
            }

            color.a = to;
            panel.color = color;
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
            JsonParser.WriteSaveData(saveData).Wait();
            _losingScene = saveData.lastScene == "Scenes/MainMenu"
            ? "Scenes/MainMenu"
            :"Scenes/MissionDebriefScene";
            return FailLevel();
        }

        public void SkipToLevel(GameSaveData saveData)
        {
            saveData.SetTimeString(dayTimeText.text.Split(' ')[0]);
            JsonParser.WriteSaveData(saveData).Wait();
            _losingScene = saveData.lastScene;
            SceneManager.LoadScene(_losingScene);
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
