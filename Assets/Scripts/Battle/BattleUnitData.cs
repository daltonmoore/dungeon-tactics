using System;
using System.Collections.Generic;
using TacticsCore.Data;
using TacticsCore.Units;
using UnityEngine;

namespace Battle
{
    [CreateAssetMenu(fileName = "Battle Unit Data", menuName = "Battle/Battle Unit Data"), System.Serializable]
    public class BattleUnitData : UnitSO
    {
        [SerializeField] public BattleUnitPosition battleUnitPosition;
        [SerializeField] public GameObject inBattleInstance;
        [SerializeField] public List<Stat> stats = new();
        [SerializeField] public bool isDead = false;

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(unitId))
            {
                unitId = Guid.NewGuid().ToString();
            }
        }

        public override string ToString()
        {
            string output = $"BattleUnitPosition {battleUnitPosition} "
                            + $" Name {characterName}" 
                            + $" Lv. {level}"
                            + $" Icon {(icon ? icon.name : " none ")}"
                            + " Leader " + isLeader
                            + " IsDead " + isDead;

            foreach (var stat in stats)
            {
                output += $" {stat.type.ToString()} {stat.value}";
            }
            
            return output;
        }

        public BattleUnitSaveRecord ToSaveRecord()
        {
            BattleUnitSaveRecord record = new BattleUnitSaveRecord
            {
                battleUnitPosition = battleUnitPosition,
                isDead = isDead,
                characterName = characterName,
                level = level,
                icon = icon,
                isLeader = isLeader,
                health = Health,
                unitId = unitId
            };

            return record;
        }
    }

    [System.Serializable]
    public class BattleUnitSaveRecord
    {
        [SerializeField] public string unitId;
        [SerializeField] public BattleUnitPosition battleUnitPosition;
        [SerializeField] public bool isDead = false;
        [SerializeField] public float health;
        [SerializeField] public string characterName;
        [SerializeField] public int level;
        [SerializeField] public Sprite icon;
        [SerializeField] public bool isLeader;
    }
}