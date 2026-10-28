using Heraldo.Character.Enemy;
using Heraldo.Global.Enums;
using Heraldo.Global.Structs;
using Heraldo.Level;
using UnityEngine;

namespace Heraldo.Flavor
{
    public class HistoricalBattleManager : MonoBehaviour
    {
        public TroopController MainTroop;
        public ObjectiveManager objectiveManager;

        public void ChangeAllWeapons(HistoricalEvent @event)
        {
            MainTroop.ChangeWeapons(@event.wpnIndex);
        }

        public void AdvancePhase(HistoricalEvent @event)
        {
            MainTroop.SwitchPhase();
        }

        public void ChangeObjective(HistoricalEvent @event)
        {
            objectiveManager.ChangeObjective(@event.objective);
        }

        public void ChangeLoseCondition(HistoricalEvent @event)
        {
            objectiveManager.uiManager.ChangeLoseCondition(@event.newLosingScene);
        }

        public void SwitchScene(HistoricalEvent @event)
        {
            objectiveManager.uiManager.ChangeLoseCondition(@event.newLosingScene);
            objectiveManager.uiManager.FadeToBlack(2f);
        }

        public void CompleteObjective(HistoricalEvent @event)
        {
            objectiveManager.CompleteObjective();
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
                case HistoricalEventType.NEXT_PHASE:
                {
                    AdvancePhase(@event);
                    break;
                }
                case HistoricalEventType.CHANGE_OBJECTIVE:
                {
                    ChangeObjective(@event);
                    break;
                }
                case HistoricalEventType.CHANGE_LOSE_CONDITION:
                {
                    ChangeLoseCondition(@event);
                    break;
                }
                case HistoricalEventType.SWITCH_SCENE:
                {
                    SwitchScene(@event);
                    break;
                }
                case HistoricalEventType.COMPLETE_OBJECTIVE:
                {
                    CompleteObjective(@event);
                    break;
                }
            }
        }
    }
}