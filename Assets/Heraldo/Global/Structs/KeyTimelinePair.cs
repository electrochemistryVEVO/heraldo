using System;
using Heraldo.Global.Enums;
using UnityEngine.Playables;

namespace Heraldo.Global.Structs
{
    [Serializable]
    public struct KeyTimelinePair
    {
        public TimelineID key;
        public PlayableDirector value;
    }
}