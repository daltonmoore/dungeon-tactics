using System;
using Units;
using UnityEngine;

namespace Battle
{
    [Serializable]
    public struct BattleUnitData
    {
        [SerializeField] public BattleUnitPosition battleUnitPosition;
        [SerializeField] public AbstractBattleUnit unitPrefab;
        [SerializeField] private string name;
        [SerializeField] private int level;
        [SerializeField] private Sprite icon;
    }
}