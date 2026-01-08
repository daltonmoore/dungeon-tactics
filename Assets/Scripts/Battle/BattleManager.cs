using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Events;
using Grid;
using HexGrid;
using Units;
using UnityEngine;
using UnityEngine.SceneManagement;
using Util;

namespace Battle
{
    public class BattleManager : MonoBehaviour
    {
        public static BattleManager Instance { get; private set; }
        
        [SerializeField] private GameObject battleUnitPrefab;
        
        public Queue<AbstractBattleUnit> TurnOrder { get; set; } = new();

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
            }
            else
            { 
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            
            
        }

        public void StartBattle(StartBattleEvent evt)
        {
            
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
        
        private void InstantiateBattleUnits(bool isPlayerUnit, List<BattleUnitData> party)
        {
            foreach (var unit in party)
            {
                var unitInstance = Instantiate(battleUnitPrefab, transform);
                var gridSlot = GetGridSlot(isPlayerUnit, unit.battleUnitPosition);
                var baseMilitaryUnit = unitInstance.GetComponent<BaseMilitaryUnit>();
                baseMilitaryUnit.Owner = isPlayerUnit ? Owner.Player1 : Owner.AI1;
                baseMilitaryUnit.GetComponent<Animator>().enabled = false;
                unitInstance.transform.position = Pathfinder.Instance.Pathfinding.Grid.GetWorldPosition(gridSlot) ;
                unitInstance.GetComponent<SpriteRenderer>().sprite = unit.icon;
            }
        }
        
        private Vector2Int GetGridSlot(bool isPlayerUnit, BattleUnitPosition battleUnitPosition)
        {
            Vector2Int slot = new(0,0);
            switch (battleUnitPosition)
            {
                case BattleUnitPosition.BackBottom:
                    slot = isPlayerUnit ? new Vector2Int(0,0) : new Vector2Int(3, 0);
                    break;
                case BattleUnitPosition.BackCenter:
                    slot = isPlayerUnit ? new Vector2Int(0,1) : new Vector2Int(3, 1);
                    break;
                case BattleUnitPosition.BackTop:
                    slot = isPlayerUnit ? new Vector2Int(0,2) : new Vector2Int(3, 2);
                    break;
                case BattleUnitPosition.FrontBottom:
                    slot = isPlayerUnit ? new Vector2Int(1,0) : new Vector2Int(2, 0);
                    break;
                case BattleUnitPosition.FrontCenter:
                    slot = isPlayerUnit ? new Vector2Int(1,1) : new Vector2Int(2, 1);
                    break;
                case BattleUnitPosition.FrontTop:
                    slot = isPlayerUnit ? new Vector2Int(1,2) : new Vector2Int(2, 2);
                    break;
            }
            
            Debug.Log($"Slot is {slot.x}, {slot.y}");
            
            return slot;
        }
    }
}
