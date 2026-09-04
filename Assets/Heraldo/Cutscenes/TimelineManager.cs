using System;
using System.Collections.Generic;
using Heraldo.Global.Enums;
using Heraldo.Global.Structs;
using UnityEngine;
using UnityEngine.Playables;

namespace Heraldo.Cutscenes
{
    public class TimelineManager : MonoBehaviour
    {
        public List<KeyTimelinePair> timelines;
        public Dictionary<TimelineID, PlayableDirector> timelineDict = new();

        private void Awake()
        {
            foreach (var pair in timelines)
            {
                Debug.Log("Inserting "+pair.key);
                timelineDict.Add(pair.key,pair.value);
            }
        }
    }
}