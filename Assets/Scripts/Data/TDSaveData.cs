using System.Collections.Generic;
using Battle;
using TacticsCore.Data;
using TacticsCore.Save;
using UnityEngine;

namespace Data
{
    public class TDSaveData : SaveData
    {
        public List<LeaderSaveData> leaders;

        public TDSaveData(List<LeaderSaveData> leaders)
        {
            this.leaders = leaders;
        }
    }
    
    [System.Serializable]
    public struct LeaderSaveData
    {
        public Owner owner;
        public Vector3 position;
        public string spriteName;
        public List<BattleUnitData> party;

        public LeaderSaveData(Owner owner, Vector3 position, string spriteName, List<BattleUnitData> party)
        {
            this.owner = owner;
            this.position = position;
            this.spriteName = spriteName;
            this.party = party;
        }
    }
}