using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using Battle;
using UnityEngine;

namespace Units
{
    public interface IAttackable
    {
        public Transform Transform { get; }
        public List<BattleUnitData> PartyList { get; }
    }
}