using System;
using Heraldo.Global.Enums;

namespace Heraldo.Global.Structs
{
    [Serializable]
    public struct HistoricalEvent
    {
        public int time;
        public HistoricalEventType eventType;
        public int wpnIndex;
        public ObjectiveID objective;
        public bool completed;
        public string commander;
        public string newLosingScene;
    }
}