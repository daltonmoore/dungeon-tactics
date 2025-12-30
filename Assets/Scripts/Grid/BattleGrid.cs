using System;
using System.Collections.Generic;
using Battle;
using Units;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Grid
{
    public class BattleGrid : MonoBehaviour
    {
        [field: SerializeField] public GridConfig gridConfig { get; private set; }
        [SerializeField] private GameObject gridCellPrefab;
        [SerializeField] private Vector3 gridOffset;
        
        Dictionary<BattleUnitPosition, GridCell> playerGridCells = new();
        Dictionary<BattleUnitPosition, GridCell> enemyGridCells = new();
        private const string GridCellLayerName = "Floor";

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
        
        public void SetupParties(BattleStartArgs args)
        {
            InstantiateBattleUnits(true, args.Party);
            InstantiateBattleUnits(false, args.EnemyParty);
        }

        private void InstantiateBattleUnits(bool isPlayerUnit, List<BattleUnitData> party)
        {
            foreach (var unit in party)
            {
                // if (unit.unitPrefab is null) continue;
                //
                // var unitInstance = Instantiate(unit.unitPrefab, transform);
                // unitInstance.transform.position = GetGridSlot(isPlayerUnit, unit.battleUnitPosition);
            }
        }

        private Vector3 GetGridSlot(bool isPlayerUnit, BattleUnitPosition unitSOPosition)
        {
            return isPlayerUnit ? playerGridCells[unitSOPosition].transform.position : enemyGridCells[unitSOPosition].transform.position;
        }
    }
}
