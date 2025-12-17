using System.Collections.Generic;

namespace Battle
{
    public struct BattleStartArgs
    {
        public List<BattleUnitData> Party;
        public List<BattleUnitData> EnemyParty;
    }
}