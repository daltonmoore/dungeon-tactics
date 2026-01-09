using System.Collections.Generic;
using Battle;
using Commands;
using EventBus;

namespace Events
{
    public struct  EngageInBattleEvent : IEvent
    {
        public List<BattleUnitData> Party;
        public List<BattleUnitData> EnemyParty;
        
        public EngageInBattleEvent(List<BattleUnitData> party, List<BattleUnitData> enemyParty)
        {
            Party = party;
            EnemyParty = enemyParty;
        }
    }
}