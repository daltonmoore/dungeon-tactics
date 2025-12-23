using System.Collections.Generic;
using Battle;

namespace Units
{
    public interface IAttackable
    {
        List<BattleUnitData> Party { get; }
    }
}