using System.Collections.Generic;
using Battle;
using UnityEngine;

namespace Data
{
    [CreateAssetMenu]
    public class CollectionDatabase : ScriptableObject
    {
        [SerializeField] private List<BattleUnitData> battleUnits;
        
        public List<BattleUnitData> BattleUnits => battleUnits;
    }
}