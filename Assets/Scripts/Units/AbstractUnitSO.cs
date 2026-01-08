using System;
using HexGrid;
using UnityEngine;
using UnityEngine.Serialization;

namespace Units
{
    public abstract class AbstractUnitSO : ScriptableObject
    {
        [field: SerializeField, Range(PathfindingHex.MOVE_STRAIGHT_COST, PathfindingHex.MOVE_STRAIGHT_COST * 30)] 
        public int MovePoints { get; private set; } = 64;
        [field: SerializeField, Range(0, 100)]
        public float Health { get; set; } = 100;
    }
}