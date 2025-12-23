using System;
using Units;
using UnityEngine;
using UnityEngine.UIElements;

namespace Battle
{
    [Serializable]
    public struct BattleUnitData : IEquatable<BattleUnitData>
    {
        [SerializeField] public AbstractBattleUnit unitPrefab;
        [SerializeField] public BattleUnitPosition battleUnitPosition;
        [SerializeField] public string name;
        [SerializeField] public int level;
        [SerializeField] public Sprite icon;
        [SerializeField] public bool isLeader;

        public BattleUnitData(AbstractBattleUnit unitPrefab, string name, int level, Sprite icon, BattleUnitPosition battleUnitPosition, bool isLeader)
        {
            this.unitPrefab = unitPrefab;
            this.name = name;
            this.level = level;
            this.icon = icon;
            this.battleUnitPosition = battleUnitPosition;
            this.isLeader = isLeader;
        }

        public override string ToString()
        {
            if (unitPrefab == null) return "No Unit";
            return "BattleUnitPosition " + battleUnitPosition + " Name "+ name + " Lv. " + level + " Icon " + icon.name + " Prefab " + unitPrefab.name + " Leader " + isLeader;
        }

        public bool Equals(BattleUnitData other)
        {
            return Equals(unitPrefab, other.unitPrefab) && battleUnitPosition == other.battleUnitPosition && name == other.name && level == other.level && Equals(icon, other.icon) && isLeader == other.isLeader;
        }

        public override bool Equals(object obj)
        {
            return obj is BattleUnitData other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(unitPrefab, (int)battleUnitPosition, name, level, icon, isLeader);
        }

        public static bool operator ==(BattleUnitData left, BattleUnitData right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(BattleUnitData left, BattleUnitData right)
        {
            return !left.Equals(right);
        }
    }
}