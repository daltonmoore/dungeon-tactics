using System.Collections.Generic;
using TacticsCore.EventBus;
using TacticsCore.Units;
using Units;

namespace Events
{
    public struct ExitBattleEvent : IEvent
    {
        List<BattleUnit> PartyA;
        List<BattleUnit> PartyB;
        
        public ExitBattleEvent(List<BattleUnit> partyA, List<BattleUnit> partyB)
        {
            PartyA = partyA;
            PartyB = partyB;
        }
    }
}