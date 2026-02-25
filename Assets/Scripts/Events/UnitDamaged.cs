using TacticsCore.EventBus;
using Units;

namespace Events
{
    public struct UnitDamaged : IEvent
    {
        public BattleUnit BattleUnit;
        public float Damage;

        public UnitDamaged(BattleUnit battleUnit, float damage)
        {
            BattleUnit = battleUnit;
            Damage = damage;
        }
    }
}