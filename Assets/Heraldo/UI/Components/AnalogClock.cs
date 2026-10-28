using System;
using Heraldo.VFX.Lighting;
using UnityEngine;

namespace Heraldo.UI.Components
{
    public class AnalogClock : MonoBehaviour
    {
        public RectTransform hours;
        public RectTransform minutes;
        private float _initialRollHours;
        private float _initialRollMinutes;

        private void Awake()
        {
            _initialRollHours = hours.eulerAngles.z;
            _initialRollMinutes = minutes.eulerAngles.z;
        }

        public void SetTime(string timeStr)
        {
            int tim = DayNightManager.ParseTime(timeStr);
            int hh = (tim / 60) % 12;
            int ss = tim % 60;
            // 360 / 60 = 6
            // 360 / 12 = 30
            hours.rotation = Quaternion.Euler(new Vector3(hours.eulerAngles.x, hours.eulerAngles.y,
                _initialRollHours + 360 - ((hh * 30 + ss * 0.5f) % 360)));
            minutes.rotation = Quaternion.Euler(new Vector3(minutes.eulerAngles.x, minutes.eulerAngles.y,
                _initialRollMinutes + 360 - ((ss * 6f) % 360)));
        }
    }
}