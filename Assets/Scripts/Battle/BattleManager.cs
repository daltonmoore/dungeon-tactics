using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Data;
using Events;
using Player;
using TacticsCore.Data;
using TacticsCore.EventBus;
using TacticsCore.Events;
using TacticsCore.HexGrid;
using TacticsCore.Save;
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
        [SerializeField] private Transform battleCameraTeleport;
        
        private Queue<BattleUnit> _turnOrder = new();
        private Dictionary<BattleUnitPosition, BattleUnit> _playerUnitDict;
        private Dictionary<BattleUnitPosition, BattleUnit> _enemyUnitDict;
        private List<BattleUnitData> _allUnits;
        // private List<LeaderSaveData> _leadersToSave;
        private Dictionary<Owner, LeaderSaveData> saveData = new();

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
            }
            else
            { 
                Instance = this;
            }

            Bus<EngageInBattleEvent>.OnEvent[Owner.Player1] += OnEngageInBattle;
            Bus<UnitDied>.RegisterForAll(OnUnitDied);
            Bus<UnitDamaged>.RegisterForAll(OnUnitDamaged);
        }

        private void OnUnitDamaged(UnitDamaged args)
        {
            var battleUnitData = args.battleUnit.UnitSO as BattleUnitData;
            Debug.Log($"Looking for unit with id = {battleUnitData.unitId}");
            saveData[args.battleUnit.Owner].party.First(u => u.unitId == battleUnitData.unitId).health -= args.damage;
             
            SaveManager.Save(new TDSaveData(saveData.Values.ToList()));
        }

        private void OnUnitDied(UnitDied args)
        {
            Debug.Log($"This guy died {args.BattleUnit.name}");
            _turnOrder = new Queue<BattleUnit>(_turnOrder.Where(u => u != args.BattleUnit));
            var battleUnitData = args.BattleUnit.UnitSO as BattleUnitData;
            Debug.Log($"Looking for unit with id = {battleUnitData.unitId}");
            var partyUnitRecord =
                saveData[args.BattleUnit.Owner].party
                    .First(u => u.unitId == battleUnitData.unitId);
            partyUnitRecord.health = 0;
            partyUnitRecord.isDead = true;
            SaveManager.Save(new TDSaveData(saveData.Values.ToList()));

            if (!_playerUnitDict.Remove(battleUnitData.battleUnitPosition))
            {
                _enemyUnitDict.Remove(battleUnitData.battleUnitPosition);
            }
            TurnUI.Instance.RemoveDeadUnit(battleUnitData);
        }
        private void OnEngageInBattle(EngageInBattleEvent evt)
        {
            // no longer do scene transition.
            // Let's just move over to the battle area in the overworld scene
            
            Bus<TeleportCameraEvent>.Raise(Owner.Player1, new TeleportCameraEvent(battleCameraTeleport.position));
            StartBattle(evt);

            LeaderSaveData GatherLeaderData(LeaderUnit leader)
            {
                List<BattleUnitSaveRecord> partySave =  new List<BattleUnitSaveRecord>(); 
                foreach (BattleUnitData battleUnitData in leader.PartyList)
                {
                    
                    partySave.Add(battleUnitData.ToSaveRecord());
                }
                return new LeaderSaveData()
                {
                    owner = leader.Owner,
                    position = leader.transform.position,
                    spriteName = leader.GetComponent<SpriteRenderer>().sprite.name,
                    party = partySave 
                };
            }
            saveData.Add(evt.Instigator.Owner, GatherLeaderData(evt.Instigator));
            saveData.Add(evt.Defender.Owner, GatherLeaderData(evt.Defender));
            
            SaveManager.Save(new TDSaveData(saveData.Values.ToList()));
        }

        public void StartBattle(EngageInBattleEvent evt)
        {
            
            // Setup Parties
            _playerUnitDict = InstantiateBattleUnits(true, evt.Party);
            _enemyUnitDict = InstantiateBattleUnits(false, evt.EnemyParty);

            _allUnits = new List<BattleUnitData>();
            _allUnits.AddRange(_playerUnitDict.Values.Select(u => u.UnitSO as BattleUnitData));
            _allUnits.AddRange(_enemyUnitDict.Values.Select(u => u.UnitSO as BattleUnitData));
            List<BattleUnitData> turnOrder = _allUnits.OrderByDescending(u => u.stats.Find(s => s.type == StatType.Initiative).value).ToList();
            
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
            bool DEBUG_EndBattleImmediately = false;
            
            while (BattleInProgress)
            {
                CurrentBattler = _turnOrder.Dequeue();
                CurrentBattler.HighlightForCurrentTurn();
                while (!CurrentBattler.EndedTurn)
                {
                    CurrentBattler.IsMyTurn = true;
                    if (Input.GetKeyDown(KeyCode.Quote))
                    {
                        DEBUG_EndBattleImmediately = true;
                        break;
                    }
                    yield return null;
                }
                CurrentBattler.ResetHighlightForCurrentTurn();
                CurrentBattler.EndedTurn = false;
                _turnOrder.Enqueue(CurrentBattler);
                TurnUI.Instance.ShiftTopEntryToBottom();

                if (_playerUnitDict.Count == 0 || _enemyUnitDict.Count == 0 || DEBUG_EndBattleImmediately)
                {
                    yield return new WaitForSeconds(1);
                    foreach (Transform child in transform)
                    {
                        Destroy(child.gameObject);
                    }
                    _turnOrder.Clear();
                    BattleInProgress = false;
                    ExitBattle();
                    Bus<ExitBattleEvent>.Raise(Owner.Player1, new ExitBattleEvent());
                    break;
                }
                yield return null;
            }
        }

        /// <summary>
        /// Save out results of the battle.
        /// </summary>
        private void ExitBattle()
        {
            Bus<TeleportCameraEvent>.Raise(Owner.Player1, new TeleportCameraEvent());
        }

        private Dictionary<BattleUnitPosition, BattleUnit> InstantiateBattleUnits(bool isPlayerUnit, List<BattleUnitData> party)
        {
            Dictionary<BattleUnitPosition, BattleUnit> instantiatedUnits = new();
            for (int index = 0; index < party.Count; index++)
            {
                var unitInstance = Instantiate(battleUnitPrefab, transform);
                var gridSlot = GetGridSlot(isPlayerUnit, party[index].battleUnitPosition);
                var battleUnit = unitInstance.GetComponent<BattleUnit>();
                battleUnit.UnitSO = Instantiate(party[index]);
                
                battleUnit.Owner = isPlayerUnit ? Owner.Player1 : Owner.AI1;
                battleUnit.GetComponent<Animator>().enabled = false;
                Pathfinder.Instance.Pathfinding.Grid.GetGridPosition(battleCameraTeleport.position, out int x, out int y);
                Debug.Log("X: " + x + " Y: " + y);
                unitInstance.transform.position = Pathfinder.Instance.Pathfinding.Grid.GetWorldPosition(gridSlot +  new Vector2Int(x, y));
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
