using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using Battle;
using Drawing;
using HexGrid;
using UnityEditor;
using UnityEngine;

namespace Units
{
    public abstract class AbstractUnit : AbstractCommandable, IAttacker, IAttackable
    {
        private const float STOPPINGDISTANCE = 0.1f;
        
        [SerializeField] private Transform flagPrefab;
        [SerializeField] protected float moveSpeed = 10f;
        [SerializeField] private AnimationConfigSO animationConfig;
        [field: SerializeField] public List<BattleUnitData> PartyList { get; set; } = new();
        public PathNodeHex BattleNode { get; set; }
        public List<PathNodeHex> Path { get; set; }

        protected UnitSO unitSO;

        private int _movePointsLeft;
        private Transform _flagParent;

        protected override void Awake()
        {
            base.Awake();
            unitSO = UnitSO as UnitSO;
            _movePointsLeft = unitSO.MovePoints;
            _flagParent = new GameObject("MoveFlags").transform;
        }

        private void OnDestroy()
        {
            if (_flagParent != null)
                Destroy(_flagParent.gameObject);
        }

        private void Start()
        {
            var gridObject = Pathfinder.Instance.Pathfinding.grid.GetGridObject(transform.position);
            gridObject.IsOccupied = true;
            gridObject.Occupant = this;
        }

        public void MoveTo(List<PathNodeHex> path, Action<bool> callback = null)
        {
            StopAllCoroutines();
            StartCoroutine(TravelPath(path, callback));
        }
        
        private IEnumerator TravelPath(List<PathNodeHex> path, Action<bool> callback)
        {
            if (path.Count == 0) yield break;
            
            HidePath();


            foreach (PathNodeHex node in path)
            {
                Vector3 targetPosition = node.worldPosition;
                // vector from A to B = B - A
                Vector3 direction = targetPosition.normalized - transform.position.normalized;
                
                while (Vector3.Distance(transform.position, targetPosition) > STOPPINGDISTANCE && _movePointsLeft - node.gCost >= 0)
                {
                    transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
                    yield return null;
                }
                _movePointsLeft -= node.gCost;
            }

            if (callback != null)
            {
                float distance = Vector3.Distance(transform.position, path[^1].worldPosition);
                bool isAtDestination = distance < STOPPINGDISTANCE;
                callback(isAtDestination);
            }
        }


        public void Attack(IAttackable attackable)
        {
            MoveTo(Path, arrivedAtDestination =>
            {
                Debug.Log(arrivedAtDestination ? "Arrived at destination" : "Failed to reach destination");
                bool arrivedAtBattleNode = BattleNode != null &&
                                           Vector3.Distance(transform.position, BattleNode.worldPosition) <=
                                           PathfindingHex.CellSize + STOPPINGDISTANCE;
                if (arrivedAtDestination || arrivedAtBattleNode)
                {
                    BattleManager.Instance.StartBattle(new BattleStartArgs
                        { Party = PartyList, EnemyParty = attackable.PartyList });
                }
            });
        }

        public void ShowPath(List<PathNodeHex> path, IAttackable attackable, out  PathNodeHex battleNode)
        {
            HidePath();

            int movePointsLeft = _movePointsLeft;
            battleNode = null;
            
            if (path.Count > 0)
            {
                Path = path;

                foreach (PathNodeHex node in path)
                {
                    Vector3 position = node.worldPosition;

                    using (Draw.ingame.WithDuration(2))
                    {
                        Draw.ingame.WireBox(position, Vector3.one * 0.2f, Color.red);
                    }

                    Transform flag = Instantiate(flagPrefab, position, Quaternion.identity);
                    flag.SetParent(_flagParent);
                    Color flagColor = movePointsLeft - node.gCost >= 0 ? Color.blue : Color.white;
                    movePointsLeft -= node.gCost;
                    
                    // flag is red when attacking or moving within a hex of unit
                    if (battleNode == null)
                    {
                        if (node.IsOccupied && node.Occupant != null && node.Occupant.Owner != Owner)
                        {
                            battleNode = node;
                        }

                        foreach (var neighbor in Pathfinder.Instance.Pathfinding.GetNeighborList(node))
                        {
                            if (neighbor.IsOccupied && neighbor.Occupant != null && neighbor.Occupant.Owner != Owner)
                            {
                                Debug.Log($"Neighbor is occupied");
                                battleNode = neighbor;
                                break;
                            }
                        }
                    }
                    if (battleNode != null) flagColor = Color.red;
                    flag.GetComponent<SpriteRenderer>().color = flagColor;
                }
            }
        }
        
        public void HidePath()
        {
            foreach (Transform flag in _flagParent)
            {
                Destroy(flag.gameObject);
            }
            
            Path = null;
        }

    }
    
    public enum BattleUnitPosition
    {
        BackBottom = 0,     // mod 3 is back to 0
        BackCenter = 1,     // mod 3 is 1
        BackTop = 2,         // mod 3 is 2
        FrontBottom = 3,    // mod 3 is 0
        FrontCenter = 4,    // mod 3 is 1
        FrontTop = 5,       // mod 3 is 2
    }
}