using System;
using Units;
using UnityEngine;
using UnityEngine.UIElements;

namespace Battle
{
    [CreateAssetMenu(fileName = "Battle Unit Data", menuName = "Battle/Battle Unit Data")]
    public class BattleUnitData : ScriptableObject
    {
        [SerializeField] public BattleUnitPosition battleUnitPosition;
        [SerializeField] public string characterName;
        [SerializeField] public int level;
        [SerializeField] public Sprite icon;
        [SerializeField] public bool isLeader;

        // Scriptable objects do not use constructors.
        // public BattleUnitData(string name, int level, Sprite icon, BattleUnitPosition battleUnitPosition, bool isLeader)
        // {
        //     this.name = name;
        //     this.level = level;
        //     this.icon = icon;
        //     this.battleUnitPosition = battleUnitPosition;
        //     this.isLeader = isLeader;
        // }
        
        public BattleUnitData(BattleUnitPosition battleUnitPosition, BattleUnitData oldData)
        {
            this.battleUnitPosition = battleUnitPosition;
            this.characterName = oldData.characterName;
            this.level = oldData.level;
            this.icon = oldData.icon;
            this.isLeader = oldData.isLeader;
        }

        public void Initialize(string name, int level, Sprite icon, BattleUnitPosition battleUnitPosition,
            bool isLeader)
        {
            this.characterName = name;
            this.level = level;
            this.icon = icon;
            this.battleUnitPosition = battleUnitPosition;
            this.isLeader = isLeader;
        }

        public override string ToString()
        {
            return 
                $"BattleUnitPosition {battleUnitPosition} "
                + $" Name {characterName}" 
                + $" Lv. {level}"
                + $" Icon {(icon ? icon.name : " none ")}"
                + " Leader " + isLeader;
        }
    }
}