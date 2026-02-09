using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Events;
using TacticsCore.Data;
using TacticsCore.EventBus;
using TacticsCore.HexGrid;
using TacticsCore.Units;
using UI_Toolkit;
using Units;
using UnityEngine;
using Util;

namespace Battle
{
    public class BattleManager : MonoBehaviour
    {
        public static BattleManager Instance { get; private set; }
        
        public BattleUnit CurrentBattler { get; private set; }
        public bool BattleInProgress { get; private set; }
        
        [SerializeField] private GameObject battleUnitPrefab;
        
        private Queue<BattleUnit> _turnOrder = new();
        private Dictionary<BattleUnitPosition, BattleUnit> _playerUnitDict;
        private Dictionary<BattleUnitPosition, BattleUnit> _enemyUnitDict;

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
            
            Bus<UnitDied>.RegisterForAll(OnUnitDied);
        }

        private void OnUnitDied(UnitDied args)
        {
            Debug.Log($"This guy died {args.BattleUnit.name}");
            _turnOrder = new Queue<BattleUnit>(_turnOrder.Where(u => u != args.BattleUnit));
            var battleUnitData = args.BattleUnit.UnitSO as BattleUnitData;
            if (!_playerUnitDict.Remove(battleUnitData.battleUnitPosition))
            {
                _enemyUnitDict.Remove(battleUnitData.battleUnitPosition);
            }
            TurnUI.Instance.RemoveDeadUnit(battleUnitData);
        }

        public void StartBattle(EngageInBattleEvent evt)
        {
            
            // Setup Parties
            _playerUnitDict = InstantiateBattleUnits(true, evt.Party);
            _enemyUnitDict = InstantiateBattleUnits(false, evt.EnemyParty);

            var allUnits = new List<BattleUnitData>();
            allUnits.AddRange(evt.Party);
            allUnits.AddRange(evt.EnemyParty);
            List<BattleUnitData> turnOrder = allUnits.OrderByDescending(u => u.stats.Find(s => s.type == StatType.Initiative).value).ToList();
            
            for (int index = 0; index < turnOrder.Count; index++)
            {
                BattleUnitData battleUnitData = turnOrder[index];
                BattleUnit battleUnit = battleUnitData.owner == Owner.Player1
                    ? _playerUnitDict[battleUnitData.battleUnitPosition]
                    : _enemyUnitDict[battleUnitData.battleUnitPosition];
                
                battleUnitData.inBattleInstance = battleUnit.gameObject;
                _turnOrder.Enqueue(battleUnit);
            }
            TurnUI.Instance.InitializeTurnOrder(turnOrder);

            StartCoroutine(BattleLoop());
        }

        private IEnumerator BattleLoop()
        {
            BattleInProgress = true;
            while (BattleInProgress)
            {
                CurrentBattler = _turnOrder.Dequeue();
                CurrentBattler.HighlightForCurrentTurn();
                while (!CurrentBattler.EndedTurn)
                {
                    CurrentBattler.IsMyTurn = true;
                    yield return null;
                }
                CurrentBattler.ResetHighlightForCurrentTurn();
                CurrentBattler.EndedTurn = false;
                _turnOrder.Enqueue(CurrentBattler);
                TurnUI.Instance.ShiftTopEntryToBottom();

                if (_playerUnitDict.Count == 0 || _enemyUnitDict.Count == 0)
                {
                    BattleInProgress = false;
                    SceneLoader.Instance.LoadScene(DTConstants.SceneNames.OverWorld, () => { });
                    break;
                }
                yield return null;
            }
        }

        private Dictionary<BattleUnitPosition, BattleUnit> InstantiateBattleUnits(bool isPlayerUnit, List<BattleUnitData> party)
        {
            Dictionary<BattleUnitPosition, BattleUnit> instantiatedUnits = new();
            for (int index = 0; index < party.Count; index++)
            {
                var unitInstance = Instantiate(battleUnitPrefab, transform);
                var gridSlot = GetGridSlot(isPlayerUnit, party[index].battleUnitPosition);
                var battleUnit = unitInstance.GetComponent<BattleUnit>();
                battleUnit.UnitSO = party[index];
                
                battleUnit.Owner = isPlayerUnit ? Owner.Player1 : Owner.AI1;
                battleUnit.GetComponent<Animator>().enabled = false;
                unitInstance.transform.position = Pathfinder.Instance.Pathfinding.Grid.GetWorldPosition(gridSlot);
                unitInstance.GetComponent<SpriteRenderer>().sprite = party[index].icon;
                
                instantiatedUnits.Add(party[index].battleUnitPosition, battleUnit);
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
