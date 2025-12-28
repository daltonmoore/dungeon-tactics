using System;
using Units;
using UnityEngine;
using UnityEngine.UIElements;

namespace Battle
{
    [Serializable]
    public class BattleUnitData
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
        
        public BattleUnitData(BattleUnitPosition battleUnitPosition, BattleUnitData oldData)
        {
            this.battleUnitPosition = battleUnitPosition;
            this.name = oldData.name;
            this.level = oldData.level;
            this.icon = oldData.icon;
            this.unitPrefab = oldData.unitPrefab;
            this.isLeader = oldData.isLeader;
        }

        public override string ToString()
        {
            return 
                $"BattleUnitPosition {battleUnitPosition} "
                + $" Name {name}" 
                + $" Lv. {level}"
                + $" Icon {(icon ? icon.name : " none ")}"
                + $" Prefab {(unitPrefab ? unitPrefab.name : " none ")}"
                + " Leader " + isLeader;
        }
    }
}