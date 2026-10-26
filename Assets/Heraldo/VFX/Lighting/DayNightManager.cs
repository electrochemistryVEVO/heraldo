using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using Heraldo.Flavor;
using Heraldo.Global.SO;
using Heraldo.Global.Structs;
using Heraldo.UI;
using UnityEngine;

namespace Heraldo.VFX.Lighting
{
    public class DayNightManager : MonoBehaviour
    {
        private static readonly float SIMULATED_TO_REAL_SECONDS = 2.5f;
        
        public Material cubemapMaterial;
        public Material sunMaterial;
        public HistoricalBattleScenario BattleScenario;
        public string hora;
        public int HoraMinMax = 600;
        
        private int _horaMins;
        private LevelUIManager _uiCanvas;
        private HistoricalBattleManager _historicalBattleManager;
        private Task _timeUpdateTask;
        private TaskCompletionSource<bool> _timeUpdateCancel = new();
        private HistoricalEvent[] _events;

        private bool _stopped = false;
        
        
        private void Awake()
        {
            
            _horaMins = ParseTime(hora);
            _uiCanvas = GameObject.FindWithTag("UI Canvas").GetComponent<LevelUIManager>();
            _historicalBattleManager = GetComponent<HistoricalBattleManager>();
            _events = BattleScenario.Events.ToArray();
        }

        private void Start()
        {
            cubemapMaterial = RenderSettings.skybox;
            RenderSettings.skybox.Lerp(cubemapMaterial,sunMaterial, 0.5f);
            _timeUpdateTask = Task.Run(TimeUpdate);
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
            return (mins / 60).ToString().PadLeft(2,'0') 
                   + ":" + (mins % 60).ToString().PadLeft(2,'0');
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

                    Task t1 = new Task(() => Awaitable.WaitForSecondsAsync(SIMULATED_TO_REAL_SECONDS));
                    await Task.WhenAny(new Task[]
                    {
                        Task.Delay((int)MathF.Round(SIMULATED_TO_REAL_SECONDS * 1000)),
                        _timeUpdateCancel.Task
                    });
                    if (_stopped)
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
                _timeUpdateCancel.SetResult(true);
                _uiCanvas.StopOrResumeDayTimeText(false);
            }
        }

        public void ResumeClock()
        {
            if (_stopped)
            {
                _stopped = false;
                _timeUpdateCancel = new();
                _uiCanvas.StopOrResumeDayTimeText(true);
                _timeUpdateTask = Task.Run(TimeUpdate);
            }
        }

        private void Update()
        {
            DynamicGI.UpdateEnvironment();
        }
    }
}