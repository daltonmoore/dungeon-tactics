using System.Collections.Generic;
using Battle;
using UnityEngine;
using UnityEngine.Serialization;

namespace Units
{
    public class Party : MonoBehaviour
    {
        [SerializeField] public List<BattleUnitData> party;
    }
}