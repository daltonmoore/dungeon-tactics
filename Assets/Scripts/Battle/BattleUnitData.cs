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
    }

    [System.Serializable]
    public class BattleUnitSaveData
    {
        [SerializeField] public BattleUnitPosition battleUnitPosition;
        [SerializeField] public bool isDead = false;
        [SerializeField] public float health;
        [SerializeField] public string characterName;
        [SerializeField] public int level;
        [SerializeField] public Sprite icon;
        [SerializeField] public bool isLeader;
    }
}