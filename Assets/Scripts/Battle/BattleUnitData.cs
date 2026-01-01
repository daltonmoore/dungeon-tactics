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
        [SerializeField] public int initiative;

        public void Initialize(string name, int level, Sprite icon, BattleUnitPosition battleUnitPosition,
            bool isLeader, int initiative)
        {
            this.characterName = name;
            this.level = level;
            this.icon = icon;
            this.battleUnitPosition = battleUnitPosition;
            this.isLeader = isLeader;
            this.initiative = initiative;
        }

        public override string ToString()
        {
            return 
                $"BattleUnitPosition {battleUnitPosition} "
                + $" Name {characterName}" 
                + $" Lv. {level}"
                + $" Icon {(icon ? icon.name : " none ")}"
                + $" Initiative {initiative}"
                + " Leader " + isLeader;
        }
    }
}