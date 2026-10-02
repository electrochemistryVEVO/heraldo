using System.Collections.Generic;
using Heraldo.Global.Structs;
using UnityEngine;

namespace Heraldo.Global.SO
{
    [CreateAssetMenu(fileName = "BattleScenario", menuName = "Troop Battle Scenario", order = 0)]
    public class HistoricalBattleScenario : ScriptableObject
    {
        [SerializeField] public List<HistoricalEvent> Events;
    }
}