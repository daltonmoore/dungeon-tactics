using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using Battle;
using UnityEngine;

namespace Units
{
    public interface IAttackable
    {
        public List<BattleUnitData> PartyList { get; }
    }
}