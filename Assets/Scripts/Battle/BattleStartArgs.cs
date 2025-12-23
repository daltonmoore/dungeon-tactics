using System.Collections.Generic;
using Units;

namespace Battle
{
    public struct BattleStartArgs
    {
        public List<BattleUnitData> Party;
        public List<BattleUnitData> EnemyParty;
    }
}