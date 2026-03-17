using TacticsCore.EventBus;
using Units;

namespace Events
{
    public struct UnitDamaged : IEvent
    {
        public BattleUnit battleUnit;
        public float damage;

        public UnitDamaged(BattleUnit battleUnit, float damage)
        {
            this.battleUnit = battleUnit;
            this.damage = damage;
        }
    }
}