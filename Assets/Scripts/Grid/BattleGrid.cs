using System;
using System.Collections.Generic;
using System.Linq;
using Battle;
using EventBus;
using Events;
using Units;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Grid
{
    public class BattleGrid : MonoBehaviour
    {
        [field: SerializeField] public GridConfig gridConfig { get; private set; }
        [SerializeField] private GameObject gridCellPrefab;
        [SerializeField] private GameObject battleUnitPrefab;
        [SerializeField] private Vector3 gridOffset;
        
        Dictionary<BattleUnitPosition, GridCell> playerGridCells = new();
        Dictionary<BattleUnitPosition, GridCell> enemyGridCells = new();
        private const string GridCellLayerName = "Floor";

        private void Awake()
        {
            Bus<StartBattleEvent>.OnEvent[Owner.Player1] += OnStartBattle;
    }

        private void OnStartBattle(StartBattleEvent evt)
        {
            ClearGrid();
            SetupGrid();
            
            // Setup Parties
            InstantiateBattleUnits(true, evt.Party);
            InstantiateBattleUnits(false, evt.EnemyParty);

            var allUnits = new List<BattleUnitData>();
            allUnits.AddRange(evt.Party);
            allUnits.AddRange(evt.EnemyParty);
            var turnOrder = allUnits.OrderByDescending(u => u.initiative).ToList();
            for (int index = 0; index < turnOrder.Count; index++)
            {
                BattleUnitData battleUnitData = turnOrder[index];
                Debug.Log($"{index}: {battleUnitData.name} initiative {battleUnitData.initiative}");
                
            }
        }

        public void SetupGrid()
        {
            int slot = 0;
            for (int x = 0; x < gridConfig.GridSize.x; x++)
            {
                if (x is 2) slot = 3;
                if (x is 3) slot = 0;
                
                for (int y = 0; y < gridConfig.GridSize.y; y++)
                {
                    GameObject square = Instantiate(gridCellPrefab, transform);
                    square.name = $"Grid Cell ({x}, {y})";
                    square.transform.position = new Vector3(x, y, -1) + gridOffset;
                    
                    square.GetComponent<SpriteRenderer>().sortingLayerName = GridCellLayerName;
                    
                    GridCell gridCell = square.AddComponent<GridCell>();
                    gridCell.Configure(gridConfig);
                    
                    if (x is 0 or 1)
                    {
                        playerGridCells.Add((BattleUnitPosition)slot, gridCell);
                    }
                    else if (x is 2 or 3)
                    {
                        enemyGridCells.Add((BattleUnitPosition)slot, gridCell);
                        
                    }

                    slot++;
                }
            }
        }
        
        public void ClearGrid()
        {
            while (transform.childCount > 0) {
                DestroyImmediate(transform.GetChild(0).gameObject);
            }

            playerGridCells.Clear();
            enemyGridCells.Clear();
        }

        private void InstantiateBattleUnits(bool isPlayerUnit, List<BattleUnitData> party)
        {
            foreach (var unit in party)
            {
                var unitInstance = Instantiate(battleUnitPrefab, transform);
                unitInstance.transform.position = GetGridSlot(isPlayerUnit, unit.battleUnitPosition);
                unitInstance.GetComponent<SpriteRenderer>().sprite = unit.icon;
            }
        }

        private Vector3 GetGridSlot(bool isPlayerUnit, BattleUnitPosition unitSOPosition)
        {
            return isPlayerUnit ? playerGridCells[unitSOPosition].transform.position : enemyGridCells[unitSOPosition].transform.position;
        }
    }
}
