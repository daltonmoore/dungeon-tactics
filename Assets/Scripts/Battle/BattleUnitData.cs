using TacticsCore.Data;
using TacticsCore.Units;
using UnityEngine;

namespace Battle
{
    [CreateAssetMenu(fileName = "Battle Unit Data", menuName = "Battle/Battle Unit Data")]
    public class BattleUnitData : UnitSO
    {
        [SerializeField] public BattleUnitPosition battleUnitPosition;
        [SerializeField] public GameObject inBattleInstance;

        public override void Initialize(string name, int level, Sprite icon, BattleUnitPosition battleUnitPosition,
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