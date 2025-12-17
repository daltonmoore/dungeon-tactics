using System.Collections.Generic;
using Battle;

namespace Units
{
    public interface IAttackable
    {
        public List<BattleUnitData> Party { get; }
    }
}