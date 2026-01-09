using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Events;
using Grid;
using HexGrid;
using UI_Toolkit;
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

        public void StartBattle(EngageInBattleEvent evt)
        {
            
            // Setup Parties
            var playerUnitDict = InstantiateBattleUnits(true, evt.Party);
            var enemyUnitDict = InstantiateBattleUnits(false, evt.EnemyParty);

            var allUnits = new List<BattleUnitData>();
            allUnits.AddRange(evt.Party);
            allUnits.AddRange(evt.EnemyParty);
            var turnOrder = allUnits.OrderByDescending(u => u.initiative).ToList();
            
            for (int index = 0; index < turnOrder.Count; index++)
            {
                BattleUnitData battleUnitData = turnOrder[index];
                battleUnitData.inBattleInstance = battleUnitData.owner == Owner.Player1
                    ? playerUnitDict[battleUnitData.battleUnitPosition].gameObject
                    : enemyUnitDict[battleUnitData.battleUnitPosition].gameObject;
                Debug.Log($"{index}: {battleUnitData.name} initiative {battleUnitData.initiative}");
                
            }
            SimpleRuntimeUI.Instance.InitializeTurnOrder(turnOrder);
            turnOrder.First().inBattleInstance.GetComponent<AbstractCommandable>().CurrentTurnHighlight();
        }
        
        private Dictionary<BattleUnitPosition, BaseMilitaryUnit> InstantiateBattleUnits(bool isPlayerUnit, List<BattleUnitData> party)
        {
            Dictionary<BattleUnitPosition, BaseMilitaryUnit> instantiatedUnits = new();
            for (int index = 0; index < party.Count; index++)
            {
                var unitInstance = Instantiate(battleUnitPrefab, transform);
                var gridSlot = GetGridSlot(isPlayerUnit, party[index].battleUnitPosition);
                var baseMilitaryUnit = unitInstance.GetComponent<BaseMilitaryUnit>();
                
                baseMilitaryUnit.Owner = isPlayerUnit ? Owner.Player1 : Owner.AI1;
                baseMilitaryUnit.GetComponent<Animator>().enabled = false;
                unitInstance.transform.position = Pathfinder.Instance.Pathfinding.Grid.GetWorldPosition(gridSlot);
                unitInstance.GetComponent<SpriteRenderer>().sprite = party[index].icon;
                
                instantiatedUnits.Add(party[index].battleUnitPosition, baseMilitaryUnit);
            }
            
            return instantiatedUnits;
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
            
            return slot;
        }
    }
}
