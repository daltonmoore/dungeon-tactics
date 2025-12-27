using System.Collections.Generic;
using Battle;
using UnityEngine;

namespace Units
{
    public interface IAttackable
    {
        public Dictionary<BattleUnitPosition, BattleUnitData> Party { get; }
    }
}