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
        public string id;
        public Owner owner;
        public Vector3 position;
        public string spriteName;
        public List<BattleUnitSaveRecord> party;
    }
}