using System;
using System.Collections;
using System.Linq;
using Heraldo.Flavor;
using Heraldo.Global.SO;
using Heraldo.Global.Structs;
using Heraldo.UI;
using UnityEngine;

namespace Heraldo.VFX.Lighting
{
    public class DayNightManager : MonoBehaviour
    {
        private static readonly float SIMULATED_TO_REAL_SECONDS = 5f;
        
        public Material cubemapMaterial;
        public Material sunMaterial;
        public HistoricalBattleScenario BattleScenario;
        public string hora;
        private int _horaMins;
        private LevelUIManager _uiCanvas;
        private HistoricalBattleManager _historicalBattleManager;
        
        
        private void Awake()
        {
            cubemapMaterial = RenderSettings.skybox;
            RenderSettings.skybox.Lerp(cubemapMaterial,sunMaterial, 0.5f);
            _horaMins = ParseTime(hora);
            _uiCanvas = GameObject.FindWithTag("UI Canvas").GetComponent<LevelUIManager>();
            _historicalBattleManager = GetComponent<HistoricalBattleManager>();
            StartCoroutine(TimeUpdate());
        }

        int ParseTime(string tim)
        {
            string[] str = tim.Split(':');
            string hrSplit = str[0];
            string minSplit = str[1];
            return int.Parse(hrSplit) * 60 + int.Parse(minSplit);
        }

        string ToTimeString(int mins)
        {
            return (mins / 60) + ":" + (mins % 60);
        }

        IEnumerator TimeUpdate()
        {
            while (true)
            {
                HistoricalEvent[] events = BattleScenario.Events
                    .Where(val => val.time == _horaMins).ToArray();
                if (events.Length > 0)
                {
                    //Debug.Log("Event triggered");
                    foreach (var currentEvent in events)
                    {
                        Debug.Log("[HISTORICAL-EVENT] Current event: "+currentEvent.eventType);
                        _historicalBattleManager.HandleHistoricalEvent(currentEvent);
                    }
                }
                yield return new WaitForSeconds(SIMULATED_TO_REAL_SECONDS); 
                _uiCanvas.DayTimeText(ToTimeString(_horaMins));
                _horaMins++;
            }
        }

        private void ProcessHistoricalEvent(HistoricalEvent @event)
        {
            
        }

        private void Update()
        {
            DynamicGI.UpdateEnvironment();
        }
    }
}