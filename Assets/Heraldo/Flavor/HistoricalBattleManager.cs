using Heraldo.Character.Enemy;
using Heraldo.Global.Enums;
using Heraldo.Global.Structs;
using UnityEngine;

namespace Heraldo.Flavor
{
    public class HistoricalBattleManager : MonoBehaviour
    {
        public TroopController MainTroop;

        public void ChangeAllWeapons(HistoricalEvent @event)
        {
            MainTroop.ChangeWeapons(@event.wpnIndex);
        }

        public void HandleHistoricalEvent(HistoricalEvent @event)
        {
            switch (@event.eventType)
            {
                case HistoricalEventType.CHANGE_WEAPONS_ALL:
                {
                    ChangeAllWeapons(@event);
                    break;
                }
            }
        }
    }
}