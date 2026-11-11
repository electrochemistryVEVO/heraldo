using System;
using System.Collections;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Heraldo.Flavor;
using Heraldo.Global;
using Heraldo.Global.SO;
using Heraldo.Global.Structs;
using Heraldo.UI;
using UnityEngine;

namespace Heraldo.VFX.Lighting
{
    public class DayNightManager : MonoBehaviour
    {
        public float simulatedToRealSeconds = 2.5f;
        
        public Material cubemapMaterial;
        public Material sunMaterial;
        public HistoricalBattleScenario BattleScenario;
        public string hora;
        public int HoraMinMax = 600;
        
        private int _horaMins;
        private LevelUIManager _uiCanvas;
        private HistoricalBattleManager _historicalBattleManager;
        private Task _timeUpdateTask;
        private CancellationTokenSource _timeUpdateCancel = new();
        private HistoricalEvent[] _events;

        private bool _stopped = false;
        
        
        private void Awake()
        {
            LoadSaveData();
            
            _horaMins = ParseTime(hora);
            _uiCanvas = GameObject.FindWithTag("UI Canvas").GetComponent<LevelUIManager>();
            _historicalBattleManager = GetComponent<HistoricalBattleManager>();
            _events = BattleScenario.Events.ToArray();
        }

        private void LoadSaveData()
        {
            if (!JsonParser.SaveFileExists()) return;
            GameSaveData? data = JsonParser.ReadSaveData();
            if (!data.HasValue) return;
            if (data.Value.timeString == null) return;
            hora = data.Value.timeString;
        }

        private void Start()
        {
            //cubemapMaterial = RenderSettings.skybox;
            //RenderSettings.skybox.Lerp(cubemapMaterial,sunMaterial, 0.5f);
            StartClock();
            _uiCanvas.DayTimeText(hora);
        }

        public static int ParseTime(string tim)
        {
            Debug.Log("Input string: "+tim);
            string[] str = tim.Split(':');
            string hrSplit = str[0];
            string minSplit = str[1];
            return int.Parse(hrSplit) * 60 + int.Parse(minSplit);
        }

        string ToTimeString(int mins)
        {
            return (mins / 60).ToString().PadLeft(2,'0') 
                   + ":" + (mins % 60).ToString().PadLeft(2,'0');
        }
        
        public void StartClock()
        {
            if (!_stopped)
            {
                _ = TimeUpdate();
            }
        }

        async Task TimeUpdate()
        { 
            await Awaitable.MainThreadAsync();
            try
            {
                while (_horaMins < HoraMinMax)
                {
                    for (int i=0;i<_events.Length;i++)
                    {
                        var currentEvent = _events[i];
                        if (currentEvent.time != _horaMins || currentEvent.completed) continue;
                        Debug.Log("[HISTORICAL-EVENT] Current event: " + currentEvent.eventType);
                        _events[i].completed = true;
                        _historicalBattleManager.HandleHistoricalEvent(currentEvent);
                    }
                    Debug.Log("[TIME-MANAGER] Tick.");
                    await Awaitable.WaitForSecondsAsync(simulatedToRealSeconds);
                    if (_stopped || !Application.isPlaying)
                    {
                        Debug.Log("[TIME-MANAGER] Timer stopped.");
                        return;
                    }
                    Debug.Log("[TIME-MANAGER] Tock. " + _horaMins);
                    _horaMins++;
                    _uiCanvas.DayTimeText(ToTimeString(_horaMins));
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public void StopClock()
        {
            if (!_stopped)
            {
                _stopped = true;
                _timeUpdateCancel.Cancel();
                _uiCanvas.StopOrResumeDayTimeText(false);
            }
        }

        public void ResumeClock()
        {
            if (_stopped)
            {
                _stopped = false;
                _timeUpdateCancel.Dispose();
                _timeUpdateCancel = new();
                _uiCanvas.StopOrResumeDayTimeText(true);
                _ = TimeUpdate();
            }
        }

        private void Update()
        {
            //DynamicGI.UpdateEnvironment();
        }
    }
}
