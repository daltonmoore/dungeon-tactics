using TacticsCore.EventBus;
using Units;

namespace Events
{
    public struct UnitDied : IEvent
    {
        public BattleUnit BattleUnit;

        public UnitDied(BattleUnit battleUnit)
        {
            BattleUnit = battleUnit;
        }
    }
}