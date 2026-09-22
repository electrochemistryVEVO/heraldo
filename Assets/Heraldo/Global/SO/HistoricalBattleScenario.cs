using System.Collections.Generic;
using UnityEngine;

namespace Heraldo.Global.SO
{
    [CreateAssetMenu(fileName = "BattleScenario", menuName = "Troop Battle Scenario", order = 0)]
    public class HistoricalBattleScenario : ScriptableObject
    {
        [SerializeField] public List<Vector3> AllySpots = new();
        [SerializeField] public List<Vector3> EnemySpots = new();
    }
}