using System.Collections.Generic;
using Heraldo.Global.Structs;
using UnityEngine;

namespace Heraldo.Global.SO
{
    [CreateAssetMenu(fileName = "Debriefings", menuName = "Flavor/DebriefingList", order = 0)]
    public class DebriefingList : ScriptableObject
    {
        public List<Debriefing> debriefings;
    }
}