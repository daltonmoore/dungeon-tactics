using System.Collections.Generic;
using Battle;
using Commands;
using TacticsCore;
using TacticsCore.Data;
using TacticsCore.EventBus;
using TacticsCore.Units;

namespace Events
{
    public struct  EngageInBattleEvent : IEvent
    {
        public LeaderUnit Instigator;
        public List<BattleUnitData> Party;
        public List<BattleUnitData> EnemyParty;
        
        public EngageInBattleEvent(LeaderUnit instigator, List<BattleUnitData> party, List<BattleUnitData> enemyParty)
        {
            Instigator = instigator;
            Party = party;
            EnemyParty = enemyParty;
        }
    }
}