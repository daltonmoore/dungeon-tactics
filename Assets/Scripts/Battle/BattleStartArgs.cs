using System.Collections.Generic;
using Units;

namespace Battle
{
    public struct BattleStartArgs
    {
        public Dictionary<BattleUnitPosition, BattleUnitData> Party;
        public Dictionary<BattleUnitPosition, BattleUnitData> EnemyParty;
    }
}